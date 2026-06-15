using System.Text.RegularExpressions;

namespace DAL.Helper
{
    public static class StringNormalizationHelper
    {
        /// <summary>
        /// Normalizes a string by converting it to lowercase and removing all non-alphanumeric characters.
        /// Useful for fuzzy matching names like "Thiên--Tôn" and "Thiên Tôn".
        /// </summary>
        public static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // 1. To Lower
            var result = input.ToLowerInvariant();

            // 2. Remove all non-alphanumeric (except basic spaces)
            // Note: This also handles Vietnamese characters as \w includes them in .NET if Unicode is handled.
            // But let's be safer and replace common AI noise like -- or multiple spaces.
            result = Regex.Replace(result, @"[^\w]", ""); 

            return result;
        }

        /// <summary>
        /// Returns a pattern for ILIKE search in PostgreSQL.
        /// </summary>
        public static string ToSearchPattern(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "%";
            return $"%{input.Replace(" ", "%")}%";
        }
    }
}
