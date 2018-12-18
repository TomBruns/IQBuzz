using System;
using System.Collections.Generic;
using System.Text;

using WP.Learning.BizLogic.Shared.Utilties;

using Xunit;
using XUnitPriorityOrderer;

namespace WP.Learning.MongoDB.UnitTests
{
    public class UtilityTests
    {
        /*
            21-Dec  Fri
            22-Dec  Sat Weekend
            23.Dec  Sun Weekend
            24.Dec  Mon
            25.Dec  Tue Banking Holiday
            26.Dec  Wed
            27.Dec  Thu
            28.Dec  Fri
            29.Dec  Sat Weekend
            30.Dec  Sun Weekend
            31.Dec  Mon
            1.Jan   Tue Banking Holiday
            2.Jan   Wed
         */

        [Fact]
        public void TestAddBusinessDays()
        {
            DateTime startDate = new DateTime(2018, 12, 21);

            DateTime endDate = startDate.AddBusinessDays(5);

            Assert.Equal(endDate, new DateTime(2018, 12, 31));
        }

        [Fact]
        public void TestSubtractBusinessDays()
        {
            DateTime startDate = new DateTime(2018, 12, 31);

            DateTime endDate = startDate.SubtractBusinessDays(5);

            Assert.Equal(endDate, new DateTime(2018, 12, 21));
        }
    }
}
