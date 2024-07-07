using PeNet.Header.Pe;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.System.Memory;

namespace ToolkitLauncher.Utility
{
    public class DLLInjector : IProcessInjector
    {
        readonly private string dll_path;
        readonly private Dictionary<Guid, EventWaitHandle> _events = new();
        readonly private Dictionary<Guid, System.Diagnostics.Process> _processes = new();

        public delegate void ModifyEnviroment(IDictionary<string, string?> Enviroment);

        readonly private ModifyEnviroment _modifyEnviroment;

		public DLLInjector(byte[] dll_binary, string dll_name = "injected.dll", ModifyEnviroment modifyEnviroment = null)
        {
            _modifyEnviroment = modifyEnviroment;

			Directory.CreateDirectory(App.TempFolder);
            dll_path = Path.Combine(App.TempFolder, "DllInjector." + Guid.NewGuid().ToString() + "." + dll_name);

            File.WriteAllBytes(dll_path, dll_binary);
        }

        ~DLLInjector()
        {
            File.Delete(dll_path);
        }

        public const string INJECTOR_ENVIROMENTAL_VARIABLE = "OSOYOOS_INJECTOR_EVENT";

        private static string GetEventName(Guid id)
        {
            return $"OSOYOOS_INJECT_{id}";
        }

        private Guid Preinject()
        {
            Guid id = Guid.NewGuid();
            EventWaitHandle signal = new(initialState: false, mode: EventResetMode.ManualReset, GetEventName(id));
            _events[id] = signal;

            return id;
        }

        public Guid SetupEnviroment(ProcessStartInfo startInfo)
        {
            Trace.WriteLine("SetupEnviroment - DLL injector");

            Guid injector_id = Preinject();

            startInfo.Environment[INJECTOR_ENVIROMENTAL_VARIABLE] = DLLInjector.GetEventName(injector_id);

            if (_modifyEnviroment is not null)
				_modifyEnviroment(startInfo.Environment);

            return injector_id;
        }

        private class WrappedProcessHandle : SafeHandle
        {
            public WrappedProcessHandle(System.Diagnostics.Process process) : base(process.Handle, false)
            {

            }

            public override bool IsInvalid => false;

			protected override bool ReleaseHandle()
			{
                return false;
			}
		}


