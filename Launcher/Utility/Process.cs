using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ToolkitLauncher.Utility
{
    internal static partial class Process
    {
        /// <summary>
        /// Run an executable and wait for it to exit
        /// </summary>
        /// <param name="directory">Path containing the executable</param>
        /// <param name="executable">Excutable name</param>
        /// <param name="args">unescaped arguments</param>
        /// <returns>A task that will complete when the executable exits</returns>
        static public Task StartProcess(string directory, string executable, List<string> args)
        {
            if (OperatingSystem.IsWindows())
                return Windows.StartProcess(directory, executable, args);
            throw new NotImplementedException("Unsupported platform!");
        }

        /// <summary>
        /// Run a executable in a shell (cmd.exe on windows) that pauses after the executable returns
        /// </summary>
        /// <param name="directory">Path containing the executable</param>
        /// <param name="executable">unescaped name</param>
        /// <param name="args">escaped arguments string</param>
        /// <returns>A task that will complete when the executable exits</returns>
        static public Task StartProcessWithShell(string directory, string executable, string args)
        {
            if (OperatingSystem.IsWindows())
                return Windows.StartProcessWithShell(directory, executable, args);
            throw new NotImplementedException("Unsupported platform!");
        }

        /// <summary>
        /// Run a executable in a shell (cmd.exe on windows) that pauses after the executable returns
        /// </summary>
        /// <param name="directory">Path containing the executable</param>
        /// <param name="executable">unescaped name</param>
        /// <param name="args">unescaped arguments</param>
        /// <returns>A task that will complete when the executable exits</returns>
        static public async Task StartProcessWithShell(string directory, string executable, List<string> args)
        {
            await StartProcessWithShell(directory, executable, EscapeArgList(args));
        }

        /// <summary>
        /// Convert an argument list to an escaped command string
        /// </summary>
        /// <param name="args">Unescaped arguments</param>
        /// <returns>Escaped command string</returns>
        static public string EscapeArgList(List<string> args)
        {
            string commnad_line = "";
            foreach (string arg in args)
                commnad_line += escape_arg(arg);
            return commnad_line;
        }

        /// <summary>
        /// Helper function to escape arguments
        /// </summary>
        /// <param name="arg">The unescaped argument</param>
        /// <returns>Escaped argument</returns>
        private static string escape_arg(string arg)
        {
            return " \"" + Regex.Replace(arg, @"(\\+)$", @"$1$1") + "\"";
        }
    }
}
