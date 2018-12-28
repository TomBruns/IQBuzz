using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit;

using WP.Learning.BizLogic.Shared;
using WP.Learning.BizLogic.Shared.Entities;
using WP.Learning.BizLogic.Shared.Controllers;

namespace WP.Learning.BizLogic.Shared.UnitTests
{
    public class UserControllerTests
    {
        [Fact]
        public void TestGetUserActivitySummary()
        {
            DateTime fromDate = DateTime.Today;

            List<UserDailyUsageSummaryBE> usage = UserController.GetUserActivitySummaryByDay(fromDate, @"EST");

            StringBuilder msg = new StringBuilder();
            msg.AppendLine($"{GeneralConstants.APP_NAME} usage stats: {fromDate.AddDays(-5):MMM d} to {fromDate:MMM d}");
            msg.AppendLine("----------------------------------");

            var users = usage.Select(u => new { u.PhoneNo, u.IQBuzzUser.FullName }).Distinct().ToList();

            foreach (var user in users)
            {
                msg.Append($"{user.FullName} |");

                for (DateTime activityDate = fromDate.AddDays(-4); activityDate <= fromDate; activityDate = activityDate.AddDays(1))
                {
                    var activityOnDate = usage.Where(u => u.PhoneNo == user.PhoneNo && u.ActivityDate == activityDate).FirstOrDefault();

                    if (activityOnDate != null)
                    {
                        msg.Append($"{activityOnDate.ActionQty}|");
                    }
                    else
                    {
                        msg.Append(@"0|");
                    }
                }
                msg.Append("/n");
            }
        }
    }
}
