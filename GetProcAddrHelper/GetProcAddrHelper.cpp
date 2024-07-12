#define WIN32_LEAN_AND_MEAN 1
#include <Windows.h>
#include <shellapi.h>

static_assert(sizeof(unsigned int) == sizeof(size_t));

// GetProcAddrHelper <library> <proc name>
unsigned int main(int argc, char* argv[])
{
    // bad arg count
    if (argc != 3)
        return 0;
    HMODULE library = LoadLibraryA(argv[1]);
    if (library)
    {
        size_t results = reinterpret_cast<size_t>(GetProcAddress(library, argv[2]));
        FreeLibrary(library);
        return results;
    }
    return 0;
}

/*
* CRT replacement code
*/

void* malloc(size_t length)
{
    return HeapAlloc(GetProcessHeap(), 0, length);
}

void free(void* ptr)
{
    if (ptr)
    {
        HeapFree(GetProcessHeap(), 0, ptr);
    }
}

// copied from stackoverflow CC BY-SA 4.0 https://stackoverflow.com/a/74999569
// https://creativecommons.org/licenses/by-sa/4.0/
// edited to use a reference instead of a pointer for the two arguments
void get_command_line_args(int &argc, char** &argv)
{
    // Get the command line arguments as wchar_t strings
    wchar_t** wargv = CommandLineToArgvW(GetCommandLineW(), &argc);
    if (!wargv) { argc = 0; *argv = NULL; return; }

    // Count the number of bytes necessary to store the UTF-8 versions of those strings
    int n = 0;
    for (int i = 0; i < argc; i++)
        n += WideCharToMultiByte(CP_UTF8, 0, wargv[i], -1, NULL, 0, NULL, NULL) + 1;

    // Allocate the argv[] array + all the UTF-8 strings
    argv = (char**)malloc((argc + 1) * sizeof(char*) + n);
    if (!argv) { argc = 0; return; }

    // Convert all wargv[] --> argv[]
    char* arg = (char*)&(argv[argc + 1]);
    for (int i = 0; i < argc; i++)
    {
        argv[i] = arg;
        arg += WideCharToMultiByte(CP_UTF8, 0, wargv[i], -1, arg, n, NULL, NULL) + 1;
    }
    argv[argc] = NULL;
}
// end copied from stackoverflow

extern "C" void WinMainCRTStartup()
{
    int argc{};
    char** argv{};

    get_command_line_args(argc, argv);
    unsigned int exitCode = main(argc, argv);
    free(argv);

    ExitProcess(exitCode);
}
