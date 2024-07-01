using Bungie;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static OsoyoosMB.MBHandler;

namespace ManagedBlamHelper
{
    internal class ManagedBlamInterface
    {
        private static void ManagedBlamCrashCallback(ManagedBlamCrashInfo info)
        {
            Trace.WriteLine($"Managed blam {info.Type.ToString().ToLower()}: {info.Message}");
            Trace.WriteLine($"Location: {info.File}:{info.Line}");
        }

        private static bool is_gen4;

        public static bool IsGen4 => is_gen4;

        public static void Start(EditingKitInfo info)
        {
            Trace.WriteLine($"Starting ManagedBlam: {info}");
            is_gen4 = info.IsGen4;
            const InitializationType InitializationLevel = InitializationType.TagsOnly;
            if (info.IsGen4)
            {
                ManagedBlamStartupParameters parameters = new();
                parameters.InitializationLevel = InitializationLevel;

                ManagedBlamSystem.Start(info.Path, ManagedBlamCrashCallback, parameters);
            }
            else
            {
                StartGen3(info, InitializationLevel);
            }
        }

        public static void Stop()
        {
            Trace.WriteLine($"Stopping ManagedBlam: IsGen4: {is_gen4}");
            if (is_gen4)
            {
                ManagedBlamSystem.Stop();
            }
            else
            {
                StopGen3();
            }
        }

        private static InitializationType _InitializationLevelGen3 = InitializationType.None;

        /// <summary>
        /// Setup Gen3 editing kit, we need a special method for this cause gen4 doesn't have the required types.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="initializationType"></param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void StartGen3(EditingKitInfo info, InitializationType InitializationLevel)
        {
            ManagedBlamSystem.InitializeProject(InitializationLevel, info.Path);
            _InitializationLevelGen3 = InitializationLevel;
        }

        /// <summary>
        /// Also no-inline due to missing required types on gen4.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void StopGen3()
        {
            ManagedBlamSystem.Dispose(_InitializationLevelGen3);
        }
    }
}
