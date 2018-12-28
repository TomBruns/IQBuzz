using System;
using System.Collections.Generic;
using System.Text;

using WP.Learning.BizLogic.Shared.Utilties;
using WP.Learning.MongoDB.Entities;

namespace WP.Learning.BizLogic.Shared.Entities
{
    public class IQBuzzUserBE : IQBuzzUserMBE
    {

        public IQBuzzUserBE()
        {
            this.Merchants = new List<MerchantMBE>();
        }

        public string FullName
        {
            get { return $"{this.first_name} {this.last_name}";  }
        }

        public List<MerchantMBE> Merchants { get; set; }

        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                return DateTimeUtilities.GetTimeZoneInfo(this.local_time_zone.ToUpper());
            }
        }
    }
}
