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
using WP.Learning.MongoDB.Entities;

namespace TwilioReceive.Controllers
{
    public class SmsController : TwilioController
    {
        [HttpPost]
        public TwiMLResult Index(SmsRequest incomingMessage)
        {
            string requestBody = incomingMessage.Body.ToLower().Trim();

            //string requestBody2 = Regex.Replace(requestBody, @"\s+", string.Empty);
            //if (requestBody2.StartsWith(@"saleshttps://"))
            //{
            //    requestBody = "sales_alexa";
            //}

            // format of sending phone no is: "+15134986016"
            string fromPhoneNumber = incomingMessage.From;

            var response = new MessagingResponse();

            // lookup the merchant using the incoming phone number
            MerchantMBE merchant = MerchantController.LookupMerchant(fromPhoneNumber);

            // make sure this is a valid phone #
            if(merchant == null)
            {
                response.Message($"HTTP 404 :) Phone #: {fromPhoneNumber} is not associated with a Merchant record.");
                return TwiML(response);
            }

            if (requestBody == @"help"
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
                //helpMsg.AppendLine("unjoin: reverse join (for testing)");
                helpMsg.AppendLine("Settings: view/update alert settings");
                helpMsg.AppendLine("User: Account Details");
                response.Message(helpMsg.ToString());
            }
            else if (requestBody == @"summary")
            {
                DateTime xctPostingDate = DateTime.Today;

                string salesInfo = MerchantController.BuildOverallSummaryMessage(merchant.merchant_id, xctPostingDate);

                response.Message(salesInfo);
            }
            else if (requestBody == @"sales")
            {
                DateTime xctPostingDate = DateTime.Today;

                string salesInfo = MerchantController.BuildSalesSummaryMessage(merchant.merchant_id, xctPostingDate);

                response.Message(salesInfo);
            }
            else if (requestBody == @"cback" || requestBody == @"chargeback" || requestBody == @"chargebacks")
            {
                DateTime xctPostingDate = DateTime.Today;

                string salesInfo = MerchantController.BuildChargebackDetails(merchant.merchant_id, xctPostingDate);

                response.Message(salesInfo);
            }
            else if (requestBody == @"returns" || requestBody == @"refunds")
            {
                DateTime xctPostingDate = DateTime.Today;

                string refundsInfo = MerchantController.BuildReturnsSummaryMessage(merchant.merchant_id, xctPostingDate);

                response.Message(refundsInfo);
            }
            else if (requestBody == @"faf")
            {
                DateTime xctPostingDate = DateTime.Today;

                string faf = MerchantController.BuildFAFMessage(merchant.merchant_id, xctPostingDate);

                response.Message(faf);
            }
            else if (requestBody == @"confirm")
            {
                DateTime xctPostingDate = DateTime.Today;
                string fafMsg = MerchantController.BuildConfirmFAFMessage(merchant.merchant_id, xctPostingDate);
                response.Message(fafMsg);
            }
            else if (requestBody == @"undo")
            {
                DateTime xctPostingDate = DateTime.Today;
                string fafMsg = MerchantController.BuildUndoFAFMessage(merchant.merchant_id, xctPostingDate);
                response.Message(fafMsg);
            }
            else if (requestBody == @"join")
            {
                string welcomeMsg = MerchantController.BuildWelcomeMessage(merchant);
                string configMsg = MerchantController.BuildConfigMessage(merchant.merchant_id);

                response.Message($"{welcomeMsg}\n{configMsg}");
            }
            else if (requestBody == @"unjoin")
            {
                string unjoinMsg = MerchantController.ResetAcceptedJoin(merchant.merchant_id);

                response.Message(unjoinMsg);
            }
            else if (requestBody == @"config" || requestBody == @"settings")
            {
                string msg = MerchantController.BuildConfigMessage(merchant.merchant_id);
                response.Message(msg);

            }
            else if (requestBody == @"user" || requestBody == @"whoami")
            {
                string msg = $"Hi {merchant.primary_contact.first_name} !\n{merchant.merchant_name} [id: {merchant.merchant_id}]\np: {merchant.primary_contact.phone_no}";
                response.Message(msg);
            }
            else if (requestBody == @"sales_alexa")
            {
                response.Message("Getting Closer to WOW");
            }
            else if (requestBody == @"yes") // welcome accept
            {
                string msg = MerchantController.StoreAcceptWelcomeMessageResponse(merchant.merchant_id, true);
                response.Message(msg);
            }
            else if (requestBody == @"status")
            {
                response.Message(@"not closed");
            }
            else if (requestBody == @"testurl")
            {
                string msg = @"https://www.google.com";
                response.Message(msg);
            }
            else if (requestBody == @"testnew")
            {
                string welcomeMsg = MerchantController.BuildWelcomeMessage(merchant);
                response.Message(welcomeMsg);
            }
            //else if (requestBody == @"testaccept")
            //{
            //    string welcomeMsg = MerchantController.TestWelcomeAccept(merchant.merchant_id);
            //    response.Message(welcomeMsg);
            //}
            else if (requestBody == @"ver")
            {
                // get the d/t this assy was built
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
                DateTime lastModifiedLocal = fileInfo.LastWriteTime;
                //TimeZoneInfo localZone = TimeZoneInfo.Local;
                TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime lastModifiedLocalEST = TimeZoneInfo.ConvertTime(lastModifiedLocal, estZone);

                response.Message($"{GeneralConstants.APP_NAME} Build: [{lastModifiedLocalEST}] ");
            }
            else
            {
                response.Message($"Sorry I do not understand [{requestBody}], text help? to see a list of the available commmands.");
            }

            return TwiML(response);
        }
    }
}