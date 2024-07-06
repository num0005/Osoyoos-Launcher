#pragma once
#include "platform.h"
#include <stdio.h>

inline static void DebugPrintf(
    _In_z_ _Printf_format_string_ const char* fmt, ...)
{
    va_list ArgList;
    va_start(ArgList, fmt);

    char message[0x1000];

    vsprintf_s(message, fmt, ArgList);
    strcat_s(message, "\n");

    printf("%s", message);
    OutputDebugStringA(message);
}
