#include "ini_config.h"
#include <shlwapi.h>

#pragma comment(lib, "shlwapi.lib")

std::wstring GetSelfDir(HMODULE selfModule)
{
    wchar_t path[MAX_PATH] = { 0 };
    GetModuleFileNameW(selfModule, path, MAX_PATH);
    PathRemoveFileSpecW(path);
    return path;
}

// 布尔值严格用 1/0：GetPrivateProfileIntW 只认整数，写其他值（如 true/false）会回落默认，属配置错误
IniConfig LoadIni(const std::wstring& baseDir)
{
    IniConfig cfg = {};
    std::wstring iniPath = baseDir + L"\\proxy.ini";

    wchar_t buf[MAX_PATH] = { 0 };

    cfg.enabled = GetPrivateProfileIntW(L"loader", L"enabled", 1, iniPath.c_str()) != 0;
    cfg.log = GetPrivateProfileIntW(L"loader", L"log", 1, iniPath.c_str()) != 0;

    GetPrivateProfileStringW(L"loader", L"patcher_dir", L"DlcUnlockPatcher",
                             buf, MAX_PATH, iniPath.c_str());
    cfg.patcherDir = buf;
    if (cfg.patcherDir.find(L':') == std::wstring::npos)
    {
        cfg.patcherDir = baseDir + L"\\" + cfg.patcherDir;
    }

    GetPrivateProfileStringW(L"patch", L"match_id", L"COSMETIC1_ID",
                             buf, MAX_PATH, iniPath.c_str());
    cfg.matchId = buf;

    cfg.managedFallback = baseDir + L"\\OxygenNotIncluded_Data\\Managed";
    return cfg;
}

void ProxyLog(const std::wstring& baseDir, const std::string& msg)
{
    // 与 C# 侧 Entry.Log 同写一个日志文件；共享读写避免两侧短暂并发时打开失败
    std::wstring logPath = baseDir + L"\\dlcpatcher.log";
    HANDLE h = CreateFileW(logPath.c_str(), FILE_APPEND_DATA, FILE_SHARE_READ | FILE_SHARE_WRITE,
                           nullptr, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
    if (h == INVALID_HANDLE_VALUE)
    {
        return;
    }
    SYSTEMTIME st;
    GetLocalTime(&st);
    char line[2048];
    int n = sprintf_s(line, sizeof(line), "%04u-%02u-%02u %02u:%02u:%02u.%03u %s\r\n",
                      st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond,
                      st.wMilliseconds, msg.c_str());
    DWORD written = 0;
    if (n > 0)
    {
        WriteFile(h, line, (DWORD)n, &written, nullptr);
    }
    CloseHandle(h);
}
