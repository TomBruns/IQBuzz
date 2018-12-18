using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WP.Learning.BizLogic.Shared.Entities;
using WP.Learning.BizLogic.Shared.SMS;
using WP.Learning.BizLogic.Shared.Utilties;
using WP.Learning.MongoDB;
using WP.Learning.MongoDB.Entities;

namespace WP.Learning.BizLogic.Shared.Merchant
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

        #region === Welcome Message =====================================================

        /// <summary>
        /// Build the text for the Welcome Message
        /// </summary>
        /// <param name="merchant"></param>
        /// <returns></returns>
        public static string BuildWelcomeMessage(MerchantMBE merchant)
        {
            var welcomeMsg = $"Welcome {merchant.merchant_name } to IQ Buzz\n" +
                $"Reply YES to confirm enrollment in {GeneralConstants.APP_NAME}. Msg&Data rates may appy. Msg freq varies by acct and prefs.";

            return welcomeMsg;
        }
        
        /// <summary>
        /// Used by the Backoffice to push a welcome message
        /// </summary>
        /// <param name="merchantId"></param>
        public static void SendWelcomeMessage(int merchantId)
        {
            var merchant = MongoDBContext.FindMerchantById(merchantId);

            var welcomeMsg = BuildWelcomeMessage(merchant);

            TwilioController.SendSMSMessage(merchant.primary_contact.phone_no, $"{welcomeMsg}");
        }

        /// <summary>
        /// Process Welcome Acceptance reponse
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="isAccepted"></param>
        /// <returns></returns>
        public static string StoreAcceptWelcomeMessageResponse(int merchantId, bool isAccepted)
        {
            // get merchant metadata (MDB ??)
            var merchant = MongoDBContext.FindMerchantById(merchantId);

            string returnMsg = string.Empty;

            // they have already accepted
            if(merchant.setup_options.is_accepted_welcome_agreement)
            {
                returnMsg = "Thanks for replying, you have already accepted!, Hint: You can always text HELP? or ??? to see a list of commands.";
            }
            // they are accepting or declining now
            else if(isAccepted)
            {
                merchant.setup_options.is_accepted_welcome_agreement = isAccepted;
                MongoDBContext.UpdateMerchant(merchant);

                returnMsg = isAccepted 
                        ? $"Welcome to {GeneralConstants.APP_NAME}, you are all setup! Hint: You can always text HELP? or ??? to see a list of commands." 
                        : "We are sorry you choose not to join, text JOIN at any time to have another opportunity to accept.";
            }

            return returnMsg;
        }

        // Command: Unjoin
        public static string ResetAcceptedJoin(int merchantId)
        {
            var merchant = MongoDBContext.FindMerchantById(merchantId);

            merchant.setup_options.is_accepted_welcome_agreement = false;

            MongoDBContext.UpdateMerchant(merchant);

            string returnMsg = "Welcome Acceptance reset";

            return returnMsg;
        }

        #endregion

        // Command: Summary 
        public static string BuildOverallSummaryMessage(int merchantId, DateTime xctPostingDate)
        {
            MerchantMBE merchant = MongoDBContext.FindMerchantById(merchantId);

            XctDailySummaryBE xctSummary = MerchantController.GetXctDailySummary(merchantId, xctPostingDate);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Merchant Account Summary for: {xctPostingDate:ddd MMM dd, yyyy} as of {DateTime.Now.ToString("h:mm tt")} ");
            sb.AppendLine("\n");

            if (xctSummary != null)
            {
                foreach (var xctType in xctSummary.SummaryByXctType)
                {
                    sb.AppendLine($"{xctType.XctTypeDesc}:");
                    sb.AppendLine($"{xctType.XctTotalValue:C} [{xctType.XctCount} txns]");

                    if(xctType.XctType == Enums.TRANSACTION_TYPE.chargeback)
                    {
                        sb.AppendLine("  (text CBACK for details)");
                    }

                    sb.AppendLine("\n");
                }
                sb.AppendLine($"Net Total:  {xctSummary.SummaryByXctType.Sum(x => x.XctTotalValue):C}");
                sb.AppendLine("(all Card Transactions)");
                sb.AppendLine("\n");
                sb.AppendLine($"Final settlement amount will be deposited into your checking account ending in {merchant.setup_options.debit_card_no.Substring(1,4)} on {DateTime.Now.AddBusinessDays(2).ToString("ddd MMM dd, yyyy")} -- to" +
                    " receive these funds tomorrow morning reply FAF");
            }
            else
            {
                sb.AppendLine(@"No activity yet for today.");
            }

            return sb.ToString();
        }

        // Command: Sales
        public static string BuildSalesSummaryMessage(int merchantId, DateTime xctPostingDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Merchant Account Summary as of: {DateTime.Now.ToString("ddd MMM dd, yyyy h:mm tt")}");
            sb.AppendLine();

            var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);

            if (merchantActivity != null && merchantActivity.transactions != null && merchantActivity.transactions.Count() > 0)
            {
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
        public static string BuildReturnsSummaryMessage(int merchantId, DateTime xctPostingDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Merchant Account Summary as of {DateTime.Now.ToString("ddd MMM dd, yyyy h:mm tt")}");
            sb.AppendLine();
            sb.AppendLine($"Returns:");

            var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);

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
        public static string BuildChargebackDetails(int merchantId, DateTime xctPostingDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Pending Chargeback Details as of {DateTime.Now.ToString("ddd MMM dd, yyyy h:mm tt")}");
            sb.AppendLine(@"---------------------");

            var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);

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

                        if (chargebackCount > 1)
                        {
                            sb.AppendLine("\n");
                        }

                        DateTime dueDate = chargebackXct.xct_dt.AddDays(3);

                        sb.AppendLine($"{chargebackXct.xct_amount:C} [Card (Last 4): {chargebackXct.card_data.primary_account_no.Right(4)}] Respond By: {dueDate.ToString("ddd MMM dd, yyyy")}");
                    }
                    sb.AppendLine(@"=====================");
                    sb.AppendLine($"Total: {chargebackXcts.Sum(x => x.xct_amount):C}  Qty: {chargebackCount}");
                    sb.AppendLine($"  To respond to your Chargebacks, go to IQ: {GeneralConstants.CHARGEBACK_URL}");
                }
                else
                {
                    sb.AppendLine($"$0.00 [0 txns]");
                    //sb.AppendLine(@"There are no outstanding chargebacks.");
                }
            }
            else {
                //sb.AppendLine($"$0.00 [0 txns]");
                sb.AppendLine(@"You have no pending chargebacks!");
            }

            return sb.ToString();
        }

        // Command: Close
        public static void FireAllTerminalsClosedEvent(int merchantId)
        {
            var merchant = MongoDBContext.FindMerchantById(merchantId);
            DateTime xctPostingDate = DateTime.Today;

            var xctSummaryMsg = BuildOverallSummaryMessage(merchantId, xctPostingDate);

            TwilioController.SendSMSMessage(merchant.primary_contact.phone_no, xctSummaryMsg);
        }

        // Command: Settings 
        public static string BuildConfigMessage(int merchantId)
        {
            var configMsg = $"To configure your daily summaries, alerts, sign-up for FastAccess Funding and adjust batch time, go to {GeneralConstants.CFG_URL}";

            return configMsg;
        }

        #region === FAF Messages =========================================
        
        // Command: FAF
        public static string BuildFAFMessage(int merchantId, DateTime xctPostingDate)
        {
            string response = string.Empty;

            var merchant = MongoDBContext.FindMerchantById(merchantId);
            var merchantDailyActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);

            if (merchantDailyActivity == null)
            {
                merchantDailyActivity = new MerchantDailyActivityMBE() { merchant_id = merchantId, xct_posting_date = xctPostingDate };
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
        public static string BuildConfirmFAFMessage(int merchantId, DateTime xctPostingDate)
        {
            string response = string.Empty;

            var merchantDailyActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);

            if (merchantDailyActivity == null)
            {
                merchantDailyActivity = new MerchantDailyActivityMBE() { merchant_id = merchantId, xct_posting_date = xctPostingDate };
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
        public static string BuildUndoFAFMessage(int merchantId, DateTime xctPostingDate)
        {
            string response = string.Empty;

            var merchantDailyActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);

            if (merchantDailyActivity == null)
            {
                merchantDailyActivity = new MerchantDailyActivityMBE() { merchant_id = merchantId, xct_posting_date = xctPostingDate };
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

                var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchantId, xctPostingDate);
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

            if (merchantActivity != null 
                && merchantActivity.transactions != null 
                && merchantActivity.transactions.Count > 0)
            {
                results = new XctDailySummaryBE();

                results.SummaryByXctType = merchantActivity.transactions
                                    .OrderBy(x => x.xct_type)
                                    .GroupBy(x => new { x.xct_type, x.is_Auth_Failed})
                                    .Select(x => new XctTypeDailySummaryBE()
                                    {
                                        XctType = x.Key.xct_type,
                                        XctCount = x.Count(),
                                        XctTotalValue = x.Sum(r => r.xct_amount),
                                        isAuthFailure = x.Key.is_Auth_Failed
                                    }).ToList();

            }

            return results;
        }

        #endregion

        #region === Generate Transactions ========================================
        /// <summary>
        /// Create and store a set of random transactions on the specified merchant
        /// </summary>
        /// <remarks>
        /// Used to drive demos
        /// </remarks>
        public static int GenerateSalesXcts(int merchantId, DateTime xctPostingDate)
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
                    xct_posting_date = xctPostingDate
                };

                MongoDBContext.InsertMerchantDailyActivity(merchantActivity);
            }
            #endregion

            int xctCntToGenerate = new Random().Next(5, 25);
            Random amountGenerator = new Random();

            var transactions = new List<TransactionMBE>();

            // create a random # of purchase xcts
            for (int loopCtr = 1; loopCtr <= xctCntToGenerate; loopCtr++)
            {
                int cardPresentIndicator = new Random().Next(1, 100);

                transactions.Add(new TransactionMBE()
                {
                    terminal_id = merchant.terminals.OrderBy(t => Guid.NewGuid()).First().terminal_id,
                    card_data = _paymentCards.OrderBy(t => Guid.NewGuid()).First(),
                    xct_amount = Math.Round(new decimal(amountGenerator.NextDouble() * 1000.0), 2),
                    xct_dt = DateTime.Now,
                    xct_id = Guid.NewGuid(),
                    xct_type = (cardPresentIndicator % 2 == 0) ?
                            Enums.TRANSACTION_TYPE.cp_sale : Enums.TRANSACTION_TYPE.cnp_sale,
                    is_Auth_Failed = (xctCntToGenerate > 10 && loopCtr == xctCntToGenerate) ? true : false
                });
            }

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
                    xct_posting_date = xctPostingDate
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
                    xct_dt = DateTime.Now,
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
                    xct_posting_date = xctPostingDate
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

                DateTime xctDate = DateTime.Now.AddDays(deltaDays);

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

        public static int GenerateSampleXcts(int merchantId, DateTime xctPostingDate)
        {
            int xctsGenerated = 0;

            xctsGenerated += GenerateSalesXcts(merchantId, xctPostingDate);
            xctsGenerated += GenerateRefundXcts(merchantId, xctPostingDate);
            xctsGenerated += GenerateChargebacksXcts(merchantId, xctPostingDate);

            return xctsGenerated;
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
                            merchant_name = @"Tom's Deli",
                            primary_contact = new ContactMBE()
                            {
                                first_name = @"Tom",
                                last_name = @"Bruns",
                                phone_no = GeneralConstants.TOMS_PHONE_NO,
                                email_address = @"xtobr39@hotmail.com"
                            },
                            setup_options = new SetupOptionsMBE()
                            {
                                is_host_data_capture_enabled = true,
                                auto_close_hh_mm = new TimeSpan(19, 0, 0),
                                is_fast_funding_enabled = true,
                                debit_card_no = @"1234567890123401"
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
                            primary_contact = new ContactMBE()
                            {
                                first_name = @"Marco",
                                last_name = @"Fernandes",
                                phone_no = GeneralConstants.MARCOS_PHONE_NO,
                                email_address = @"Marco.Fernandes@worldpay.com"
                            },
                            setup_options = new SetupOptionsMBE()
                            {
                                is_host_data_capture_enabled = true,
                                auto_close_hh_mm = new TimeSpan(19, 0, 0),
                                is_fast_funding_enabled = true,
                                debit_card_no = @"1234567890126702"
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
                            primary_contact = new ContactMBE()
                            {
                                first_name = @"Dusty",
                                last_name = @"Gomez",
                                phone_no = GeneralConstants.DUSTYS_PHONE_NO,
                                email_address = @"Dusty.Gomez@worldpay.com"
                            },
                            setup_options = new SetupOptionsMBE()
                            {
                                is_host_data_capture_enabled = true,
                                auto_close_hh_mm = new TimeSpan(19, 0, 0),
                                is_fast_funding_enabled = true,
                                debit_card_no = @"1234567890125203"
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
                            primary_contact = new ContactMBE()
                            {
                                first_name = @"Josh",
                                last_name = @"Byrne",
                                phone_no = GeneralConstants.JOSHS_PHONE_NO,
                                email_address = @"Joshua.Byrne@worldpay.com"
                            },
                            setup_options = new SetupOptionsMBE()
                            {
                                is_host_data_capture_enabled = true,
                                auto_close_hh_mm = new TimeSpan(19, 0, 0),
                                is_fast_funding_enabled = true,
                                debit_card_no = @"1234567890122704"
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
                            primary_contact = new ContactMBE()
                            {
                                first_name = @"Alex",
                                last_name = @"Boeding",
                                phone_no = GeneralConstants.ALEXS_PHONE_NO,
                                email_address = @"Axex.Boeding@worldpay.com"
                            },
                            setup_options = new SetupOptionsMBE()
                            {
                                is_host_data_capture_enabled = true,
                                auto_close_hh_mm = new TimeSpan(19, 0, 0),
                                is_fast_funding_enabled = true,
                                debit_card_no = @"1234567890129905"
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
                        primary_contact = new ContactMBE()
                        {
                            first_name = @"Pallavi",
                            last_name = @"TBD",
                            phone_no = GeneralConstants.PALLAVI_PHONE_NO,
                            email_address = @"Pallavi.TBD@worldpay.com"
                        },
                        setup_options = new SetupOptionsMBE()
                        {
                            is_host_data_capture_enabled = true,
                            auto_close_hh_mm = new TimeSpan(19, 0, 0),
                            is_fast_funding_enabled = true,
                            debit_card_no = @"1234567890121106"
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
                        primary_contact = new ContactMBE()
                        {
                            first_name = @"Joe",
                            last_name = @"Pellar",
                            phone_no = GeneralConstants.JOES_PHONE_NO,
                            email_address = @"Joe.Pellar@worldpay.com"
                        },
                        setup_options = new SetupOptionsMBE()
                        {
                            is_host_data_capture_enabled = true,
                            auto_close_hh_mm = new TimeSpan(19, 0, 0),
                            is_fast_funding_enabled = true,
                            debit_card_no = @"1234567890121107"
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
                        primary_contact = new ContactMBE()
                        {
                            first_name = @"Jianan",
                            last_name = @"Hou",
                            phone_no = GeneralConstants.JAKES_PHONE_NO,
                            email_address = @"Jianan.Hou@worldpay.com"
                        },
                        setup_options = new SetupOptionsMBE()
                        {
                            is_host_data_capture_enabled = true,
                            auto_close_hh_mm = new TimeSpan(19, 0, 0),
                            is_fast_funding_enabled = true,
                            debit_card_no = @"1234567890122208"
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
                        primary_contact = new ContactMBE()
                        {
                            first_name = @"Jon",
                            last_name = @"Pollock",
                            phone_no = @"+16153309751",
                            email_address = @"Jon.Pollock@worldpay.com"
                        },
                        setup_options = new SetupOptionsMBE()
                        {
                            is_host_data_capture_enabled = true,
                            auto_close_hh_mm = new TimeSpan(19, 0, 0),
                            is_fast_funding_enabled = true,
                            debit_card_no = @"1234567890122208"
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

        public static void FireClosedEventsForAllMerchants()
        {
            Dictionary<int, MerchantMBE> merchants = BuildListOfMerchants();

            foreach (KeyValuePair<int, MerchantMBE> kvp in merchants)
            {
                FireAllTerminalsClosedEvent(kvp.Key);
            }
        }

        #endregion
    }
}
