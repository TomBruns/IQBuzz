using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Twilio;
using Twilio.Rest.Api.V2010.Account;

using WP.Learning.BizLogic.Shared.Entities;
using WP.Learning.BizLogic.Shared.Utilties;
using WP.Learning.MongoDB;
using WP.Learning.MongoDB.Entities;

namespace WP.Learning.BizLogic.Shared.Controllers
{
    /// <summary>
    /// This class supports all the cmds supported by IQBuzz
    /// </summary>
    public static class RequestController
    {
        public static List<string> ProcessIncommingText(string fromPhoneNo, string requestBody)
        {
            // this variable will hold all the msgs to send in response to the incoming text 
            List<string> responseMsgs = new List<string>();

            // try to lookup the user texting us using the incoming phone number
            IQBuzzUserBE user = UserController.FindIQBuzzUser(fromPhoneNo);

            // ===================================================
            // if the phone number is not associated with a registered user end here
            // ===================================================
            if (user == null)
            {
                responseMsgs.Add($"Sorry :) Phone #: {fromPhoneNo} is not setup to use {GeneralConstants.APP_NAME}.  Please contact the {GeneralConstants.APP_NAME} Hackathon team to become a beta tester.");

                UserController.LogUserActivity(fromPhoneNo, requestBody, DateTime.Now, responseMsgs);

                return responseMsgs;
            }

            // grab the current time in UTC
            DateTime currentUTCDT = DateTime.Now.ToUniversalTime();

            // optionally translate the reqeust
            if((requestBody != @"reset") && (user.language_code != LanguageType.ENGLISH.ToString()))
            {
                requestBody = TranslateRequest(requestBody);
            }

            // TODO: Change the if/elseif code below to a switch statement for performance 
            // ===================================================
            // Everything below here is supported BEFORE you have accepted the T&C
            // ===================================================
            if (requestBody == @"join" || requestBody == @"start")    // user requests to join
            {
                if (user.has_seen_welcome_message)
                {
                    responseMsgs.Add(UserController.BuildJoinMessage(user.user_id));
                }
                else
                {
                    UserController.SendWelcomeMessage(user.user_id);
                }
            }
            else if (requestBody == @"welcome")     // send welcome msg
            {
                UserController.SendWelcomeMessage(user.user_id);
            }
            else if (requestBody == @"yes")     // confirm & accept welcome msg
            { 
                responseMsgs.Add(UserController.StoreAcceptWelcomeMessageResponse(user.user_id, true));
            }
            else if (requestBody == @"unwelcome")     // reset user has seen welcome flag
            {
                UserController.ResetHasSeenWelcomeMessageFlag(user.user_id);
            }
            else if (!user.has_accepted_welcome_agreement)
            {
                responseMsgs.Add($"Please text JOIN to allow {GeneralConstants.APP_NAME} to talk to you.");
            }

            // =====================================================
            // Everything below here is supported AFTER you have accepted the T&C
            // =====================================================
            else if (requestBody == @"user" || requestBody == @"whoami")    // display current user info
            {
                responseMsgs.Add(UserController.BuildUserInfoMsg(user));
            }
            else if (requestBody == @"users")    // display list of current users
            {
                responseMsgs.Add(UserController.BuildUsersInfoMsg());
            }
            else if (requestBody.StartsWith(@"setup"))    // setup new user
            {
                string[] msgParts = requestBody.Split('-');
                responseMsgs.Add(UserController.SetupNewUser(user.user_id, currentUTCDT.Date, msgParts));
            }
            else if (requestBody == @"lang?" || requestBody == @"lang")
            {
                responseMsgs.Add(LanguageType.GetSupportedLanguages(@"lang"));
            }
            else if (requestBody.StartsWith(@"lang"))    // set user language
            {
                string[] msgParts = requestBody.Split('-');
                responseMsgs.Add(UserController.SetUserLanguage(user.user_id, msgParts[1]));

                // refresh user to get updated language setting
                user = UserController.FindIQBuzzUser(fromPhoneNo);
            }
            else if (requestBody == @"reset")   // force back to english
            {
                responseMsgs.Add(UserController.SetUserLanguage(user.user_id, LanguageType.ENGLISH.ToString()));

                // refresh user to get updated language setting
                user = UserController.FindIQBuzzUser(fromPhoneNo);
            }
            else if (requestBody == @"summary" || requestBody == @"summ")     // Summary for today
            {
                responseMsgs.Add(MerchantController.BuildOverallSummaryMessage(user.merchant_ids, currentUTCDT.Date, user.local_time_zone));
            }
            else if (requestBody == @"sales")       // total sales for today
            {
                responseMsgs.Add(MerchantController.BuildSalesSummaryMessage(user.merchant_ids, currentUTCDT.Date, user.local_time_zone));
            }
            else if (requestBody == @"cback" || requestBody == @"chargeback" || requestBody == @"chargebacks")  // chargebacks for today
            {
                responseMsgs.Add(MerchantController.BuildChargebackDetails(user.merchant_ids, currentUTCDT.Date, user.local_time_zone));
            }
            else if (requestBody == @"returns" || requestBody == @"return" || requestBody == @"ret" 
                        || requestBody == @"refunds" || requestBody == @"refund" || requestBody == @"ref")    // returns for today
            {
                responseMsgs.Add(MerchantController.BuildReturnsSummaryMessage(user.merchant_ids, currentUTCDT.Date, user.local_time_zone));
            }
            else if (requestBody == @"faf")     // fast access funding
            {
                responseMsgs.Add(MerchantController.BuildFAFMessage(user.merchant_ids, currentUTCDT.Date));
            }
            else if (requestBody == @"confirm") // confirm faf request
            {
                responseMsgs.Add(MerchantController.BuildConfirmFAFMessage(user.merchant_ids, currentUTCDT.Date));
            }
            else if (requestBody == @"undo")    // undo faf request
            {
                responseMsgs.Add(MerchantController.BuildUndoFAFMessage(user.merchant_ids, currentUTCDT.Date));
            }
            else if (requestBody == @"batch-miss")
            {
                responseMsgs.Add(BatchController.BuildBatchMissingMessage(user.user_id, user.merchant_ids[0]));
            }
            else if (requestBody == @"batch-ok")
            {
                responseMsgs.Add(BatchController.BuildBatchReceivedOkMessage(user.user_id, user.merchant_ids[0]));
            }
            else if (requestBody == @"batch-err")
            {
                responseMsgs.Add(BatchController.BuildBatchReceivedErrorMessage(user.user_id, user.merchant_ids[0]));
            }
            else if (requestBody == @"batch-auto")
            {
                responseMsgs.Add(BatchController.BuildBatchAutoCloseMessage(user.user_id, user.merchant_ids[0]));
            }
            else if (requestBody == @"unjoin")  // unjoin
            {
                responseMsgs.Add(UserController.ResetAcceptedJoin(user.user_id));
            }
            else if (requestBody == @"config" || requestBody == @"settings")    // show user config
            {
                responseMsgs.Add(UserController.BuildConfigMessage(user.user_id));
            }
            else if (requestBody == @"ver") // display software build info
            {
                // get the d/t this assy was built
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
                DateTime lastModifiedDTLocal = fileInfo.LastWriteTime;

                DateTime lastModifiedDTUser = lastModifiedDTLocal.CovertToUserLocalDT(user.local_time_zone);

                responseMsgs.Add($"Running {GeneralConstants.APP_NAME} built on: [{lastModifiedDTUser} {user.local_time_zone}]");
            }
            else if (requestBody == @"genxcts" || requestBody == @"gen") // generate random xcts for today
            {
                (int xctGeneratedCount, int merchantsCount) = GenSampleXcts(user, currentUTCDT);

                responseMsgs.Add($"[{xctGeneratedCount}] random xcts generated across [{merchantsCount}] merchants and posted to {currentUTCDT.Date:M/dd/yyyy}");
            }
            else if (requestBody.StartsWith(@"genxcts"))    // gen xcts for a specifc user
            {
                if(user.is_admin_user)
                {
                    string[] msgParts = requestBody.Split('-');

                    if (msgParts.Length != 2)
                    {
                        responseMsgs.Add($"Usage: genxcts-<userid>");
                    }
                    else
                    {
                        int targetUserID = 0;
                        if (Int32.TryParse(msgParts[1], out targetUserID))
                        {
                           
                            var targetUser = UserController.GetIQBuzzUser(targetUserID);

                            (int xctGeneratedCount, int merchantsCount) = GenSampleXcts(targetUser, currentUTCDT);

                            responseMsgs.Add($"[{xctGeneratedCount}] random xcts generated across [{merchantsCount}] merchants and posted to {currentUTCDT.Date:M/dd/yyyy}");
                        }
                        else
                        {
                            responseMsgs.Add($"[{msgParts[1]}] is not a valid userid");
                        }   
                    }
                }
                else
                {
                    responseMsgs.Add($"Sorry, this cmd is not available unless you are an admin.");
                }
            }
            else if (requestBody == @"usage" || requestBody == @"stats")    // display usage stats
            {
                DateTime currentUserDT = DateTimeUtilities.CovertToUserLocalDT(currentUTCDT, user.local_time_zone);
                DateTime fromDate = currentUserDT.Date;
                TimeZone localZone = TimeZone.CurrentTimeZone;

                List<UserDailyUsageSummaryBE> usage = UserController.GetUserActivitySummaryByDay(fromDate, user.local_time_zone);

                StringBuilder msg = new StringBuilder();
                msg.AppendLine($"{GeneralConstants.APP_NAME} usage stats: {fromDate.AddDays(-5):MMM d} to {fromDate:MMM d}");
                msg.AppendLine($"  (as of { currentUserDT.ToString("ddd MMM dd, yyyy h:mm tt")})");
                msg.AppendLine($"  (Server Local Time{ DateTime.Now.ToString("ddd MMM dd, yyyy h:mm tt")}), TZ: {localZone.StandardName}");
                msg.AppendLine("----------------------------------");

                // get a list of unique user entities from the collection
                var uniqueUsers = usage.DistinctBy(u => u.IQBuzzUser.user_id).Select(u => u.IQBuzzUser).OrderBy(u => u.user_id).ToList();
                    
                // build a summary line for each unique user
                foreach (var uniqueUser in uniqueUsers)
                {
                    msg.Append($"[{uniqueUser.user_id}] {uniqueUser.FullName} |");

                    // loop over the date range
                    for (DateTime activityDate = fromDate.AddDays(-4); activityDate <= fromDate; activityDate = activityDate.AddDays(1))
                    {
                        var activityOnDate = usage.Where(u => u.PhoneNo == uniqueUser.phone_no && u.ActivityDate == activityDate).FirstOrDefault();

                        // add a field for each day
                        if (activityOnDate != null)
                        {
                            msg.Append($"{activityOnDate.ActionQty}|");
                        }
                        else
                        {
                            msg.Append(@"0|");
                        }
                    }
                    msg.AppendLine();
                }

                responseMsgs.Add(msg.ToString());
            }
            else if (requestBody == @"help"
                        || requestBody == @"help?"
                        || requestBody == @"???"
                        || requestBody == @"?")
            {
                responseMsgs.Add(BuildHelpMessage());
            }
            else if (requestBody == @"help+" || requestBody == @"help*")
            {
                responseMsgs.Add(BuildExtendedHelpMessage());
            }
            else
            {
                responseMsgs.Add($"Sorry I do not understand [{requestBody}], text help? to see a list of the available commmands.");
            }

            UserController.LogUserActivity(fromPhoneNo, requestBody, DateTime.Now, responseMsgs);

            if (user.language_code != LanguageType.ENGLISH.ToString())
            {
                responseMsgs = TranslationController.Translate(responseMsgs, user.language_code);
            }

            return responseMsgs;
        }

