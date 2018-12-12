using System;
using System.Collections.Generic;
using System.Text;

namespace WP.Learning.MongoDB.Utilities
{
    /// <summary>
    /// General Merchant Utilities
    /// </summary>
    public static class MerchantUtilities
    {
        public static DateTime DetermineActivityDate(int merchant_id, DateTime xct_dt)
        {
            DateTime activityDate = DateTime.Today;

            var merchant = MongoDBContext.FindMerchantById(merchant_id);

            if(merchant.setup_options.is_host_data_capture_enabled)
            {
                TimeSpan xct_dt_ts = new TimeSpan(xct_dt.Hour, xct_dt.Minute, xct_dt.Second);
                if(xct_dt_ts > merchant.setup_options.auto_close_hh_mm.Value)
                {
                    activityDate = activityDate.AddDays(1);
                }
            }

            return activityDate;
        }
    }
}
