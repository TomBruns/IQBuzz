using System;

using WP.Learning.BizLogic.Shared;
using WP.Learning.BizLogic.Shared.Merchant;
using WP.Learning.BizLogic.Shared.SMS;

namespace WP.Learning.BackOffice.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // display options menu
            ConsoleColor defaultColor = ConsoleColor.Gray;
            System.Console.ForegroundColor = defaultColor;

            System.Console.Clear();
            System.Console.ForegroundColor = ConsoleColor.Blue;
            System.Console.WriteLine("=============================================");
            System.Console.WriteLine("   Hackathon Test Methods");
            System.Console.WriteLine("=============================================");
            System.Console.ForegroundColor = defaultColor;
            System.Console.WriteLine();
            System.Console.ForegroundColor = ConsoleColor.Magenta;
            System.Console.WriteLine("=== SMS Test Messages =======================");
            System.Console.ForegroundColor = defaultColor;
            System.Console.WriteLine("1.1   Test Message to Tom's Phone");
            System.Console.WriteLine("");
            System.Console.ForegroundColor = ConsoleColor.Magenta;
            System.Console.WriteLine("=== Generate Xcts ===========================");
            System.Console.ForegroundColor = defaultColor;
            System.Console.WriteLine("2.1   Clear Xcts");
            System.Console.WriteLine("2.2   Generate Purchases");
            System.Console.WriteLine("2.3   Generate Returns");
            System.Console.WriteLine("2.4   Generate Chargebacks");
            System.Console.WriteLine("2.5   Fire All Terminals Closed Event");
            System.Console.WriteLine("");
            System.Console.ForegroundColor = ConsoleColor.Magenta;
            System.Console.WriteLine("=== Support ===========================");
            System.Console.ForegroundColor = defaultColor;
            System.Console.WriteLine("3.1   Load Merchant");
            System.Console.WriteLine("3.2   Load ALL Merchants");
            System.Console.WriteLine("3.3   Send Welcome Message");
            System.Console.WriteLine("");
            System.Console.WriteLine();
            System.Console.Write("Enter # or Hit <enter> to Exit");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.Write(" > ");
            System.Console.ForegroundColor = defaultColor;
            var selectionString = System.Console.ReadLine();
            decimal selection = !string.IsNullOrEmpty(selectionString.Trim()) ? decimal.Parse(selectionString) : 0;

            int merchantId = 0;

            switch(selection)
            {
                case 1.1M:  // Test Message to Tom's Phone
                    TwilioController.SendTestSMSMessage(GeneralConstants.TOMS_PHONE_NO);
                    break;

                case 2.1M:  // Clear Purchaes for Merchant N
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.ResetAllXctsForMerchantDate(merchantId, DateTime.Today);
                    break;

                case 2.2M:  // Purchases for Merchant N
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.GenerateRandomPurchaseXcts(merchantId);
                    break;

                case 2.3M:  // Generate refunds
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.GenerateRefundXcts(merchantId);
                    break;

                case 2.4M:  // Generate chargebacks
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.GenerateChargebacksXcts(merchantId);
                    break;

                case 2.5M:  // Fire all terminals closed event
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.FireAllTerminalsClosedEvent(merchantId);
                    break;

                case 3.1M:  // Create Merchant
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.CreateMerchant(merchantId);
                    break;

                case 3.2M:  // Create All Merchants
                    MerchantController.CreateAllMerchants();
                    break;

                case 3.3M:  // Send Welcome Message
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.SendWelcomeMessage(merchantId);
                    break;

                default:
                    break;
            }
        }
    }
}
