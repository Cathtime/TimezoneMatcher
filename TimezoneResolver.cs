using System.Globalization;
using System.Text.RegularExpressions;

static partial class TimezoneUtilities
{
    private static readonly Dictionary<string, string> TimezoneIds =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["UTC"] = "UTC",
            ["GMT"] = "UTC",
            ["Z"] = "UTC",
            ["BST"] = "GMT Standard Time",
            ["CST"] = "Central Standard Time",
            ["CDT"] = "Central Standard Time",
            ["EST"] = "Eastern Standard Time",
            ["EDT"] = "Eastern Standard Time",
            ["MST"] = "Mountain Standard Time",
            ["MDT"] = "Mountain Standard Time",
            ["PST"] = "Pacific Standard Time",
            ["PDT"] = "Pacific Standard Time",
            ["AKST"] = "Alaskan Standard Time",
            ["AKDT"] = "Alaskan Standard Time",
            ["HST"] = "Hawaiian Standard Time"
        };

    // converts a UTC DateTime to the local time in the specified timezone
    public static DateTime ConvertFromUtc(DateTime utcDateTime, TimezoneTarget timezone)
    {
        // if the timezone has a TimeZoneInfo, use that to convert the time
        if (timezone.TimeZoneInfo != null)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timezone.TimeZoneInfo);
        }

        // if the timezone has an offset, use that to convert the time
        if (timezone.Offset != null)
        {
            return utcDateTime + timezone.Offset.Value;
        }

        // if the timezone has neither a TimeZoneInfo nor an offset, return the UTC time
        return utcDateTime;
    }

    // attempts to resolve a timezone abbreviation to a TimezoneTarget object
    // returns true if the abbreviation was successfully resolved, false otherwise
    public static bool TryResolveTimezone(string abbreviation, out TimezoneTarget timezone)
    {
        // trim whitespace
        string normalized = abbreviation.Trim();

        
        if (TryParseUtcOffset(normalized, out TimeSpan offset))
        {
            // if the abbreviation is a valid UTC offset, 
            // create a TimezoneTarget with the offset
            timezone = new TimezoneTarget(normalized.ToUpperInvariant(), null, offset);
            return true;
        }

        if (TryGetTimeZoneInfo(normalized, out TimeZoneInfo timeZoneInfo))
        {
            // if the abbreviation is a valid timezone ID,
            // create a TimezoneTarget with the TimeZoneInfo
            timezone = new TimezoneTarget(normalized.ToUpperInvariant(), timeZoneInfo, null);
            return true;
        }

        // if the abbreviation is neither a valid UTC offset nor a valid timezone ID,
        // return false and set the out parameter to default
        timezone = default;
        return false;
    }

    // this method attempts to get a TimeZoneInfo object from a timezone abbreviation.
    // returns true if the abbreviation was successfully resolved, false otherwise.
    private static bool TryGetTimeZoneInfo(string abbreviation, out TimeZoneInfo timezone)
    {
        // if the abbreviation is not in the dictionary, return false
        if (!TimezoneIds.TryGetValue(abbreviation, out string? timezoneId))
        {
            timezone = default!;
            return false;
        }

        // attempt to find the TimeZoneInfo by ID, 
        // and handle exceptions for invalid or unavailable timezones
        try
        {
            timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return true;
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException) 
        {
            // Invalid or unavailable timezone IDs are treated as lookup failures.
            Console.WriteLine(ex.GetType().Name + ": triggered ");
            timezone = default!;
            return false;
        }
    }

    // "UTC+2" or "GMT-3:30" get ripped apart to useable offsets
    // returns true if the input string is a valid UTC offset, false otherwise.
    // the parsed offset is returned through the 'offset' out parameter.
    private static bool TryParseUtcOffset(string input, out TimeSpan offset)
    {
        offset = default;

        
        Match match = MyRegex().Match(input);
        if (!match.Success)
        {
            return false;
        }

        // parse the hours and minutes from the matched groups of the regex
        int hours = int.Parse(match.Groups["hours"].Value, CultureInfo.InvariantCulture);
        int minutes = match.Groups["minutes"].Success
            ? int.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture)
            : 0;

        // utc only goes from -12 to +14, and minutes can only be 0-59, so we check for that here
        // there can be malformed inputs like "UTC+2:60" and we don't want that (it doesn't exist for obvious reasons)
        if (hours > 14|| hours < -12 || minutes > 59)
        {
            return false;
        }

        // create a TimeSpan from the parsed hours and minutes, and negate it if the sign is negative
        TimeSpan parsedOffset = new(hours, minutes, 0);
        if (match.Groups["sign"].Value == "-")
        {
            parsedOffset = parsedOffset.Negate();
        }

        offset = parsedOffset; // set the out parameter to the parsed offset
        return true;
    }

    // This regex matches timezone offsets in the format of "UTC+2", "GMT-3:30", etc.
    // It captures the sign, hours, and optional minutes for further processing.
    [GeneratedRegex("^(?:UTC|GMT)?(?<sign>[+-])(?<hours>\\d{1,2})(?::?(?<minutes>\\d{2}))?$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex MyRegex();
}