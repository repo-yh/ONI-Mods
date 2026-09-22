// version.dll 真身转发：lazy 加载 System32 真身 + GetProcAddress 函数指针调用。
// 签名以 Windows Kits winver.h 为准；GetFileVersionInfoByHandle 无公开原型，
// 按 5 参透传（与 GetFileVersionInfoExW 同构 + handle）。
// VerLanguageNameA/W 不在此实现——.def 里 PE 转发到 KERNEL32。

#include <windows.h>

static HMODULE g_real = nullptr;

static FARPROC RealProc(const char* name)
{
    if (!g_real)
    {
        g_real = LoadLibraryW(L"C:\\Windows\\System32\\version.dll");
        if (!g_real)
        {
            return nullptr;
        }
    }
    return GetProcAddress(g_real, name);
}

// ----- GetFileVersionInfo 系列 -----

BOOL WINAPI GetFileVersionInfoA(LPCSTR lptstrFilename, DWORD dwHandle, DWORD dwLen, LPVOID lpData)
{
    typedef BOOL(WINAPI * Fn)(LPCSTR, DWORD, DWORD, LPVOID);
    Fn f = (Fn)RealProc("GetFileVersionInfoA");
    return f ? f(lptstrFilename, dwHandle, dwLen, lpData) : FALSE;
}

BOOL WINAPI GetFileVersionInfoW(LPCWSTR lptstrFilename, DWORD dwHandle, DWORD dwLen, LPVOID lpData)
{
    typedef BOOL(WINAPI * Fn)(LPCWSTR, DWORD, DWORD, LPVOID);
    Fn f = (Fn)RealProc("GetFileVersionInfoW");
    return f ? f(lptstrFilename, dwHandle, dwLen, lpData) : FALSE;
}

DWORD WINAPI GetFileVersionInfoSizeA(LPCSTR lptstrFilename, LPDWORD lpdwHandle)
{
    typedef DWORD(WINAPI * Fn)(LPCSTR, LPDWORD);
    Fn f = (Fn)RealProc("GetFileVersionInfoSizeA");
    return f ? f(lptstrFilename, lpdwHandle) : 0;
}

DWORD WINAPI GetFileVersionInfoSizeW(LPCWSTR lptstrFilename, LPDWORD lpdwHandle)
{
    typedef DWORD(WINAPI * Fn)(LPCWSTR, LPDWORD);
    Fn f = (Fn)RealProc("GetFileVersionInfoSizeW");
    return f ? f(lptstrFilename, lpdwHandle) : 0;
}

BOOL WINAPI GetFileVersionInfoExA(DWORD dwFlags, LPCSTR lpwstrFilename, DWORD dwHandle, DWORD dwLen, LPVOID lpData)
{
    typedef BOOL(WINAPI * Fn)(DWORD, LPCSTR, DWORD, DWORD, LPVOID);
    Fn f = (Fn)RealProc("GetFileVersionInfoExA");
    return f ? f(dwFlags, lpwstrFilename, dwHandle, dwLen, lpData) : FALSE;
}

BOOL WINAPI GetFileVersionInfoExW(DWORD dwFlags, LPCWSTR lpwstrFilename, DWORD dwHandle, DWORD dwLen, LPVOID lpData)
{
    typedef BOOL(WINAPI * Fn)(DWORD, LPCWSTR, DWORD, DWORD, LPVOID);
    Fn f = (Fn)RealProc("GetFileVersionInfoExW");
    return f ? f(dwFlags, lpwstrFilename, dwHandle, dwLen, lpData) : FALSE;
}

DWORD WINAPI GetFileVersionInfoSizeExA(DWORD dwFlags, LPCSTR lpwstrFilename, LPDWORD lpdwHandle)
{
    typedef DWORD(WINAPI * Fn)(DWORD, LPCSTR, LPDWORD);
    Fn f = (Fn)RealProc("GetFileVersionInfoSizeExA");
    return f ? f(dwFlags, lpwstrFilename, lpdwHandle) : 0;
}

DWORD WINAPI GetFileVersionInfoSizeExW(DWORD dwFlags, LPCWSTR lpwstrFilename, LPDWORD lpdwHandle)
{
    typedef DWORD(WINAPI * Fn)(DWORD, LPCWSTR, LPDWORD);
    Fn f = (Fn)RealProc("GetFileVersionInfoSizeExW");
    return f ? f(dwFlags, lpwstrFilename, lpdwHandle) : 0;
}

