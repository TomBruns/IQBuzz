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
    public class IQBuzzUserTests : BaseClassTests, IClassFixture<IQBuzzUserTestsGlobal>
    {
        IQBuzzUserTestsGlobal _iqBuzzUserTestsGlobal;

        // test setup
        public IQBuzzUserTests(IQBuzzUserTestsGlobal iqBuzzUserTestsGlobal)
        {
            _iqBuzzUserTestsGlobal = iqBuzzUserTestsGlobal;
        }

        [Fact, Order(1)]
        public void TestInsertUser()
        {
            var iqBuzzUser = _iqBuzzUserTestsGlobal.IQBuzzUser;

            MongoDBContext.InsertIQBuzzUser(iqBuzzUser);
        }

        [Fact, Order(2)]
        public void TestInsertDupUserThrowsEx()
        {
            var iqBuzzUser = _iqBuzzUserTestsGlobal.IQBuzzUser;

            // this tests our unique index
            Assert.Throws<MongoWriteException>(() => MongoDBContext.InsertIQBuzzUser(iqBuzzUser));
        }

        [Fact, Order(3)]
        public void TestFindIQBuzzUserByPhoneNo()
        {
            string phone_no = _iqBuzzUserTestsGlobal.IQBuzzUser.phone_no;

            var iqBuzzUser = MongoDBContext.FindIQBuzzUser(phone_no);

            Assert.NotNull(iqBuzzUser);
        }

        [Fact, Order(4)]
        public void TestFindUserByUnknownPhoneNoReturnsNull()
        {
            string phone_no = @"+0000000000";

            Assert.Null(MongoDBContext.FindIQBuzzUser(phone_no));
        }

    }

    public class IQBuzzUserTestsGlobal : IDisposable
    {
        public IQBuzzUserMBE IQBuzzUser { get; set; }

        public IQBuzzUserTestsGlobal()
        {
            string phoneNo = @"+15131234567";

            // Do "global" initialization here; Only called once.
            var mDeleteResult = MongoDBContext.DeleteIQBuzzUser(phoneNo);

            // init global variables for test run
            this.IQBuzzUser = new IQBuzzUserMBE()
            {
                user_id = -1,
                first_name = @"Test",
                last_name = @"User",
                phone_no = phoneNo,
                email_address = @"test.user@hotmail.com",
                has_accepted_welcome_agreement = true,
                local_time_zone = "EST",
                merchant_ids = new List<int>() { 1, 2, 3, 4, 5 }
                
            };
        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.
        }
    }
}
