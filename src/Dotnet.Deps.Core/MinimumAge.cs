using System;
using System.Globalization;

namespace Dotnet.Deps.Core
{
    /// <summary>
    /// Parses and formats the minimum age a package version must have before it is considered for an update.
    /// </summary>
    public static class MinimumAge
    {
        /// <summary>
        /// Tries to parse a minimum age such as "2d" (two days), "12h" (twelve hours) or "2" (two days).
        /// </summary>
        public static bool TryParse(string value, out TimeSpan minimumAge)
        {
            minimumAge = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var trimmedValue = value.Trim();
            var unit = 'd';
            var lastCharacter = trimmedValue[trimmedValue.Length - 1];
            if (!char.IsDigit(lastCharacter))
            {
                unit = char.ToLowerInvariant(lastCharacter);
                trimmedValue = trimmedValue.Substring(0, trimmedValue.Length - 1).Trim();
            }

            if (!double.TryParse(trimmedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) || number < 0)
            {
                return false;
            }

            if (unit == 'd' && number <= TimeSpan.MaxValue.TotalDays)
            {
                minimumAge = TimeSpan.FromDays(number);
                return true;
            }

            if (unit == 'h' && number <= TimeSpan.MaxValue.TotalHours)
            {
                minimumAge = TimeSpan.FromHours(number);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Formats a minimum age for display purposes.
        /// </summary>
        public static string Format(TimeSpan minimumAge)
        {
            if (minimumAge.TotalDays >= 1)
            {
                return $"{minimumAge.TotalDays.ToString("0.##", CultureInfo.InvariantCulture)} {(minimumAge.TotalDays == 1 ? "day" : "days")}";
            }

            return $"{minimumAge.TotalHours.ToString("0.##", CultureInfo.InvariantCulture)} {(minimumAge.TotalHours == 1 ? "hour" : "hours")}";
        }
    }
}
