using System;
using System.Collections.Generic;
using System.Text;

namespace ManagedBlamHelper
{
    internal class Utility
    {
        public static IEnumerable<int> FindPattern(byte[] data, byte[] pattern)
        {
            int searchLimit = data.Length - pattern.Length + 1;
            int patternLength = pattern.Length;
            for (int i = 0; i < searchLimit; i++)
            {
                for (int j = 0; j < patternLength; j++)
                {
                    if (data[i + j] != pattern[j])
                        break;

                    if (j == patternLength - 1)
                    {
                        yield return i;
                        i += j;
                        break;
                    }
                }
            }
        }
        public static IEnumerable<ArraySegment<byte>> FindStringsWithPrefixInBinary(byte[] data, string prefix)
        {
            byte[] pattern = Encoding.UTF8.GetBytes(prefix);
            foreach (int matchIndex in FindPattern(data, pattern))
            {
                // only search the next few hundred bytes for the string termination
                int stringTerminationLimit = Math.Min(matchIndex + 0x100, data.Length);
                for (int i = matchIndex; i < stringTerminationLimit; i++)
                {
                    // check if this is the terminator
                    if (data[i] == 0)
                    {
                        yield return new ArraySegment<byte>(data, matchIndex, i - matchIndex + 1);
                        break;
                    }
                }
            }
        }
    }
}
