using System;
using System.Collections.Generic;
using WP.Learning.BizLogic.Shared;
using WP.Learning.BizLogic.Shared.Controllers;
using WP.Learning.MongoDB;
using static WP.Learning.MongoDB.Entities.Enums;

namespace WP.Learning.BackOffice.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            DisplayMenu();
        }

        static void DisplayMenu()
        { 
            // display options menu
            ConsoleColor defaultColor = ConsoleColor.Gray;
            System.Console.ForegroundColor = defaultColor;

            System.Console.Clear();
            System.Console.ForegroundColor = ConsoleColor.Blue;
            System.Console.WriteLine("=============================================");
            System.Console.WriteLine($"   {GeneralConstants.APP_NAME} Test Harness Methods");
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
            System.Console.WriteLine("2.01   Clear Xcts");
            System.Console.WriteLine("2.02   Generate Sample Xcts");
            System.Console.WriteLine("2.03   Generate CP Purchases");
            System.Console.WriteLine("2.04   Generate CNP Purchases");
            System.Console.WriteLine("2.05   Generate CP Returns");
            System.Console.WriteLine("2.06   Generate CNP Returns");
            System.Console.WriteLine("2.07   Generate Chargebacks");
            //System.Console.WriteLine("2.6   Fire All Terminals Closed Event");
            //System.Console.WriteLine("2.7   Fire Closed Event for all Merchants");
            System.Console.WriteLine("");
            System.Console.ForegroundColor = ConsoleColor.Magenta;
            System.Console.WriteLine("=== User Commands =======================");
            System.Console.ForegroundColor = defaultColor;
            System.Console.WriteLine("3.01  Summary");
            System.Console.WriteLine("3.02  Sales");
            System.Console.WriteLine("3.03  Cback");
            System.Console.WriteLine("3.04  Returns");
            System.Console.WriteLine("3.05  Stop");
            System.Console.WriteLine("3.06  FAF");
            System.Console.WriteLine("");
            System.Console.WriteLine("3.11  help?");
            System.Console.WriteLine("3.12  join");
            System.Console.WriteLine("3.13  Settings");
            System.Console.WriteLine("3.14  User");
            System.Console.WriteLine("");
            System.Console.WriteLine("3.21  help+");
            System.Console.WriteLine("3.22  unjoin");
            System.Console.WriteLine("3.23  ver");
            System.Console.WriteLine("3.24  genxcts");
            System.Console.WriteLine("3.25  usage");
            System.Console.WriteLine("");
            System.Console.ForegroundColor = ConsoleColor.Magenta;
            System.Console.WriteLine("=== Merchant Admin ======================");
            System.Console.ForegroundColor = defaultColor;
            System.Console.WriteLine("4.1   Load Merchant");
            System.Console.WriteLine("4.2   Load ALL Merchants");
            System.Console.WriteLine("4.3   Reload ALL Merchants");
            System.Console.WriteLine("4.4   Send Welcome Message");
            System.Console.WriteLine("");
            System.Console.ForegroundColor = ConsoleColor.Magenta;
            System.Console.WriteLine("=== User Admin ===========================");
            System.Console.ForegroundColor = defaultColor;
            System.Console.WriteLine("5.1   Load User");
            System.Console.WriteLine("5.2   Load All Users");
            System.Console.WriteLine("5.3   Reload All Users");
            System.Console.WriteLine("");
            System.Console.WriteLine();
            System.Console.Write("Enter # or 0 to Exit");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.Write(" > ");
            System.Console.ForegroundColor = defaultColor;
            var selectionString = System.Console.ReadLine();
            decimal selection = !string.IsNullOrEmpty(selectionString.Trim()) ? decimal.Parse(selectionString) : 0;

            int merchantId = 0;
            int userId = 0;
            DateTime nowUTC = DateTime.Today.ToUniversalTime();
            string response = string.Empty;

            switch (selection)
            {
                #region === 1.x

                case 1.1M:  // Test Message to Tom's Phone
                    SMSController.SendTestSMSMessage(GeneralConstants.TOMS_PHONE_NO);
                    break;

                #endregion

                #region === 2.x
                case 2.01M:  // Clear Xcts for Merchant N
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    if (merchantId == 0)
                    {
                        MerchantController.DeleteXctsForAllMerchants();
                    }
                    else
                    {
                        MerchantController.DeleteAllMerchantXctsForDate(merchantId, DateTime.Today);
                    }
                    break;

                case 2.02M:  // Gen All Xcts for Merchant N
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.GenerateSampleXcts(merchantId, nowUTC.Date);
                    break;

                case 2.03M:  // Gen CP Purchases Xcts for Merchant N
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.GenerateSalesXcts(merchantId, nowUTC.Date, TRANSACTION_TYPE.cp_sale);
                    break;

                case 2.04M:  // Gen CNP Purchases Xcts for Merchant N
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.GenerateSalesXcts(merchantId, nowUTC.Date, TRANSACTION_TYPE.cnp_sale);
                    break;

                case 2.05M:  // Gen refunds Xcts for Merchant N
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.GenerateReturnXcts(merchantId, nowUTC.Date, TRANSACTION_TYPE.cp_return);
                    break;

                case 2.06M:  // Gen refunds Xcts for Merchant N
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.GenerateReturnXcts(merchantId, nowUTC.Date, TRANSACTION_TYPE.cnp_return);
                    break;

                case 2.07M:  // Generate chargebacks Xcts for Merchant N
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.GenerateChargebacksXcts(merchantId, nowUTC.Date);
                    break;

                //case 2.5M:  // Fire all terminals closed event
                //    System.Console.Write("MerchantId:> ");
                //    merchantId = Int32.Parse(System.Console.ReadLine());
                //    MerchantController.FireAllTerminalsClosedEvent(merchantId);
                //    break;

                //case 2.6M:  // Fire all terminals closed event
                //    System.Console.Write("MerchantId:> ");
                //    merchantId = Int32.Parse(System.Console.ReadLine());
                //    MerchantController.FireClosedEventsForAllMerchants();
                //    break;

                #endregion

                #region === User Commands =========================================================

                case 3.01M:  // summary
                    ProcessUserCommand(@"summary");
                    break;

                case 3.02M:  // sales
                    ProcessUserCommand(@"sales");
                    break;

                case 3.03M:  // cback
                    ProcessUserCommand(@"cback");
                    break;

                case 3.04M:  // returns
                    ProcessUserCommand(@"returns");
                    break;

                case 3.05M:  // stop
                    ProcessUserCommand(@"stop");
                    break;

                case 3.06M:  // faf
                    ProcessUserCommand(@"faf");
                    break;

                // ============
                case 3.11M:  // help?
                    ProcessUserCommand(@"help?");
                    break;

                case 3.12M:  // join
                    ProcessUserCommand(@"join");
                    break;

                case 3.13M:  // settings
                    ProcessUserCommand(@"settings");
                    break;

                case 3.14M:  // user
                    ProcessUserCommand(@"user");
                    break;

                // ============
                case 3.21M:  // help+
                    ProcessUserCommand(@"help+");
                    break;

                case 3.22M:  // unjoin
                    ProcessUserCommand(@"unjoin");
                    break;

                case 3.23M:  // ver
                    ProcessUserCommand(@"ver");
                    break;

                case 3.24M:  // genxcts
                    ProcessUserCommand(@"genxcts");
                    break;

                case 3.25M:  // usage
                    ProcessUserCommand(@"usage");
                    break;

                #endregion

                #region === Merchants =========================================================

                case 4.1M:  // Create Merchant
                    System.Console.Write("MerchantId:> ");
                    merchantId = Int32.Parse(System.Console.ReadLine());
                    MerchantController.CreateMerchant(merchantId, true);
                    break;

                case 4.2M:  // Create All Merchants
                    MerchantController.CreateAllMerchants(false);
                    break;

                case 4.3M:  // Recreate All Merchants
                    MerchantController.CreateAllMerchants(true);
                    break;

                case 4.4M:  // Send Welcome Message
                    System.Console.Write("UserId:> ");
                    userId = Int32.Parse(System.Console.ReadLine());
                    UserController.SendWelcomeMessage(userId);
                    break;

                #endregion

                #region === Users =========================================================

                case 5.1M:  // Load User
                    System.Console.Write("UserId:> ");
                    userId = Int32.Parse(System.Console.ReadLine());
                    UserController.CreateUser(userId, true);
                    break;

                case 5.2M:  // Load All Users
                    UserController.CreateAllUsers(false);
                    break;

                case 5.3M:  // Reload All Users
                    UserController.CreateAllUsers(true);
                    break;

                #endregion

                case 99.0M: // test translate
                    string translation = TranslationController.Translate("Hello World.", "ru");
                    System.Console.OutputEncoding = System.Text.Encoding.Unicode;
                    System.Console.Write(translation);
                    System.Console.ReadLine();
                    break;

                default:
                    break;
            }

            if(selection != 0)
            {
                DisplayMenu();
            }
        }

        private static void ProcessUserCommand(string command)
        {
            System.Console.Write("UserID:> ");
            int userId = Int32.Parse(System.Console.ReadLine());

            var user = MongoDBContext.FindIQBuzzUser(userId);

            List<string> responses = RequestController.ProcessIncommingText(user.phone_no, command);
            foreach (string response in responses)
            {
                System.Console.Write(response);
            }
            System.Console.Write("\n Hit Return to continue ...");
            System.Console.ReadLine();
        }
    }
}
