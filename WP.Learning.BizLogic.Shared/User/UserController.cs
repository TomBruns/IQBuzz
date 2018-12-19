using System;
using System.Collections.Generic;
using System.Text;

using WP.Learning.MongoDB;
using WP.Learning.MongoDB.Entities;

namespace WP.Learning.BizLogic.Shared.User
{
    public static class UserController
    {
        public static void LogUserActivity(UserActivityMBE userActivity)
        {
            MongoDBContext.InsertUserActivity(userActivity);
        }

        public static void LogUserActivity(string fromPhoneNumber, string action, DateTime activityDT, string comments)
        {
            LogUserActivity(new UserActivityMBE() { phone_no = fromPhoneNumber, action = action, activity_dt = activityDT, comments = comments });
        }
    }
}
