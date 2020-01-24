using System;
using System.Collections.Generic;
using System.Text;

using RestSharp;

namespace WP.Learning.BizLogic.Shared.Controllers
{
    public static class PrestoController
    {
        const string BASE_URL = @"https://teamsmbuzzwebapi220191021015918.azurewebsites.net";

        static RestClient _client = new RestClient(BASE_URL);

        /// <summary>
        /// Call the Presto WebAPI to drive a step in the demo
        /// </summary>
        /// <param name="fromPhoneNumber"></param>
        /// <param name="stepNo"></param>
        /// <returns></returns>
        public static string DriveDemo(string fromPhoneNumber, int stepNo)
        {
            // remove any leading + signs
            fromPhoneNumber = fromPhoneNumber.Replace(@"+", string.Empty);

            // create the request
            var request = new RestRequest("/api/Test/{phoneNo}/demo/step/{stepNo}", Method.POST);
            request.AddUrlSegment("phoneNo", fromPhoneNumber);
            request.AddUrlSegment("stepNo", stepNo);

            // Automatically deserialize result
            var response = _client.Execute(request);
            var data = response.Content;

            return data;
        }

        public static string DriveDemo(int userId, int stepNo)
        {
            // create the request
            var request = new RestRequest("/api/Test/{userId}/demo/step/{stepNo}", Method.POST);
            request.AddUrlSegment("userId", userId);
            request.AddUrlSegment("stepNo", stepNo);

            // Automatically deserialize result
            var response = _client.Execute(request);
            var data = response.Content;

            return data;
        }

        public static List<string> GetLast5Users()
        {
            // create the request
            var request = new RestRequest("/api/Test/demo/recentnewusers", Method.GET);

            // Automatically deserialize result
            var response = _client.Execute<List<string>>(request);

            response.Data.Add("presto-1-<user#> : reset");
            response.Data.Add("presto-2-<user#> : promo");
            response.Data.Add("presto-3-<user#> : earn");
            response.Data.Add("presto-4-<user#> : redeem");
            return response.Data;
            
            //List<string> response = new List<string>();
            //response.Add("[5] (513-498-6016)");
            //response.Add("[4] (513-498-6076)");
            //response.Add("[3] (513-498-6086)");
            //response.Add("[2] (513-498-6019)");
            //response.Add("[1] (513-498-6010)");


            //return response;
        }


    }
}
