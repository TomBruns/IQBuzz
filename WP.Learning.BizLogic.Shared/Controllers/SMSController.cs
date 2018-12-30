﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Twilio;
using Twilio.Rest.Api.V2010.Account;

using WP.Learning.BizLogic.Shared.Entities;
using WP.Learning.MongoDB;
using WP.Learning.MongoDB.Entities;

namespace WP.Learning.BizLogic.Shared.Controllers
{
    public static class SMSController
    {
        /// <summary>
        /// Send a Test SMS Message
        /// </summary>
        /// <param name="toPhoneNumber"></param>
        /// <returns></returns>
        public static string SendTestSMSMessage(string toPhoneNumber)
        {
            // ex: toPhoneNumber  => @"+15134986016"

            return SendSMSMessage(toPhoneNumber, $"{GeneralConstants.APP_NAME} Test Message.");
        }

        /// <summary>
        /// Send (Push) a SMS (Text) message to a phone number using Twilio
        /// </summary>
        /// <param name="toPhoneNumber"></param>
        /// <param name="messageBody"></param>
        public static string SendSMSMessage(string toPhoneNumber, string messageBody)
        {
            // ex: toPhoneNumber  => @"+15134986016"

            // Find your Account Sid and Token at twilio.com/console
            var smsConfig = GetTwilioConfig();

            // init the client
            TwilioClient.Init(smsConfig.account_sid, smsConfig.auth_token);

            // create and send a SMS message
            var message = MessageResource.Create(
                body: messageBody,
                from: new Twilio.Types.PhoneNumber(smsConfig.phone_number),
                to: new Twilio.Types.PhoneNumber(toPhoneNumber)
            );

            return message.Sid;
        }

        /// <summary>
        /// Retrieve the Trilio config info from the DB
        /// </summary>
        /// <returns></returns>
        private static SMSServiceConfigBE GetTwilioConfig()
        {
            var allConfigData = MongoDBContext.GetAllConfigData();

            SMSServiceConfigBE smsServiceConfig = new SMSServiceConfigBE()
            {
                account_sid = allConfigData.Where(cd => cd.name == Constants.ACCOUNT_SID_ITEM_NAME).FirstOrDefault().value,
                auth_token = allConfigData.Where(cd => cd.name == Constants.AUTH_TOKEN_ITEM_NAME).FirstOrDefault().value,
                phone_number = allConfigData.Where(cd => cd.name == Constants.PHONE_NUMBER_ITEM_NAME).FirstOrDefault().value
            };

            return smsServiceConfig;
        }
    }
}