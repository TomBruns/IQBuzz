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
    /// <summary>
    /// This class exposes all of the Merchant oriented functionality
    /// </summary>
    public static class MerchantController
    {
        // this is private cache of random payment cards used when we generate synthetic transactions
        private static List<PaymentCardDataMBE> _paymentCards;

        // static constructor
        static MerchantController()
        {
            // preload some sample credit cards
            _paymentCards = new List<PaymentCardDataMBE>()
            {
                new PaymentCardDataMBE()
                {
                    primary_account_no = @"4111111111111111",
                    name = @"Visa User 1",
                    additional_data =new AddtionalDataMBE()
                    {
                        expiration_date = @"2001",  // YYMM
                        service_code = @"XYZ"
                    },
                    discretionary_data = @"Foo"
                },
                 new PaymentCardDataMBE()
                {
                    primary_account_no = @"4012888888881881",
                    name = @"Visa User 2",
                    additional_data =new AddtionalDataMBE()
                    {
                        expiration_date = @"2005",  // YYMM
                        service_code = @"KLM"
                    },
                    discretionary_data = @"Foo"
                },
                new PaymentCardDataMBE()
                {
                    primary_account_no = @"5555555555554444",
                    name = @"Mastercard User 1",
                    additional_data =new AddtionalDataMBE()
                    {
                        expiration_date = @"2106",  // YYMM
                        service_code = @"ABC"
                    },
                    discretionary_data = @"Bar"
                },
                 new PaymentCardDataMBE()
                {
                    primary_account_no = @"5105105105105100",
                    name = @"Mastercard User 2",
                    additional_data =new AddtionalDataMBE()
                    {
                        expiration_date = @"2006",  // YYMM
                        service_code = @"ABC"
                    },
                    discretionary_data = @"Oreo"
                },
                new PaymentCardDataMBE()
                {
                    primary_account_no = @"378282246310005",
                    name = @"AmEx User 1",
                    additional_data =new AddtionalDataMBE()
                    {
                        expiration_date = @"2212",  // YYMM
                        service_code = @"DEF"
                    },
                    discretionary_data = @"42"
                },
                new PaymentCardDataMBE()
                {
                    primary_account_no = @"371449635398431",
                    name = @"AmEx User 2",
                    additional_data =new AddtionalDataMBE()
                    {
                        expiration_date = @"2212",  // YYMM
                        service_code = @"DEF"
                    },
                    discretionary_data = @"42"
                },
                new PaymentCardDataMBE()
                {
                    primary_account_no = @"6011111111111117",
                    name = @"Discover 1",
                    additional_data =new AddtionalDataMBE()
                    {
                        expiration_date = @"2212",  // YYMM
                        service_code = @"DEF"
                    },
                    discretionary_data = @"42"
                },
                new PaymentCardDataMBE()
                {
                    primary_account_no = @"6011000990139424",
                    name = @"Discover 2",
                    additional_data =new AddtionalDataMBE()
                    {
                        expiration_date = @"2212",  // YYMM
                        service_code = @"DEF"
                    },
                    discretionary_data = @"42"
                }
            };
        }

        // Command: Summary 
        public static string BuildOverallSummaryMessage(List<int> merchantIds, DateTime xctPostingDateUTC, string userTZCode)
        {
            // get summaries for a list of merchants
            List<XctDailySummaryBE> xctDailySummaries = MerchantController.GetXctDailySummaries(merchantIds, xctPostingDateUTC);

            DateTime xctPostingDateUser = DateTimeUtilities.CovertToUserLocalDT(xctPostingDateUTC, userTZCode);
            DateTime currentUserDT = DateTimeUtilities.CovertToUserLocalDT(DateTime.Now.ToUniversalTime(), userTZCode);
            string currentUserTimeText = $"{currentUserDT.ToString("h:mm tt")} {userTZCode}";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Merchant Account Summary for: {xctPostingDateUser:ddd MMM dd, yyyy} as of: {currentUserTimeText}");
            sb.AppendLine("---------------------------------------------------------");
            sb.AppendLine();
            if (merchantIds.Count > 1) { sb.AppendLine($"For all {merchantIds.Count} stores and online:"); }
            else { sb.AppendLine("For store and online:"); }
            sb.AppendLine();

            if (xctDailySummaries != null && xctDailySummaries.Count > 0)
            {
                decimal subtotalSales = 0.0M;
                decimal subtotalDeniedSales = 0.0M;
                decimal subtotalReturns = 0.0M;
                decimal subtotalDeniedReturns = 0.0M;
                decimal subtotalChargebacks = 0.0M;

                int approvedSalesXctsCount = 0;
                int approvedReturnsXctsCount = 0;
                int deniedSalesXctsCount = 0;
                int deniedReturnsXctsCount = 0;
                int countChargebacks = 0;

                // calc summary across all associated merchants
                foreach (var xctDailySummary in xctDailySummaries)
                {
                    // cp sales
                    if (xctDailySummary.CPSalesSummary != null)
                    {
                        subtotalSales += xctDailySummary.CPSalesSummary.SuccessXctSubtotalValue;
                        approvedSalesXctsCount += xctDailySummary.CPSalesSummary.SuccessXctCount;
                        subtotalDeniedSales += xctDailySummary.CPSalesSummary.FailureXctSubtotalValue;
                        deniedSalesXctsCount += xctDailySummary.CPSalesSummary.FailureXctCount;
                    }

                    // cnp sales
                    if (xctDailySummary.CNPSalesSummary != null)
                    {
                        subtotalSales += xctDailySummary.CNPSalesSummary.SuccessXctSubtotalValue;
                        approvedSalesXctsCount += xctDailySummary.CNPSalesSummary.SuccessXctCount;
                        subtotalDeniedSales += xctDailySummary.CNPSalesSummary.FailureXctSubtotalValue;
                        deniedSalesXctsCount += xctDailySummary.CNPSalesSummary.FailureXctCount;
                    }

                    // cp returns
                    if (xctDailySummary.CPReturnsSummary != null)
                    {
                        subtotalReturns += xctDailySummary.CPReturnsSummary.SuccessXctSubtotalValue;
                        approvedReturnsXctsCount += xctDailySummary.CPReturnsSummary.SuccessXctCount;
                        subtotalDeniedReturns += xctDailySummary.CPReturnsSummary.FailureXctSubtotalValue;
                        deniedReturnsXctsCount += xctDailySummary.CPReturnsSummary.FailureXctCount;
                    }

                    // cnp returns
                    if (xctDailySummary.CNPReturnsSummary != null)
                    {
                        subtotalReturns += xctDailySummary.CNPReturnsSummary.SuccessXctSubtotalValue;
                        approvedReturnsXctsCount += xctDailySummary.CNPReturnsSummary.SuccessXctCount;
                        subtotalDeniedReturns += xctDailySummary.CNPReturnsSummary.FailureXctSubtotalValue;
                        deniedReturnsXctsCount += xctDailySummary.CNPReturnsSummary.FailureXctCount;
                    }

                    // chargebacks
                    if (xctDailySummary.ChargebacksSummary != null)
                    {
                        subtotalChargebacks += xctDailySummary.ChargebacksSummary.SuccessXctSubtotalValue;
                        countChargebacks += xctDailySummary.ChargebacksSummary.SuccessXctCount;
                    }
                }

                sb.AppendLine($"Total Sales:  {subtotalSales:C} ({approvedSalesXctsCount} transactions)");
                sb.AppendLine($"Total Returns:  {subtotalReturns:C} ({approvedReturnsXctsCount} transactions)");
                sb.AppendLine($"Net Volume:  {(subtotalSales + subtotalReturns):C}");
                sb.AppendLine();
                sb.AppendLine("--Other Account Activity--");
                sb.AppendLine();
                sb.AppendLine("Chargebacks:");
                sb.AppendLine($"Total Chargebacks: {subtotalChargebacks:C} ({countChargebacks} cases)");
                sb.AppendLine();

                decimal salesAuthPercentage = (approvedSalesXctsCount + deniedSalesXctsCount > 0) 
                                                ? Decimal.Divide(approvedSalesXctsCount, (approvedSalesXctsCount + deniedSalesXctsCount)) 
                                                : 0.0M;
                decimal returnsAuthPercentage = (approvedReturnsXctsCount + deniedReturnsXctsCount > 0)
                                                ? Decimal.Divide(approvedReturnsXctsCount, (approvedReturnsXctsCount + deniedReturnsXctsCount))
                                                : 0.0M;
                sb.AppendLine("Auth success rate:");
                sb.AppendLine($"Sales: {salesAuthPercentage:P}");
                sb.AppendLine($"   Total Failed: {subtotalDeniedSales:C} ({deniedSalesXctsCount} transactions)");
                sb.AppendLine($"Returns: {returnsAuthPercentage:P}");
                sb.AppendLine($"   Total Failed: {subtotalDeniedReturns:C} ({deniedReturnsXctsCount} transactions)");
                sb.AppendLine();
                sb.AppendLine($"-----");
                sb.AppendLine($"- text SALES for sales breakdown");
                sb.AppendLine($"- text RETURNS for Returns breakdown");
                sb.AppendLine($"- text CBACK for Chargebacks details");
                sb.AppendLine($"------");
                sb.AppendLine();
                sb.AppendLine($"Next expected batch date: today, at store closing (if manual batching).");
                sb.AppendLine($"If you are enrolled in automatic batch closing, I will let you know when the next batch closes. To learn more about Automatic Batch Closing and see if it is right for your business, go to iQ {GeneralConstants.IQ_URL}.");
                sb.AppendLine();
                sb.AppendLine($"Text Help? for available commands");
            }
            else
            {
                sb.AppendLine(@"No activity yet for today.");
                sb.AppendLine();
                sb.AppendLine($"Text Help? for available commands");
            }

            return sb.ToString();
        }

        // Command: Sales
        public static string BuildSalesSummaryMessage(List<int> merchantIds, DateTime xctPostingDateUTC, string userTZCode)
        {
            // get summaries for a list of merchants
            List<XctDailySummaryBE> xctDailySummaries = MerchantController.GetXctDailySummaries(merchantIds, xctPostingDateUTC);

            DateTime xctPostingDateUser = DateTimeUtilities.CovertToUserLocalDT(xctPostingDateUTC, userTZCode);
            DateTime currentUserDT = DateTimeUtilities.CovertToUserLocalDT(DateTime.Now.ToUniversalTime(), userTZCode);
            string currentUserTimeText = $"{currentUserDT.ToString("h:mm tt")} {userTZCode}";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Merchant Sales Breakdown for: {xctPostingDateUser:ddd MMM dd, yyyy} as of: {currentUserTimeText}");
            //sb.AppendLine("---------------------------------------------------------");
            sb.AppendLine();

            if (xctDailySummaries != null && xctDailySummaries.Count > 0)
            {
                decimal subtotalCPSales = 0.0M;
                decimal subtotalCNPSales = 0.0M;
                int subtotalCNPXctCount = 0;

                sb.AppendLine("In Store Sales:");
                foreach (var xctDailySummary in xctDailySummaries)
                {
                    if (xctDailySummary.CPSalesSummary != null)
                    {
                        sb.AppendLine($"  Store: {xctDailySummary.CPSalesSummary.MerchantName} (MID: {xctDailySummary.CPSalesSummary.MerchantID})");
                        sb.AppendLine($"    {xctDailySummary.CPSalesSummary.SuccessXctSubtotalValue:C} ({xctDailySummary.CPSalesSummary.SuccessXctCount} transactions)");
                        sb.AppendLine();

                        subtotalCPSales += xctDailySummary.CPSalesSummary.SuccessXctSubtotalValue;
                    }

                    if (xctDailySummary.CNPSalesSummary != null)
                    {
                        subtotalCNPSales += xctDailySummary.CNPSalesSummary.SuccessXctSubtotalValue;
                        subtotalCNPXctCount += xctDailySummary.CNPSalesSummary.SuccessXctCount;
                    }
                }

                sb.AppendLine("Online Sales:");
                sb.AppendLine($"    {subtotalCNPSales:C} ({subtotalCNPXctCount} transactions)");

                sb.AppendLine("---------------------------------");
                sb.AppendLine("Total Sales:");
                sb.AppendLine($"    {(subtotalCPSales + subtotalCNPSales):C}");
            }
            else
            {
                sb.AppendLine(@"No activity yet for today.");
            }
            sb.AppendLine();
            sb.AppendLine($"Text Help? for available commands");

            return sb.ToString();
        }

        // Command: Returns
        public static string BuildReturnsSummaryMessage(List<int> merchantIds, DateTime xctPostingDateUTC, string userTZCode)
        {
            // get summaries for a list of merchants
            List<XctDailySummaryBE> xctDailySummaries = MerchantController.GetXctDailySummaries(merchantIds, xctPostingDateUTC);

            DateTime xctPostingDateUser = DateTimeUtilities.CovertToUserLocalDT(xctPostingDateUTC, userTZCode);
            DateTime currentUserDT = DateTimeUtilities.CovertToUserLocalDT(DateTime.Now.ToUniversalTime(), userTZCode);
            string currentUserTimeText = $"{currentUserDT.ToString("h:mm tt")} {userTZCode}";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Merchant Returns Breakdown for: {xctPostingDateUser:ddd MMM dd, yyyy} as of: {currentUserTimeText}");
            //sb.AppendLine("---------------------------------------------------------");
            sb.AppendLine();

            if (xctDailySummaries != null && xctDailySummaries.Count > 0)
            {
                decimal subtotalCPReturns = 0.0M;
                decimal subtotalCNPReturns = 0.0M;
                int subtotalCNPReturnsCount = 0;

                sb.AppendLine("In Store Returns:");
                foreach (var xctDailySummary in xctDailySummaries)
                {
                    if (xctDailySummary.CPReturnsSummary != null)
                    {
                        sb.AppendLine($"  Store: {xctDailySummary.CPReturnsSummary.MerchantName} (MID: {xctDailySummary.CPReturnsSummary.MerchantID})");
                        sb.AppendLine($"    {xctDailySummary.CPReturnsSummary.SuccessXctSubtotalValue:C} ({xctDailySummary.CPReturnsSummary.SuccessXctCount} transactions)");
                        sb.AppendLine();

                        subtotalCPReturns += xctDailySummary.CPReturnsSummary.SuccessXctSubtotalValue;
                    }

                    if (xctDailySummary.CNPReturnsSummary != null)
                    {
                        subtotalCNPReturns += xctDailySummary.CNPReturnsSummary.SuccessXctSubtotalValue;
                        subtotalCNPReturnsCount += xctDailySummary.CNPReturnsSummary.SuccessXctCount;
                    }
                }

                sb.AppendLine("Online Initiated Returns:");
                sb.AppendLine($"    {subtotalCNPReturns:C} ({subtotalCNPReturnsCount} transactions)");

                sb.AppendLine("---------------------------------");
                sb.AppendLine("Total Returns:");
                sb.AppendLine($"    {(subtotalCPReturns + subtotalCNPReturns):C}");
            }
            else
            {
                sb.AppendLine(@"No activity yet for today.");
            }
            sb.AppendLine();
            sb.AppendLine($"Text Help? for available commands");

            return sb.ToString();
        }

        // Command: Cback
        public static string BuildChargebackDetails(List<int> merchantIds, DateTime xctPostingDateUTC, string userTZCode)
        {
            var merchantId = merchantIds[0];

            DateTime xctPostingDateUser = DateTimeUtilities.CovertToUserLocalDT(xctPostingDateUTC, userTZCode);
            DateTime currentUserDT = DateTimeUtilities.CovertToUserLocalDT(DateTime.Now.ToUniversalTime(), userTZCode);
            string currentUserTimeText = $"{currentUserDT.ToString("h:mm tt")} {userTZCode}";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Pending Chargeback Details as of: {currentUserTimeText}");
            sb.AppendLine(@"----------------------------------------------------------");

            var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDateUTC);

            if (merchantActivity != null && merchantActivity.transactions != null && merchantActivity.transactions.Count() > 0)
            {
                var chargebackXcts = merchantActivity.transactions
                                    .Where(x => x.xct_type == Enums.TRANSACTION_TYPE.chargeback).ToList();

                if (chargebackXcts != null)
                {
                    int chargebackCount = 0;
                    foreach (var chargebackXct in chargebackXcts)
                    {
                        chargebackCount++;

                        DateTime dueDate = chargebackXct.xct_dt.AddDays(3);

                        sb.AppendLine($"{chargebackXct.xct_amount:C} Card (Last 4): x{chargebackXct.card_data.primary_account_no.Right(4)}");
                        sb.AppendLine($"    Respond By: {dueDate.ToString("ddd MMM dd, yyyy")}");
                    }
                    sb.AppendLine(@"====================================");
                    sb.AppendLine($"Total: {chargebackXcts.Sum(x => x.xct_amount):C}  Qty: {chargebackCount}");
                    sb.AppendLine();
                    sb.AppendLine($"  To respond to your Chargebacks, go to IQ: {GeneralConstants.CHARGEBACK_URL}");
                }
                else
                {
                    sb.AppendLine(@"You have no pending chargebacks!");
                }
            }
            else
            {
                sb.AppendLine(@"You have no pending chargebacks!");
            }

            return sb.ToString();
        }

        // Command: Close
        //public static void FireAllTerminalsClosedEvent(int merchantId)
        //{
        //    var merchant = MongoDBContext.FindMerchantById(merchantId);
        //    DateTime xctPostingDate = DateTime.Today;

        //    var xctSummaryMsg = BuildOverallSummaryMessage(merchantId, xctPostingDate, merchant.primary_contact.local_time_zone);

        //    TwilioController.SendSMSMessage(merchant.primary_contact.phone_no, xctSummaryMsg);
        //}

        // Command: Settings 

        #region === FAF Messages =========================================
        
        // Command: FAF
        public static string BuildFAFMessage(int merchantId)
        {
            DateTime activityDate = DateTime.Now.ToUniversalTime();

            string response = string.Empty;

            var merchant = MongoDBContext.FindMerchantById(merchantId);
            var merchantDailyActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, activityDate);

            if (merchantDailyActivity == null)
            {
                merchantDailyActivity = new MerchantDailyActivityMBE() { merchant_id = merchantId, xct_posting_date = activityDate };
                MongoDBContext.InsertMerchantDailyActivity(merchantDailyActivity);
            }

            if (!merchantDailyActivity.is_fast_access_funding_enabled.HasValue
                || !merchantDailyActivity.is_fast_access_funding_enabled.Value)
            {
                response = $"Reply CONFIRM to have today's settlement deposited into debit card account ending in {merchant.setup_options.debit_card_no.Right(4)} tomorrow morning upon batch close.";
            }
            else
            {
                response = $"Reply UNDO to NOT use Fast Access Funding for today's settlement and have it deposited on the normal schedule.";
            }

            return response;
        }

        // Command: Confirm
        public static string BuildConfirmFAFMessage(int merchantId)
        {
            DateTime activityDate = DateTime.Now.ToUniversalTime();

            string response = string.Empty;

            var merchantDailyActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, activityDate);

            if (merchantDailyActivity == null)
            {
                merchantDailyActivity = new MerchantDailyActivityMBE() { merchant_id = merchantId, xct_posting_date = activityDate };
                MongoDBContext.InsertMerchantDailyActivity(merchantDailyActivity);
            }

            if (!merchantDailyActivity.is_fast_access_funding_enabled.HasValue
                 || !merchantDailyActivity.is_fast_access_funding_enabled.Value)
            {
                response = @"Funds for the Final Settlement amount will now be deposited via FastAccess tomorrow morning.";

                merchantDailyActivity.is_fast_access_funding_enabled = true;
                MongoDBContext.UpdateMerchantDailyActivity(merchantDailyActivity);
            }
            else
            {
                response = @"Funds for the Final Settlement amount are already set to be deposited via FastAccess tomorrow morning.";
            }

            return response;
        }

        // Command: Undo
        public static string BuildUndoFAFMessage(int merchantId)
        {
            DateTime activityDate = DateTime.Now.ToUniversalTime();

            string response = string.Empty;

            var merchantDailyActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, activityDate);

            if (merchantDailyActivity == null)
            {
                merchantDailyActivity = new MerchantDailyActivityMBE() { merchant_id = merchantId, xct_posting_date = activityDate };
                MongoDBContext.InsertMerchantDailyActivity(merchantDailyActivity);
            }

            if (!merchantDailyActivity.is_fast_access_funding_enabled.HasValue
                 || !merchantDailyActivity.is_fast_access_funding_enabled.Value)
            {
                response = @"Funds for this Final Settlement amount are already set Not to use Fast Access Funding.";
            }
            else
            {
                response = @"Funds for this Final Settlement amount will Not use Fast Access Funding and will be deposited on the normal date.";

                var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, activityDate);
                merchantActivity.is_fast_access_funding_enabled = false;
                MongoDBContext.UpdateMerchantDailyActivity(merchantActivity);
            }

            return response;
        }

        #endregion

        #region === Utility Functions ========================================

        /// <summary>
        /// Find the merchant record using a registered phone no
        /// </summary>
        /// <param name="phoneNo"></param>
        /// <returns></returns>
        public static MerchantMBE LookupMerchant(string phoneNo)
        {
            var merchant = MongoDBContext.FindMerchantByPrimaryContactPhoneNo(phoneNo);

            return merchant;
        }

        public static MerchantMBE GetMerchant(int merchantId)
        {
            var merchant = MongoDBContext.FindMerchantById(merchantId);

            return merchant;
        }

        private static List<XctDailySummaryBE> GetXctDailySummaries(List<int> merchantIds, DateTime xctPostingDate)
        {
            List<XctDailySummaryBE> merchantDailySummaries = new List<XctDailySummaryBE>();

            foreach(int merchantId in merchantIds)
            {
                merchantDailySummaries.Add(GetXctDailySummary(merchantId, xctPostingDate));
            }

            return merchantDailySummaries;
        }

        /// <summary>
        /// Build a Xct Summary for the specified merchant & date
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="xctPostingDate"></param>
        /// <returns></returns>
        private static XctDailySummaryBE GetXctDailySummary(int merchantId, DateTime xctPostingDate)
        {
            var merchant = MongoDBContext.FindMerchantById(merchantId);
            var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);

            XctDailySummaryBE results = new XctDailySummaryBE();

            //List<Enums.TRANSACTION_TYPE> cpSalesXctTypes = new List<Enums.TRANSACTION_TYPE>()
            //{
            //    Enums.TRANSACTION_TYPE.cp_sale
            //};

            //List<Enums.TRANSACTION_TYPE>cnpSalesXctTypes = new List<Enums.TRANSACTION_TYPE>()
            //{
            //    Enums.TRANSACTION_TYPE.cnp_sale
            //};

            //List<Enums.TRANSACTION_TYPE> returnsXctTypes = new List<Enums.TRANSACTION_TYPE>()
            //{
            //    Enums.TRANSACTION_TYPE.credit_return
            //};

            //List<Enums.TRANSACTION_TYPE> chargeBacksXctTypes = new List<Enums.TRANSACTION_TYPE>()
            //{
            //    Enums.TRANSACTION_TYPE.chargeback
            //};

            if (merchantActivity != null 
                && merchantActivity.transactions != null 
                && merchantActivity.transactions.Count > 0)
            {
                results.CPSalesSummary = merchantActivity.transactions
                            .Where(x => x.xct_type == Enums.TRANSACTION_TYPE.cp_sale)
                            .GroupBy(x => new { x.xct_type })
                            .Select(x => new XctTypeDailySummaryBE()
                            {
                                MerchantName = merchant.merchant_name,
                                MerchantID = merchantId,
                                XctType = x.Key.xct_type,
                                SuccessXctCount = x.Where(r => r.is_Auth_Failed == false).Count(),
                                SuccessXctSubtotalValue = x.Where(r => r.is_Auth_Failed == false).Sum(r => r.xct_amount),
                                FailureXctCount = x.Where(r => r.is_Auth_Failed == true).Count(),
                                FailureXctSubtotalValue = x.Where(r => r.is_Auth_Failed == true).Sum(r => r.xct_amount),
                            }).FirstOrDefault();

                results.CNPSalesSummary = merchantActivity.transactions
                            .Where(x => x.xct_type == Enums.TRANSACTION_TYPE.cnp_sale)
                            .GroupBy(x => new { x.xct_type })
                            .Select(x => new XctTypeDailySummaryBE()
                            {
                                MerchantName = merchant.merchant_name,
                                MerchantID = merchantId,
                                XctType = x.Key.xct_type,
                                SuccessXctCount = x.Where(r => r.is_Auth_Failed == false).Count(),
                                SuccessXctSubtotalValue = x.Where(r => r.is_Auth_Failed == false).Sum(r => r.xct_amount),
                                FailureXctCount = x.Where(r => r.is_Auth_Failed == true).Count(),
                                FailureXctSubtotalValue = x.Where(r => r.is_Auth_Failed == true).Sum(r => r.xct_amount),
                            }).FirstOrDefault();

                results.CPReturnsSummary = merchantActivity.transactions
                            .Where(x => x.xct_type == Enums.TRANSACTION_TYPE.cp_return)
                            .GroupBy(x => new { x.xct_type })
                            .Select(x => new XctTypeDailySummaryBE()
                            {
                                MerchantName = merchant.merchant_name,
                                MerchantID = merchantId,
                                XctType = x.Key.xct_type,
                                SuccessXctCount = x.Where(r => r.is_Auth_Failed == false).Count(),
                                SuccessXctSubtotalValue = x.Where(r => r.is_Auth_Failed == false).Sum(r => r.xct_amount),
                                FailureXctCount = x.Where(r => r.is_Auth_Failed == true).Count(),
                                FailureXctSubtotalValue = x.Where(r => r.is_Auth_Failed == true).Sum(r => r.xct_amount),
                            }).FirstOrDefault();

                results.CNPReturnsSummary = merchantActivity.transactions
                            .Where(x => x.xct_type == Enums.TRANSACTION_TYPE.cnp_return)
                            .GroupBy(x => new { x.xct_type })
                            .Select(x => new XctTypeDailySummaryBE()
                            {
                                MerchantName = merchant.merchant_name,
                                MerchantID = merchantId,
                                XctType = x.Key.xct_type,
                                SuccessXctCount = x.Where(r => r.is_Auth_Failed == false).Count(),
                                SuccessXctSubtotalValue = x.Where(r => r.is_Auth_Failed == false).Sum(r => r.xct_amount),
                                FailureXctCount = x.Where(r => r.is_Auth_Failed == true).Count(),
                                FailureXctSubtotalValue = x.Where(r => r.is_Auth_Failed == true).Sum(r => r.xct_amount),
                            }).FirstOrDefault();

                results.ChargebacksSummary = merchantActivity.transactions
                            .Where(x => x.xct_type == Enums.TRANSACTION_TYPE.chargeback)
                            .GroupBy(x => new { x.xct_type })
                            .Select(x => new XctTypeDailySummaryBE()
                            {
                                MerchantName = merchant.merchant_name,
                                MerchantID = merchantId,
                                XctType = x.Key.xct_type,
                                SuccessXctCount = x.Where(r => r.is_Auth_Failed == false).Count(),
                                SuccessXctSubtotalValue = x.Where(r => r.is_Auth_Failed == false).Sum(r => r.xct_amount),
                                FailureXctCount = x.Where(r => r.is_Auth_Failed == true).Count(),
                                FailureXctSubtotalValue = x.Where(r => r.is_Auth_Failed == true).Sum(r => r.xct_amount),
                            }).FirstOrDefault();
            }

            return results;
        }

        #endregion

        #region === Generate Transactions ========================================
        public static int GenerateSampleXcts(int merchantId, DateTime xctPostingDate)
        {
            int xctsGenerated = 0;

            // get merchant metadata (MDB ??)
            var merchant = MongoDBContext.FindMerchantById(merchantId);

            if (merchant.setup_options.supports_cp_sales_xcts)
            {
                xctsGenerated += GenerateSalesXcts(merchantId, xctPostingDate, Enums.TRANSACTION_TYPE.cp_sale);
            }
            if (merchant.setup_options.supports_cnp_sales_xcts)
            {
                xctsGenerated += GenerateSalesXcts(merchantId, xctPostingDate, Enums.TRANSACTION_TYPE.cnp_sale);
            }
            if (merchant.setup_options.supports_cp_returns_xcts)
            {
                xctsGenerated += GenerateReturnXcts(merchantId, xctPostingDate, Enums.TRANSACTION_TYPE.cp_return);
            }
            if (merchant.setup_options.supports_cnp_returns_xcts)
            {
                xctsGenerated += GenerateReturnXcts(merchantId, xctPostingDate, Enums.TRANSACTION_TYPE.cnp_return);
            }

            xctsGenerated += GenerateChargebacksXcts(merchantId, xctPostingDate);

            return xctsGenerated;
        }

        /// <summary>
        /// Create and store a set of random transactions on the specified merchant
        /// </summary>
        /// <remarks>
        /// Used to drive demos
        /// </remarks>
        public static int GenerateSalesXcts(int merchantId, DateTime xctPostingDate, Enums.TRANSACTION_TYPE xctType)
        {
            // get merchant metadata (MDB ??)
            var merchant = MongoDBContext.FindMerchantById(merchantId);

            if((xctType == Enums.TRANSACTION_TYPE.cp_sale && !merchant.setup_options.supports_cp_sales_xcts)
                || (xctType == Enums.TRANSACTION_TYPE.cnp_sale && !merchant.setup_options.supports_cnp_sales_xcts))
            {
                return 0;
            }

            #region Optionally Create MerchantDailyActivity record (if reqd)
            var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);

            if (merchantActivity == null)
            {
                merchantActivity = new MerchantDailyActivityMBE()
                {
                    merchant_id = merchantId,
                    xct_posting_date = xctPostingDate.Date,
                    open_start_dt = DateTime.Now.ToUniversalTime()
                };

                MongoDBContext.InsertMerchantDailyActivity(merchantActivity);
            }
            #endregion

            int xctCntToGenerate = new Random().Next(35, 50);
            Random amountGenerator = new Random();

            var transactions = new List<TransactionMBE>();

            // create a random # of purchase xcts
            for (int loopCtr = 1; loopCtr <= xctCntToGenerate - 1; loopCtr++)
            {
                int cardPresentIndicator = new Random().Next(1, 100);

                transactions.Add(new TransactionMBE()
                {
                    terminal_id = (xctType == Enums.TRANSACTION_TYPE.cp_sale) 
                                    ? merchant.terminals.OrderBy(t => Guid.NewGuid()).First().terminal_id
                                    : "Online",
                    card_data = _paymentCards.OrderBy(t => Guid.NewGuid()).First(),
                    xct_amount = Math.Round(new decimal(amountGenerator.NextDouble() * 100.0), 2),
                    xct_dt = DateTime.Now.ToUniversalTime(),
                    xct_id = Guid.NewGuid(),
                    xct_type = xctType,
                    is_Auth_Failed = false
                });
            }

            // add one auth failure
            transactions.Add(new TransactionMBE()
            {
                terminal_id = (xctType == Enums.TRANSACTION_TYPE.cp_sale)
                                    ? merchant.terminals.OrderBy(t => Guid.NewGuid()).First().terminal_id
                                    : "Online",
                card_data = _paymentCards.OrderBy(t => Guid.NewGuid()).First(),
                xct_amount = Math.Round(new decimal(amountGenerator.NextDouble() * 100.0), 2),
                xct_dt = DateTime.Now.ToUniversalTime(),
                xct_id = Guid.NewGuid(),
                xct_type = Enums.TRANSACTION_TYPE.cp_sale,
                is_Auth_Failed = true
            });

            // store xcts
            MongoDBContext.UpsertMerchantDailyActivity(merchantId, xctPostingDate, transactions);

            return xctCntToGenerate;
        }

        public static int GenerateReturnXcts(int merchantId, DateTime xctPostingDate, Enums.TRANSACTION_TYPE xctType)
        {
            // get merchant metadata (MDB ??)
            var merchant = MongoDBContext.FindMerchantById(merchantId);

            if ((xctType == Enums.TRANSACTION_TYPE.cp_sale && !merchant.setup_options.supports_cp_returns_xcts)
                    || (xctType == Enums.TRANSACTION_TYPE.cnp_sale && !merchant.setup_options.supports_cnp_returns_xcts))
            {
                return 0;
            }   


            #region Optionally Create MerchantDailyActivity record (if reqd)
            var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);

            if (merchantActivity == null)
            {
                merchantActivity = new MerchantDailyActivityMBE()
                {
                    merchant_id = merchantId,
                    xct_posting_date = xctPostingDate.Date,
                    open_start_dt = DateTime.Now.ToUniversalTime()
                };

                MongoDBContext.InsertMerchantDailyActivity(merchantActivity);
            }
            #endregion

            int xctCntToGenerate = new Random().Next(1, 3);
            Random amountGenerator = new Random();

            var transactions = new List<TransactionMBE>();

            // create a random # of purchase xcts
            for (int loopCtr = 1; loopCtr <= xctCntToGenerate - 1; loopCtr++)
            {
                transactions.Add(new TransactionMBE()
                {
                    terminal_id = (xctType == Enums.TRANSACTION_TYPE.cp_return)
                                    ? merchant.terminals.OrderBy(t => Guid.NewGuid()).First().terminal_id
                                    : "Online",
                    card_data = _paymentCards.OrderBy(t => Guid.NewGuid()).First(),
                    xct_amount = Math.Round(new decimal(amountGenerator.NextDouble() * -100.0), 2),
                    xct_dt = DateTime.Now.ToUniversalTime(),
                    xct_id = Guid.NewGuid(),
                    xct_type = xctType,
                    is_Auth_Failed = false
                });
            }

            // add one auth failure
            transactions.Add(new TransactionMBE()
            {
                terminal_id = (xctType == Enums.TRANSACTION_TYPE.cp_return)
                                    ? merchant.terminals.OrderBy(t => Guid.NewGuid()).First().terminal_id
                                    : "Online",
                card_data = _paymentCards.OrderBy(t => Guid.NewGuid()).First(),
                xct_amount = Math.Round(new decimal(amountGenerator.NextDouble() * 100.0), 2),
                xct_dt = DateTime.Now.ToUniversalTime(),
                xct_id = Guid.NewGuid(),
                xct_type = xctType,
                is_Auth_Failed = true
            });

            // store xcts
            MongoDBContext.UpsertMerchantDailyActivity(merchantId, xctPostingDate, transactions);

            return xctCntToGenerate;
        }

        public static int GenerateChargebacksXcts(int merchantId, DateTime xctPostingDate)
        {
            // get merchant metadata (MDB ??)
            var merchant = MongoDBContext.FindMerchantById(merchantId);

            #region Optionally Create MerchantDailyActivity record (if reqd)
            var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);

            if (merchantActivity == null)
            {
                merchantActivity = new MerchantDailyActivityMBE()
                {
                    merchant_id = merchantId,
                    xct_posting_date = xctPostingDate.Date,
                    open_start_dt = DateTime.Now.ToUniversalTime()
                };

                MongoDBContext.InsertMerchantDailyActivity(merchantActivity);
            }
            #endregion

            int xctCntToGenerate = new Random().Next(1, 2);
            Random amountGenerator = new Random();
            Random deltaDaysGenerator = new Random();

            var transactions = new List<TransactionMBE>();

            // create a random # of purchase xcts
            for (int loopCtr = 1; loopCtr <= xctCntToGenerate; loopCtr++)
            {
                int deltaDays = -1 * deltaDaysGenerator.Next(1, 3); // creates a number between 1 and 12

                DateTime xctDate = DateTime.Now.ToUniversalTime().AddDays(deltaDays);

                transactions.Add(new TransactionMBE()
                {
                    terminal_id = merchant.terminals.OrderBy(t => Guid.NewGuid()).First().terminal_id,
                    card_data = _paymentCards.OrderBy(t => Guid.NewGuid()).First(),
                    xct_amount = Math.Round(new decimal(amountGenerator.NextDouble() * -50.0), 2),
                    xct_dt = xctDate,
                    xct_id = Guid.NewGuid(),
                    xct_type = Enums.TRANSACTION_TYPE.chargeback
                });
            }

            // store xcts
            MongoDBContext.UpsertMerchantDailyActivity(merchantId, xctPostingDate, transactions);

            return xctCntToGenerate;
        }

        #endregion

        #region === Merchant Admin Functions ========================================

        public static void CreateAllMerchants(bool isDeleteIfExists)
        {
            Dictionary<int, MerchantMBE> merchants = BuildListOfMerchants();

            foreach(KeyValuePair<int, MerchantMBE> kvp in merchants)
            {
                CreateMerchant(kvp.Key, isDeleteIfExists);
            }

            System.Console.WriteLine();
            System.Console.WriteLine("Hit <enter> to exit");
            System.Console.ReadLine();
        }

        public static void CreateMerchant(int merchantId, bool isDeleteIfExists)
        {
            //  exists  isDeleteIfExists => Delete  Insert
            //      F       N/A               N/A       T
            //      T       F                 F         F
            //      T       T                 T         T

            bool isMerchantAlreadyExists = false;

            MerchantMBE exisitingMerchant = MongoDBContext.FindMerchantById(merchantId);

            if(exisitingMerchant != null && isDeleteIfExists)
            {
                MongoDBContext.DeleteMerchant(merchantId);
                exisitingMerchant = null;
                isMerchantAlreadyExists = true;
            }

            if (exisitingMerchant == null) 
            {
                Dictionary<int, MerchantMBE> merchants = BuildListOfMerchants();
                var newMerchant = merchants[merchantId];

                if (newMerchant != null)
                {
                    MongoDBContext.InsertMerchant(newMerchant);
                    if (!isMerchantAlreadyExists)
                    {
                        System.Console.WriteLine($"Loaded Merchant: {newMerchant.merchant_id} [{newMerchant.merchant_name}]");
                    }
                    else
                    {
                        System.Console.WriteLine($"Purged & Reloaded Merchant: {newMerchant.merchant_id} [{newMerchant.merchant_name}]");
                    }
                }
                else
                {
                    throw new ApplicationException($"MerchantID: {merchantId} is not recognized");
                }
            }
            else
            {
                System.Console.WriteLine($"Existing Merchant: {exisitingMerchant.merchant_id} [{exisitingMerchant.merchant_name}] not overwritten");
            }
        }

        public static Dictionary<int, MerchantMBE> BuildListOfMerchants()
        {
            Dictionary<int, MerchantMBE> listOfMerchants = new Dictionary<int, MerchantMBE>()
            {
                {
                    1, new MerchantMBE()
                        {
                            merchant_id = 1,
                            merchant_name = @"Blisks R US",
                            setup_options = new SetupOptionsMBE()
                            {
                                is_host_data_capture_enabled = true,
                                auto_close_hh_mm = new TimeSpan(19, 0, 0),
                                is_fast_funding_enabled = true,
                                debit_card_no = @"1234567890123401",
                                supports_cnp_sales_xcts = false,
                                supports_cp_sales_xcts = true,
                                supports_cnp_returns_xcts = false,
                                supports_cp_returns_xcts = true
                            },
                            terminals = new List<TerminalMBE>()
                            {
                                new TerminalMBE() { terminal_id = "TID-001", terminal_type = @"610", terminal_desc = @"Checkout 1" },
                                new TerminalMBE() { terminal_id = "TID-002", terminal_type = @"610", terminal_desc = @"Checkout 2" },
                            }
                        }
                },
                {
                    2, new MerchantMBE()
                        {
                            merchant_id = 2,
                            merchant_name = @"Marcos Canoe Livery",
                            setup_options = new SetupOptionsMBE()
                            {
                                is_host_data_capture_enabled = true,
                                auto_close_hh_mm = new TimeSpan(19, 0, 0),
                                is_fast_funding_enabled = true,
                                debit_card_no = @"1234567890126702",
                                supports_cnp_sales_xcts = true,
                                supports_cp_sales_xcts = true,
                                supports_cnp_returns_xcts = true,
                                supports_cp_returns_xcts = true
                            },
                            terminals = new List<TerminalMBE>()
                            {
                                new TerminalMBE() { terminal_id = "TID-001", terminal_type = @"610", terminal_desc = @"Checkout 1" },
                                new TerminalMBE() { terminal_id = "TID-002", terminal_type = @"610", terminal_desc = @"Checkout 2" },
                            }
                        }
                },
                {
                    3, new MerchantMBE()
                        {
                            merchant_id = 3,
                            merchant_name = @"Dusty's Cookies",
                            setup_options = new SetupOptionsMBE()
                            {
                                is_host_data_capture_enabled = true,
                                auto_close_hh_mm = new TimeSpan(19, 0, 0),
                                is_fast_funding_enabled = true,
                                debit_card_no = @"1234567890125203",
                                supports_cnp_sales_xcts = true,
                                supports_cp_sales_xcts = true,
                                supports_cnp_returns_xcts = false,
                                supports_cp_returns_xcts = false
                            },
                            terminals = new List<TerminalMBE>()
                            {
                                new TerminalMBE() { terminal_id = "TID-001", terminal_type = @"610", terminal_desc = @"Checkout 1" },
                                new TerminalMBE() { terminal_id = "TID-002", terminal_type = @"610", terminal_desc = @"Checkout 2" },
                            }
                        }
                },
                {
                    4, new MerchantMBE()
                        {
                            merchant_id = 4,
                            merchant_name = @"Byrne's Bar & Grill",
                            setup_options = new SetupOptionsMBE()
                            {
                                is_host_data_capture_enabled = true,
                                auto_close_hh_mm = new TimeSpan(19, 0, 0),
                                is_fast_funding_enabled = true,
                                debit_card_no = @"1234567890122704",
                                supports_cnp_sales_xcts = false,
                                supports_cp_sales_xcts = true,
                                supports_cnp_returns_xcts = false,
                                supports_cp_returns_xcts = true
                            },
                            terminals = new List<TerminalMBE>()
                            {
                                new TerminalMBE() { terminal_id = "TID-001", terminal_type = @"610", terminal_desc = @"Checkout 1" },
                                new TerminalMBE() { terminal_id = "TID-002", terminal_type = @"610", terminal_desc = @"Checkout 2" },
                            }
                        }
                },
                {
                    5, new MerchantMBE()
                        {
                            merchant_id = 5,
                            merchant_name = @"Dr. Boeding",
                            setup_options = new SetupOptionsMBE()
                            {
                                is_host_data_capture_enabled = true,
                                auto_close_hh_mm = new TimeSpan(19, 0, 0),
                                is_fast_funding_enabled = true,
                                debit_card_no = @"1234567890129905",
                                supports_cnp_sales_xcts = false,
                                supports_cp_sales_xcts = true,
                                supports_cnp_returns_xcts = false,
                                supports_cp_returns_xcts = true
                            },
                            terminals = new List<TerminalMBE>()
                            {
                                new TerminalMBE() { terminal_id = "TID-001", terminal_type = @"610", terminal_desc = @"Checkout 1" },
                                new TerminalMBE() { terminal_id = "TID-002", terminal_type = @"610", terminal_desc = @"Checkout 2" },
                            }
                        }
                },
                {
                    6, new MerchantMBE()
                    {
                        merchant_id = 6,
                        merchant_name = @"Pallavi's Robot Hobby Shop",
                        setup_options = new SetupOptionsMBE()
                        {
                            is_host_data_capture_enabled = true,
                            auto_close_hh_mm = new TimeSpan(19, 0, 0),
                            is_fast_funding_enabled = true,
                            debit_card_no = @"1234567890121106",
                            supports_cnp_sales_xcts = true,
                            supports_cp_sales_xcts = true,
                            supports_cnp_returns_xcts = true,
                            supports_cp_returns_xcts = true
                        },
                        terminals = new List<TerminalMBE>()
                        {
                            new TerminalMBE() { terminal_id = "TID-001", terminal_type = @"610", terminal_desc = @"Checkout 1" },
                            new TerminalMBE() { terminal_id = "TID-002", terminal_type = @"610", terminal_desc = @"Checkout 2" },
                        }
                    }
                },
                {
                    7, new MerchantMBE()
                    {
                        merchant_id = 7,
                        merchant_name = @"Joe's Java Hut",
                        setup_options = new SetupOptionsMBE()
                        {
                            is_host_data_capture_enabled = true,
                            auto_close_hh_mm = new TimeSpan(19, 0, 0),
                            is_fast_funding_enabled = true,
                            debit_card_no = @"1234567890121107",
                            supports_cnp_sales_xcts = false,
                            supports_cp_sales_xcts = true,
                            supports_cnp_returns_xcts = false,
                            supports_cp_returns_xcts = true
                        },
                        terminals = new List<TerminalMBE>()
                        {
                            new TerminalMBE() { terminal_id = "TID-001", terminal_type = @"610", terminal_desc = @"Checkout 1" },
                            new TerminalMBE() { terminal_id = "TID-002", terminal_type = @"610", terminal_desc = @"Checkout 2" },
                        }
                    }
                },
                {
                    8, new MerchantMBE()
                    {
                        merchant_id = 8,
                        merchant_name = @"Jake's State Farm",
                        setup_options = new SetupOptionsMBE()
                        {
                            is_host_data_capture_enabled = true,
                            auto_close_hh_mm = new TimeSpan(19, 0, 0),
                            is_fast_funding_enabled = true,
                            debit_card_no = @"1234567890122208",
                            supports_cnp_sales_xcts = true,
                            supports_cp_sales_xcts = true,
                            supports_cnp_returns_xcts = true,
                            supports_cp_returns_xcts = true
                        },
                        terminals = new List<TerminalMBE>()
                        {
                            new TerminalMBE() { terminal_id = "TID-001", terminal_type = @"610", terminal_desc = @"Checkout 1" },
                            new TerminalMBE() { terminal_id = "TID-002", terminal_type = @"610", terminal_desc = @"Checkout 2" },
                        }
                    }
                },
                {
                    9, new MerchantMBE()
                    {
                        merchant_id = 9,
                        merchant_name = @"Jon's Hardware Store",
                        setup_options = new SetupOptionsMBE()
                        {
                            is_host_data_capture_enabled = true,
                            auto_close_hh_mm = new TimeSpan(19, 0, 0),
                            is_fast_funding_enabled = true,
                            debit_card_no = @"1234567890122208",
                            supports_cnp_sales_xcts = false,
                            supports_cp_sales_xcts = true,
                            supports_cnp_returns_xcts = false,
                            supports_cp_returns_xcts = true
                        },
                        terminals = new List<TerminalMBE>()
                        {
                            new TerminalMBE() { terminal_id = "TID-001", terminal_type = @"610", terminal_desc = @"Checkout 1" },
                            new TerminalMBE() { terminal_id = "TID-002", terminal_type = @"610", terminal_desc = @"Checkout 2" },
                        }
                    }
                }
            };

            return listOfMerchants;
        }

        public static void DeleteAllMerchantXctsForDate(int merchantId, DateTime xctPostingDate)
        {
            MongoDBContext.DeleteAllMerchantDailyActivity(merchantId, xctPostingDate);
        }

        public static void DeleteXctsForAllMerchants()
        {
            MongoDBContext.DeleteAllMerchantsDailyActivity();
        }

        //public static void FireClosedEventsForAllMerchants()
        //{
        //    Dictionary<int, MerchantMBE> merchants = BuildListOfMerchants();

        //    foreach (KeyValuePair<int, MerchantMBE> kvp in merchants)
        //    {
        //        FireAllTerminalsClosedEvent(kvp.Key);
        //    }
        //}

        #endregion
    }
}
