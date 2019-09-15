using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using MongoDB.Driver;
using Xunit;
using XUnitPriorityOrderer;

using WP.Learning.MongoDB;
using WP.Learning.MongoDB.Entities;

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
            MongoDBContext.InsertConfigData(Constants.ACCOUNT_SID_ITEM_NAME, _configDataTestsGlobal.SID_TOKEN);
            MongoDBContext.InsertConfigData(Constants.AUTH_TOKEN_ITEM_NAME, _configDataTestsGlobal.AUTH_TOKEN);
            MongoDBContext.InsertConfigData(Constants.PHONE_NUMBER_ITEM_NAME, _configDataTestsGlobal.PHONE_NO);
        }

        [Fact, Order(2)]
        public void TestGetSMSConfigData()
        {
            var allConfigData = MongoDBContext.GetAllConfigData();
            Assert.NotNull(allConfigData);
            Assert.NotEmpty(allConfigData);

            Assert.Equal(allConfigData.Where(cd => cd.name == Constants.ACCOUNT_SID_ITEM_NAME).FirstOrDefault().value, _configDataTestsGlobal.SID_TOKEN);
            Assert.Equal(allConfigData.Where(cd => cd.name == Constants.AUTH_TOKEN_ITEM_NAME).FirstOrDefault().value, _configDataTestsGlobal.AUTH_TOKEN);
            Assert.Equal(allConfigData.Where(cd => cd.name == Constants.PHONE_NUMBER_ITEM_NAME).FirstOrDefault().value, _configDataTestsGlobal.PHONE_NO);
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
            this.SID_TOKEN = @"AC33b75be219502815b2bd3f4cdbfc23bb";
            this.AUTH_TOKEN = @"33717d3aae306c09eb78b3008bb4f66f";
            this.PHONE_NO = @"+15138541944";
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
        }
    }
}
