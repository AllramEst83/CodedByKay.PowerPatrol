using System.Runtime.InteropServices;

namespace CodedByKay.PowerPatrol.Extensions
{
    internal static class DateTimeExtensions
    {
        public static DateTime ToSwedishTime(this DateTime utcDateTime)
        {
            // Ensure utcDateTime is treated as UTC
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            // Swedish time zone ID
            string swedishTimeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                     ? "W. Europe Standard Time"
                                     : "Europe/Stockholm";
            try
            {
                TimeZoneInfo swedishTimeZone = TimeZoneInfo.FindSystemTimeZoneById(swedishTimeZoneId);
                DateTime swedishTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, swedishTimeZone);
                return swedishTime;
            }
            catch (TimeZoneNotFoundException)
            {
                Console.WriteLine($"The time zone '{swedishTimeZoneId}' could not be found on this system.");
                // Handle the case where the time zone ID is not found. You could default to UTC or another known ID.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                // Handle unexpected errors
            }

            return utcDateTime; // Return the original UTC time if conversion fails
        }
    }
}
