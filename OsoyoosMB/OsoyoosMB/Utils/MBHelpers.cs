using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsoyoosMB.Utils
{
    internal class MBHelpers
    {
        public static string GetBitmapRelativePath(string base_path, string full_path)
        {
            return Path.ChangeExtension(PathNetCore.GetRelativePath(base_path, full_path), null);
        }
    }
}
