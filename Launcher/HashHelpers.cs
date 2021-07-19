using Palit.TLSHSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ToolkitLauncher
{
    static class HashHelpers
    {
        private static IEnumerable<string> GetExecutableNames(string directory)
        {
            return Directory.GetFiles(directory).Where(fileName => Path.GetExtension(fileName) == ".exe");
        }

        /// <summary>
        /// Calculate TLS hashes for all executables in the directory
        /// </summary>
        /// <param name="directory">Directory containing the executables</param>
        /// <returns>A list containing a tuple containing the file name and hash</returns>
        public static List<(string name, TlshHash hash)> GetExecutableTLSHashes(string directory)
        {
            List<(string, TlshHash)> result = new();
            foreach (string fileName in GetExecutableNames(directory))
            {
                TlshBuilder tlshBuilder = new();
                var buffer = new byte[1024 * 4];
                using (FileStream stream = File.OpenRead(fileName))
                {
                    long length = stream.Length;
                    while (stream.Position != length)
                        tlshBuilder.Update(buffer, 0, stream.Read(buffer, 0, buffer.Length));
                }

                TlshHash hash = tlshBuilder.GetHash(true);

                result.Add((fileName, hash));
            }

            return result;
        }

        /// <summary>
        /// Calculate MD5 hashes for all executables in the directory
        /// </summary>
        /// <param name="directory">Directory containing the executables</param>
        /// <returns>A list containing a tuple containing the file name and hash</returns>
        public static List<(string name, string hash)> GetExecutableMD5Hashes(string directory)
        {
            List<(string, string)> result = new();
            foreach (string fileName in GetExecutableNames(directory))
            {
                using var md5 = System.Security.Cryptography.MD5.Create();
                using var stream = File.OpenRead(fileName);
                string hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");

                result.Add((fileName, hash));
            }

            return result;
        }
    }
}
