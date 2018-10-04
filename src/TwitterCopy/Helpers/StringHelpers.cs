using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TwitterCopy.Helpers
{
    public static class StringHelpers
    {
        public static string GenerateSlug(this string phrase)
        {
            string str = phrase.RemoveDiacritics();

            // Invalid chars
            str = Regex.Replace(str, @"[^A-Za-z0-9\s-]", "");
            // Remove spaces
            str = Regex.Replace(str, @"\s+", "").Trim();

            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();

            return str;
        }

        public static string RemoveDiacritics(this string text)
        {
            var s = new string(text.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            return s.Normalize(NormalizationForm.FormC);
        }
    }
}
