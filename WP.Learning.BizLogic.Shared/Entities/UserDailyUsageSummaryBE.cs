using System;
using System.Collections.Generic;
using System.Text;

namespace WP.Learning.BizLogic.Shared.Entities
{
    public class UserDailyUsageSummaryBE
    {
        public string PhoneNo { get; set; }

        public DateTime ActivityDate { get; set; }

        public int ActionQty { get; set; }

        public IQBuzzUserBE IQBuzzUser { get; set; }
    }
}
