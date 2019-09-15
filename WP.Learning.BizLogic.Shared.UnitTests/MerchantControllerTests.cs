using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using XUnitPriorityOrderer;

using WP.Learning.MongoDB;
using WP.Learning.BizLogic.Shared.Controllers;

namespace WP.Learning.BizLogic.Shared.UnitTests
{
    public class MerchantControllerTests : BaseClassTests, IClassFixture<MerchantControllerTestsGlobal>
    {
        MerchantControllerTestsGlobal _merchantControllerTestsGlobal;

        // test setup
        public MerchantControllerTests(MerchantControllerTestsGlobal merchantControllerTestsGlobal)
        {
            _merchantControllerTestsGlobal = merchantControllerTestsGlobal;
        }

        [Fact, Order(1)]
        public void TestBuildSalesSummaryMessage()
        {
            MerchantController.BuildSalesSummaryMessage(new List<int> { 1 }, DateTime.Today, @"EST");
        }

        //[Fact, Order(4)]
        //public void TestFindMerchantByPhoneNo()
        //{
        //    string phone_no = _merchantControllerTestsGlobal.PHONE_NO;
        //    int merchant_id = _merchantControllerTestsGlobal.MERCHANT_ID;

        //    var merchant = MongoDBContext.FindMerchantByPrimaryContactPhoneNo(phone_no);

        //    Assert.NotNull(merchant);
        //    Assert.Equal(merchant_id, merchant.merchant_id);
        //}

        [Fact, Order(5)]
        public void TestFindMerchantByUnknownPhoneNoThrowsEx()
        {
            string phone_no = @"+0000000000";

            Assert.Throws<InvalidOperationException>(() => MongoDBContext.FindMerchantByPrimaryContactPhoneNo(phone_no));
        }
    }

    public class MerchantControllerTestsGlobal : IDisposable
    {
        public MerchantControllerTestsGlobal() { }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
        }
    }
}
