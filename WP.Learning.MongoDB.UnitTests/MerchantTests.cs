using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using MongoDB.Driver;
using Xunit;
using XUnitPriorityOrderer;

using WP.Learning.MongoDB;
using WP.Learning.MongoDB.Entities;
using WP.Learning.BizLogic.Shared.Merchant;

namespace WP.Learning.MongoDB.UnitTests
{
    // https://xunit.github.io/docs/comparisons.html
    // http://hamidmosalla.com/2018/08/16/xunit-control-the-test-execution-order/
    public class MerchantTests : BaseClassTests, IClassFixture<MerchantTestsGlobal>
    {
        MerchantTestsGlobal _merchantTestsGlobal;

        // test setup
        public MerchantTests(MerchantTestsGlobal merchantTestsGlobal)
        {
            _merchantTestsGlobal = merchantTestsGlobal;
        }

        // ========================
        // Merchant Tests
        // ========================
        [Fact, Order(1)]
        public void TestInsertMerchant()
        {
            var merchant = new MerchantMBE()
            {
                merchant_id = _merchantTestsGlobal.MERCHANT_ID,
                merchant_name = @"Tom's Pet Shop",
                primary_contact = new ContactMBE()
                {
                    first_name = @"Tom",
                    last_name = @"Bruns",
                    phone_no = _merchantTestsGlobal.PHONE_NO,
                    email_address = @"xtobr39@hotmail.com"
                },
                setup_options = new SetupOptionsMBE()
                {
                    is_host_data_capture_enabled = true,
                    auto_close_hh_mm = new TimeSpan(19, 0, 0),
                    is_fast_funding_enabled = true
                },
                terminals = new List<TerminalMBE>()
                {
                    new TerminalMBE() {terminal_id = "TID-001", terminal_type = @"610", terminal_desc = @"Checkout 1"},
                    new TerminalMBE() {terminal_id = "TID-002", terminal_type = @"610", terminal_desc = @"Checkout 2"},
                }
            };

            MongoDBContext.InsertMerchant(merchant);
        }

        [Fact, Order(2)]
        public void TestInsertDupMerchantThrowsEx()
        {
            MerchantMBE merchant = new MerchantMBE()
            {
                merchant_id = _merchantTestsGlobal.MERCHANT_ID,
                merchant_name = @"Tom's Pet Shop",
                primary_contact = new ContactMBE()
                {
                    first_name = @"Tom",
                    last_name = @"Bruns",
                    phone_no = _merchantTestsGlobal.PHONE_NO,
                    email_address = @"xtobr39@hotmail.com"
                },
                setup_options = new SetupOptionsMBE()
                {
                    is_host_data_capture_enabled = true,
                    auto_close_hh_mm = new TimeSpan(19, 0, 0),
                    is_fast_funding_enabled = true
                },
                terminals = new List<TerminalMBE>()
                {
                    new TerminalMBE() {terminal_id = "TID-001", terminal_type = @"610", terminal_desc = @"Checkout 1"},
                    new TerminalMBE() {terminal_id = "TID-002", terminal_type = @"610", terminal_desc = @"Checkout 2"},
                }
            };

            // this tests our unique index
            Assert.Throws<MongoWriteException>(() => MongoDBContext.InsertMerchant(merchant));
        }

        [Fact, Order(3)]
        public void TestFindMerchantById()
        {
            int merchant_id = _merchantTestsGlobal.MERCHANT_ID;

            var merchant = MongoDBContext.FindMerchantById(merchant_id);

            Assert.NotNull(merchant);
            Assert.Equal(merchant_id, merchant.merchant_id);
        }

        [Fact, Order(4)]
        public void TestFindMerchantByPhoneNo()
        {
            string phone_no = _merchantTestsGlobal.PHONE_NO;
            int merchant_id = _merchantTestsGlobal.MERCHANT_ID;

            var merchant = MongoDBContext.FindMerchantByPrimaryContactPhoneNo(phone_no);

            Assert.NotNull(merchant);
            Assert.Equal(merchant_id, merchant.merchant_id);
        }

