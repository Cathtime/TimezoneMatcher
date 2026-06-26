using System.Collections.Generic;
using TimeMatcher.UserOptions;

partial class Program
{
    static readonly List<string> timezones = [];

    static void Main(string[] args)
    {
        // do while loop bc I feel it's better to always check
        // after first input
        do
        {
            Console.Write("Enter a timezone (for example CST, EST, UTC, UTC+2, GMT-3): ");
            string abbreviation = Console.ReadLine() ?? string.Empty;

            if (abbreviation == "")
            {
                break;
            }

            timezones.Add(abbreviation);
        } while (true);

        List<TimezoneTarget> timezoneTargets = [];

        // Filter timezones: check for spelling errors, invalid timezones etc etc
        foreach (string abbreviation in timezones)
        {
            if (TimezoneUtilities.TryResolveTimezone(abbreviation, out TimezoneTarget timezone))
            {
                timezoneTargets.Add(timezone);
            }
            else
            {
                Console.WriteLine($"Unknown timezone abbreviation: {abbreviation}");
            }
        }

        DefaultSearch.Run(timezoneTargets); // time for THE MEATS of the code
    }
}