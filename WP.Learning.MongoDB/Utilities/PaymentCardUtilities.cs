using System;
using System.Collections.Generic;
using System.Text;

using static WP.Learning.MongoDB.Entities.Enums;

namespace WP.Learning.MongoDB.Utilities
{
    /// <summary>
    /// General Payment Card Utilites
    /// </summary>
    public static class PaymentCardUtilities
    {
        // Poor mans Bin Table for Hackathon
        public static PAYMENT_CARD_TYPE ParsePANForCardType(string primary_account_no)
        {
            if (primary_account_no.StartsWith(@"2221"))
            {
                return PAYMENT_CARD_TYPE.MASTERCARD;
            }
            else if(primary_account_no.StartsWith(@"34"))
            {
                return PAYMENT_CARD_TYPE.AMERICAN_EXPRESS;
            }
            else if (primary_account_no.StartsWith(@"37"))
            {
                return PAYMENT_CARD_TYPE.AMERICAN_EXPRESS;
            }
            else if (primary_account_no.StartsWith(@"4"))
            {
                return PAYMENT_CARD_TYPE.VISA;
            }
            else if (primary_account_no.StartsWith(@"51"))
            {
                return PAYMENT_CARD_TYPE.MASTERCARD;
            }
            else if (primary_account_no.StartsWith(@"52"))
            {
                return PAYMENT_CARD_TYPE.MASTERCARD;
            }
            else if (primary_account_no.StartsWith(@"53"))
            {
                return PAYMENT_CARD_TYPE.MASTERCARD;
            }
            else if (primary_account_no.StartsWith(@"54"))
            {
                return PAYMENT_CARD_TYPE.MASTERCARD;
            }
            else if (primary_account_no.StartsWith(@"55"))
            {
                return PAYMENT_CARD_TYPE.MASTERCARD;
            }
            else if (primary_account_no.StartsWith(@"6011"))
            {
                return PAYMENT_CARD_TYPE.DISCOVER;
            }
            else if (primary_account_no.StartsWith(@"9"))
            {
                // not how gift cards are really identified, short cut for hackathon
                return PAYMENT_CARD_TYPE.GIFT_CARD;
            }
            else
            {
                return PAYMENT_CARD_TYPE.unknown;
            }
        }
    }
}
