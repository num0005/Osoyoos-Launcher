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
using ManagedBlamHelper;
using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace OsoyoosMB
{
    public class MBHandler
    {
        // random ID for MBHandler
        public const string command_id = "70512702-FBD6-400F-8398-E96D8EB3D802";
        // command to setup bitmaps
        public const string setup_bitmaps_command = "setup_bitmap_compression";

        internal record EditingKitInfo(string Path, bool IsGen4, string TagDirectory, string DataDirectory);

        private static Assembly _managedblam_assembly = null;
        private static FileStream _patchedBinary = null;

        /// <summary>
        /// Load assemblies from the bin folder in the working directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly LoadFromBinFolder(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new AssemblyName(args.Name);
            Debug.WriteLine($"{assemblyName}");

            if (assemblyName.Name == "managedblam")
            {
                Trace.WriteLine("Redirecting managedblam assembly load!");
                return _managedblam_assembly;
            }

            string currentPath = Directory.GetCurrentDirectory();
            string assemblyPath = Path.Join(currentPath, "bin", assemblyName.Name) + ".dll";

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
        private static bool PreloadManagedBlam(bool is_corinth, string corinth_temp_path)
        {
            string currentPath = Directory.GetCurrentDirectory();
            string binManagedBlamPath = Path.Join(currentPath, "bin", "ManagedBlam.dll");
            try
            {
                _managedblam_assembly = is_corinth ? LoadCorinthAssembly(binManagedBlamPath, corinth_temp_path) : Assembly.LoadFile(binManagedBlamPath);
                return true;
            } catch (FileNotFoundException ex)
            {
                Trace.WriteLine($"ManagedBlam not found! Expection: {ex}");
                // bad times
                return false;
            }
        }

        private static Assembly LoadCorinthAssembly(string sourcePath, string temporaryPath)
        {

            byte[] libraryBinary = File.ReadAllBytes(sourcePath);

            Trace.WriteLine("Patching h4/h2a managedBlam binary!");
            foreach (ArraySegment<byte> stringToEdit in Utility.FindStringsWithPrefixInBinary(libraryBinary, "Corinth"))
            {
                string decodedString = Encoding.UTF8.GetString(stringToEdit);
                string patchedString = decodedString.Replace("Corinth", "Bungie");

                Trace.WriteLine($"Patching string at offfset {stringToEdit.Offset:X} \"{decodedString[0..(decodedString.Length-1)]}\" -> \"{patchedString[0..(patchedString.Length - 1)]}\"");

                byte[] patchedStringBytes = Encoding.UTF8.GetBytes(patchedString);

                Trace.Assert(patchedString.Length <= decodedString.Length);
                Trace.Assert(patchedStringBytes.Length <= stringToEdit.Count);

                patchedStringBytes.CopyTo(stringToEdit.AsSpan());
            }

#if DEBUG
            try
            {
                string debugDumpPath = @"R:\managedblam.patched.dll";
                File.WriteAllBytes(debugDumpPath, libraryBinary);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to dump assembly for debugging");
                Trace.WriteLine(ex);
            }
#endif

            
            _patchedBinary = OpenPatchedBinaryStream(temporaryPath);
            _patchedBinary.Write(libraryBinary);
            _patchedBinary.Close();

            return Assembly.LoadFile(temporaryPath);
        }

        [SupportedOSPlatform("windows")]
        private static FileStream OpenPatchedBinaryStream(string path)
        {
            const uint FILE_ATTRIBUTE_NORMAL = 0x80;
            const uint FILE_ATTRIBUTE_TEMPORARY = 0x100;
            const uint FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;
            const int INVALID_HANDLE_VALUE = -1;
            const uint GENERIC_READ = 0x80000000;
            const uint GENERIC_WRITE = 0x40000000;
            const uint CREATE_NEW = 1;
            const uint CREATE_ALWAYS = 2;
            const uint OPEN_EXISTING = 3;

            const uint FILE_SHARE_READ = 0x1;
            const uint FILE_SHARE_WRITE = 0x2;
            const uint FILE_SHARE_DELETE = 0x4;

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess,
                uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
                uint dwFlagsAndAttributes, IntPtr hTemplateFile);

            SafeFileHandle fileHandle = CreateFile(path, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, CREATE_NEW, FILE_ATTRIBUTE_TEMPORARY, IntPtr.Zero);

            if (fileHandle.IsInvalid)
                throw new Exception("Failed to open stream for patched binary");

            return new FileStream(fileHandle, FileAccess.ReadWrite);

        }

        [SupportedOSPlatform("windows")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Premain(String[] args)
        {
            try
            {
                bool corinth_patch_needed = Boolean.Parse(args[4]);
                string corinth_temporary_path = null;
                if (corinth_patch_needed)
                    corinth_temporary_path = args[5];

                EditingKitInfo ek_info = new(args[0], Boolean.Parse(args[1]), args[2], args[3]);
                // premain setup
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromBinFolder);
                if (!PreloadManagedBlam(corinth_patch_needed, corinth_temporary_path))
                {
                    Trace.WriteLine($"Failed to load managed blam!");
                    return -2;
                }

                return RunCommands(ek_info, args[(corinth_patch_needed ? 6: 5)..]);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in pre-main: {ex}");
                return -3;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int RunCommands(EditingKitInfo ek_info, String[] args)
        {
            if (args[0] == setup_bitmaps_command && args.Length == 4)
            {
                ManagedBlamInterface.Start(ek_info);
                Trace.WriteLine("Running setup_bitmap_compression");
                BitmapSettings.ConfigureCompression(ek_info, args[1], args[2], Boolean.Parse(args[3]));
                ManagedBlamInterface.Stop();
                return 0;
            }
            else
            {
                Trace.WriteLine("Unknown command!");
                return -1;
            }
        }
    }
}
