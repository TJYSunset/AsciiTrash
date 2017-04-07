using System;
using System.Text.RegularExpressions;

namespace AsciiTrash.Utils
{
    internal static class StringHelper
    {
        private static readonly Regex Whitespace = new Regex(@"\s", RegexOptions.Compiled);

        public static bool FuzzyEquals(this string left, string right)
            =>
                string.Equals(Whitespace.Replace(left, string.Empty), Whitespace.Replace(right, string.Empty),
                    StringComparison.CurrentCultureIgnoreCase);
    }
}