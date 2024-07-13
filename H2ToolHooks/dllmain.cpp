/*
 Copyright (c) num0005. Some rights reserved
 This software is part of the Osoyoos Launcher.
 Released under the MIT License, see LICENSE.md for more information.
*/


#include "platform.h"
#include "H2ToolHooks.h"
#include "Debug.h"
#include <cstdio>
#include <iostream>

static void attach_to_console()
{
    AllocConsole();
    FILE* pCout;
    freopen_s(&pCout, "CONOUT$", "w", stdout);
    freopen_s(&pCout, "CONOUT$", "w", stderr);
}

static bool is_enviroment_variable_set(const char* var)
{
    char var_value[2] = {};
    return GetEnvironmentVariableA(var, var_value, sizeof(var_value)) != 0;
}

static bool is_launcher_variable_set(const char* var)
{
    char env_variable[0x100];
    sprintf_s(env_variable, "OSOYOOS_INJECTOR_%s", var);

    return is_enviroment_variable_set(env_variable);
}

template <size_t length>
size_t get_launcher_variable(const char* var, char(&value)[length])
{
    char env_variable_name[0x100];
    sprintf_s(env_variable_name, "OSOYOOS_INJECTOR_%s", var);

    size_t len = GetEnvironmentVariableA(env_variable_name, value, length);
    value[length - 1] = 0; // ensure null teriminateion
    return len;
}

static bool pause_on_exit = false;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    {
        OutputDebugStringA("[DLL FIX] DLL_PROCESS_ATTACH\n");
        attach_to_console();

        pause_on_exit = is_launcher_variable_set("PAUSE_ON_EXIT");

        int flags = {};

        if (is_launcher_variable_set("DISABLE_ASSERTIONS"))
            flags |= H2ToolHooks::HookFlags::DisableAsserts;
        if (is_launcher_variable_set("PATCH_QUALITY"))
            flags |= H2ToolHooks::HookFlags::PatchLightmapQuality;

        if (flags == 0 && !is_launcher_variable_set("EVENT"))
        {
            DebugPrintf("[DLL FIX] Not injected by launcher?! Enabling assertions patch. Safe flying pilot.");
            flags |= H2ToolHooks::HookFlags::DisableAsserts;
        }

        if (!H2ToolHooks::hook(static_cast<H2ToolHooks::HookFlags>(flags)))
        {
            DebugPrintf("[DLL FIX] FAILURE?");
            DebugPrintf("[DLL FIX] Failed to apply launcher hooks to tool. This is quite bad.");

        }
        else
        {
            char event_name[0x100];
            if (get_launcher_variable("EVENT", event_name))
            {
                HANDLE event = OpenEventA(EVENT_MODIFY_STATE, FALSE, event_name);
                if (event != 0)
                {
                    if (!SetEvent(event))
                    {
                        DebugPrintf("[DLL FIX] Failed to communicate back to launcher: %x!", GetLastError());
                    }
                    else
                    {
                        DebugPrintf("[DLL FIX] Injected successfully!");
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
    }
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        
        if (pause_on_exit)
        {
            std::string _;
            std::cout << "Press enter to close console" << std::endl;
            std::cin >> _;
        }
        break;
    }
    return TRUE;
}

