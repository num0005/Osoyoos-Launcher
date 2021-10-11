using System.Threading.Tasks;

namespace ToolkitLauncher.ToolkitInterface
{
    interface IToolkitFBX2ASS : IToolkitFBX2Jointed
    {

        /// <summary>
        /// Create an ASS from an FBX file
        /// </summary>
        /// <param name="fbxPath">Path to the source FBX file</param>
        /// <param name="assPath">Path to save the ASS file to</param>
        /// <returns></returns>
        public Task ASSFromFBX(string fbxPath, string assPath);
    }
}
