namespace TimeMatcher.UserOptions;

public static class DefaultSearch
{
    public static void Run(IReadOnlyList<TimezoneTarget> timezoneList)
    {
        if (timezoneList.Count == 0)
        {
            Console.WriteLine("No timezones were entered.");
            return;
        }

        DateTime searchDate = DateTime.UtcNow.Date;

        Console.WriteLine("Optimal times:");
        for (int hour = 7; hour <= 21; hour++)
        {
            DateTime candidateUtc = new(searchDate.Year, searchDate.Month, searchDate.Day, hour, 0, 0, DateTimeKind.Utc);
            List<DateTime> localTimes = [];
            bool hasEarlyTime = false;

            foreach (TimezoneTarget timezone in timezoneList)
            {
                DateTime localTime = TimezoneUtilities.ConvertFromUtc(candidateUtc, timezone);
                localTimes.Add(localTime);

                if (localTime.Hour < 7)
                {
                    hasEarlyTime = true;
                    break;
                }
            }

            if (!hasEarlyTime)
            {
                for (int index = 0; index < localTimes.Count; index++)
                {
                    Console.WriteLine($"{timezoneList[index].Label}: {localTimes[index]:MM/dd/yyyy hh:mm tt}");
                }

                return;
            }
        }

        Console.WriteLine("No optimal time was found between 07:00 and 21:00 that keeps every timezone out of 00:00-06:59.");
    }
}
