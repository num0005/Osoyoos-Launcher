/*
 Copyright (c) num0005. Some rights reserved
 This software is part of the Osoyoos Launcher.
 Released under the MIT License, see LICENSE.md for more information.
*/


#include "platform.h"
#include "H2ToolHooks.h"
#include "Debug.h"
#include <cstdio>

static void attach_to_console()
{
    AllocConsole();
    FILE* pCout;
    freopen_s(&pCout, "CONOUT$", "w", stdout);
    freopen_s(&pCout, "CONOUT$", "w", stderr);
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        OutputDebugStringA("[DLL FIX] DLL_PROCESS_ATTACH\n");
        attach_to_console();
        if (!H2ToolHooks::hook())
        {
            DebugPrintf("[DLL FIX] FAILURE?");
            DebugPrintf("[DLL FIX] Failed to apply launcher hooks to tool. This is quite bad.");

        }
        else
        {
            char event_name[0x1000];
            if (GetEnvironmentVariableA("OSOYOOS_INJECTOR_EVENT", event_name, sizeof(event_name)))
            {
                HANDLE event = OpenEventA(EVENT_MODIFY_STATE, FALSE, event_name);
                if (event != 0)
                {
#if _DEBUG
                    DebugPrintf("[DLL FIX] Injected successfully!");
#endif
                    if (!SetEvent(event))
                    {
                        DebugPrintf("[DLL FIX] Failed to communicate back to launcher: %x!", GetLastError());
                    }
                    CloseHandle(event);
                }
                else
                {
                    DebugPrintf("[DLL FIX] Failed to open event");
                }
            }
            else
            {
                DebugPrintf("[DLL FIX] Failed to get injector event name!");
            }
        }
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

