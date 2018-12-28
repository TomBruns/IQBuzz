using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WP.Learning.BizLogic.Shared.Entities;
using WP.Learning.BizLogic.Shared.Utilties;
using WP.Learning.MongoDB;
using WP.Learning.MongoDB.Entities;

namespace WP.Learning.BizLogic.Shared.Controllers
{
    public static class UserController
    {
        #region === IQBuzzUsers ================================================================

        public static IQBuzzUserBE FindIQBuzzUser(string phoneNo)
        {
            IQBuzzUserBE iqBuzzUser = MongoDBContext.FindIQBuzzUser(phoneNo).As<IQBuzzUserBE>();

            foreach(var merchantId in iqBuzzUser.merchant_ids)
            {
                var merchant = MongoDBContext.FindMerchantById(merchantId);

                if(merchant != null)
                {
                    iqBuzzUser.Merchants.Add(merchant);
                }
            }

            return iqBuzzUser;
        }

        public static void CreateAllUsers(bool isDeleteIfExists)
        {
            Dictionary<int, IQBuzzUserMBE> usersLU = BuildUsersLU();

            foreach (KeyValuePair<int, IQBuzzUserMBE> kvp in usersLU)
            {
                CreateUser(kvp.Value, isDeleteIfExists);
            }

            System.Console.WriteLine();
            System.Console.WriteLine("Hit <enter> to exit");
            System.Console.ReadLine();
        }

        private static void CreateUser(IQBuzzUserMBE newUser, bool isDeleteIfExists)
        {
            //  exists  isDeleteIfExists => Delete  Insert
            //      F       N/A               N/A       T
            //      T       F                 F         F
            //      T       T                 T         T

            bool isUserAlreadyExists = false;

            // see if the user is already on file
            IQBuzzUserMBE exisitingUser = MongoDBContext.FindIQBuzzUser(newUser.user_id);

            if (exisitingUser != null && isDeleteIfExists)
            {
                MongoDBContext.DeleteIQBuzzUser(newUser.user_id);
                exisitingUser = null;
                isUserAlreadyExists = true;
            }

            string newUserFullName = $"{newUser.first_name} {newUser.last_name}";

            if (exisitingUser == null)
            {
                MongoDBContext.InsertIQBuzzUser(newUser);
                if (!isUserAlreadyExists)
                {
                    System.Console.WriteLine($"Loaded User: {newUser.user_id} [{newUserFullName}]");
                }
                else
                {
                    System.Console.WriteLine($"Purged & Reloaded User: {newUser.user_id} [{newUserFullName}]");
                }
            }
            else
            {
                System.Console.WriteLine($"Existing User: {newUser.user_id} [{newUserFullName}] not overwritten");
            }
        }

        public static void CreateUser(int userId, bool isDeleteIfExists)
        {
            Dictionary<int, IQBuzzUserMBE> usersLU = BuildUsersLU();

            var user = usersLU[userId];

            CreateUser(user, isDeleteIfExists);
        }

        #endregion

        #region === User Activity ================================================================

        public static void LogUserActivity(UserActivityMBE userActivity)
        {
            MongoDBContext.InsertUserActivity(userActivity);
        }

        public static void LogUserActivity(string fromPhoneNumber, string action, DateTime activityDT, string comments)
        {
            LogUserActivity(new UserActivityMBE() { phone_no = fromPhoneNumber, action = action, activity_dt = activityDT, comments = comments });
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
            Dictionary<string, IQBuzzUserMBE> iqBuzzUsersLU = BuildIQBuzzUsersLU();

            // step 6: translate the phoneNos to a full name
            List<UserDailyUsageSummaryBE> userDailyUsageSummary = new List<UserDailyUsageSummaryBE>();
            foreach (var summaryItem in summary)
            {
                userDailyUsageSummary.Add(new UserDailyUsageSummaryBE()
                {
                    PhoneNo = summaryItem.PhoneNo,
                    ActionQty = summaryItem.ActionQty,
                    ActivityDate = summaryItem.ActivityDate,
                    IQBuzzUser = LookupIQBuzzUser(summaryItem.PhoneNo, iqBuzzUsersLU).As<IQBuzzUserBE>()
            });
            }

            // return the results
            return userDailyUsageSummary;
        }

        #endregion

        #region === Welcome Message =====================================================

