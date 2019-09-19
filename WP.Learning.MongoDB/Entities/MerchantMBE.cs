using System;
using System.Collections.Generic;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using static WP.Learning.MongoDB.Entities.Enums;

namespace WP.Learning.MongoDB.Entities
{
    /// <summary>
    /// This class represents a row (document) in the merchants collection
    /// </summary>
    public class MerchantMBE
    {
        public const string COLLECTION_NAME = @"gfos2.merchants";

        [BsonId]
        public ObjectId ID { get; set; }

        public int merchant_id { get; set; }
        public string merchant_name { get; set; }
        //public ContactMBE primary_contact { get; set; }
        public SetupOptionsMBE setup_options { get; set; }
        public List<TerminalMBE> terminals { get; set; }
    }

    // models a contact (person) for a merchant
    //public class ContactMBE
    //{
    //    public string first_name { get; set; }
    //    public string last_name { get; set; }
    //    public string phone_no { get; set; }
    //    public string email_address { get; set; }
    //    public string local_time_zone { get; set; }
    //}

    // models the current setup options for a merchant
    public class SetupOptionsMBE
    {
        public bool is_host_data_capture_enabled { get; set; }
        // time the auto close should occur (if enabled)
        public TimeSpan? auto_close_hh_mm { get; set; }
        // time to alert if all terminals are not closed with terminal data capture
        public TimeSpan? manual_close_alert_hh_mm { get; set; }
        public bool is_fast_funding_enabled { get; set; }
        public string debit_card_no { get; set; }

        public bool supports_cp_xcts { get; set; }
        public bool supports_cnp_xcts { get; set; }
        public bool supports_returns_xcts { get; set; }
    }
    
    // defines the terminals configured for a merchant
    public class TerminalMBE
    {
        public string terminal_id { get; set; }
        public string terminal_desc { get; set; }
        public string terminal_type { get; set; }
    }

}
