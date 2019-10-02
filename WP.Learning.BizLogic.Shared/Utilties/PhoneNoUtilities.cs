using System;
using System.Collections.Generic;
using System.Text;

using PhoneNumbers;

namespace WP.Learning.BizLogic.Shared.Utilties
{
    public static class PhoneNoUtilities
    {
        public static string CleanUpPhoneNo(string rawPhoneNo)
        {
            string formatedPhoneNo = string.Empty;

            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            var parsedPhoneNo = phoneNumberUtil.Parse(rawPhoneNo, "US");

            if(phoneNumberUtil.IsValidNumber(parsedPhoneNo))
            {
                formatedPhoneNo = phoneNumberUtil.Format(parsedPhoneNo, PhoneNumberFormat.E164);
            }

            return formatedPhoneNo;
        }
    }
}
