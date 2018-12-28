using System;
using System.Collections.Generic;
using System.Text;

namespace WP.Learning.BizLogic.Shared.Utilties
{
    /// <summary>
    /// Extension Methods for Business Days
    /// </summary>
    public static class DateExtensions
    {
        /// <summary>
        /// Adds the given number of business days to the <see cref="DateTime"/>.
        /// </summary>
        /// <param name="current">The date to be changed.</param>
        /// <param name="days">Number of business days to be added.</param>
        /// <returns>A <see cref="DateTime"/> increased by a given number of business days.</returns>
        public static DateTime AddBusinessDays(this DateTime current, int days)
        {
            var sign = Math.Sign(days);
            var unsignedDays = Math.Abs(days);
            List<DateTime> bankingHolidays = GetBankingHolidays();

            for (var i = 0; i < unsignedDays; i++)
            {
                do
                {
                    current = current.AddDays(sign);
                }
                while (current.DayOfWeek == DayOfWeek.Saturday ||
                        current.DayOfWeek == DayOfWeek.Sunday ||
                        bankingHolidays.Contains(current));
            }
            return current;
        }

        /// <summary>
        /// Subtracts the given number of business days to the <see cref="DateTime"/>.
        /// </summary>
        /// <param name="current">The date to be changed.</param>
        /// <param name="days">Number of business days to be subtracted.</param>
        /// <returns>A <see cref="DateTime"/> increased by a given number of business days.</returns>
        public static DateTime SubtractBusinessDays(this DateTime current, int days)
        {
            return AddBusinessDays(current, -days);
        }

        // ACH payment do no occur on banking holidays
        internal static List<DateTime> GetBankingHolidays()
        {
            List<DateTime> bankingHolidays = new List<DateTime>()
            {
                new DateTime(2018, 12, 25), // Christmas Day
                new DateTime(2019, 1, 1), // New Year's Day
                new DateTime(2019, 1, 21), // Martin Luther King Jr. Day
                new DateTime(2019, 2, 18), // President’s Day
                new DateTime(2019, 5, 27), // Memorial Day
                new DateTime(2019, 7, 4), // Independence Day
                new DateTime(2019, 9, 2), // Labor Day
                new DateTime(2019, 10, 14), // Columbus Day
                new DateTime(2019, 11, 11), // Veterans’ Day
                new DateTime(2019, 11, 28), // Thanksgiving Day 
                new DateTime(2019, 12, 25), // Christmas Day
                new DateTime(2020, 1, 1), // New Year's Day
                new DateTime(2020, 1, 20), // Martin Luther King Jr. Day
                new DateTime(2020, 2, 17), // President’s Day
                new DateTime(2020, 5, 25), // Memorial Day
                //new DateTime(2020, 7, 4), // Independence Day (Sat)
                new DateTime(2020, 9, 7), // Labor Day
                new DateTime(2020, 10, 12), // Columbus Day
                new DateTime(2020, 11, 11), // Veterans’ Day
                new DateTime(2020, 11, 28), // Thanksgiving Day 
                new DateTime(2020, 12, 25), // Christmas Day
            };

            return bankingHolidays;
        }
    }
}
