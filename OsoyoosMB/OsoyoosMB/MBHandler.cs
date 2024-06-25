/*
BUILD INFORMATION

This project uses ManagedBlam.dll, but that can't be added to the GitHub repo.
In order to still be built, the project uses a Reference Assembly version
of the ManagedBlam DLL. This is generated using the NetBrains tool Refasmer.

In theory it already contains all namespaces/methods etc present in the full DLL.
In case it needs to be regenerated in future howerver:

Refasmer can be installed from the terminal with "dotnet tool install -g JetBrains.Refasmer.CliTool"
Once installed, run with "refasmer -v -O ref -c ManagedBlam.dll"

When you need to debug this code, you need to switch the project reference to the "full" .dll or it
will crash at runtime. Don't forget to revert the reference back to version in the "ref" folder
before committing or release.
*/
using System;
using System.Configuration.Assemblies;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OsoyoosMB
{
    internal class MBHandler
    {

        /// <summary>
        /// Load assemblies from the bin folder in the working directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static Assembly LoadFromBinFolder(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new AssemblyName(args.Name);
            Debug.Write($"{assemblyName}");

            string currentPath = Directory.GetCurrentDirectory();
            string assemblyPath = Path.Combine(currentPath, "bin", assemblyName.Name) + ".dll";

            if (File.Exists(assemblyPath))
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Preload the managed blam assembly
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool PreloadManagedBlam()
        {
            string currentPath = Directory.GetCurrentDirectory();
            string binManagedBlamPath = Path.Combine(currentPath, "bin", "ManagedBlam.dll");
            try
            {
                Assembly.LoadFile(binManagedBlamPath);
                return true;
            } catch (FileNotFoundException)
            {
                Console.WriteLine("Unable to find ManagedBlam!");
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Main(String[] args)
        {
            // premain setup
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromBinFolder);
            PreloadManagedBlam();
            ProgramMain(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProgramMain(String[] args)
        {
            // not used for anything apart from testing
            System.Threading.Tasks.Task.Run(() => { });
            if (args.Length == 0)
            {
                Console.WriteLine("Do not run this manually, it is a helper executable for Osoyoos. This is not a standalone application.\nPress Enter to exit.");
                Console.ReadLine();
            }
            else
            {
                if (args[0] == "getbitmapdata" && args.Length == 5)
                {
                    Console.WriteLine("Running GetBitmapData");
                    BitmapSettings.GetBitmapData(args[1], args[2], args[3], int.Parse(args[4]));
                }
                else
                {
                    Console.WriteLine("Insufficient arguments");
                }
            }
        }
        
        /*
        //Use this instead if you need to debug GetBitmapData(), can't debug when run from the main Osoyoos solution
        public static void Main()
        {
            BitmapSettings.GetBitmapData(@"C:\Program Files (x86)\Steam\steamapps\common\H3EK", @"objects\scenery\minecraft_door\bitmaps", @"C:\Program Files (x86)\Steam\steamapps\common\H3EK\tags", "2");
        }
        */
    }
}
