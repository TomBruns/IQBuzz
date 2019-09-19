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
            int merchantId = merchantIds[0];

            MerchantMBE merchant = MongoDBContext.FindMerchantById(merchantId);

            XctDailySummaryBE xctSummary = MerchantController.GetXctDailySummary(merchantId, xctPostingDateUTC);

            DateTime xctPostingDateUser = DateTimeUtilities.CovertToUserLocalDT(xctPostingDateUTC, userTZCode);
            DateTime currentUserDT = DateTimeUtilities.CovertToUserLocalDT(DateTime.Now.ToUniversalTime(), userTZCode);
            string currentUserTimeText = $"{currentUserDT.ToString("h:mm tt")} {userTZCode}";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Overall Summary for: {xctPostingDateUser:ddd MMM dd, yyyy} as of: {currentUserTimeText}");
            sb.AppendLine("---------------------------------------------------------");

            if (xctSummary != null)
            {
                decimal netTotal = 0.00M;
                bool isFirstLine = true;
                decimal authPercentage = 0M;

                // cp sales
                if (merchant.setup_options.supports_cp_xcts)
                {
                    foreach (var xctType in xctSummary.CPSalesSummary)
                    {
                        // only sum total for auth success
                        netTotal += xctType.SuccessXctSubtotalValue;
                        // use Decimal.Divide because there are all ints
                        authPercentage = Decimal.Divide(xctType.SuccessXctCount, (xctType.SuccessXctCount + xctType.FailureXctCount));

                        if (!isFirstLine) { sb.AppendLine(); }

                        sb.AppendLine($"{xctType.XctTypeDesc}:");
                        sb.AppendLine($" {xctType.SuccessXctSubtotalValue:C} [{xctType.SuccessXctCount} xcts] Auth: {authPercentage:P}");

                        isFirstLine = false;
                    }
                }

                // cnp sales
                if (merchant.setup_options.supports_cnp_xcts)
                {
                    foreach (var xctType in xctSummary.CNPSalesSummary)
                    {
                        // only sum total for auth success
                        netTotal += xctType.SuccessXctSubtotalValue;
                        // use Decimal.Divide because there are all ints
                        authPercentage = Decimal.Divide(xctType.SuccessXctCount, (xctType.SuccessXctCount + xctType.FailureXctCount));

                        if (!isFirstLine) { sb.AppendLine(); }

                        sb.AppendLine($"{xctType.XctTypeDesc}:");
                        sb.AppendLine($" {xctType.SuccessXctSubtotalValue:C} [{xctType.SuccessXctCount} xcts] Auth: {authPercentage:P}");

                        isFirstLine = false;
                    }
                }

                // returns
                if (merchant.setup_options.supports_returns_xcts)
                {
                    foreach (var xctType in xctSummary.ReturnsSummary)
                    {
                        netTotal += xctType.SuccessXctSubtotalValue;
                        // use Decimal.Divide because there are all ints
                        authPercentage = Decimal.Divide(xctType.SuccessXctCount, (xctType.SuccessXctCount + xctType.FailureXctCount));

                        if (!isFirstLine) { sb.AppendLine(); }

                        sb.AppendLine($"{xctType.XctTypeDesc}:");
                        sb.AppendLine($" {xctType.SuccessXctSubtotalValue:C} [{xctType.SuccessXctCount} xcts] Auth: {authPercentage:P}");

                        isFirstLine = false;
                    }
                }

                sb.AppendLine($"============================");
                sb.AppendLine($"Net Total:  {netTotal:C}");
                sb.AppendLine(" (all Card Transactions)");
                sb.AppendLine();

                foreach (var xctType in xctSummary.ChargebacksSummary)
                {
                    sb.AppendLine($"{xctType.XctTypeDesc}:");
                    sb.AppendLine($" {xctType.SuccessXctSubtotalValue:C} [{xctType.SuccessXctCount} xcts]");
                }
                if (xctSummary.ChargebacksSummary != null && xctSummary.ChargebacksSummary.Count > 0)
                {
                    sb.AppendLine("  (text CBACK for details)");
                }

                // build batch close info
                sb.AppendLine();
                if(merchant.setup_options.is_host_data_capture_enabled)
                {
                    sb.AppendLine($"Your batches will Auto Close at 10:00PM");
                }
                else
                {
                    sb.AppendLine($"Remember to close your batches by: [{merchant.setup_options.manual_close_alert_hh_mm:h:mm tt}]");
                }

                sb.AppendLine();
                sb.AppendLine($"Final settlement amount will be deposited into your checking account ending in x{merchant.setup_options.debit_card_no.Substring(1, 4)} on {DateTime.Now.AddBusinessDays(2).ToString("ddd MMM dd, yyyy")}");
                sb.AppendLine();
                sb.AppendLine($"To receive these funds tomorrow morning reply FAF");
            }
            else
            {
                sb.AppendLine(@"No activity yet for today.");
            }

            return sb.ToString();
        }

        // Command: Sales
        public static string BuildSalesSummaryMessage(List<int> merchantIds, DateTime xctPostingDateUTC, string userTZCode)
        {
            DateTime xctPostingDateUser = DateTimeUtilities.CovertToUserLocalDT(xctPostingDateUTC, userTZCode);
            DateTime currentUserDT = DateTimeUtilities.CovertToUserLocalDT(DateTime.Now.ToUniversalTime(), userTZCode);
            string currentUserTimeText = $"{currentUserDT.ToString("h:mm tt")} {userTZCode}";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Sales Summary as of: {currentUserTimeText}");
            sb.AppendLine();

            var merchantsActivity = MongoDBContext.FindMerchantsDailyActivity(merchantIds, xctPostingDateUTC);

            if (merchantsActivity != null)
            {
                var merchantActivity = merchantsActivity.First().Value;

                // ----------------------------------
                // Card Present (In-Store) Xcts
                // ----------------------------------
                // auth success
                var cpSalesXcts = merchantActivity.transactions
                                    .Where(x => x.xct_type == Enums.TRANSACTION_TYPE.cp_sale
                                            && !x.is_Auth_Failed).ToList();

                decimal cpSales = cpSalesXcts.Sum(x => x.xct_amount);
                int cpQty = cpSalesXcts.Count();

                // failed auth
                var cpSalesFailedAuthXcts = merchantActivity.transactions
                                                .Where(x => x.xct_type == Enums.TRANSACTION_TYPE.cp_sale
                                                        && x.is_Auth_Failed).ToList();
                int cpFailedAuthQty = cpSalesFailedAuthXcts.Count();

                decimal cpTotalQty = cpQty + cpFailedAuthQty;   // force to decimal to avoid integer division
                decimal? cpSalesPassesAuthPercentage = cpTotalQty > 0 ? cpQty / cpTotalQty : (decimal?) null;

                sb.AppendLine($"In-Store:");
                if (cpTotalQty > 0)
                {
                    sb.AppendLine($"{cpSales:C} [{cpQty} txns] Auth: {cpSalesPassesAuthPercentage:P}");
                }
                else
                {
                    sb.AppendLine($"$0.00 [0 txns] Auth: N/A%");
                }

                sb.AppendLine();

                // ----------------------------------
                // Card Not Present (On-line) Xcts
                // ----------------------------------
                var cnpSalesXcts = merchantActivity.transactions
                    .Where(x => x.xct_type == Enums.TRANSACTION_TYPE.cnp_sale
                            && !x.is_Auth_Failed).ToList();

                decimal cnpSales = cnpSalesXcts.Sum(x => x.xct_amount);
                int cnpQty = cnpSalesXcts.Count();

                var cnpSalesFailedAuthXcts = merchantActivity.transactions
                                                .Where(x => x.xct_type == Enums.TRANSACTION_TYPE.cnp_sale
                                                        && x.is_Auth_Failed).ToList();
                int cnpFailedAuthQty = cnpSalesFailedAuthXcts.Count();

                decimal cnpTotalQty = cnpQty + cnpFailedAuthQty;   // force to decimal to avoid integer division
                decimal? cnpSalesPassesAuthPercentage = cnpTotalQty > 0 ? cnpQty / cnpTotalQty : (decimal?)null;

                sb.AppendLine($"Online:");
                if (cnpTotalQty > 0)
                {

                    sb.AppendLine($"{cnpSales:C} [{cnpQty} txns] Auth: {cnpSalesPassesAuthPercentage:P}");
                }
                else
                {
                    sb.AppendLine($"$0.00 [0 txns] Auth: N/A%");
                }

                decimal allSalesPassesAuthPercentage = (cpQty + cnpQty) / (cpTotalQty + cnpTotalQty);

                sb.AppendLine($"------------------------------");
                sb.AppendLine($"Total: {(cpSales + cnpSales):C} [{cpQty + cnpQty} txns] Auth {allSalesPassesAuthPercentage:P}");
                sb.AppendLine("(all Card Transactions)");
            }
            else
            {
                sb.AppendLine($"In-Store:");
                sb.AppendLine($"$0.00 [0 txns] Auth N/A");
                sb.AppendLine();
                sb.AppendLine($"Online:");
                sb.AppendLine($"$0.00 [0 txns] Auth N/A");
                sb.AppendLine($"-------------------------------");
                sb.AppendLine($"Total: $0.00 [0 txns] Auth N/A");
                sb.AppendLine("(all Card Transactions)");
            }

            return sb.ToString();
        }

        // Command: Returns
        public static string BuildReturnsSummaryMessage(List<int> merchantIds, DateTime xctPostingDateUTC, string userTZCode)
        {
            int merchantId = merchantIds[0];

            DateTime xctPostingDateUser = DateTimeUtilities.CovertToUserLocalDT(xctPostingDateUTC, userTZCode);
            DateTime currentUserDT = DateTimeUtilities.CovertToUserLocalDT(DateTime.Now.ToUniversalTime(), userTZCode);
            string currentUserTimeText = $"{currentUserDT.ToString("h:mm tt")} {userTZCode}";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Returns Summary as of: {currentUserTimeText}");
            sb.AppendLine();
            sb.AppendLine($"Returns:");

            var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDateUTC);

            if (merchantActivity != null && merchantActivity.transactions != null && merchantActivity.transactions.Count() > 0)
            {
                var refundXcts = merchantActivity.transactions
                                        .Where(x => x.xct_type == Enums.TRANSACTION_TYPE.credit_return).ToList();


                if (refundXcts != null && refundXcts.Count() > 0)
                {

                    sb.AppendLine($"{refundXcts.Sum(x => x.xct_amount):C}  [{refundXcts.Count()} txns]");
                }
                else
                {
                    sb.AppendLine($"$0.00 [0 txns]");
                    //sb.AppendLine(@"There are no refunds yet today.");
                }
            }
            else
            {
                sb.AppendLine($"$0.00 [0 txns]");
                //sb.AppendLine(@"There are no activity yet today.");
            }

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

        /// <summary>
        /// Build a Xct Summary for the specified merchant & date
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="xctPostingDate"></param>
        /// <returns></returns>
        private static XctDailySummaryBE GetXctDailySummary(int merchantId, DateTime xctPostingDate)
        {
            var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);

            XctDailySummaryBE results = null;

            List<Enums.TRANSACTION_TYPE> cpSalesXctTypes = new List<Enums.TRANSACTION_TYPE>()
            {
                Enums.TRANSACTION_TYPE.cp_sale
            };

            List<Enums.TRANSACTION_TYPE>cnpSalesXctTypes = new List<Enums.TRANSACTION_TYPE>()
            {
                Enums.TRANSACTION_TYPE.cnp_sale
            };

            List<Enums.TRANSACTION_TYPE> returnsXctTypes = new List<Enums.TRANSACTION_TYPE>()
            {
                Enums.TRANSACTION_TYPE.credit_return
            };

            List<Enums.TRANSACTION_TYPE> chargeBacksXctTypes = new List<Enums.TRANSACTION_TYPE>()
            {
                Enums.TRANSACTION_TYPE.chargeback
            };

            if (merchantActivity != null 
                && merchantActivity.transactions != null 
                && merchantActivity.transactions.Count > 0)
            {
                results = new XctDailySummaryBE();

                results.CPSalesSummary = merchantActivity.transactions
                                    .Where(x => cpSalesXctTypes.Contains(x.xct_type))
                                    .OrderBy(x => x.xct_type)
                                    .GroupBy(x => new { x.xct_type })
                                    .Select(x => new XctTypeDailySummaryBE()
                                    {
                                        XctType = x.Key.xct_type,
                                        SuccessXctCount = x.Where(r => r.is_Auth_Failed == false).Count(),
                                        SuccessXctSubtotalValue = x.Where(r => r.is_Auth_Failed == false).Sum(r => r.xct_amount),
                                        FailureXctCount = x.Where(r => r.is_Auth_Failed == true).Count(),
                                        FailureXctSubtotalValue = x.Where(r => r.is_Auth_Failed == true).Sum(r => r.xct_amount),
                                    }).ToList();

                results.CNPSalesSummary = merchantActivity.transactions
                                    .Where(x => cnpSalesXctTypes.Contains(x.xct_type))
                                    .OrderBy(x => x.xct_type)
                                    .GroupBy(x => new { x.xct_type })
                                    .Select(x => new XctTypeDailySummaryBE()
                                    {
                                        XctType = x.Key.xct_type,
                                        SuccessXctCount = x.Where(r => r.is_Auth_Failed == false).Count(),
                                        SuccessXctSubtotalValue = x.Where(r => r.is_Auth_Failed == false).Sum(r => r.xct_amount),
                                        FailureXctCount = x.Where(r => r.is_Auth_Failed == true).Count(),
                                        FailureXctSubtotalValue = x.Where(r => r.is_Auth_Failed == true).Sum(r => r.xct_amount),
                                    }).ToList();

                results.ReturnsSummary = merchantActivity.transactions
                                    .Where(x => returnsXctTypes.Contains(x.xct_type))
                                    .OrderBy(x => x.xct_type)
                                    .GroupBy(x => new { x.xct_type })
                                    .Select(x => new XctTypeDailySummaryBE()
                                    {
                                        XctType = x.Key.xct_type,
                                        SuccessXctCount = x.Where(r => r.is_Auth_Failed == false).Count(),
                                        SuccessXctSubtotalValue = x.Where(r => r.is_Auth_Failed == false).Sum(r => r.xct_amount),
                                        FailureXctCount = x.Where(r => r.is_Auth_Failed == true).Count(),
                                        FailureXctSubtotalValue = x.Where(r => r.is_Auth_Failed == true).Sum(r => r.xct_amount),
                                    }).ToList();

                results.ChargebacksSummary = merchantActivity.transactions
                                    .Where(x => chargeBacksXctTypes.Contains(x.xct_type))
                                    .OrderBy(x => x.xct_type)
                                    .GroupBy(x => new { x.xct_type })
                                    .Select(x => new XctTypeDailySummaryBE()
                                    {
                                        XctType = x.Key.xct_type,
                                        SuccessXctCount = x.Where(r => r.is_Auth_Failed == false).Count(),
                                        SuccessXctSubtotalValue = x.Where(r => r.is_Auth_Failed == false).Sum(r => r.xct_amount),
                                        FailureXctCount = x.Where(r => r.is_Auth_Failed == true).Count(),
                                        FailureXctSubtotalValue = x.Where(r => r.is_Auth_Failed == true).Sum(r => r.xct_amount),
                                    }).ToList();

            }

            return results;
        }

        #endregion

        #region === Generate Transactions ========================================
        public static int GenerateSampleXcts(int merchantId, DateTime xctPostingDate)
        {
            // get merchant metadata (MDB ??)
            var merchant = MongoDBContext.FindMerchantById(merchantId);

            int xctsGenerated = 0;

            if (merchant.setup_options.supports_cp_xcts)
            {
                xctsGenerated += GenerateSalesXcts(merchantId, xctPostingDate, Enums.TRANSACTION_TYPE.cp_sale);
            }
            if (merchant.setup_options.supports_cp_xcts)
            {
                xctsGenerated += GenerateSalesXcts(merchantId, xctPostingDate, Enums.TRANSACTION_TYPE.cnp_sale);
            }
            if (merchant.setup_options.supports_returns_xcts)
            {
                xctsGenerated += GenerateRefundXcts(merchantId, xctPostingDate);
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
        public static int GenerateSalesXcts(int merchantId, DateTime xctPostingDate, Enums.TRANSACTION_TYPE xctType )
        {
            // get merchant metadata (MDB ??)
            var merchant = MongoDBContext.FindMerchantById(merchantId);

            if((xctType == Enums.TRANSACTION_TYPE.cp_sale && !merchant.setup_options.supports_cp_xcts)
                || (xctType == Enums.TRANSACTION_TYPE.cnp_sale && !merchant.setup_options.supports_cnp_xcts))
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
                    xct_posting_date = xctPostingDate,
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

        public static int GenerateRefundXcts(int merchantId, DateTime xctPostingDate)
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
                    xct_posting_date = xctPostingDate,
                    open_start_dt = DateTime.Now.ToUniversalTime()
                };

                MongoDBContext.InsertMerchantDailyActivity(merchantActivity);
            }
            #endregion

            int xctCntToGenerate = new Random().Next(1, 3);
            Random amountGenerator = new Random();

            var transactions = new List<TransactionMBE>();

            // create a random # of purchase xcts
            for (int loopCtr = 1; loopCtr <= xctCntToGenerate; loopCtr++)
            {
                transactions.Add(new TransactionMBE()
                {
                    terminal_id = merchant.terminals.OrderBy(t => Guid.NewGuid()).First().terminal_id,
                    card_data = _paymentCards.OrderBy(t => Guid.NewGuid()).First(),
                    xct_amount = Math.Round(new decimal(amountGenerator.NextDouble() * -100.0), 2),
                    xct_dt = DateTime.Now.ToUniversalTime(),
                    xct_id = Guid.NewGuid(),
                    xct_type = Enums.TRANSACTION_TYPE.credit_return
                });
            }

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
                    xct_posting_date = xctPostingDate,
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
                                supports_cnp_xcts = false,
                                supports_cp_xcts = true,
                                supports_returns_xcts = false
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
                                supports_cnp_xcts = true,
                                supports_cp_xcts = true,
                                supports_returns_xcts = true
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
                                supports_cnp_xcts = true,
                                supports_cp_xcts = true,
                                supports_returns_xcts = false
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
                                supports_cnp_xcts = false,
                                supports_cp_xcts = true,
                                supports_returns_xcts = true
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
                                supports_cnp_xcts = true,
                                supports_cp_xcts = true,
                                supports_returns_xcts = false
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
                            supports_cnp_xcts = true,
                            supports_cp_xcts = true,
                            supports_returns_xcts = true
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
                            supports_cnp_xcts = false,
                            supports_cp_xcts = true,
                            supports_returns_xcts = false
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
                            supports_cnp_xcts = true,
                            supports_cp_xcts = true,
                            supports_returns_xcts = false
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
                            supports_cnp_xcts = false,
                            supports_cp_xcts = true,
                            supports_returns_xcts = false
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

        public static void ResetAllXctsForMerchantDate(int merchantId, DateTime xctPostingDate)
        {
            MongoDBContext.DeleteAllMerchantDailyActivity(merchantId, xctPostingDate);
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
