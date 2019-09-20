using System;
using System.Collections.Generic;
using System.Text;

namespace WP.Learning.MongoDB.Entities
{
    public class Enums
    {
        public enum TRANSACTION_TYPE
        {
            unknown = 0,
            cp_sale = 1,
            cnp_sale = 2,
            cp_return = 3, 
            cnp_return = 4,
            chargeback = 5
        }

        public enum PAYMENT_CARD_TYPE
        {
            unknown,
            //DINERS_CLUB,
            AMERICAN_EXPRESS,
            VISA,
            MASTERCARD,
            DISCOVER,
            GIFT_CARD
        }
    }
}
