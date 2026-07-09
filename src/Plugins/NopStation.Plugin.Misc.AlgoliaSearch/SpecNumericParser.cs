using System.Globalization;
using System.Text.RegularExpressions;

namespace NopStation.Plugin.Misc.AlgoliaSearch
{
    public static class SpecNumericParser
    {
        private static readonly Regex _numRegex =
            new Regex(@"[-+]?\d+(?:[.,]\d+)?", RegexOptions.Compiled);

        public static decimal? ParseFirstDecimal(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var m = _numRegex.Match(raw);
            if (!m.Success)
                return null;

            var token = m.Value.Replace(',', '.');

            if (decimal.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return d;

            return null;
        }
    }
}
