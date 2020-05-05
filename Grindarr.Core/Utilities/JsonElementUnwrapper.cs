using System;
using System.Linq;
using System.Text.Json;

namespace Grindarr.Core.Utilities
{
    /// <summary>
    /// Provides useful helpers to get the underlying data from a <code>JsonElement</code>
    /// </summary>
    public static class JsonElementUnwrapper
    {
        /// <summary>
        /// Takes in a JsonElement and returns the underlying data. 
        /// So far I cannot find out where this is in .NET Core OR why .NET Core doesn't have this
        /// </summary>
        /// <param name="srcData">JsonElement to unwrap</param>
        /// <returns>The underlying data</returns>
        public static object Unwrap(JsonElement srcData) 
            => srcData.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.Number => srcData.GetDouble(),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.Undefined => null,
                JsonValueKind.String => srcData.GetString(),
                JsonValueKind.Object => srcData,
                JsonValueKind.Array => srcData.EnumerateArray().Select(o => Unwrap(o)).ToArray(),
                _ => throw new NotImplementedException($"Unable to unwrap {srcData.ValueKind} - not implemented"),
            };
    }
}