        private static (int xctGeneratedCount, int merchantsCount) GenSampleXcts(IQBuzzUserBE user, DateTime currentUTCDT)
        {
            int xctGeneratedCount = 0;
            int merchantsCount = 0;
            foreach (var merchant in user.Merchants)
            {
                merchantsCount++;

                xctGeneratedCount += MerchantController.GenerateSampleXcts(merchant.merchant_id, currentUTCDT.Date);
            }

            return (xctGeneratedCount, merchantsCount);
        }

        private static string TranslateRequest(string requestBody)
        {
            var translatedRequest = TranslationController.Translate(requestBody, LanguageType.ENGLISH.ToString());

            return translatedRequest;
        }

        public static string BuildHelpMessage()
        {
            StringBuilder helpMsg = new StringBuilder();

            helpMsg.AppendLine("--------------------------------------------------------");
            helpMsg.AppendLine("Hint: Commands are not case sensitive");
            helpMsg.AppendLine("--------------------------------------------------------");
            helpMsg.AppendLine("SUMMARY: today's summary of all stores and online activity:");
            helpMsg.AppendLine("SALES: today's sales figures by store and online:");
            helpMsg.AppendLine("RETURNS: today's returns by store and online:");
            helpMsg.AppendLine("CBACK: pending Chargebacks:");
            helpMsg.AppendLine("USER: your account details, including language preference:");
            helpMsg.AppendLine("SETTINGS: configure alert preferences:");
            helpMsg.AppendLine("JOIN: re-send the welcome message:");
            helpMsg.AppendLine($"UNJOIN: Unsubscribe from {GeneralConstants.APP_NAME} (I hope you won't!):");
            helpMsg.AppendLine("HELP?: displays this list of commands:");

            return (helpMsg.ToString());
        }

        public static string BuildExtendedHelpMessage()
        {
            StringBuilder helpMsg = new StringBuilder();

            helpMsg.AppendLine("------------------------------");
            helpMsg.AppendLine(" Additional Commands");
            helpMsg.AppendLine("------------------------------");
            helpMsg.AppendLine("unjoin: reverse join (for testing)");
            helpMsg.AppendLine("ver: display software build d/t");
            helpMsg.AppendLine("genxcts or gen: generate random xcts");
            helpMsg.AppendLine("usage: display usage stats for last 5 days");
            helpMsg.AppendLine("batch-miss: send missign batch msg");
            helpMsg.AppendLine("batch-ok: send received ok msg");
            helpMsg.AppendLine("batch-err: send received err msg");
            helpMsg.AppendLine("batch-auto: set auto close");
            helpMsg.AppendLine("welcome: send welcome msg");
            helpMsg.AppendLine("unwelcome: reset welcome msg flag");
            helpMsg.AppendLine("setup: setup a new user");
            helpMsg.AppendLine("users: display list of all users");

            return (helpMsg.ToString());
        }
    }
}
