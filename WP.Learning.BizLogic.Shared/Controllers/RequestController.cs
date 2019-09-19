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

            // try to lookup the merchant using the incoming phone number
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

            DateTime currentUTCDT = DateTime.Now.ToUniversalTime();
            //DateTime currentUserDT = DateTimeUtilities.CovertToUserLocalDT(currentUTCDT, user.local_time_zone);
            //string currentUserTimeText = $"{currentUserDT.ToString("h:mm tt")} {user.local_time_zone}";

            // ===================================================
            // Everything below here is supported before you have accepted the T&C
            // ===================================================
            if (requestBody == @"join" || requestBody == @"start")    // user requests to join
            {
                responseMsgs.Add(UserController.BuildWelcomeMessage(user));
            }
            else if (requestBody == @"yes")     // confirm & accept welcome msg
            { 
                responseMsgs.Add(UserController.StoreAcceptWelcomeMessageResponse(user.user_id, true));

                responseMsgs.Add(BuildHelpMessage());
            }
            else if (!user.has_accepted_welcome_agreement)
            {
                responseMsgs.Add($"You must accept the Terms&Conditions before using {GeneralConstants.APP_NAME}, Text join to do that.");
            }

            // =====================================================
            // Everything below here requires you to have accepted the T&C first
            // =====================================================
            else if (requestBody == @"user" || requestBody == @"whoami")    // display current user info
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Hi {user.first_name} {user.last_name}!");
                sb.AppendLine($"User ID: {user.user_id}");
                sb.AppendLine($"Accepted EULA?: {user.has_accepted_welcome_agreement}");
                sb.AppendLine($"Phone No: {user.phone_no}");
                sb.AppendLine($"Timezone: {user.local_time_zone}");
                sb.AppendLine($"Prefered Language: {LanguageType.GetDescription(user.language_code)}");
                sb.AppendLine($"  Hint: To chg Buzz's language text lang?");
                sb.AppendLine($"--------------------------------------");
                sb.AppendLine($"Locations:");
                foreach (var merchant in user.Merchants)
                {
                    sb.AppendLine($"  [{merchant.merchant_id}] {merchant.merchant_name}");
                }

                responseMsgs.Add(sb.ToString());
            }
            else if (requestBody == @"lang?")
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
                responseMsgs.Add(MerchantController.BuildFAFMessage(user.merchant_ids[0]));
            }
            else if (requestBody == @"confirm") // confirm faf request
            {
                responseMsgs.Add(MerchantController.BuildConfirmFAFMessage(user.merchant_ids[0]));
            }
            else if (requestBody == @"undo")    // undo faf request
            {
                responseMsgs.Add(MerchantController.BuildUndoFAFMessage(user.merchant_ids[0]));
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
                int xctGeneratedCount = 0;
                int merchantsCount = 0;
                foreach (var merchant in user.Merchants)
                {
                    merchantsCount++;

                    xctGeneratedCount += MerchantController.GenerateSampleXcts(merchant.merchant_id, currentUTCDT.Date);
                }

                responseMsgs.Add($"[{xctGeneratedCount}] random xcts generated across [{merchantsCount}] merchants and posted to {currentUTCDT.Date:M/dd/yyyy}");
            }
            else if (requestBody == @"usage" || requestBody == @"stats")    // display usage stats
            {
                DateTime currentUserDT = DateTimeUtilities.CovertToUserLocalDT(DateTime.Now.ToUniversalTime(), user.local_time_zone);
                DateTime fromDate = currentUserDT.Date;

                List<UserDailyUsageSummaryBE> usage = UserController.GetUserActivitySummaryByDay(fromDate, user.local_time_zone);

                StringBuilder msg = new StringBuilder();
                msg.AppendLine($"{GeneralConstants.APP_NAME} usage stats: {fromDate.AddDays(-5):MMM d} to {fromDate:MMM d}");
                msg.AppendLine($"  (as of { currentUserDT.ToString("ddd MMM dd, yyyy h:mm tt")})");
                msg.AppendLine("----------------------------------");

                // get a list of unique user entities from the collection
                var uniqueUsers = usage.DistinctBy(u => u.IQBuzzUser.user_id).Select(u => u.IQBuzzUser).ToList();
                    
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
                StringBuilder helpMsg = new StringBuilder();

                helpMsg.AppendLine(" Additional Commands");
                helpMsg.AppendLine("------------------------------");
                helpMsg.AppendLine("unjoin: reverse join (for testing)");
                helpMsg.AppendLine("ver: display software build d/t");
                helpMsg.AppendLine("genxcts or gen: generate random xcts");
                helpMsg.AppendLine("usage: display usage stats for last 5 days");
                helpMsg.AppendLine("lang: set language code");

                responseMsgs.Add(helpMsg.ToString());
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

        private static string BuildHelpMessage()
        {
            StringBuilder helpMsg = new StringBuilder();

            helpMsg.AppendLine("");
            helpMsg.AppendLine("Here is a list of the commands that I understand:");
            helpMsg.AppendLine("--------------------------------------------------------");
            helpMsg.AppendLine("summary or summ:  Today's Summary");
            helpMsg.AppendLine("sales:  Today's Sales");
            helpMsg.AppendLine("cback:  Pending Chargebacks");
            helpMsg.AppendLine("returns or ret:  Today's Returns");
            helpMsg.AppendLine("stop:  Unsubscribe");
            helpMsg.AppendLine("faf:  Sign-up for Fast Access");
            helpMsg.AppendLine("");
            helpMsg.AppendLine("help?:  this list");
            helpMsg.AppendLine("join:  resend welcome message");
            helpMsg.AppendLine("Settings:  view/update alert settings");
            helpMsg.AppendLine("User:  Account Details");
            helpMsg.AppendLine("");
            helpMsg.AppendLine("To see this list again at any time, text help? or ??? back to me.");

            return (helpMsg.ToString());
        }
    }
}
