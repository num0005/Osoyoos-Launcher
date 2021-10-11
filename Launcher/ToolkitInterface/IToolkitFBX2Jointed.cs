using System.Threading.Tasks;

namespace ToolkitLauncher.ToolkitInterface
{
    interface IToolkitFBX2Jointed
    {
        /// <summary>
        /// Create a JMA from an FBX file
        /// </summary>
        /// <param name="fbxPath">Path to the FBX file</param>
        /// <param name="jmaPath">Path to save the JMA at</param>
        /// <param name="startIndex">First keyframe index to include</param>
        /// <param name="startIndex">Last keyframe index to include</param>
        /// <returns>A task that will end when the conversion is either completed or has failed</returns>
        public Task JMAFromFBX(string fbxPath, string jmaPath, int startIndex = 0, int? endIndex = null);

        /// <summary>
        /// Create an JMS from an FBX file
        /// </summary>
        /// <param name="fbxPath"></param>
        /// <param name="jmsPath"></param>
        /// <param name="geoClass"></param>
        /// <returns>A task that will end when the conversion is either completed or has failed</returns>
        public Task JMSFromFBX(string fbxPath, string jmsPath, string? geoClass);
    }
}
