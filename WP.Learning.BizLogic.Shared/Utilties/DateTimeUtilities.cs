using System;
using System.Collections.Generic;
using System.Text;

namespace WP.Learning.BizLogic.Shared.Utilties
{
    public static class DateTimeUtilities
    {
        public static TimeZoneInfo GetTimeZoneInfo(string timeZoneAbbreviation)
        {
            switch (timeZoneAbbreviation.ToUpper())
            {
                case @"PST":
                    return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

                case @"MST":
                    return TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");

                case @"CST":
                    return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

                case @"EST":
                    return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

                case @"GMT":
                default:
                    return TimeZoneInfo.FindSystemTimeZoneById("Greenwich Standard Time");
            }
        }

        public static DateTime CovertToUserLocalDT (DateTime originalDT, string timeZoneAbbreviation)
        {
            TimeZoneInfo usersTimeZone = GetTimeZoneInfo(timeZoneAbbreviation);

            DateTime userDT = TimeZoneInfo.ConvertTime(originalDT, usersTimeZone);

            return userDT;
        }
    }
}
