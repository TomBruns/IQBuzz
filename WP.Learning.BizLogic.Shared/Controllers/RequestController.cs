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
        public static string ProcessIncommingText(string fromPhoneNo, string requestBody)
        {
            string responseMsg = string.Empty;

            // lookup the merchant using the incoming phone number
            IQBuzzUserBE user = UserController.FindIQBuzzUser(fromPhoneNo);

            // make sure the phone number is associated with a registered user
            if (user == null)
            {
                responseMsg = $"Sorry :) Phone #: {fromPhoneNo} is not setup to use {GeneralConstants.APP_NAME}.  Please contact the {GeneralConstants.APP_NAME} Hackathon team to become a beta tester.";

                UserController.LogUserActivity(fromPhoneNo, requestBody, DateTime.Now, responseMsg);

                return responseMsg;
            }

            // ===================================================
            // Logic to recognize all of the supported commands
            // ===================================================
            if (requestBody == @"join")    // user requests to join
            {
                responseMsg = UserController.BuildWelcomeMessage(user);
            }
            else if (requestBody == @"yes")     // welcome accept
            {
                string welcomeAcceptMsg = UserController.StoreAcceptWelcomeMessageResponse(user.user_id, true);
                string configMsg = UserController.BuildConfigMessage(user.user_id);
                responseMsg = $"{welcomeAcceptMsg}\n{configMsg}";
            }
            else if (!user.has_accepted_welcome_agreement)
            {
                responseMsg = $"You must accept the Terms&Conditions before using {GeneralConstants.APP_NAME}, Text JOIN to do that.";
            }
            // =====================================================
            // Everything below here requires you to have accepted the T&C first
            // =====================================================
            else if (requestBody == @"summary")     // Summary for today
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg = MerchantController.BuildOverallSummaryMessage(user.merchant_ids[0], xctPostingDate, user.local_time_zone);
            }
            else if (requestBody == @"sales")       // total sales for today
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg = MerchantController.BuildSalesSummaryMessage(user.merchant_ids[0], xctPostingDate, user.local_time_zone);
            }
            else if (requestBody == @"cback" || requestBody == @"chargeback" || requestBody == @"chargebacks")  // chargebacks for today
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg = MerchantController.BuildChargebackDetails(user.merchant_ids[0], xctPostingDate, user.local_time_zone);
            }
            else if (requestBody == @"returns" || requestBody == @"refunds")    // returns for today
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg = MerchantController.BuildReturnsSummaryMessage(user.merchant_ids[0], xctPostingDate, user.local_time_zone);
            }
            else if (requestBody == @"faf")     // fast access funding
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg = MerchantController.BuildFAFMessage(user.merchant_ids[0], xctPostingDate);
            }
            else if (requestBody == @"confirm") // confirm faf request
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg = MerchantController.BuildConfirmFAFMessage(user.merchant_ids[0], xctPostingDate);
            }
            else if (requestBody == @"undo")    // undo faf request
            {
                DateTime xctPostingDate = DateTime.Today;

                responseMsg = MerchantController.BuildUndoFAFMessage(user.merchant_ids[0], xctPostingDate);
            }
            else if (requestBody == @"unjoin")  // unjoin
            {
                responseMsg = UserController.ResetAcceptedJoin(user.user_id);
            }
            else if (requestBody == @"config" || requestBody == @"settings")    // show user config
            {
                responseMsg = UserController.BuildConfigMessage(user.user_id);
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

                responseMsg = sb.ToString();
            }
            else if (requestBody == @"ver") // display software build info
            {
                // get the d/t this assy was built
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
                DateTime lastModifiedLocal = fileInfo.LastWriteTime;

                TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime lastModifiedLocalEST = TimeZoneInfo.ConvertTime(lastModifiedLocal, estZone);

                responseMsg = $"Running {GeneralConstants.APP_NAME} built on: [{lastModifiedLocalEST} EST]";
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

                responseMsg = $"[{xctGeneratedCount}] random xcts generated across [{merchantsCount}] merchants and posted to {xctPostingDate:M/dd/yyyy}";
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

                var users = usage.OrderBy(u => u.IQBuzzUser.user_id).ToList();
                    
                foreach (var userInLoop in users)
                {
                    msg.Append($"[{userInLoop.IQBuzzUser.user_id}] {userInLoop.IQBuzzUser.FullName} |");

                    for (DateTime activityDate = fromDate.AddDays(-4); activityDate <= fromDate; activityDate = activityDate.AddDays(1))
                    {
                        var activityOnDate = usage.Where(u => u.PhoneNo == userInLoop.PhoneNo && u.ActivityDate == activityDate).FirstOrDefault();

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

                responseMsg = msg.ToString();
            }
            else if (requestBody == @"help"
                        || requestBody == @"help?"
                        || requestBody == @"???"
                        || requestBody == @"?")
            {
                StringBuilder helpMsg = new StringBuilder();

                helpMsg.AppendLine(" Available Commands");
                helpMsg.AppendLine("------------------------------");
                helpMsg.AppendLine("Summary: Today's Summary");
                helpMsg.AppendLine("Sales: Today's Sales");
                helpMsg.AppendLine("Cback: Pending Chargebacks");
                helpMsg.AppendLine("Returns: Today's Returns");
                helpMsg.AppendLine("Stop: Unsubscribe");
                helpMsg.AppendLine("FAF: Sign-up for Fast Access");
                helpMsg.AppendLine("help?: this list");
                helpMsg.AppendLine("join: resend welcome message");
                helpMsg.AppendLine("Settings: view/update alert settings");
                helpMsg.AppendLine("User: Account Details");

                responseMsg = helpMsg.ToString();
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

                responseMsg = helpMsg.ToString();
            }
            else
            {
                responseMsg = $"Sorry I do not understand [{requestBody}], text help? to see a list of the available commmands.";
            }

            UserController.LogUserActivity(fromPhoneNo, requestBody, DateTime.Now, responseMsg);

            return responseMsg;
        }
    }
}