        /// <summary>
        /// Build the text for the Welcome Message
        /// </summary>
        /// <param name="merchant"></param>
        /// <returns></returns>
        public static string BuildWelcomeMessage(IQBuzzUserBE iqBuzzUser)
        {
            string welcomeMsg = string.Empty;

            if (!iqBuzzUser.has_accepted_welcome_agreement)
            {
                welcomeMsg = $"Hi {iqBuzzUser.first_name} {iqBuzzUser.last_name}, Welcome to IQ Buzz\n"
                   + $"Reply YES to confirm enrollment in {GeneralConstants.APP_NAME}. "
                   + "Msg&Data rates may appy. Msg freq varies by acct and prefs.";
            }
            else
            {
                welcomeMsg = $"Hi {iqBuzzUser.first_name} {iqBuzzUser.last_name}, Welcome to IQ Buzz\n"
                   + $"You have already confirmed enrollment in {GeneralConstants.APP_NAME}. "
                   + "Msg&Data rates may appy. Msg freq varies by acct and prefs.";
            }

            return welcomeMsg;
        }

        /// <summary>
        /// Used by the Backoffice to push a welcome message
        /// </summary>
        /// <param name="userId"></param>
        public static void SendWelcomeMessage(int userId)
        {
            var user = (IQBuzzUserBE)MongoDBContext.FindIQBuzzUser(userId);

            var welcomeMsg = BuildWelcomeMessage(user);

            SMSController.SendSMSMessage(user.phone_no, $"{welcomeMsg}");
        }

        /// <summary>
        /// Process Welcome Acceptance reponse
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isAccepted"></param>
        /// <returns></returns>
        public static string StoreAcceptWelcomeMessageResponse(int userId, bool isAccepted)
        {
            // get merchant metadata (MDB ??)
            var user = MongoDBContext.FindIQBuzzUser(userId);

            string returnMsg = string.Empty;

            // they have already accepted
            if (user.has_accepted_welcome_agreement)
            {
                returnMsg = "Thanks for replying, you have already accepted!, Hint: You can always text HELP? or ??? to see a list of commands.";
            }
            // they are accepting or declining now
            else if (isAccepted)
            {
                user.has_accepted_welcome_agreement = isAccepted;
                MongoDBContext.UpdateIQBUzzUser(user);

                returnMsg = isAccepted
                        ? $"Welcome to {GeneralConstants.APP_NAME}, you are all setup! Hint: You can always text HELP? or ??? to see a list of commands."
                        : "We are sorry you choose not to join, text JOIN at any time to have another opportunity to accept.";
            }

            return returnMsg;
        }

        // Command: Unjoin
        public static string ResetAcceptedJoin(int userId)
        {
            var user = MongoDBContext.FindIQBuzzUser(userId);

            user.has_accepted_welcome_agreement = false;

            MongoDBContext.UpdateIQBUzzUser(user);

            string returnMsg = "Welcome Acceptance reset";

            return returnMsg;
        }

        #endregion

        public static string BuildConfigMessage(int userId)
        {
            var configMsg = $"To configure your daily summaries, alerts, sign-up for FastAccess Funding and adjust batch time, go to {GeneralConstants.CFG_URL}";

            return configMsg;
        }


        #region --- Helpers --------------------------------------------------------
        private static Dictionary<string, IQBuzzUserMBE> BuildIQBuzzUsersLU()
        {
            // get a list of all all the registered users (coverting to the derived type)
            //List<IQBuzzUserBE> users = MongoDBContext.GetAllIQBuzzUsers().ConvertAll(u => (IQBuzzUserBE)u);
            var users = MongoDBContext.GetAllIQBuzzUsers();

            // convert to a dictionary
            var usersLU = users.ToDictionary(t => t.phone_no, t => t);

            return usersLU;
        }

        private static IQBuzzUserMBE LookupIQBuzzUser(string phoneNo, Dictionary<string, IQBuzzUserMBE> iqBuzzUsers)
        {
            if (iqBuzzUsers == null)
            {
                iqBuzzUsers = BuildIQBuzzUsersLU();
            }

            IQBuzzUserMBE iqBuzzUser = null;

            if (!iqBuzzUsers.TryGetValue(phoneNo, out iqBuzzUser))
            {
                iqBuzzUser = new IQBuzzUserMBE()
                {
                    phone_no = phoneNo,
                    first_name = @"Unknown",
                    last_name = @"User",
                    local_time_zone = @"EST"
                };
            }

            return iqBuzzUser;
        }

