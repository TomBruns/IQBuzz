using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WP.Learning.BizLogic.Shared.Entities;
using WP.Learning.BizLogic.Shared.Utilties;
using WP.Learning.MongoDB;
using WP.Learning.MongoDB.Entities;

namespace WP.Learning.BizLogic.Shared.User
{
    public static class UserController
    {
        public static void LogUserActivity(UserActivityMBE userActivity)
        {
            MongoDBContext.InsertUserActivity(userActivity);
        }

        public static void LogUserActivity(string fromPhoneNumber, string action, DateTime activityDT, string comments)
        {
            LogUserActivity(new UserActivityMBE() { phone_no = fromPhoneNumber, action = action, activity_dt = activityDT, comments = comments });
        }

        public static Dictionary<string, IQBuzzUserBE> GetIQBuzzUsers()
        {
            var merchants = MongoDBContext.GetAllMerchants();

            var userXref = merchants.Select(m => new IQBuzzUserBE()
            {
                FirstName = m.primary_contact.first_name,
                LastName = m.primary_contact.last_name,
                PhoneNo = m.primary_contact.phone_no,
                EmailAddress = m.primary_contact.email_address,
                LocalTimeZone = m.primary_contact.local_time_zone
            }).ToDictionary(t => t.PhoneNo, t => t);

            return userXref;
        }

        public static List<UserDailyUsageSummaryBE> GetUserActivitySummaryByDay(DateTime fromDate, string usersTimeZoneAbbreviation)
        {
            // step 1: get the list of activity in the timeframe
            List<UserActivityMBE> rawActivities = MongoDBContext.GetUserActivitySummary(fromDate);

            // step 2: filter the list
            List<string> unwantedActions = new List<string>() { };
            List<UserActivityMBE> filteredAdjActivities = new List<UserActivityMBE>();

            // step 3: filter & translate the activity dates to my tz
            TimeZoneInfo usersTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            foreach (var activity in rawActivities.Where(x => !unwantedActions.Contains(x.action)))
            {
                filteredAdjActivities.Add(new UserActivityMBE()
                {
                    phone_no = activity.phone_no,
                    activity_dt = TimeZoneInfo.ConvertTime(activity.activity_dt, DateTimeUtilities.GetTimeZoneInfo(usersTimeZoneAbbreviation)),
                    action = activity.action
                });
            }

            // step 4: build summary
            var summary = (from fa in filteredAdjActivities select fa)
                        .GroupBy(x => new { x.activity_dt.Date, x.phone_no } )
                        .Select(g => new UserDailyUsageSummaryBE() { ActivityDate = (g.Key.Date).Date, PhoneNo = g.Key.phone_no, ActionQty = g.Count() }).ToList();

            // step 5: build user xref
            Dictionary<string, IQBuzzUserBE> iqBuzzUsers = GetIQBuzzUsers();

            // step 6: translate the phoneNos to a full name
            List<UserDailyUsageSummaryBE> userDailyUsageSummary = new List<UserDailyUsageSummaryBE>();
            foreach (var summaryItem in summary)
            {
                userDailyUsageSummary.Add(new UserDailyUsageSummaryBE()
                {
                    PhoneNo = summaryItem.PhoneNo,
                    ActionQty = summaryItem.ActionQty,
                    ActivityDate = summaryItem.ActivityDate,
                    IQBuzzUser = FindIQBuzzUser(summaryItem.PhoneNo, iqBuzzUsers)
                });
            }

            // return the results
            return userDailyUsageSummary;
        }

        private static IQBuzzUserBE FindIQBuzzUser(string phoneNo, Dictionary<string, IQBuzzUserBE> iqBuzzUsers)
        {
            IQBuzzUserBE iqBuzzUser = null;

            if (!iqBuzzUsers.TryGetValue(phoneNo, out iqBuzzUser))
            {
                iqBuzzUser = new IQBuzzUserBE() { PhoneNo = phoneNo, FirstName = @"Unknown", LastName = @"User", LocalTimeZone = @"EST" };
            }

            return iqBuzzUser;
        }
    }
}
