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
    }
}
