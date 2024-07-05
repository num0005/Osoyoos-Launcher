using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace ToolkitLauncher.Utility
{
	internal class H2ToolLightmapFixInjector : IProcessInjector
	{
		private static readonly Guid _uuid = Guid.Parse("1B6E9E6C-DBA1-47DA-A7B4-7B940808FCB2");

		public struct NopFill
		{
			public NopFill(uint offset, uint length)
			{
				Offset = offset;
				Length = length;
			}

			public uint Offset;
			public uint Length;
		}

		private readonly uint _baseAddress;
		private readonly IEnumerable<NopFill> _nopfills;

		public H2ToolLightmapFixInjector(uint baseAddress, IEnumerable<NopFill> nopFills)
		{
			_baseAddress = baseAddress;
			_nopfills = nopFills;
		}

		public Guid SetupEnviroment(ProcessStartInfo startInfo)
		{
			return _uuid;
		}

		[SupportedOSPlatform("windows5.1.2600")]
		[DllImport("ntdll.dll", PreserveSig = false)]
		public static extern void NtSuspendProcess(IntPtr processHandle);

		[SupportedOSPlatform("windows5.1.2600")]
		[DllImport("ntdll.dll", PreserveSig = false)]
		public static extern void NtResumeProcess(IntPtr processHandle);

		[SupportedOSPlatform("windows5.1.2600")]
		public Task<bool> Inject(Guid id, System.Diagnostics.Process process)
		{
			Debug.Assert(id == _uuid);

			// use try-finally to ensure the process is always resumed no matter whatever the patching was sucessful or not
			try
			{
				NtSuspendProcess(process.Handle);
				Trace.WriteLine($"[H2 LM Patcher] Target process suspended, ready for patching (last error = {Marshal.GetLastWin32Error()})");

				ProcessModule mainModule = process.MainModule;
				Trace.Assert(mainModule is not null);

				Trace.WriteLine($"[H2 LM Patcher] Patch target: {mainModule.ModuleName} base offset: {mainModule.BaseAddress:X}");

				foreach (NopFill fill in _nopfills)
				{
					IntPtr translatedAddress = IntPtr.Add(mainModule.BaseAddress, (int)(fill.Offset - _baseAddress));
					Trace.WriteLine($"Nopfilling {fill.Length} bytes at {fill.Offset:X} (translated address => {translatedAddress:X})");

					byte[] nopValues = new byte[fill.Length];
					Array.Fill<byte>(nopValues, 0x90);

					bool success;

					unsafe
					{
						fixed (byte* nopVals = nopValues)
							success = PInvoke.WriteProcessMemory((HANDLE)process.Handle, translatedAddress.ToPointer(), nopVals, (nuint)nopValues.Length, null);
					}

					if (!success)
					{
						Trace.WriteLine("Failed to write patch to memory!");
						return Task.FromResult(false);
					}
				}

				Trace.WriteLine($"[H2 LM Patcher] Done patching");

				return Task.FromResult(true);
			} catch(Exception ex)
			{
				Trace.WriteLine($"[H2 LM Patcher] Unexpected expection, bailing out: {ex}");

				return Task.FromResult(false);
			} finally
			{
				NtResumeProcess(process.Handle);
				Trace.WriteLine($"[H2 LM Patcher] Process resumed, all done (last error = {Marshal.GetLastWin32Error()})");
			}

		}
	}
}
