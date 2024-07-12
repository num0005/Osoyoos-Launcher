using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ToolkitLauncher.Utility
{
	public interface IProcessInjector
	{
		/// <summary>
		/// Modify process startup options relating to enviroment.
		/// </summary>
		/// <param name="startInfo"></param>
		/// <returns>ID of the process, may not be unique if the injector does not need to distinguish between processes</returns>
		public Guid SetupEnviroment(ProcessStartInfo startInfo);

		/// <summary>
		/// Inject our changes into a process.
		/// </summary>
		/// <param name="id">UUID returned by SetupEnviroment</param>
		/// <param name="process">Process object</param>
		/// <returns>Was the process sucessfully modified?</returns>
		public Task<bool> Inject(Guid id, System.Diagnostics.Process process);

		public virtual bool ShouldSuspendOnLaunch => false;
	}
}