        [Fact, Order(5)]
        public void TestFindMerchantByUnknownPhoneNoThrowsEx()
        {
            string phone_no = @"+0000000000";

            Assert.Throws<InvalidOperationException>(() => MongoDBContext.FindMerchantByPrimaryContactPhoneNo(phone_no));
        }

        [Fact, Order(6)]
        public void TestFindNonExistentMerchantById()
        {
            var merchant = MongoDBContext.FindMerchantById(-1);

            Assert.Null(merchant);
        }

        // ========================
        // Merchant Daily Activity Tests
        // ========================
        [Fact, Order(10)]
        public void TestInsertMerchantDailyActivity()
        {
            var activity = new MerchantDailyActivityMBE()
            {
                merchant_id = _merchantTestsGlobal.MERCHANT_ID,
                xct_posting_date = _merchantTestsGlobal.XCT_POSTING_DATE
            };

            MongoDBContext.InsertMerchantDailyActivity(activity);
        }

        [Fact, Order(11)]
        public void TestInsertDupMerchantDailyActivityThrowsEx()
        {
            var activity = new MerchantDailyActivityMBE()
            {
                merchant_id = _merchantTestsGlobal.MERCHANT_ID,
                xct_posting_date = _merchantTestsGlobal.XCT_POSTING_DATE
            };

            // this tests our unique index
            Assert.Throws<MongoWriteException>(() => MongoDBContext.InsertMerchantDailyActivity(activity));
        }

        [Fact, Order(12)]
        public void TestFindMerchantDailyActivity()
        {
            int merchant_id = _merchantTestsGlobal.MERCHANT_ID;
            DateTime xct_posting_date = _merchantTestsGlobal.XCT_POSTING_DATE;

            var merchantActivity = MongoDBContext.FindMerchantDailyActivity(merchant_id, xct_posting_date);

            Assert.NotNull(merchantActivity);
            Assert.Equal(merchant_id, merchantActivity.merchant_id);
            // local time is converted to utc when store to mongo
            Assert.Equal(xct_posting_date, merchantActivity.xct_posting_date);
        }

        [Fact, Order(13)]
        public void TestUpdateMerchantDailyActivity1stTerminal()
        {
            int merchant_id = _merchantTestsGlobal.MERCHANT_ID;
            DateTime xct_posting_date = _merchantTestsGlobal.XCT_POSTING_DATE;

            var merchant = MongoDBContext.FindMerchantById(merchant_id);
            Assert.NotNull(merchant);

            var terminal = merchant.terminals.OrderBy(t => t.terminal_id).FirstOrDefault();
            Assert.NotNull(terminal);

            var terminalsStatus = new List<TerminalStatusMBE>()
            {
                new TerminalStatusMBE() { terminal_id = terminal.terminal_id, open_dt = xct_posting_date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute)}
            };

            MongoDBContext.UpsertMerchantDailyActivity(merchant_id, xct_posting_date, terminalsStatus);
        }

        [Fact, Order(14)]
        public void TestUpdateMerchantDailyActivityMoreTerminals()
        {
            int merchant_id = _merchantTestsGlobal.MERCHANT_ID;
            DateTime xct_posting_date = _merchantTestsGlobal.XCT_POSTING_DATE;

            var merchant = MongoDBContext.FindMerchantById(merchant_id);
            Assert.NotNull(merchant);

            var terminals = merchant.terminals.OrderBy(t => t.terminal_id).Skip(1).ToList();
            Assert.NotNull(terminals);

            var terminalsStatus = from t in terminals
                                  select new TerminalStatusMBE()
                                  {
                                      terminal_id = t.terminal_id,
                                      open_dt = xct_posting_date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute)
                                  };

