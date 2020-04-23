using System;
using System.Collections.Generic;
using System.Text;

namespace Grindarr.Core.Utilities
{
    public static class FileSizeUtilities
    {
        private readonly static Dictionary<char, long> suffixToMultiplierMap = new Dictionary<char, long>
        {
            ['K'] = 1024L,
            ['M'] = 1024L * 1024L,
            ['G'] = 1024L * 1024L * 1024L,
            ['T'] = 1024L * 1024L * 1024L * 1024L
        };

        public static long ParseFromSuffixedString(string str)
        {
            StringBuilder numbers = new StringBuilder();
            char? sizeChar = null;
            foreach (char c in str)
            {
                if (char.IsDigit(c))
                    numbers.Append(c);
                else
                {
                    sizeChar = char.ToUpper(c);
                    break;
                }
            }

            if (!sizeChar.HasValue || !suffixToMultiplierMap.ContainsKey(sizeChar.Value) || numbers.Length == 0)
                return 0;

            long multiplier = suffixToMultiplierMap[sizeChar.Value];

            long value = long.Parse(numbers.ToString());
            return value * multiplier;
        }
    }
}