        private static Dictionary<int, IQBuzzUserMBE> BuildUsersLU()
        {
            Dictionary<int, IQBuzzUserMBE> usersLU= new Dictionary<int, IQBuzzUserMBE>()
            {
                {
                    1, new IQBuzzUserMBE()
                    {
                        user_id = 1,
                        first_name = @"Tom",
                        last_name = @"Bruns",
                        phone_no = GeneralConstants.TOMS_PHONE_NO,
                        email_address = @"xtobr39@hotmail.com",
                        local_time_zone = @"EST",
                        has_accepted_welcome_agreement = true,
                        merchant_ids = new List<int>() { 1 }
                    }
                },
                {
                    2, new IQBuzzUserMBE()
                    {
                        user_id = 2,
                        first_name = @"Marco",
                        last_name = @"Fernandes",
                        phone_no = GeneralConstants.MARCOS_PHONE_NO,
                        email_address = @"Marco.Fernandes@worldpay.com",
                        local_time_zone = @"EST",
                        has_accepted_welcome_agreement = true,
                        merchant_ids = new List<int>() { 2 }
                    }
                },
                {
                    3, new IQBuzzUserMBE()
                    {
                        user_id = 3,
                        first_name = @"Dusty",
                        last_name = @"Gomez",
                        phone_no = GeneralConstants.DUSTYS_PHONE_NO,
                        email_address = @"Dusty.Gomez@worldpay.com",
                        local_time_zone = @"EST",
                        has_accepted_welcome_agreement = true,
                        merchant_ids = new List<int>() { 3 }
                    }
                },
                {
                    4, new IQBuzzUserMBE()
                    {
                        user_id = 4,
                        first_name = @"Josh",
                        last_name = @"Byrne",
                        phone_no = GeneralConstants.JOSHS_PHONE_NO,
                        email_address = @"Joshua.Byrne@worldpay.com",
                        local_time_zone = @"EST",
                        has_accepted_welcome_agreement = true,
                        merchant_ids = new List<int>() { 4 }
                    }
                },
                {
                    5, new IQBuzzUserMBE()
                    {
                        user_id = 5,
                        first_name = @"Alex",
                        last_name = @"Boeding",
                        phone_no = GeneralConstants.ALEXS_PHONE_NO,
                        email_address = @"Axex.Boeding@worldpay.com",
                        local_time_zone = @"EST",
                        has_accepted_welcome_agreement = true,
                        merchant_ids = new List<int>() { 5 }
                    }
                },
                {
                    6, new IQBuzzUserMBE()
                    {
                        user_id = 6,
                        first_name = @"Pallavi",
                        last_name = @"Sher",
                        phone_no = GeneralConstants.PALLAVI_PHONE_NO,
                        email_address = @"Pallavi.Sher@worldpay.com",
                        local_time_zone = @"EST",
                        has_accepted_welcome_agreement = true,
                        merchant_ids = new List<int>() { 6 }
                    }
                },
                {
                    7, new IQBuzzUserMBE()
                    {
                        user_id = 7,
                        first_name = @"Joe",
                        last_name = @"Pellar",
                        phone_no = GeneralConstants.JOES_PHONE_NO,
                        email_address = @"Joe.Pellar@worldpay.com",
                        local_time_zone = @"CST",
                        has_accepted_welcome_agreement = true,
                        merchant_ids = new List<int>() { 7 }
                    }
                },
                {
                    8, new IQBuzzUserMBE()
                    {
                        user_id = 8,
                        first_name = @"Jianan",
                        last_name = @"Hou",
                        phone_no = GeneralConstants.JAKES_PHONE_NO,
                        email_address = @"Jianan.Hou@worldpay.com",
                        local_time_zone = @"EST",
                        has_accepted_welcome_agreement = true,
                        merchant_ids = new List<int>() { 8 }
                    }
                },
                {
                    9, new IQBuzzUserMBE()
                    {
                        user_id = 9,
                        first_name = @"Jon",
                        last_name = @"Pollock",
                        phone_no = @"+16153309751",
                        email_address = @"Jon.Pollock@worldpay.com",
                        local_time_zone = @"EST",
                        has_accepted_welcome_agreement = true,
                        merchant_ids = new List<int>() { 9 }
                    }
                }
            };

            return usersLU;
        }

        #endregion
    }
}