BOOL WINAPI GetFileVersionInfoByHandle(DWORD a1, LPCWSTR a2, DWORD a3, DWORD a4, LPVOID a5)
{
    typedef BOOL(WINAPI * Fn)(DWORD, LPCWSTR, DWORD, DWORD, LPVOID);
    Fn f = (Fn)RealProc("GetFileVersionInfoByHandle");
    return f ? f(a1, a2, a3, a4, a5) : FALSE;
}

// ----- VerFindFile / VerInstallFile -----

DWORD WINAPI VerFindFileA(DWORD uFlags, LPCSTR szFileName, LPCSTR szWinDir, LPCSTR szAppDir,
                          LPSTR szCurDir, PUINT puCurDirLen, LPSTR szDestDir, PUINT puDestDirLen)
{
    typedef DWORD(WINAPI * Fn)(DWORD, LPCSTR, LPCSTR, LPCSTR, LPSTR, PUINT, LPSTR, PUINT);
    Fn f = (Fn)RealProc("VerFindFileA");
    return f ? f(uFlags, szFileName, szWinDir, szAppDir, szCurDir, puCurDirLen, szDestDir, puDestDirLen) : 0;
}

DWORD WINAPI VerFindFileW(DWORD uFlags, LPCWSTR szFileName, LPCWSTR szWinDir, LPCWSTR szAppDir,
                          LPWSTR szCurDir, PUINT puCurDirLen, LPWSTR szDestDir, PUINT puDestDirLen)
{
    typedef DWORD(WINAPI * Fn)(DWORD, LPCWSTR, LPCWSTR, LPCWSTR, LPWSTR, PUINT, LPWSTR, PUINT);
    Fn f = (Fn)RealProc("VerFindFileW");
    return f ? f(uFlags, szFileName, szWinDir, szAppDir, szCurDir, puCurDirLen, szDestDir, puDestDirLen) : 0;
}

DWORD WINAPI VerInstallFileA(DWORD uFlags, LPCSTR szSrcFileName, LPCSTR szDestFileName,
                             LPCSTR szSrcDir, LPCSTR szDestDir, LPCSTR szCurDir,
                             LPSTR szTmpFile, PUINT puTmpFileLen)
{
    typedef DWORD(WINAPI * Fn)(DWORD, LPCSTR, LPCSTR, LPCSTR, LPCSTR, LPCSTR, LPSTR, PUINT);
    Fn f = (Fn)RealProc("VerInstallFileA");
    return f ? f(uFlags, szSrcFileName, szDestFileName, szSrcDir, szDestDir, szCurDir, szTmpFile, puTmpFileLen) : 0;
}

DWORD WINAPI VerInstallFileW(DWORD uFlags, LPCWSTR szSrcFileName, LPCWSTR szDestFileName,
                             LPCWSTR szSrcDir, LPCWSTR szDestDir, LPCWSTR szCurDir,
                             LPWSTR szTmpFile, PUINT puTmpFileLen)
{
    typedef DWORD(WINAPI * Fn)(DWORD, LPCWSTR, LPCWSTR, LPCWSTR, LPCWSTR, LPCWSTR, LPWSTR, PUINT);
    Fn f = (Fn)RealProc("VerInstallFileW");
    return f ? f(uFlags, szSrcFileName, szDestFileName, szSrcDir, szDestDir, szCurDir, szTmpFile, puTmpFileLen) : 0;
}

// ----- VerQueryValue -----

BOOL WINAPI VerQueryValueA(LPCVOID pBlock, LPCSTR lpSubBlock, LPVOID * lplpBuffer, PUINT puLen)
{
    typedef BOOL(WINAPI * Fn)(LPCVOID, LPCSTR, LPVOID *, PUINT);
    Fn f = (Fn)RealProc("VerQueryValueA");
    return f ? f(pBlock, lpSubBlock, lplpBuffer, puLen) : FALSE;
}

BOOL WINAPI VerQueryValueW(LPCVOID pBlock, LPCWSTR lpSubBlock, LPVOID * lplpBuffer, PUINT puLen)
{
    typedef BOOL(WINAPI * Fn)(LPCVOID, LPCWSTR, LPVOID *, PUINT);
    Fn f = (Fn)RealProc("VerQueryValueW");
    return f ? f(pBlock, lpSubBlock, lplpBuffer, puLen) : FALSE;
}
