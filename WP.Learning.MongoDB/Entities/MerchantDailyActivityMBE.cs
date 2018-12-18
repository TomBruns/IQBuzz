using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using static WP.Learning.MongoDB.Entities.Enums;

namespace WP.Learning.MongoDB.Entities
{
    /// <summary>
    /// This class represents a row (document) in the merchant_daily_activity collection
    /// </summary>
    public class MerchantDailyActivityMBE
    {
        public const string COLLECTION_NAME = @"gfos2.merchant_daily_activity";

        [BsonId]
        public ObjectId ID { get; set; }

        public int merchant_id { get; set; }
        public DateTime xct_posting_date { get; set; }

        public bool? is_fast_access_funding_enabled { get; set; }
        public List<TransactionMBE> transactions { get; set; }

        public DateTime? open_start_dt { get; set; }
        public DateTime? close_complete_dt { get; set; }
        public List<TerminalStatusMBE> terminals_status { get; set; }
    }

    // defines a payment card transaction
    public class TransactionMBE
    {
        public Guid xct_id { get; set; }
        public DateTime xct_dt { get; set; }
        public string terminal_id { get; set; }
        public PaymentCardDataMBE card_data { get; set; }
        public decimal xct_amount { get; set; }
        public TRANSACTION_TYPE xct_type { get; set; }
        public bool is_Auth_Failed { get; set; }
    }

    // defines the payment card data
    // https://www.magtek.com/content/documentationfiles/d99800004.pdf
    public class PaymentCardDataMBE
    {
        // PAN 16 - 19 digits
        public string primary_account_no { get; set; }

        // 26 alphanumeric characters
        public string name { get; set; }

        // Expiration Dat
        public AddtionalDataMBE additional_data { get; set; }

        public string discretionary_data { get; set; }

    }

    public class AddtionalDataMBE
    {
        // YYMM
        public string expiration_date { get; set; }
        // 3 alphanumeric characters
        public string service_code { get; set; }
    }

    public class TerminalStatusMBE
    {
        public string terminal_id { get; set; }
        public DateTime? open_dt { get; set; }
        public DateTime? close_dt { get; set; }
    }
}
