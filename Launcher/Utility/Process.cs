using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ToolkitLauncher.Utility
{
    public static partial class Process
    {
        /// <summary>
        /// Final result of running a process
        /// </summary>
        public record Result(string Output, string Error, int ReturnCode)
        {
            public bool HasErrorOccured
            {
                get => ReturnCode != 0;
            }

            public bool Success
            {
                get => ReturnCode == 0;
            }
        }
        /// <summary>
        /// Run an executable and wait for it to exit
        /// </summary>
        /// <param name="directory">Path containing the executable</param>
        /// <param name="executable">Excutable name</param>
        /// <param name="args">unescaped arguments</param>
        /// <param name="cancellationToken"> Cancellation token for canceling the process before it exists</param>
        /// <param name="lowPriority">Lower priority if possible</param>
        /// <returns>A task that will complete when the executable exits</returns>
        static public Task<Result> StartProcess(string directory, string executable, List<string> args, CancellationToken cancellationToken = default, bool lowPriority = false, bool admin = false)
        {
            Debug.Print($"starting(): directory: {directory}, executable:{executable}, args:{args}, admin: {admin}, low priority {lowPriority}");
            if (OperatingSystem.IsWindows())
                return Windows.StartProcess(directory, executable, args, cancellationToken, lowPriority, admin);
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Run a executable in a shell (cmd.exe on windows) that pauses after the executable returns
        /// </summary>
        /// <param name="directory">Path containing the executable</param>
        /// <param name="executable">unescaped name</param>
        /// <param name="args">escaped arguments string</param>
        /// <param name="cancellationToken"> Cancellation token for canceling the process before it exists</param>
        /// <param name="lowPriority">Lower priority if possible</param>
        /// <returns>A task that will complete when the executable exits</returns>
        static public Task<Result?> StartProcessWithShell(string directory, string executable, string args, CancellationToken cancellationToken = default, bool lowPriority = false)
        {
            Debug.Print($"starting_with_shell(): directory: {directory}, executable:{executable}, args:{args}");
            if (OperatingSystem.IsWindows())
                return Windows.StartProcessWithShell(directory, executable, args, cancellationToken, lowPriority);
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Run a executable in a shell (cmd.exe on windows) that pauses after the executable returns
        /// </summary>
        /// <param name="directory">Path containing the executable</param>
        /// <param name="executable">unescaped name</param>
        /// <param name="args">unescaped arguments</param>
        /// <param name="cancellationToken"> Cancellation token for canceling the process before it exists</param>
        /// <param name="lowPriority">Lower priority if possible</param>
        /// <returns>A task that will complete when the executable exits</returns>
        static public async Task<Result?> StartProcessWithShell(string directory, string executable, List<string> args, CancellationToken cancellationToken = default, bool lowPriority = false)
        {
            return await StartProcessWithShell(directory, executable, EscapeArgList(args), cancellationToken, lowPriority);
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
        /// Open the URL in the default web browser
        /// </summary>
        /// <param name="url"></param>
        static public void OpenURL(string url)
        {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException();
            System.Diagnostics.Process.Start("explorer", url);
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
