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

using WP.Learning.BizLogic.Shared.Controllers;

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

            string responseText = string.Empty;

            // ===================================================
            // Logic to recognize all of the supported commands
            // ===================================================
            if (requestBody == @"ping")   
            {
                responseText = @"icmp echo";
            }
            else
            {
                // process the request
                responseText = RequestController.ProcessIncommingText(fromPhoneNumber, requestBody);
            }

            // build the response
            var response = new MessagingResponse();
            response.Message(responseText);

            // return the response
            return TwiML(response);
        }
    }
}