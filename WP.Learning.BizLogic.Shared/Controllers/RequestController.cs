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
    /// This class provides some methods used to send SMS messages via Twilio
    /// </summary>
    public static class RequestController
    {
        public static List<string> ProcessIncommingText(string fromPhoneNo, string requestBody)
        {
            List<string> responseMsg = new List<string>();

            // lookup the merchant using the incoming phone number
            IQBuzzUserBE user = UserController.FindIQBuzzUser(fromPhoneNo);

            // make sure the phone number is associated with a registered user
            if (user == null)
            {
                string msg = $"Sorry :) Phone #: {fromPhoneNo} is not setup to use {GeneralConstants.APP_NAME}.  Please contact the {GeneralConstants.APP_NAME} Hackathon team to become a beta tester.";

                UserController.LogUserActivity(fromPhoneNo, requestBody, DateTime.Now, new List<string>() { msg });

                responseMsg.Add(msg);
            }

            // ===================================================
            // Logic to recognize all of the supported commands
            // ===================================================
            if (requestBody == @"join")    // user requests to join
            {
                responseMsg.Add(UserController.BuildWelcomeMessage(user));
            }
            else if (requestBody == @"yes")     // welcome accept
            {
                string welcomeAcceptMsg = UserController.StoreAcceptWelcomeMessageResponse(user.user_id, true);
                responseMsg.Add(welcomeAcceptMsg);
                //string configMsg = UserController.BuildConfigMessage(user.user_id);
                //responseMsg.Add($"{welcomeAcceptMsg}\n{configMsg}");

                //string addContactMsg = UserController.BuildSaveContactMessage(user.user_id);
                responseMsg.Add(BuildHelpMessage());

                string helpReminderMsg = $"\nTo see this list again at any time, text HELP? or ??? back to me.";
                responseMsg.Add(helpReminderMsg);
            }
            else if (!user.has_accepted_welcome_agreement)
            {
                responseMsg.Add($"You must accept the Terms&Conditions before using {GeneralConstants.APP_NAME}, Text JOIN to do that.");
            }
            // =====================================================
            // Everything below here requires you to have accepted the T&C first
            // =====================================================
            else if (requestBody == @"summary")     // Summary for today
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg.Add(MerchantController.BuildOverallSummaryMessage(user.merchant_ids, xctPostingDate, user.local_time_zone));
            }
            else if (requestBody == @"sales")       // total sales for today
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg.Add(MerchantController.BuildSalesSummaryMessage(user.merchant_ids, xctPostingDate, user.local_time_zone));
            }
            else if (requestBody == @"cback" || requestBody == @"chargeback" || requestBody == @"chargebacks")  // chargebacks for today
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg.Add(MerchantController.BuildChargebackDetails(user.merchant_ids, xctPostingDate, user.local_time_zone));
            }
            else if (requestBody == @"returns" || requestBody == @"refunds")    // returns for today
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg.Add(MerchantController.BuildReturnsSummaryMessage(user.merchant_ids, xctPostingDate, user.local_time_zone));
            }
            else if (requestBody == @"faf")     // fast access funding
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg.Add(MerchantController.BuildFAFMessage(user.merchant_ids[0], xctPostingDate));
            }
            else if (requestBody == @"confirm") // confirm faf request
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg.Add(MerchantController.BuildConfirmFAFMessage(user.merchant_ids[0], xctPostingDate));
            }
            else if (requestBody == @"undo")    // undo faf request
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg.Add(MerchantController.BuildUndoFAFMessage(user.merchant_ids[0], xctPostingDate));
            }
            else if (requestBody == @"unjoin")  // unjoin
            {
                responseMsg.Add(UserController.ResetAcceptedJoin(user.user_id));
            }
            else if (requestBody == @"config" || requestBody == @"settings")    // show user config
            {
                responseMsg.Add(UserController.BuildConfigMessage(user.user_id));
            }
            else if (requestBody == @"user" || requestBody == @"whoami")    // display current user info
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Hi {user.first_name} {user.last_name} | ID: {user.user_id} | Accepted: {user.has_accepted_welcome_agreement}");
                sb.AppendLine($"PhoneNo: {user.phone_no} tz: {user.local_time_zone}");
                sb.AppendLine($"--------------------------------------");
                foreach (var merchant in user.Merchants)
                {
                    sb.AppendLine($"[{merchant.merchant_id}] {merchant.merchant_name}");
                }

                responseMsg.Add(sb.ToString());
            }
            else if (requestBody == @"ver") // display software build info
            {
                // get the d/t this assy was built
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
                DateTime lastModifiedLocal = fileInfo.LastWriteTime;

                TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime lastModifiedLocalEST = TimeZoneInfo.ConvertTime(lastModifiedLocal, estZone);

                responseMsg.Add($"Running {GeneralConstants.APP_NAME} built on: [{lastModifiedLocalEST} EST]");
            }
            else if (requestBody == @"genxcts") // generate random xcts for today
            {
                DateTime xctPostingDate = DateTime.Today;

                int xctGeneratedCount = 0;
                int merchantsCount = 0;
                foreach (var merchant in user.Merchants)
                {
                    merchantsCount++;

                    xctGeneratedCount += MerchantController.GenerateSampleXcts(merchant.merchant_id, xctPostingDate);
                }

                responseMsg.Add($"[{xctGeneratedCount}] random xcts generated across [{merchantsCount}] merchants and posted to {xctPostingDate:M/dd/yyyy}");
            }
            else if (requestBody == @"usage" || requestBody == @"stats")    // display usage stats
            {
                DateTime fromDate = DateTime.Today;
                DateTime asOfLocalDT = DateTime.Now;
                DateTime asOfUserDT = DateTimeUtilities.CovertToUserLocalDT(asOfLocalDT, user.local_time_zone);

                List<UserDailyUsageSummaryBE> usage = UserController.GetUserActivitySummaryByDay(fromDate, user.local_time_zone);

                StringBuilder msg = new StringBuilder();
                msg.AppendLine($"{GeneralConstants.APP_NAME} usage stats: {fromDate.AddDays(-5):MMM d} to {fromDate:MMM d}");
                msg.AppendLine($"  (as of { asOfUserDT.ToString("ddd MMM dd, yyyy h:mm tt")})");
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

                responseMsg.Add(msg.ToString());
            }
            else if (requestBody == @"help"
                        || requestBody == @"help?"
                        || requestBody == @"???"
                        || requestBody == @"?")
            {
                //StringBuilder helpMsg = new StringBuilder();

                //helpMsg.AppendLine(" Available Commands");
                //helpMsg.AppendLine("------------------------------");
                //helpMsg.AppendLine("Summary: Today's Summary");
                //helpMsg.AppendLine("Sales: Today's Sales");
                //helpMsg.AppendLine("Cback: Pending Chargebacks");
                //helpMsg.AppendLine("Returns: Today's Returns");
                //helpMsg.AppendLine("Stop: Unsubscribe");
                //helpMsg.AppendLine("FAF: Sign-up for Fast Access");
                //helpMsg.AppendLine("help?: this list");
                //helpMsg.AppendLine("join: resend welcome message");
                //helpMsg.AppendLine("Settings: view/update alert settings");
                //helpMsg.AppendLine("User: Account Details");

                responseMsg.Add(BuildHelpMessage());
            }
            else if (requestBody == @"help+" || requestBody == @"help*")
            {
                StringBuilder helpMsg = new StringBuilder();

                helpMsg.AppendLine(" Additional Commands");
                helpMsg.AppendLine("------------------------------");
                helpMsg.AppendLine("unjoin: reverse join (for testing)");
                helpMsg.AppendLine("ver: display software build d/t");
                helpMsg.AppendLine("genxcts: generate random xcts");
                helpMsg.AppendLine("usage: display usage stats for last 5 days");

                responseMsg.Add(helpMsg.ToString());
            }
            else
            {
                responseMsg.Add($"Sorry I do not understand [{requestBody}], text help? to see a list of the available commmands.");
            }

            UserController.LogUserActivity(fromPhoneNo, requestBody, DateTime.Now, responseMsg);

            return responseMsg;
        }

        private static string BuildHelpMessage()
        {
            StringBuilder helpMsg = new StringBuilder();

            helpMsg.AppendLine("");
            helpMsg.AppendLine("Here is a list of the commands that I understand:");
            helpMsg.AppendLine("--------------------------------------------------------");
            helpMsg.AppendLine("Summary:  Today's Summary");
            helpMsg.AppendLine("Sales:  Today's Sales");
            helpMsg.AppendLine("Cback:  Pending Chargebacks");
            helpMsg.AppendLine("Returns:  Today's Returns");
            helpMsg.AppendLine("Stop:  Unsubscribe");
            helpMsg.AppendLine("FAF:  Sign-up for Fast Access");
            helpMsg.AppendLine("");
            helpMsg.AppendLine("help?:  this list");
            helpMsg.AppendLine("join:  resend welcome message");
            helpMsg.AppendLine("Settings:  view/update alert settings");
            helpMsg.AppendLine("User:  Account Details");

            return (helpMsg.ToString());
        }
    }
}
