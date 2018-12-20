using System;
using System.Collections.Generic;
using System.Text;

namespace WP.Learning.BizLogic.Shared.Entities
{
    public class IQBuzzUserBE
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNo { get; set; }
        public string EmailAddress { get; set; }
        public string LocalTimeZone { get; set; }

        public string FullName
        {
            get { return $"{LastName}, {FirstName}";  }
        }

        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                switch (this.LocalTimeZone.ToUpper())
                {
                    case @"PST":
                        return TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

                    case @"MST":
                        return TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");

                    case @"CST":
                        return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

                    case @"EST":
                        return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

                    case @"GMT":
                    default:
                        return TimeZoneInfo.FindSystemTimeZoneById("Greenwich Standard Time");
                }
            }
        }
    }
}
