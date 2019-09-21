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

        public static string BuildUserInfoMsg(IQBuzzUserBE user)
        {
            StringBuilder returnMsg = new StringBuilder();

            returnMsg.AppendLine($"Hi {user.first_name}! Here is your {GeneralConstants.APP_NAME} user information:");
            returnMsg.AppendLine($"User ID: {user.user_id}");
            returnMsg.AppendLine($"Current Status: {user.has_accepted_welcome_agreement}");
            returnMsg.AppendLine($"Phone No: {user.phone_no}");
            returnMsg.AppendLine($"Timezone: {user.local_time_zone}");
            returnMsg.AppendLine($"Prefered Language: {LanguageType.GetDescription(user.language_code)}");
            returnMsg.AppendLine($"  Hint: To chg your language text lang? back to me");
            returnMsg.AppendLine($"--------------------------------------");
            returnMsg.AppendLine($"You are currently receiving information for the following Businesses:");
            foreach (var merchant in user.Merchants)
            {
                returnMsg.AppendLine($"  Store: [MID: {merchant.merchant_id}] {merchant.merchant_name}");
            }

            return returnMsg.ToString();
        }

        #endregion
       
        #region === User Activity ================================================================

        public static void LogUserActivity(UserActivityMBE userActivity)
        {
            MongoDBContext.InsertUserActivity(userActivity);
        }

        public static void LogUserActivity(string fromPhoneNumber, string action, DateTime activityDT, List<string> comments)
        {
            foreach (string comment in comments)
            {
                LogUserActivity(new UserActivityMBE() { phone_no = fromPhoneNumber, action = action, activity_dt = activityDT, comments = comment });
            }
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

        internal static string SetUserLanguage(int user_id, string languageCode)
        {
            // see if the user is already on file
            IQBuzzUserMBE exisitingUser = MongoDBContext.FindIQBuzzUser(user_id);

            // TODO: need to add validation
            exisitingUser.language_code = languageCode;

            MongoDBContext.UpdateIQBUzzUser(exisitingUser);

            return $"User: {exisitingUser.first_name} {exisitingUser.last_name} language set to [{LanguageType.GetDescription(exisitingUser.language_code)}]";
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
            StringBuilder welcomeMsg = new StringBuilder();
            StringBuilder merchantList = new StringBuilder();
            MerchantMBE merchant = null;


            foreach(var merchantId in iqBuzzUser.merchant_ids)
            {
                merchant = MongoDBContext.FindMerchantById(merchantId);
                merchantList.AppendLine($" MID: {merchant.merchant_id} ({merchant.merchant_name})");
            }

            welcomeMsg.AppendLine($"Hello, {iqBuzzUser.FullName}!");
            welcomeMsg.AppendLine();
            welcomeMsg.AppendLine($"On behalf of FIS, thank you for trusting us with payment acceptance for");
            welcomeMsg.AppendLine(merchantList.ToString());
            //welcomeMsg.AppendLine();
            welcomeMsg.AppendLine("My name is Buzz, and I’ll keep you informed of key activity on your account.");

            if (!iqBuzzUser.has_accepted_welcome_agreement)
            {
                welcomeMsg.AppendLine("To confirm your subscription, reply YES to this message.");
            }
            else
            {
                welcomeMsg.AppendLine("You have already confirmed enrollment.");
            }

            return welcomeMsg.ToString();
        }

        /// <summary>
        /// Used by the Backoffice to push a welcome message
        /// </summary>
        /// <param name="userId"></param>
        public static void SendWelcomeMessage(int userId)
        {
            var user = (IQBuzzUserBE)MongoDBContext.FindIQBuzzUser(userId).As<IQBuzzUserBE>();

            var welcomeMsg = BuildWelcomeMessage(user);

            SMSController.SendSMSMessage(user.phone_no, $"{welcomeMsg}");
        }

        /// <summary>
        /// Builds the join message.
        /// </summary>
        /// <param name="iqBuzzUser">The iq buzz user.</param>
        /// <returns>System.String.</returns>
        public static string BuildJoinMessage(int userId)
        {
            var user = MongoDBContext.FindIQBuzzUser(userId);

            StringBuilder joinMsg = new StringBuilder();

            joinMsg.AppendLine($"Welcome back, {user.first_name}! I will be glad to help keep you informed of your processing activity going forward. I'll give you key information, and help you find more information when you need it!");
            joinMsg.AppendLine($"As a reminder, here is a list of commands that I understand:");
            joinMsg.AppendLine(RequestController.BuildHelpMessage());

            user.has_accepted_welcome_agreement = true;
            MongoDBContext.UpdateIQBUzzUser(user);

            return joinMsg.ToString();
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

            StringBuilder returnMsg = new StringBuilder();

            // they have already accepted
            if (user.has_accepted_welcome_agreement)
            {
                returnMsg.AppendLine($"Thanks for replying, you have already accepted!, Hint: You can always text HELP? or ??? to see a list of commands.");
            }
            // they are accepting or declining now
            else if (isAccepted)
            {
                user.has_accepted_welcome_agreement = isAccepted;
                MongoDBContext.UpdateIQBUzzUser(user);

                returnMsg.AppendLine($"Great! Welcome to {GeneralConstants.APP_NAME}!");
                returnMsg.AppendLine($"If you add my number: {GeneralConstants.TWILIO_PHONE_NO} to your contact list, you’ll always know it’s me when I message you.");
                returnMsg.AppendLine("And then you can tell your phone's voice assistant what you need my help with!");
                returnMsg.AppendLine();
                returnMsg.AppendLine("I will automatically notify you when we receive your daily batch(es) or when we close them for you (if you're enrolled in Host Data Capture). I'll also let you know when we finish processing your daily settlement, so you'll know everything is on track!");
                returnMsg.AppendLine();
                returnMsg.AppendLine("In addition, here is a list of commands you can text me anytime you need information:");
                returnMsg.AppendLine(RequestController.BuildHelpMessage());
            }
            else
            {
                returnMsg.AppendLine("OK, I won't send you any messages until you tell me to. To do that, simply text JOIN to my number again!");
                returnMsg.AppendLine($"If you have questions about iQBuzz, please give us a call anytime! {GeneralConstants.WORLDPAY_CONTACT_CENTER_PHONE_NO}");
            }

            return returnMsg.ToString(); ;
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
                        merchant_ids = new List<int>() { 1, 2 },
                        language_code = LanguageType.ENGLISH.ToString()
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
                        merchant_ids = new List<int>() { 1, 2 },
                        language_code = LanguageType.ENGLISH.ToString()
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
                        merchant_ids = new List<int>() { 3 },
                        language_code = LanguageType.ENGLISH.ToString()
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
                        merchant_ids = new List<int>() { 4 },
                        language_code = LanguageType.ENGLISH.ToString()
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
                        merchant_ids = new List<int>() { 5 },
                        language_code = LanguageType.ENGLISH.ToString()
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
                        merchant_ids = new List<int>() { 6 },
                        language_code = LanguageType.ENGLISH.ToString()
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
                        merchant_ids = new List<int>() { 7 },
                        language_code = LanguageType.ENGLISH.ToString()
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
                        merchant_ids = new List<int>() { 8 },
                        language_code = LanguageType.ENGLISH.ToString()
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
                        merchant_ids = new List<int>() { 9 },
                        language_code = LanguageType.ENGLISH.ToString()
                    }
                }
            };

            return usersLU;
        }

        #endregion
    }
}
