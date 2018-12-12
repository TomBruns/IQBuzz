using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TwilioSend
{

    class Program
    {
        static void Main(string[] args)
        {
            // Find your Account Sid and Token at twilio.com/console
            const string accountSid = @"AC8a0322c81593dc526baacfbfe6a0220a";
            const string authToken = @"8594f8e7b2b3b47275eb15b6f7fffe1e";

            // init the client
            TwilioClient.Init(accountSid, authToken);

            // create and send a SMS message
            var message = MessageResource.Create(
                body: @"Join Earth's mightiest heroes. Like Kevin Bacon.",
                from: new Twilio.Types.PhoneNumber(@"+18125944088"),
                to: new Twilio.Types.PhoneNumber(@"+15134986016")
            );

            Console.WriteLine(message.Sid);
        }
    }
}
