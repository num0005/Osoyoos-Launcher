/*
BUILD INFORMATION

This project uses ManagedBlam.dll, but that can't be added to the GitHub repo.
In order to still be built, the project uses a Reference Assembly version
of the ManagedBlam DLL. This is generated using the NetBrains tool Refasmer.

In theory it already contains all namespaces/methods etc present in the full DLL.
In case it needs to be regenerated in future howerver:

Refasmer can be installed from the terminal with "dotnet tool install -g JetBrains.Refasmer.CliTool"
Once installed, run with "refasmer -v -O ref -c ManagedBlam.dll"
*/
using System;

namespace OsoyoosMB
{
    internal class MBHandler
    {
        
        public static void Main(String[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Do not run this manually, it is a helper executable for Osoyoos. This is not a standalone application.\nPress Enter to exit.");
                Console.ReadLine();

            }
            else
            {
                if (args[0] == "getbitmapdata" && args.Length >= 4)
                {
                    Console.WriteLine("Running GetBitmapData");
                    BitmapSettings.GetBitmapData(args[1], args[2], args[3]);
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
            BitmapSettings.GetBitmapData(@"C:\Program Files (x86)\Steam\steamapps\common\H3EK", @"objects\scenery\minecraft_door\bitmaps", "Uncompressed");
        }
        */
    }
}
