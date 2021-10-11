using System.IO;
using System.Threading.Tasks;

namespace ToolkitLauncher.Utility
{
    static class StreamExtensions
    {
        /// <summary>
        /// Reads all remaining bytes from a memory stream
        /// </summary>
        /// <param name="stream">The memory stream to operate on</param>
        /// <returns>A byte array</returns>
        public static byte[] ReadBytesToEnd(this Stream stream)
        {
            using (MemoryStream ms = new())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Reads all remaining bytes from a memory stream asynchronously
        /// </summary>
        /// <param name="stream">The memory stream to operate on</param>
        /// <returns>A byte array</returns>
        public async static Task<byte[]> ReadBytesToEndAsync(this Stream stream)
        {
            using (MemoryStream ms = new())
            {
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
        }
    }
}
