using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using XUnitPriorityOrderer;

using WP.Learning.BizLogic.Shared.Merchant;

namespace WP.Learning.MongoDB.UnitTests
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
            MerchantController.BuildSalesSummaryMessage(1, DateTime.Today, @"EST");
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
