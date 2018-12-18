using System;
using System.Collections.Generic;
using System.Text;
using static WP.Learning.MongoDB.Entities.Enums;

namespace WP.Learning.BizLogic.Shared.Entities
{
    /// <summary>
    /// This class holds a xct summary for a merchant on a date
    /// </summary>
    public class XctDailySummaryBE
    {
        public List<XctTypeDailySummaryBE> SummaryByXctType { get; set; }
    }

    public class XctTypeDailySummaryBE
    {
        public TRANSACTION_TYPE XctType { get; set; }

        public int XctCount { get; set; }

        public decimal XctTotalValue { get; set; }

        public bool isAuthFailure { get; set; }

        public string XctTypeDesc
        {
            get
            {
                switch (XctType)
                {
                    case TRANSACTION_TYPE.chargeback:
                        return @"Chargebacks";
                    case TRANSACTION_TYPE.cnp_sale:
                        return !isAuthFailure ? @"Online Sales" : @"Online Sales (Auth Failure)";
                    case TRANSACTION_TYPE.cp_sale:
                        return !isAuthFailure ? @"In-Store Sales" : @"In-Store Sales (Auth Failure)";
                    case TRANSACTION_TYPE.credit_return:
                        return @"Returns";
                    default:
                        return @"Unknown";
                }
            }
        }
    }
}
