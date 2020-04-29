using System;
using System.Collections.Generic;
using System.Text;

namespace Grindarr.Core.Utilities
{
    public static class FileSizeUtilities
    {
        private readonly static Dictionary<char, ulong> suffixToMultiplierMap = new Dictionary<char, ulong>
        {
            ['K'] = 1024L,
            ['M'] = 1024L * 1024L,
            ['G'] = 1024L * 1024L * 1024L,
            ['T'] = 1024L * 1024L * 1024L * 1024L
        };

        public static ulong ParseFromSuffixedString(string str)
        {
            StringBuilder numbers = new StringBuilder();
            char? sizeChar = null;
            foreach (char c in str.Trim())
            {
                if (char.IsDigit(c) || c == '.')
                    numbers.Append(c);
                else if (char.IsWhiteSpace(c))
                    continue;
                else
                {
                    sizeChar = char.ToUpper(c);
                    break;
                }
            }

            if (!sizeChar.HasValue || !suffixToMultiplierMap.ContainsKey(sizeChar.Value) || numbers.Length == 0)
                return 0;

            ulong multiplier = suffixToMultiplierMap[sizeChar.Value];

            double value = double.Parse(numbers.ToString());
            return (ulong)Math.Floor(value * multiplier);
        }
    }
}
