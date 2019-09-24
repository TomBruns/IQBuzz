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
    public static class BatchController
    {
        public static string BuildBatchMissingMessage(int userId, int merchantId)
        {
            var merchant = MongoDBContext.FindMerchantById(merchantId);
            var user = MongoDBContext.FindIQBuzzUser(userId);

            StringBuilder sb = new StringBuilder();


            sb.AppendLine($"Hi {user.first_name}! We have not received any new transaction batches from you since {DateTime.Now.AddDays(-2)}. If you haven't had any transactions since then, please disregard. Otherwise, we encourage you to batch your transactions at least once daily to avoid issues in processing and settlement.");
            sb.AppendLine($"If you submitted transaction batches since the date above, something may have gone wrong. Please re-submit. If you have any trouble, give us a call at {GeneralConstants.WORLDPAY_CONTACT_CENTER_PHONE_NO}.");

            return sb.ToString();
        }

        public static string BuildBatchReceivedOkMessage(int userId, int merchantId)
        {
            var merchant = MongoDBContext.FindMerchantById(merchantId);
            string maskedAcctNo = $"x{merchant.setup_options.debit_card_no.Right(5)}";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"We've received your transaction batch and are processing it for settlement!");
            sb.AppendLine($"Final amounts will be deposited to your account ending in {maskedAcctNo} in 3 business days, or you can receive these funds tomorrow morning by using FastAccess Funding. Just text FAF to me now.");
            sb.AppendLine();
            sb.AppendLine($"The batch reference number is [{GenBatchNo()}].");

            return sb.ToString();
        }

        public static string BuildBatchReceivedErrorMessage(int userId, int merchantId)
        {
            var merchant = MongoDBContext.FindMerchantById(merchantId);
            var user = MongoDBContext.FindIQBuzzUser(userId);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{user.first_name}, something appears to be wrong with the latest batch you submitted, and we cannot process it appropriately. Please submit the batch again. If you have any trouble, please call us for help - {GeneralConstants.WORLDPAY_CONTACT_CENTER_PHONE_NO}.");
            sb.AppendLine($"The batch reference number is [{GenBatchNo()}]");

            return sb.ToString();
        }

        public static string BuildBatchAutoCloseMessage(int userId, int merchantId)
        {
            var merchant = MongoDBContext.FindMerchantById(merchantId);
            string maskedAcctNo = $"x{merchant.setup_options.debit_card_no.Right(5)}";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"We have closed your {merchant.setup_options.auto_close_hh_mm} batch and we're processing it for settlement.");
            sb.AppendLine($"Final amounts will be deposited to your account ending in {maskedAcctNo} in 3 business days, or you can receive these funds tomorrow morning by using FastAccess Funding. Just text FAF to me now.");
            sb.AppendLine();
            sb.AppendLine($"The batch reference number is [{GenBatchNo()}].");
            
            return sb.ToString();
        }

        #region Test Harness Methods

        public static void SendBatchMissingMessage(int userId)
        {
            var user = MongoDBContext.FindIQBuzzUser(userId);

            string msg = BuildBatchMissingMessage(userId, user.merchant_ids[0]);

            SMSController.SendSMSMessage(user.phone_no, $"{msg}");
        }

        public static void SendBatchReceivedOkMessage(int userId)
        {
            var user = MongoDBContext.FindIQBuzzUser(userId);

            string msg = BuildBatchReceivedOkMessage(userId, user.merchant_ids[0]);

            SMSController.SendSMSMessage(user.phone_no, $"{msg}");
        }

        public static void SendBatchReceivedErrorMessage(int userId)
        {
            var user = MongoDBContext.FindIQBuzzUser(userId);

            string msg = BuildBatchReceivedErrorMessage(userId, user.merchant_ids[0]);

            SMSController.SendSMSMessage(user.phone_no, $"{msg}");
        }

        public static void SendBatchAutoCloseMessage(int userId)
        {
            var user = MongoDBContext.FindIQBuzzUser(userId);

            string msg = BuildBatchAutoCloseMessage(userId, user.merchant_ids[0]);

            SMSController.SendSMSMessage(user.phone_no, $"{msg}");
        }

        #endregion
        private static string GenBatchNo()
        {
            string bacthNo = DateTime.Now.ToString("yyyyMMdd_hhmmss");

            return bacthNo;
        }
    }
}