            MongoDBContext.UpsertMerchantDailyActivity(merchant_id, xct_posting_date, terminalsStatus.ToList());
        }

        [Fact, Order(15)]
        public void TestUpdateMerchantDailyActivity1stXct()
        {
            int merchant_id = _merchantTestsGlobal.MERCHANT_ID;
            DateTime xct_posting_date = _merchantTestsGlobal.XCT_POSTING_DATE;

            var merchant = MongoDBContext.FindMerchantById(merchant_id);
            Assert.NotNull(merchant);

            var terminal = merchant.terminals.OrderBy(t => t.terminal_id).FirstOrDefault();
            Assert.NotNull(terminal);

            var transactions = new List<TransactionMBE>()
            {
                new TransactionMBE()
                {
                    terminal_id = terminal.terminal_id,  
                    card_data = _merchantTestsGlobal.PAYMENT_CARDS.First(),
                    xct_amount = 1234.56M,
                    xct_dt = DateTime.Now,
                    xct_id = Guid.NewGuid(),
                    xct_type = Enums.TRANSACTION_TYPE.cp_sale
                }
            };

            MongoDBContext.UpsertMerchantDailyActivity(merchant_id, xct_posting_date, transactions);
        }

        [Fact, Order(16)]
        public void TestUpdateMerchantDailyActivityMoreXcts()
        {
            int merchant_id = _merchantTestsGlobal.MERCHANT_ID;
            DateTime xct_posting_date = _merchantTestsGlobal.XCT_POSTING_DATE;

            var merchant = MongoDBContext.FindMerchantById(merchant_id);
            Assert.NotNull(merchant);

            Assert.NotNull(merchant.terminals);

            int xctCntToGenerate = new Random().Next(1, 10);
            Random amountGenerator = new Random();

            var transactions = new List<TransactionMBE>();

            for(int loopCtr = 1; loopCtr <= xctCntToGenerate; loopCtr++)
            {
                transactions.Add(new TransactionMBE()
                {
                    terminal_id = merchant.terminals.OrderBy(t => Guid.NewGuid()).First().terminal_id,
                    card_data = _merchantTestsGlobal.PAYMENT_CARDS.OrderBy(t => Guid.NewGuid()).First(),
                    xct_amount = Math.Round(new decimal(amountGenerator.NextDouble() * 1000.0), 2),
                    xct_dt = DateTime.Now,
                    xct_id = Guid.NewGuid(),
                    xct_type = Enums.TRANSACTION_TYPE.cp_sale
                });
            }

            MongoDBContext.UpsertMerchantDailyActivity(merchant_id, xct_posting_date, transactions);
        }
    }
}

public class MerchantTestsGlobal : IDisposable
{
    public int MERCHANT_ID { get; set; }
    public string PHONE_NO { get; set; }
    public DateTime XCT_POSTING_DATE { get; set; }
    public List<PaymentCardDataMBE> PAYMENT_CARDS { get; set; }

    public MerchantTestsGlobal()
    {
        // Do "global" initialization here; Only called once.
        var mdaDeleteResult = MongoDBContext.DeleteAllMerchantsDailyActivity();

        var mDeleteResult = MongoDBContext.DeleteAllMerchants();

        // init global variables for test run
        this.MERCHANT_ID = 1;
        this.PHONE_NO = @"+15134986016";
        // local time is converted to utc when store to mongo
        this.XCT_POSTING_DATE = DateTime.Today.ToUniversalTime();

        this.PAYMENT_CARDS = new List<PaymentCardDataMBE>()
        {
            new PaymentCardDataMBE()
            {
                primary_account_no = @"4111111111111111",
                name = @"Visa User",
                additional_data =new AddtionalDataMBE()
                {
                    expiration_date = @"2001",  // YYMM
                    service_code = @"XYZ"
                },
                discretionary_data = @"Foo"
            },
            new PaymentCardDataMBE()
            {
                primary_account_no = @"5555555555554444",
                name = @"Mastercard User",
                additional_data =new AddtionalDataMBE()
                {
                    expiration_date = @"2106",  // YYMM
                    service_code = @"ABC"
                },
                discretionary_data = @"Bar"
            },
            new PaymentCardDataMBE()
            {
                primary_account_no = @"378282246310005",
                name = @"AmEx User",
                additional_data =new AddtionalDataMBE()
                {
                    expiration_date = @"2212",  // YYMM
                    service_code = @"DEF"
                },
                discretionary_data = @"42"
            }
        };
    }

    public void Dispose()
    {
        // Do "global" teardown here; Only called once.
    }
}
