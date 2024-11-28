using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
        private readonly FileStream _dll_filestream_lock;
        readonly private Dictionary<Guid, EventWaitHandle> _events = new();
        readonly private Dictionary<Guid, System.Diagnostics.Process> _processes = new();

        public delegate void ModifyEnviroment(IDictionary<string, string?> Enviroment);

        readonly private ModifyEnviroment _modifyEnviroment;
        readonly private bool _earlyInjection;

        virtual public bool ShouldSuspendOnLaunch => _earlyInjection;

		public DLLInjector(byte[] dll_binary, string dll_name = "injected.dll", ModifyEnviroment modifyEnviroment = null, bool earlyInjection = false)
        {
            _modifyEnviroment = modifyEnviroment;
            _earlyInjection = earlyInjection;

			Directory.CreateDirectory(App.TempFolder);
            dll_path = Path.Combine(App.TempFolder, "DllInjector." + Guid.NewGuid().ToString() + "." + dll_name);

            File.WriteAllBytes(dll_path, dll_binary);

            // prevent other processes from deleting the dll while we are still using it.
			_dll_filestream_lock = new(dll_path, FileMode.Open, FileAccess.Read, FileShare.Read);

		}

        ~DLLInjector()
        {
            _dll_filestream_lock.Close();

			File.Delete(dll_path);
        }

        public static string GetVariableName(string variable)
        {
            return $"OSOYOOS_INJECTOR_{variable}";
		}

        public string INJECTOR_ENVIROMENTAL_VARIABLE => GetVariableName("EVENT");


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

			public WrappedProcessHandle(HANDLE process) : base(process, false)
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


        private static bool IsProcessWow64(System.Diagnostics.Process process)
        {
			bool isWOW64;
			if (!IsWow64Process(process.Handle, out isWOW64))
				throw new Win32Exception();
            return isWOW64;
		}

        static object _32bitLock = new();
        static string? _32bitHelperPath = null;
        static FileStream _32bitHelperPathLock = null; // filestream used to lock the file for write and delete
		static Dictionary<(string, string), FARPROC> _32bit_procs_cache = new();

        private static void _deploy_get_proc_helper()
        {

            _32bitHelperPath = Path.Combine(App.TempFolder, "DllInjector.GetProcAddrHelper." + Guid.NewGuid().ToString() + ".exe");
            File.WriteAllBytes(_32bitHelperPath, Utility.Resources.GetProcAddrHelper);
            _32bitHelperPathLock = new(_32bitHelperPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

        /// <summary>
        /// Get the address of a procdure. Only works for a few special libararies that are mapped at the same address in all modules
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="procName"></param>
        /// <returns></returns>
		static private async Task<FARPROC> GetLibraryProcAddress32(string moduleName, string procName)
        {
            System.Diagnostics.Process process;
            var cacheKey = (moduleName.ToUpperInvariant(), procName);

            // check cache first
            // this method only works for modules loaded at the same address in all processes anyways
            lock (_32bit_procs_cache)
            {
			    if (_32bit_procs_cache.ContainsKey(cacheKey))
                    return _32bit_procs_cache[cacheKey];
			}

			lock (_32bitLock)
            {
                if (_32bitHelperPath is null || !File.Exists(_32bitHelperPath))
                    _deploy_get_proc_helper();

                List<string> args = new() { moduleName.Trim(), procName.Trim() };
				process = System.Diagnostics.Process.Start(_32bitHelperPath, args);
            }

            await process.WaitForExitAsync();
            IntPtr ptr = new(process.ExitCode);
            FARPROC ptrProc = new(ptr);


			if (ptr != IntPtr.Zero)
            {
                lock (_32bit_procs_cache)
                {
                    _32bit_procs_cache[cacheKey]  = ptrProc;
				}
            }

            return ptrProc;
        }


		[SupportedOSPlatform("windows6.0.6000")]
		static private async Task<FARPROC> GetLibraryProcAddressForProcess(System.Diagnostics.Process process, string moduleName, string procName, string? moduleNameBackup = null)
        {
            bool isWOW64 = IsProcessWow64(process);
            FARPROC procAddr = FARPROC.Null;

			if (!isWOW64)
            {
                Trace.WriteLine($"64-bit target process, getting {procName} the easy way");

				var module = PInvoke.GetModuleHandle(moduleName);
				procAddr = PInvoke.GetProcAddress(module, procName);
			}
            else
            {
				Trace.WriteLine($"32-bit target process, getting {procName} the hard way using helper exe");
				procAddr = await GetLibraryProcAddress32(moduleName, procName);
                if (procAddr == FARPROC.Null && moduleNameBackup is not null)
                {
                    procAddr = await GetLibraryProcAddress32(moduleNameBackup, procName);
				}
			}

            return procAddr;
		}
		[SupportedOSPlatform("windows6.0.6000")]
		static private unsafe void* AllocateAndWriteIntoProcess(HANDLE processHandle, byte[] data, bool isExecutable)
        {
			void* remoteData = PInvoke.VirtualAllocEx(
	            processHandle,
	            null,
	            (nuint)(data.Length + 1),
	            VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE,
	            PAGE_PROTECTION_FLAGS.PAGE_READWRITE);

			if (remoteData is null)
			{
				Trace.WriteLine($"Failed to allocate data (executable?:{isExecutable}) in remote process - {Marshal.GetLastWin32Error()}!");
				return null;
			}

			if (!WriteProcessMemory(processHandle, remoteData, data, (nuint)data.Length))
			{
                PInvoke.VirtualFreeEx(processHandle, remoteData, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE);
				Trace.WriteLine($"Failed to write data (executable?:{isExecutable}) into remote process - {Marshal.GetLastWin32Error()}!");
				return null;
			}

            if (isExecutable)
            {
                PAGE_PROTECTION_FLAGS oldProtection;

                if (!PInvoke.VirtualProtectEx(new WrappedProcessHandle(processHandle), remoteData, (nuint)data.Length, PAGE_PROTECTION_FLAGS.PAGE_EXECUTE, out oldProtection))
                {
					PInvoke.VirtualFreeEx(processHandle, remoteData, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE);
					Trace.WriteLine($"Failed make data in remote process executable - {Marshal.GetLastWin32Error()}!");
                    return null;
				}

			}

            return remoteData;
		}

		[SupportedOSPlatform("windows6.0.6000")]
		private async Task<HANDLE> EarlyProcessInject(System.Diagnostics.Process process, string dllName, uint timeout, bool injectWOW64 = false)
        {
			bool isWOW64 = IsProcessWow64(process);

			injectWOW64 = injectWOW64 && isWOW64;

			bool amd64Bytecode = injectWOW64 || !isWOW64;

			FARPROC LdrLoadDll = await GetLibraryProcAddressForProcess(process, "ntdll", "LdrLoadDll");
            int ptrSize = amd64Bytecode ? 8 : 4;
            int align(int ptr)
            {
                return (ptr + ptrSize - 1) & ~(ptrSize - 1);
            }

			if (injectWOW64)
            {
				var module = PInvoke.GetModuleHandle("ntdll");
				LdrLoadDll = PInvoke.GetProcAddress(module, "LdrLoadDll");
			}

			// shell code arguments structure
			// 
			// handle: Handle
			// dllName: UNICODE_STRING
            // dllNameContents: wchar_t[lenght]

			int returnHandleOffset = 0;
            int dllNameOffset = align(returnHandleOffset + ptrSize);
            int dllNamePtrOffset = align(dllNameOffset + 4);

			int dllNameContents = dllNamePtrOffset + ptrSize;
            Debug.Assert(dllNameContents >= 0);
            Debug.Assert(dllNamePtrOffset >= 0);
            Debug.Assert(dllNamePtrOffset > dllNameContents);

            int paramtersLengthTotal = dllNameContents + Encoding.Unicode.GetByteCount(dllName) + 2;

            // build paramaters structure

            byte[] shell_code_arguments = new byte[paramtersLengthTotal];

            // 1: dll name contents
            {
                byte[] dllNameDecoded = Encoding.Unicode.GetBytes(dllName);
                dllNameDecoded.CopyTo(shell_code_arguments.AsSpan(dllNameContents));
            }
			// 2: dll name UNICODE_STRING
			{
                // length stuff, ptr is not set
				ushort dllNameLength = (ushort)Encoding.Unicode.GetByteCount(dllName);
                ushort maxDllNameLength = (ushort)(dllNameLength + 2);

				byte[] dllNameLengthBytes = BitConverter.GetBytes(dllNameLength);
				byte[] maxDllNameLengthBytes = BitConverter.GetBytes(maxDllNameLength);

                dllNameLengthBytes.CopyTo(shell_code_arguments, dllNameOffset);
				maxDllNameLengthBytes.CopyTo(shell_code_arguments, dllNameOffset + 2);
			}

			// copy parameters structure to target memory
			UIntPtr shell_code_arguments_addr;
			unsafe
			{
				void* shell_code_arguments_ptr = AllocateAndWriteIntoProcess((HANDLE)process.Handle, shell_code_arguments, isExecutable: false);

                if (shell_code_arguments_ptr is null)
                {
                    Trace.WriteLine($"Failed to allocate and write arguments structure for early DLL injector - {Marshal.GetLastWin32Error()}");
                    return HANDLE.Null;
				}

				shell_code_arguments_addr = new UIntPtr(shell_code_arguments_ptr);
                UIntPtr dll_name_process_addr = shell_code_arguments_addr + (nuint)dllNameContents;
                byte[] dll_name_process_addr_bytes = isWOW64 ? BitConverter.GetBytes(dll_name_process_addr.ToUInt32()) : BitConverter.GetBytes(dll_name_process_addr.ToUInt64());


                if (!WriteProcessMemory((HANDLE)process.Handle, (shell_code_arguments_addr + (nuint)dllNamePtrOffset).ToPointer(), dll_name_process_addr_bytes, (nuint)dll_name_process_addr_bytes.Length))
                {
					PInvoke.VirtualFreeEx((HANDLE)process.Handle, shell_code_arguments_ptr, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE);
					Trace.WriteLine($"Failed to write string pointer into remote process - {Marshal.GetLastWin32Error()}!");
					return HANDLE.Null;
				}
			}

			// done building and writing paramters

			// build shell code
			List<byte> shellCode = new();
			UIntPtr GetOffsetInStructure(nuint structureOffset)
			{
				UIntPtr address;

				unsafe
				{
					address = shell_code_arguments_addr + structureOffset;
				}

				return address;
			}
			// write a push index into the arguments structure
			void WritePushSturcture(int structureOffset)
            {
				Debug.Assert(structureOffset >= 0);

				UIntPtr address = GetOffsetInStructure((nuint)structureOffset);

				if (!amd64Bytecode)
                {
                    shellCode.Add(0x68);
					shellCode.AddRange(BitConverter.GetBytes((uint)address));
				}
                else
                {
					// mov rax, address
					shellCode.AddRange(new byte[] {0x48, 0xb8});
					shellCode.AddRange(BitConverter.GetBytes(address.ToUInt64()));
					// push rax
					shellCode.Add(0x50);

				}
            }
            // write a u8 value
            void WritePushU8(byte u8)
            {
				shellCode.Add(0x6a);
				shellCode.Add(u8);
			}

			// write a call to a FARPROC
			void WriteCall(FARPROC callTarget)
			{
				if (!amd64Bytecode)
				{
                    // mov eax, imm32
					shellCode.Add(0xb8);
					shellCode.AddRange(BitConverter.GetBytes(callTarget.Value.ToInt32()));
				}
				else
				{
					// mov rax, imm64
					shellCode.AddRange(new byte[] { 0x48, 0xb8 });
					shellCode.AddRange(BitConverter.GetBytes(callTarget.Value.ToInt64()));

				}
				// call rax/eax
				shellCode.AddRange(new byte[] { 0xff, 0xd0 });
			}

			void WriteReturn(ushort u16)
			{
                if (u16 == 0)
                {
					shellCode.Add(0xc3);
				}
                else
                {
					shellCode.Add(0xc2);
					shellCode.AddRange(BitConverter.GetBytes(u16));
				}
			}

            void SwitchTo64Bit()
            {
                Debug.Assert(amd64Bytecode);

                // push 0x33 (cs_64)
				shellCode.AddRange(new byte[] { 0x6a, 0x33 });
				// call $+5
				shellCode.AddRange(new byte[] { 0xe8, 0x00, 0x00, 0x00, 0x00 });
				// add dword [esp], 5
				shellCode.AddRange(new byte[] { 0x83, 0x04, 0x24, 0x05 });
                shellCode.Add(0xcb); // retf
			}

            void SwitchTo32Bit()
            {
				Debug.Assert(amd64Bytecode);

				// call $+5
				shellCode.AddRange(new byte[] { 0xe8, 0x00, 0x00, 0x00, 0x00 }); 
                // mov dword [rsp + 4], 0x23 (cs_32)
				shellCode.AddRange(new byte[] { 0xc7, 0x44, 0x24, 0x04, 0x23, 0x00, 0x00, 0x00 });
				// add dword [esp], D
				shellCode.AddRange(new byte[] { 0x83, 0x04, 0x24, 0x0d });
				shellCode.Add(0xcb); // retf
			}

            void Amd64SetArgument(int index, UInt64 argument)
            {
                switch (index)
                {
                    case 1:
                    case 2:
						shellCode.AddRange(new byte[] { 0x48, (byte)(0xb9 + index - 1) });
                        break;
                    case 3:
                    case 4:
						shellCode.AddRange(new byte[] { 0x49, (byte)(0xb8 + index - 3) });
                        break;
                    default:
                        throw new InvalidOperationException();
				}

				shellCode.AddRange(BitConverter.GetBytes(argument));
			}

			void Amd64SetArgumentOffset(int index, int structureOffset)
            {
                Debug.Assert(structureOffset >= 0);
				UIntPtr address = GetOffsetInStructure((nuint)structureOffset);
				Amd64SetArgument(index, address.ToUInt64());
			}

			// shell code (x86)
			//
			//1: push &returnHandleOffset
			//2: push dllName: UNICODE_STRING*
			//3: push 0: flags
			//4: push 1: ldrFlags
			//5: call LdrLoadDll
			//6: ret 4; pop the unused argument

			// shell code (am65)
			//
			//1: mov arg4, &returnHandleOffset
			//2: mov arg3, UNICODE_STRING*
			//3: mov arg2, flags
			//4: mov arg1, ldrFlags
			//5: call LdrLoadDll
			//6: ret 0


			if (injectWOW64)
			{
				SwitchTo64Bit();
			}

			if (amd64Bytecode)
			{
				Amd64SetArgumentOffset(4, returnHandleOffset);
				Amd64SetArgumentOffset(3, dllNameOffset);
				Amd64SetArgument(2, 0);
				Amd64SetArgument(1, 1);
			}
			else
			{
				WritePushSturcture(returnHandleOffset);
				WritePushSturcture(dllNameOffset);
				WritePushU8(0);
				WritePushU8(1);
			}

			WriteCall(LdrLoadDll);

			if (injectWOW64)
			{
				SwitchTo32Bit();
			}

			WriteReturn((ushort)(amd64Bytecode ? 0 : 4));

			// done building shell code

			HANDLE injection_thread;
			unsafe
			{
				void* shell_code_ptr = AllocateAndWriteIntoProcess((HANDLE)process.Handle, shellCode.ToArray(), isExecutable: true);

                if (shell_code_ptr is null)
                {
					Trace.WriteLine($"Failed to allocate shell code - {Marshal.GetLastWin32Error()}");
                    return HANDLE.Null;
				}

				IntPtr shell_code_intptr = new(shell_code_ptr);

				Trace.WriteLine("Injecting DLL using shell code in remote thread!");
                // no argument is passed to the shell code, since the offsets used are already part of the generated shell code
				injection_thread = CreateRemoteThread((HANDLE)process.Handle, null, 0, (FARPROC)shell_code_intptr, null, 0, null);
				if (injection_thread == IntPtr.Zero)
				{
					Trace.WriteLine($"Failed create injection thread for remote process - {Marshal.GetLastWin32Error()}!");
					return HANDLE.Null;
				}

                Trace.WriteLine("Thread injection done! Waiting for thread to terminate!");
			}

			WAIT_EVENT wait_result = await Task.Run(() => PInvoke.WaitForSingleObject(injection_thread, timeout));

			if (wait_result == WAIT_EVENT.WAIT_TIMEOUT || wait_result == WAIT_EVENT.WAIT_FAILED)
			{
				Trace.WriteLine($"Waiting on injection thread failed: {wait_result} - {Marshal.GetLastWin32Error()}!");
				return HANDLE.Null;
			}

			uint exit_code;
			unsafe
			{
				if (!PInvoke.GetExitCodeThread(injection_thread, &exit_code))
				{
					Trace.WriteLine($"Failed to get thread exit code  {Marshal.GetLastWin32Error()}");
					return HANDLE.Null;
				}
			}
			Trace.WriteLine($"Thread exit code {exit_code:X}");

            byte[] HandleBytes = new byte[ptrSize];
            bool readSuccess;
            unsafe {
                fixed (byte* HandleBytesPtr = HandleBytes)
					readSuccess = PInvoke.ReadProcessMemory((HANDLE)process.Handle, (shell_code_arguments_addr + (nuint)returnHandleOffset).ToPointer(), HandleBytesPtr, (nuint)HandleBytes.Length);
            }

            if (!readSuccess)
            {
				Trace.WriteLine($"Failed to get module handle?? - {Marshal.GetLastWin32Error()}!");
				return HANDLE.Null;
            }

            IntPtr modulePtr = new (isWOW64 ? BitConverter.ToInt32(HandleBytes) : BitConverter.ToInt64(HandleBytes));

            if (modulePtr == IntPtr.Zero)
            {
				Trace.WriteLine($"Failed to load library!");
                return HANDLE.Null;
			}

            return new HANDLE(modulePtr);
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

			EventWaitHandle injectionSignal = _events[id];
			_processes[id] = process;

            // suspend the process in prep for DLL injection
            if (!ShouldSuspendOnLaunch)
            {
                NtSuspendProcess(processHandle);
                Trace.WriteLine($"Target process suspended, ready for DLL injection sequence (last error = {Marshal.GetLastWin32Error()})");
            }
            try
            {
                if (!ShouldSuspendOnLaunch)
                {

                    FARPROC load_library_proc = await GetLibraryProcAddressForProcess(process, "kernelbase.dll", "LoadLibraryA", "kernel32.dll");

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
                }
                else
                {
                    Trace.WriteLine("Using early injection mode!");
                    HANDLE moduleHandle = await EarlyProcessInject(process, dll_path, timeout);

                    if (moduleHandle == HANDLE.Null)
                    {
                        Trace.WriteLine("Early injection failed!");
                    }
                    else
                    {
                        Trace.WriteLine($"DLL injected, module: {moduleHandle}!");
                    }
                }
            } finally
            {
                NtResumeProcess(processHandle);
				Trace.WriteLine("Process resumed!");
			}

#if DEBUG
            bool has_started_up_successfully = await Task.Run(() => injectionSignal.WaitOne());
#else
            bool has_started_up_successfully = await Task.Run(() => injectionSignal.WaitOne(TimeSpan.FromMilliseconds(timeout)));
#endif
            Trace.WriteLine($"DLL injection final result: {has_started_up_successfully}");

			return has_started_up_successfully;
        }
    }
}
