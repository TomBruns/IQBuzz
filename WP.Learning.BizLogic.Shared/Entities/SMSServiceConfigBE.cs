using System;
using System.Collections.Generic;
using System.Text;

namespace WP.Learning.BizLogic.Shared.Entities
{
    /// <summary>
    /// This class is the necessary config info to use Trilio
    /// </summary>
    public class SMSServiceConfigBE
    {
        public string account_sid { get; set; }
        public string auth_token { get; set; }
        public string phone_number { get; set; }
    }
}
