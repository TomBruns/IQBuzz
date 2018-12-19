using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.TwiML;
using Twilio.TwiML.Messaging;
using WP.Learning.BizLogic.Shared;
using WP.Learning.BizLogic.Shared.Entities;
using WP.Learning.BizLogic.Shared.Merchant;
using WP.Learning.BizLogic.Shared.User;
using WP.Learning.MongoDB.Entities;

namespace TwilioReceive.Controllers
{
    public class SmsController : TwilioController
    {
        [HttpPost]
        public TwiMLResult Index(SmsRequest incomingMessage)
        {
            // fyi: the format of sending phone no is: "+15134986016"
            string fromPhoneNumber = incomingMessage.From;

            // pull out the command that was texted
            string requestBody = incomingMessage.Body.ToLower().Trim();

            //string requestBody2 = Regex.Replace(requestBody, @"\s+", string.Empty);
            //if (requestBody2.StartsWith(@"saleshttps://"))
            //{
            //    requestBody = "sales_alexa";
            //}

            // create an object to hold the response
            var response = new MessagingResponse();

            // lookup the merchant using the incoming phone number
            MerchantMBE merchant = MerchantController.LookupMerchant(fromPhoneNumber);

            // make sure the phone number is associated with at least 1 merchant
            if(merchant == null)
            {
                response.Message($"Sorry :) Phone #: {fromPhoneNumber} is not setup to use {GeneralConstants.APP_NAME}.  Please contact the {GeneralConstants.APP_NAME} Hackathon team to become a beta tester.");
                return TwiML(response);
            }

            // ===================================================
            // Logic to recognize all of the supported commands
            // ===================================================
            if (requestBody == @"join")    // user requests to join
            {
                string welcomeMsg = MerchantController.BuildWelcomeMessage(merchant);

                UserController.LogUserActivity(fromPhoneNumber, requestBody, DateTime .Now, welcomeMsg);

                response.Message(welcomeMsg);
            }
            else if (requestBody == @"yes")     // welcome accept
            {
                string welcomeAcceptMsg = MerchantController.StoreAcceptWelcomeMessageResponse(merchant.merchant_id, true);
                string configMsg = MerchantController.BuildConfigMessage(merchant.merchant_id);

                response.Message($"{welcomeAcceptMsg}\n{configMsg}");
            }
            else if (!merchant.setup_options.is_accepted_welcome_agreement)
            {
                response.Message($"You must accept the Terms&Conditions before using {GeneralConstants.APP_NAME}, Text JOIN to do that.");
            }
            // =====================================================
            // Everything below here requires you to have accepted the T&C first
            // =====================================================
            else if (requestBody == @"summary")     // Summary for today
            {
                DateTime xctPostingDate = DateTime.Today;

                string salesInfo = MerchantController.BuildOverallSummaryMessage(merchant.merchant_id, xctPostingDate);

                response.Message(salesInfo);
            }
            else if (requestBody == @"sales")       // total sales for today
            {
                DateTime xctPostingDate = DateTime.Today;

                string salesInfo = MerchantController.BuildSalesSummaryMessage(merchant.merchant_id, xctPostingDate);

                response.Message(salesInfo);
            }
            else if (requestBody == @"cback" || requestBody == @"chargeback" || requestBody == @"chargebacks")  // chargebacks for today
            {
                DateTime xctPostingDate = DateTime.Today;

                string salesInfo = MerchantController.BuildChargebackDetails(merchant.merchant_id, xctPostingDate);

                response.Message(salesInfo);
            }
            else if (requestBody == @"returns" || requestBody == @"refunds")    // returns for today
            {
                DateTime xctPostingDate = DateTime.Today;

                string refundsInfo = MerchantController.BuildReturnsSummaryMessage(merchant.merchant_id, xctPostingDate);

                response.Message(refundsInfo);
            }
            else if (requestBody == @"faf")     // fast access funding
            {
                DateTime xctPostingDate = DateTime.Today;

                string faf = MerchantController.BuildFAFMessage(merchant.merchant_id, xctPostingDate);

                response.Message(faf);
            }
            else if (requestBody == @"confirm") // confirm faf request
            {
                DateTime xctPostingDate = DateTime.Today;
                string fafMsg = MerchantController.BuildConfirmFAFMessage(merchant.merchant_id, xctPostingDate);

                response.Message(fafMsg);
            }
            else if (requestBody == @"undo")    // undo faf request
            {
                DateTime xctPostingDate = DateTime.Today;
                string fafMsg = MerchantController.BuildUndoFAFMessage(merchant.merchant_id, xctPostingDate);
                response.Message(fafMsg);
            }
            else if (requestBody == @"unjoin")  // unjoin
            {
                string unjoinMsg = MerchantController.ResetAcceptedJoin(merchant.merchant_id);

                response.Message(unjoinMsg);
            }
            else if (requestBody == @"config" || requestBody == @"settings")    // show user config
            {
                string msg = MerchantController.BuildConfigMessage(merchant.merchant_id);
                response.Message(msg);

            }
            else if (requestBody == @"user" || requestBody == @"whoami")    // display current user info
            {
                string msg = $"Hi {merchant.primary_contact.first_name} !\n{merchant.merchant_name} [id: {merchant.merchant_id}]\np: {merchant.primary_contact.phone_no}";
                response.Message(msg);
            }
            //else if (requestBody == @"sales_alexa")
            //{
            //    response.Message("Getting Closer to WOW");
            //}
            else if (requestBody == @"ver") // display software build info
            {
                // get the d/t this assy was built
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
                DateTime lastModifiedLocal = fileInfo.LastWriteTime;

                TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime lastModifiedLocalEST = TimeZoneInfo.ConvertTime(lastModifiedLocal, estZone);

                response.Message($"{GeneralConstants.APP_NAME} Built on: [{lastModifiedLocalEST}] EST");
            }
            else if (requestBody == @"genxcts") // generate random xcts for today
            {
                DateTime xctPostingDate = DateTime.Today;
                int xctGeneratedCount = MerchantController.GenerateSampleXcts(merchant.merchant_id, xctPostingDate);

                string msg = $"[{xctGeneratedCount}] random xcts generated and posted to {xctPostingDate:M/dd/yyyy}";
                response.Message(msg);
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

                response.Message(helpMsg.ToString());
            }
            else if (requestBody == @"help*")
            {
                StringBuilder helpMsg = new StringBuilder();

                helpMsg.AppendLine(" Additional Commands");
                helpMsg.AppendLine("------------------------------");
                helpMsg.AppendLine("unjoin: reverse join (for testing)");
                helpMsg.AppendLine("ver: display software build d/t");
                helpMsg.AppendLine("genxtcs: generate random xcts");

                response.Message(helpMsg.ToString());
            }
            else
            {
                response.Message($"Sorry I do not understand [{requestBody}], text help? to see a list of the available commmands.");
            }

            return TwiML(response);
        }
    }
}