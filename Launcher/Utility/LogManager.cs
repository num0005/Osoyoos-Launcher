using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ToolkitLauncher.Utility
{
    class LogManager
    {
        static public readonly string LogFolder = Path.Combine(App.OsoyoosSavePath, "logs");

        private const int _min_logs_to_keep = 10;
        private const int _min_days_to_store_logs = 14;

        static private readonly TimeSpan _time_to_keep_logs = TimeSpan.FromDays(_min_days_to_store_logs);
        static private string _log_filename = null;

        static public void RotateLogs()
        {
            Trace.WriteLine($"Rotating logs....");
            try
            {
                IEnumerable<FileInfo> files = new DirectoryInfo(LogFolder).EnumerateFiles()
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(_min_logs_to_keep)
                    .SkipWhile(f => DateTime.UtcNow - f.CreationTimeUtc < _time_to_keep_logs);
                int deletedCount = 0;
                // delete selected files
                foreach (var file in files)
                {
                    try
                    {
                        file.Delete();
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Failed to delete old log file {file.FullName} \r\n {ex}");
                        Trace.WriteLine(ex);
                    }
                }

                Trace.WriteLine($"Deleted {deletedCount} logs.");
            } catch (Exception ex)
            {
                Trace.WriteLine($"Failed to enumerate log files {ex}");
            }

        }

        private static string GetVersion()
        {
            try
            {
                return Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            } catch (Exception ex)
            {
                Trace.WriteLine($"Failed to get assembly version {ex}");
                return "unknown";
            }
        }

        public static void InitializeLogging(string application)
        {
            Directory.CreateDirectory(LogFolder);

            _log_filename = Path.Combine(LogFolder, $"{application}_{DateTime.Now:yyyy-MMM-dd-THHmmss}.log");

            TextWriterTraceListener logListener = new(_log_filename, application);

            Debug.AutoFlush = true;
            Trace.AutoFlush = true;

            Trace.Listeners.Add(logListener);

            Trace.WriteLine($"Launcher (subapplication {application}) version {GetVersion()} at {DateTime.Now}");
        }
    }
}