        [DllImport("KERNEL32.dll", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [SupportedOSPlatform("windows5.1.2600")]
        internal static extern unsafe bool WriteProcessMemory(HANDLE hProcess, void* lpBaseAddress, byte[] lpBuffer, nuint nSize, [Optional] nuint* lpNumberOfBytesWritten);

        [DllImport("KERNEL32.dll", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [SupportedOSPlatform("windows5.1.2600")]
        internal static extern unsafe HANDLE CreateRemoteThread(HANDLE hProcess, SECURITY_ATTRIBUTES* lpThreadAttributes, nuint dwStackSize, FARPROC lpStartAddress, void* lpParameter, uint dwCreationFlags, uint* lpThreadId);


		[DllImport("ntdll.dll", PreserveSig = false)]
		public static extern void NtSuspendProcess(IntPtr processHandle);

		[DllImport("ntdll.dll", PreserveSig = false, SetLastError = true)]
		public static extern void NtResumeProcess(IntPtr processHandle);

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWow64Process([In] IntPtr processHandle,
	 [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

		private record ModuleInfo(string Name, string? Filename, IntPtr Base);


#if false
        [SupportedOSPlatform("windows5.1.2600")]
		private unsafe List<ModuleInfo> EnumerateLoadedModules64(System.Diagnostics.Process process)
		{
            Trace.WriteLine("Enumerating process modules");

			List<ModuleInfo> moduleInfos = new();
            PInvoke.EnumerateLoadedModulesEx((HANDLE)process.Handle, 
                (PCSTR ModuleName, ulong ModuleBase, uint ModuleSize, void* UserContext) => {
                    moduleInfos.Add(new ModuleInfo(ModuleName.ToString(), ModuleBase, ModuleSize));
                    return true;
                });

            return moduleInfos;
		}
#endif
        [SupportedOSPlatform("windows6.0.6000")]
		private unsafe List<ModuleInfo>? EnumProcessModules32Bit(System.Diagnostics.Process process)
		{
			Trace.WriteLine("Enumerating process modules");
            HANDLE handle = (HANDLE)process.Handle;
            WrappedProcessHandle wrappedProcessHandle = new(process);

			uint bytesRequired = 0;
            if (!PInvoke.EnumProcessModulesEx(handle, null, 0, &bytesRequired, Windows.Win32.System.ProcessStatus.ENUM_PROCESS_MODULES_EX_FLAGS.LIST_MODULES_32BIT))
            {
                Trace.WriteLine($"Failed to get module list size {Marshal.GetLastWin32Error()}");
                return null;
            }

            HMODULE[] modules = new HMODULE[(bytesRequired / IntPtr.Size) * 2];
            fixed (HMODULE *ptr = modules)
            {
				if (!PInvoke.EnumProcessModulesEx(handle, ptr, (uint)(modules.Length*IntPtr.Size), &bytesRequired, Windows.Win32.System.ProcessStatus.ENUM_PROCESS_MODULES_EX_FLAGS.LIST_MODULES_32BIT))
                {
					Trace.WriteLine($"Failed to get module list {Marshal.GetLastWin32Error()}");
					return null;
				}
			}
            
            

			List<ModuleInfo> moduleInfos = new();
            char[] scratch_backing = new char[0x1000];
            uint scratch_size = (uint)scratch_backing.Length;

			fixed (char* scratch = scratch_backing)
            {

                foreach (HMODULE moduleHandle in modules)
                {
					// skip null entries
					if (moduleHandle == HMODULE.Null)
						continue;

                    uint lengthOfBaseName = PInvoke.GetModuleBaseName(handle, moduleHandle, scratch, scratch_size);
					if (lengthOfBaseName == 0)
                    {
                        Trace.WriteLine($"Failed to get base name for module {moduleHandle}");
                        continue;
                    }
                    string BaseName = ((PWSTR)scratch).ToString();

#if false
                    string? Filename = null;
                    uint lengthOfFilename = PInvoke.GetModuleFileNameEx(handle, moduleHandle, scratch, scratch_size);
                    if (lengthOfFilename == 0)
                    {
                        Trace.WriteLine($"Failed to get file name for module {moduleHandle}");
                    }
                    else
                    {
						Filename = ((PWSTR)scratch).ToString();
					}
#endif
					string? Filename = null;
                    uint lengthOfFilename = PInvoke.GetMappedFileName(handle, moduleHandle.Value.ToPointer(), scratch, scratch_size);
					if (lengthOfFilename == 0)
					{
						Trace.WriteLine($"Failed to get file name for module {moduleHandle}");
					}
					else
					{
						Filename = ((PWSTR)scratch).ToString();
					}

                    const string device_path_prefix = @"\Device\";

					if (Filename is not null && Filename.StartsWith(device_path_prefix))
                    {
                        Filename = @"\\?\" + Filename[device_path_prefix.Length..];
                    }

					moduleInfos.Add(new(BaseName, Filename, moduleHandle.Value));
				}
            }

			return moduleInfos;
		}

        static private uint? GetImportOffsetForPE(string filepath, string importName)
        {
            var peFile = new PeNet.PeFile(filepath);
			ExportFunction? export = peFile.ExportedFunctions.First(e => importName == e.Name);

            if (export is null)
                return null;

            if (export.HasForward)
            {
                Trace.WriteLine("Found API but it is a forward, failing");
                return null;
            }

            return export.Address;

		}


		[SupportedOSPlatform("windows6.0.6000")]
		private async Task<FARPROC> GetLoadLibraryForProcess(System.Diagnostics.Process process)
        {
            bool isWOW64;
            if (!IsWow64Process(process.Handle, out isWOW64))
                throw new Win32Exception();
            if (!isWOW64)
            {
                Trace.WriteLine("64-bit target process, getting LoadLibraryA the easy way");

				var kernal32 = PInvoke.GetModuleHandle("kernel32.dll");
				FARPROC load_library_proc = PInvoke.GetProcAddress(kernal32, "LoadLibraryA");
                return load_library_proc;
			}
            else
            {
				Trace.WriteLine("32-bit target process, hopefully getting LoadLibraryA anyways");
				WrappedProcessHandle wrappedProcessHandle = new(process);

				for (int i = 0; i < 100; i++)
                {
					List<ModuleInfo>? moduleList = EnumProcessModules32Bit(process);

                    if (moduleList is not null)
                    {

                        foreach (ModuleInfo module in moduleList)
                        {
                            Trace.WriteLine($"Mod: {module}");
                        }

						ModuleInfo? kernel32 = moduleList.Find(m => m.Name.ToUpper() == "KERNELBASE.DLL");
                        if (kernel32 is null)
							kernel32 = moduleList.Find(m => m.Name.ToUpper() == "KERNEL32.DLL");


						if (kernel32 is not null)
                        {
                            uint? rva = GetImportOffsetForPE(kernel32.Filename, "LoadLibraryA");
                            if (rva is null)
                                return FARPROC.Null;
                            return (FARPROC)IntPtr.Add(kernel32.Base, (int)rva.Value);
						}
                        else
                        {
                            Trace.WriteLine("Failed to find kernelbase dll!");
                        }

					}
                    else
                    {
                        Trace.WriteLine("Failed to get module list!");
                    }

					// resume the process and hope it loads the DLL needed next time
					NtResumeProcess(process.Handle);
					// geometric backoff;
					await Task.Delay(10*i);
                    NtSuspendProcess(process.Handle);
                }

                return FARPROC.Null;

			}

		}

		[SupportedOSPlatform("windows6.0.6000")]
		public async Task<bool> Inject(Guid id, System.Diagnostics.Process process)
        {
			Trace.WriteLine("Inject - DLL injector");
#if DEBUG
            // disable timeout for debug builds so we can debug the injection process
            const uint timeout = uint.MaxValue;
#else
            const uint timeout = 1800; // 1.8 seconds
#endif

            HANDLE processHandle = (HANDLE)process.Handle;
            Trace.WriteLine($"processHandle: {processHandle}");

            // suspend the process in prep for DLL injection
            NtSuspendProcess(processHandle);
            Trace.WriteLine($"Target process suspended, ready for DLL injection sequence (last error = {Marshal.GetLastWin32Error()})");

			EventWaitHandle injectionSignal = _events[id];
            _processes[id] = process;

            FARPROC load_library_proc = await GetLoadLibraryForProcess(process);

            Trace.WriteLine($"load_library_proc: {load_library_proc.Value:X}");

            if (load_library_proc == FARPROC.Null)
                return false;


			byte[] file_path = Encoding.Default.GetBytes(dll_path + "\0");
            HANDLE injection_thread;

            unsafe
            {

                void* remote_dll_string = PInvoke.VirtualAllocEx(
					processHandle,
                    null,
                    (nuint)(file_path.Length + 1),
                    VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE,
                    PAGE_PROTECTION_FLAGS.PAGE_READWRITE);

                if (remote_dll_string is null)
                {
                    Trace.WriteLine($"Failed to allocate DLL path string in remote process - {Marshal.GetLastWin32Error()}!");
                    return false;
                }

                if (!WriteProcessMemory(processHandle, remote_dll_string, file_path, (nuint)file_path.Length))
                {
                    Trace.WriteLine($"Failed to write DLL path string into remote process - {Marshal.GetLastWin32Error()}!");
                    return false;
                }

				NtResumeProcess(process.Handle);
				Trace.WriteLine($"Target process resumed, ready for thread injection (last error = {Marshal.GetLastWin32Error()})");

				Trace.WriteLine("Injecting DLL into remote thread!");
                injection_thread = CreateRemoteThread(processHandle, null, 0, load_library_proc, remote_dll_string, 0, null);
                if (injection_thread == IntPtr.Zero)
                {
                    Trace.WriteLine($"Failed create injection thread for remote process - {Marshal.GetLastWin32Error()}!");
                    return false;
                }
            }

			WAIT_EVENT wait_result = await Task.Run(() => PInvoke.WaitForSingleObject(injection_thread, timeout));

            if (wait_result == WAIT_EVENT.WAIT_TIMEOUT || wait_result == WAIT_EVENT.WAIT_FAILED)
            {
                Trace.WriteLine($"Waiting on injection thread failed: {wait_result} - {Marshal.GetLastWin32Error()}!");
                return false;
            }

			uint exit_code;

            unsafe
            {
				if (!PInvoke.GetExitCodeThread(injection_thread, &exit_code))
                {
                    Trace.WriteLine("Failed to get thread exit code!");
                    return false;
                }
			}

            

#if DEBUG
            bool has_started_up_successfully = await Task.Run(() => injectionSignal.WaitOne());
#else
            bool has_started_up_successfully = await Task.Run(() => injectionSignal.WaitOne(TimeSpan.FromMilliseconds(timeout)));
#endif

			return has_started_up_successfully;
        }
    }
}
