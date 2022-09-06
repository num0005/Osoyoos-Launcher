using System.Threading.Tasks;

namespace ToolkitLauncher.ToolkitInterface
{
    interface IToolkitFBX2GR2
    {
        /// <summary>
        /// Create a GR2 from an FBX file
        /// </summary>
        /// <param name="fbxPath"></param>
        /// <param name="jsonPath"></param>
        /// <param name="gr2Path"></param>
        /// <returns>A task that will end when the conversion is either completed or has failed</returns>
        public Task GR2FromFBX(string fbxPath, string jsonPath, string? gr2Path);
    }
}
