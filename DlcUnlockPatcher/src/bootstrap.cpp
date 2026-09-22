#include "bootstrap.h"
#include "ini_config.h"
#include <string>
#include <vector>

// ----- Mono 不透明类型与稳定导出签名（Mono 2.0 世代至今未变） -----

typedef struct _MonoDomain MonoDomain;
typedef struct _MonoImage MonoImage;
typedef struct _MonoAssembly MonoAssembly;
typedef struct _MonoClass MonoClass;
typedef struct _MonoMethod MonoMethod;
typedef struct _MonoObject MonoObject;
typedef int MonoImageOpenStatus;

typedef MonoDomain * (*mono_get_root_domain_t)(void);
typedef void *       (*mono_thread_attach_t)(MonoDomain *);
typedef MonoImage *  (*mono_image_loaded_t)(const char *);
typedef MonoImage *  (*mono_image_open_t)(const char *, MonoImageOpenStatus *);
typedef MonoAssembly * (*mono_assembly_load_from_full_t)(MonoImage *, const char *, MonoImageOpenStatus *, int);
typedef MonoImage *  (*mono_assembly_get_image_t)(MonoAssembly *);
typedef MonoClass *  (*mono_class_from_name_t)(MonoImage *, const char *, const char *);
typedef MonoMethod * (*mono_class_get_method_from_name_t)(MonoClass *, const char *, int);
typedef MonoObject * (*mono_runtime_invoke_t)(MonoMethod *, void *, void **, MonoObject **);

static mono_get_root_domain_t pGetRootDomain = nullptr;
static mono_thread_attach_t pThreadAttach = nullptr;
static mono_image_loaded_t pImageLoaded = nullptr;
static mono_image_open_t pImageOpen = nullptr;
static mono_assembly_load_from_full_t pAssemblyLoadFromFull = nullptr;
static mono_assembly_get_image_t pAssemblyGetImage = nullptr;
static mono_class_from_name_t pClassFromName = nullptr;
static mono_class_get_method_from_name_t pClassGetMethod = nullptr;
static mono_runtime_invoke_t pRuntimeInvoke = nullptr;

static std::wstring g_baseDir;
static bool g_logEnabled = true; // ini [loader] log 控制；C# 侧经 DLC_PATCHER_LOG 同步

static void Log(const std::string& msg)
{
    if (!g_logEnabled)
    {
        return;
    }
    ProxyLog(g_baseDir, msg);
}

static std::string WideToUtf8(const std::wstring& w)
{
    if (w.empty())
    {
        return std::string();
    }
    int n = WideCharToMultiByte(CP_UTF8, 0, w.c_str(), (int)w.size(), nullptr, 0, nullptr, nullptr);
    std::string s(n, 0);
    WideCharToMultiByte(CP_UTF8, 0, w.c_str(), (int)w.size(), &s[0], n, nullptr, nullptr);
    return s;
}

static bool ResolveMonoApis(HMODULE mono)
{
    struct Entry { const char* name; void** target; };
    Entry table[] = {
        { "mono_get_root_domain",            (void**)&pGetRootDomain },
        { "mono_thread_attach",              (void**)&pThreadAttach },
        { "mono_image_loaded",               (void**)&pImageLoaded },
        { "mono_image_open",                 (void**)&pImageOpen },
        { "mono_assembly_load_from_full",    (void**)&pAssemblyLoadFromFull },
        { "mono_assembly_get_image",         (void**)&pAssemblyGetImage },
        { "mono_class_from_name",            (void**)&pClassFromName },
        { "mono_class_get_method_from_name", (void**)&pClassGetMethod },
        { "mono_runtime_invoke",             (void**)&pRuntimeInvoke },
    };
    for (auto& e : table)
    {
        void* p = (void*)GetProcAddress(mono, e.name);
        if (!p)
        {
            Log(std::string("missing mono export: ") + e.name);
            return false;
        }
        *e.target = p;
    }
    return true;
}

