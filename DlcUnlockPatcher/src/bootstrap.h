#pragma once

#include <windows.h>

// lpParameter = 自身 HMODULE（用于定位 proxy.ini）
DWORD WINAPI BootstrapThread(LPVOID lpParameter);
