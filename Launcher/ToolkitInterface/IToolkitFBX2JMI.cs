using System.Threading.Tasks;

namespace ToolkitLauncher.ToolkitInterface
{
    interface IToolkitFBX2JMI : IToolkitFBX2Jointed
    {

        /// <summary>
        /// Create an JMI from an FBX file
        /// </summary>
        /// <param name="fbxPath">Path to the source FBX file</param>
        /// <param name="jmiPath">Path to save the JMI file to</param>
        /// <returns></returns>
        public Task JMIFromFBX(string fbxPath, string jmiPath);
    }
}
