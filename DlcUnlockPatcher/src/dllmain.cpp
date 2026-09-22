#include <windows.h>
#include "bootstrap.h"

BOOL APIENTRY DllMain(HMODULE selfModule, DWORD reason, LPVOID lpReserved)
{
    switch (reason)
    {
    case DLL_PROCESS_ATTACH:
        DisableThreadLibraryCalls(selfModule);
        CreateThread(nullptr, 0, BootstrapThread, selfModule, 0, nullptr);
        break;
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}