// mono 程序集加载：宽路径 → UTF-8 → image_open + assembly_load_from_full
static MonoAssembly * LoadManaged(const std::wstring& pathW)
{
    std::string path = WideToUtf8(pathW);
    MonoImageOpenStatus st = (MonoImageOpenStatus)0;
    MonoImage * img = pImageOpen(path.c_str(), &st);
    if (!img)
    {
        Log("image_open failed: " + path);
        return nullptr;
    }
    MonoAssembly * asm_ = pAssemblyLoadFromFull(img, path.c_str(), &st, 0);
    if (!asm_)
    {
        Log("assembly_load_from_full failed: " + path);
        return nullptr;
    }
    Log("loaded: " + path);
    return asm_;
}

static std::wstring g_managedFallback;

static MonoAssembly * LoadManagedWithFallback(const std::wstring& primaryDir, const wchar_t* file)
{
    MonoAssembly * a = LoadManaged(primaryDir + L"\\" + file);
    if (!a)
    {
        a = LoadManaged(g_managedFallback + L"\\" + file);
    }
    return a;
}

DWORD WINAPI BootstrapThread(LPVOID lpParameter)
{
    HMODULE selfModule = (HMODULE)lpParameter;
    g_baseDir = GetSelfDir(selfModule);

    IniConfig cfg = LoadIni(g_baseDir);
    g_logEnabled = cfg.log;
    if (!cfg.enabled)
    {
        Log("disabled by ini, exit");
        return 0;
    }
    g_managedFallback = cfg.managedFallback;
    Log("boot, match_id=" + WideToUtf8(cfg.matchId) + ", patcher_dir=" + WideToUtf8(cfg.patcherDir));

    // 1. 等 mono 模块（最长 120s）
    HMODULE mono = nullptr;
    for (int i = 0; i < 2400; i++)
    {
        mono = GetModuleHandleW(L"mono-2.0-bdwgc.dll");
        if (mono)
        {
            break;
        }
        Sleep(50);
    }
    if (!mono)
    {
        Log("mono-2.0-bdwgc.dll timeout");
        return 0;
    }

    // 2. 解析 8 组稳定导出
    if (!ResolveMonoApis(mono))
    {
        return 0;
    }

    // 3. 等 firstpass 镜像（DlcManager 所在；必然早于首场景 Awake）
    for (int i = 0; i < 2400; i++)
    {
        if (pImageLoaded("Assembly-CSharp-firstpass"))
        {
            break;
        }
        Sleep(50);
    }
    if (!pImageLoaded("Assembly-CSharp-firstpass"))
    {
        Log("firstpass image timeout");
        return 0;
    }

    // 4. attach 托管线程
    MonoDomain * dom = pGetRootDomain();
    if (!dom)
    {
        Log("root domain null");
        return 0;
    }
    pThreadAttach(dom);

    // 5. 传参给 C# 侧（match_id + patcher 目录 + 日志开关）
    SetEnvironmentVariableW(L"DLC_PATCHER_MATCH_ID", cfg.matchId.c_str());
    SetEnvironmentVariableW(L"DLC_PATCHER_DIR", cfg.patcherDir.c_str());
    SetEnvironmentVariableW(L"DLC_PATCHER_LOG", cfg.log ? L"1" : L"0");

    // 6. 显式加载：先 0Harmony（兜底 Managed 目录），再 patcher
    if (!LoadManagedWithFallback(cfg.patcherDir, L"0Harmony.dll"))
    {
        Log("0Harmony.dll load failed (both candidates)");
        return 0;
    }
    MonoAssembly * patcherAsm = LoadManaged(cfg.patcherDir + L"\\DlcUnlockPatcher.dll");
    if (!patcherAsm)
    {
        Log("DlcUnlockPatcher.dll load failed");
        return 0;
    }

    // 7. 定位 DlcUnlockPatcher.Entry.Run() 并执行
    MonoImage * img = pAssemblyGetImage(patcherAsm);
    MonoClass * cls = pClassFromName(img, "DlcUnlockPatcher", "Entry");
    if (!cls)
    {
        Log("class not found: DlcUnlockPatcher.Entry");
        return 0;
    }
    MonoMethod * run = pClassGetMethod(cls, "Run", 0);
    if (!run)
    {
        Log("method not found: Entry.Run");
        return 0;
    }

    Log("invoking Entry.Run");
    pRuntimeInvoke(run, nullptr, nullptr, nullptr);
    Log("bootstrap done");
    return 0;
}
