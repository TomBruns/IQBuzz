using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using MongoDB.Driver;
using Xunit;
using XUnitPriorityOrderer;

using WP.Learning.MongoDB;
using WP.Learning.MongoDB.Entities;
using WP.Learning.BizLogic.Shared.SMS;

namespace WP.Learning.MongoDB.UnitTests
{
    public class ConfigDataTests : BaseClassTests, IClassFixture<ConfigDataTestsGlobal>
    {
        ConfigDataTestsGlobal _configDataTestsGlobal;

        // test setup
        public ConfigDataTests(ConfigDataTestsGlobal configDataTestsGlobal)
        {
            _configDataTestsGlobal = configDataTestsGlobal;
        }

        [Fact, Order(1)]
        public void TestInsertSMSConfigData()
        {
            MongoDBContext.InsertConfigData(TwilioController.ACCOUNT_SID_ITEM_NAME, _configDataTestsGlobal.SID_TOKEN);
            MongoDBContext.InsertConfigData(TwilioController.AUTH_TOKEN_ITEM_NAME, _configDataTestsGlobal.AUTH_TOKEN);
            MongoDBContext.InsertConfigData(TwilioController.PHONE_NUMBER_ITEM_NAME, _configDataTestsGlobal.PHONE_NO);
        }

        [Fact, Order(2)]
        public void TestGetSMSConfigData()
        {
            var allConfigData = MongoDBContext.GetAllConfigData();
            Assert.NotNull(allConfigData);
            Assert.NotEmpty(allConfigData);

            Assert.Equal(allConfigData.Where(cd => cd.name == TwilioController.ACCOUNT_SID_ITEM_NAME).FirstOrDefault().value, _configDataTestsGlobal.SID_TOKEN);
            Assert.Equal(allConfigData.Where(cd => cd.name == TwilioController.AUTH_TOKEN_ITEM_NAME).FirstOrDefault().value, _configDataTestsGlobal.AUTH_TOKEN);
            Assert.Equal(allConfigData.Where(cd => cd.name == TwilioController.PHONE_NUMBER_ITEM_NAME).FirstOrDefault().value, _configDataTestsGlobal.PHONE_NO);
        }
    }

    public class ConfigDataTestsGlobal : IDisposable
    {
        public string SID_TOKEN { get; set; }
        public string AUTH_TOKEN { get; set; }
        public string PHONE_NO { get; set; }

        public ConfigDataTestsGlobal()
        {
            // Do "global" initialization here; Only called once.
            var cdDeleteResult = MongoDBContext.DeleteAllConfigData();

            // init global variables for test run
            this.SID_TOKEN = @"AC8a0322c81593dc526baacfbfe6a0220a";
            this.AUTH_TOKEN = @"8594f8e7b2b3b47275eb15b6f7fffe1e";
            this.PHONE_NO = @"+18125944088";
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
        }
    }
}
