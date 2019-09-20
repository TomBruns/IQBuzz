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
        public XctDailySummaryBE()
        {
            this.CPSalesSummary = new XctTypeDailySummaryBE();
            this.CNPSalesSummary = new XctTypeDailySummaryBE();
            this.CPReturnsSummary = new XctTypeDailySummaryBE();
            this.CNPReturnsSummary = new XctTypeDailySummaryBE();
            this.ChargebacksSummary = new XctTypeDailySummaryBE();
        }

        public XctTypeDailySummaryBE CPSalesSummary { get; set; }

        public XctTypeDailySummaryBE CNPSalesSummary { get; set; }

        public XctTypeDailySummaryBE CPReturnsSummary { get; set; }

        public XctTypeDailySummaryBE CNPReturnsSummary { get; set; }

        public XctTypeDailySummaryBE ChargebacksSummary { get; set; }
    }

    public class XctTypeDailySummaryBE
    {
        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public TRANSACTION_TYPE XctType { get; set; }

        public int SuccessXctCount { get; set; }

        public decimal SuccessXctSubtotalValue { get; set; }

        public int FailureXctCount { get; set; }

        public decimal FailureXctSubtotalValue { get; set; }

        public string XctTypeDesc
        {
            get
            {
                switch (XctType)
                {
                    case TRANSACTION_TYPE.chargeback:
                        return @"Chargebacks";
                    case TRANSACTION_TYPE.cnp_sale:
                        return @"Online Sales";
                    case TRANSACTION_TYPE.cp_sale:
                        return @"In-Store Sales";
                    case TRANSACTION_TYPE.cp_return:
                        return @"In-store Returns";
                    case TRANSACTION_TYPE.cnp_return:
                        return @"Online Returns";
                    default:
                        return @"Unknown";
                }
            }
        }
    }
}
