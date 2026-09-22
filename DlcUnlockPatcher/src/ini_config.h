#pragma once

#include <windows.h>
#include <string>

struct IniConfig
{
    bool enabled;
    bool log;                  // 日志开关（dlcpatcher.log），默认开
    std::wstring patcherDir;   // 已解析为绝对路径
    std::wstring matchId;
    std::wstring managedFallback; // <游戏根>\OxygenNotIncluded_Data\Managed
};

// baseDir = version.dll 所在目录（所有相对路径的解析基准）
IniConfig LoadIni(const std::wstring& baseDir);

std::wstring GetSelfDir(HMODULE selfModule);
void ProxyLog(const std::wstring& baseDir, const std::string& msg);
