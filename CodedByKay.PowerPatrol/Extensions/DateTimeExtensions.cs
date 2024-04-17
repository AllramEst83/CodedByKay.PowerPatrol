using System.Runtime.InteropServices;

namespace CodedByKay.PowerPatrol.Extensions
{
    internal static class DateTimeExtensions
    {
        public static DateTime ConvertToTimeZone(this DateTime dateTime, string? timeZoneId = null)
        {
            // Default to Swedish time zone if none specified
            if (string.IsNullOrEmpty(timeZoneId))
            {
                timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                             ? "W. Europe Standard Time"
                             : "Europe/Stockholm";
            }

            TimeZoneInfo targetTimeZone;
            try
            {
                targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine($"The time zone '{timeZoneId}' could not be found on this system.");
                return dateTime; // Return original if TZ not found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                return dateTime; // Return original on other errors
            }

            // Convert dateTime based on its DateTimeKind
            return ConvertDateTime(dateTime, targetTimeZone);
        }

        private static DateTime ConvertDateTime(DateTime dateTime, TimeZoneInfo targetTimeZone)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Unspecified:
                case DateTimeKind.Utc:
                    return TimeZoneInfo.ConvertTimeFromUtc(dateTime, targetTimeZone);
                case DateTimeKind.Local:
                    if (TimeZoneInfo.Local.Id == targetTimeZone.Id)
                    {
                        return dateTime; // No conversion needed if already in the target time zone
                    }
                    else
                    {
                        // Convert from local to target time zone via UTC
                        DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(dateTime);
                        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, targetTimeZone);
                    }
                default:
                    return dateTime; // Fallback for unexpected DateTimeKind values
            }
        }
    }
}
