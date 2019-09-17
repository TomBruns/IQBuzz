using System;
using System.Collections.Generic;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;

namespace WP.Learning.BizLogic.Shared.Controllers
{
    public static class TranslationController
    {
        private static GoogleCredential _googleCred;
        private static TranslationClient _client;

        static TranslationController()
        {
            string acctInfo = @"{
  'type': 'service_account',
  'project_id': 'my-project-1568712612462',
  'private_key_id': '8111372e5f7b4bfc15b8466c82a25a6201d7b778',
  'private_key': '-----BEGIN PRIVATE KEY-----\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDBDZo+eT6fOTeH\nBiGIfUbH+us2E/UpdivF8K69+APTdankV16er4SYR1g3mCD4VwrcUHLGCqaBFTvB\nb0Hytxw5RfU6rY1fq21cwcqq9lVep+34acGwAnKSOq2QWm4lR/MPehLagx5g/LCE\nN1yilb/rbpDg5QN/DfFLyevhqcmti0BS/h2DSqp1TVomA4GB4BrnW2TtfoirtN4k\nEpgYTRvGlilzZNOHzD6Y8KGggiPiMUODR4N3yHakgABRz+NddvlNcc++PF5mZrcf\nlvOne9JEJdJgTEYLS6HlPLGZxt163OyF+pdVL00tjxzFXg0MKg5maomzLkN8UQYJ\ncyBKyfnPAgMBAAECggEACvloesOurSN2Bh1xm44vj1+yrkTNK3m1vE9Vx6PJJ+So\n7zuOO+wTWAq6N5LU0Pk1u1dxZtOlKnJYDsNx1sfU+WaCLb8+6aDXmy3DBRvyt2pJ\na1UrtwM0mx5tzmZtm33vEcKpPrW0bOQAbsus3usvWUVURAyQudDDlkDYwV/DALPv\nxNDOlKqMi465Nh4pv2y3RpeheWjEcnnpsbV9eunSUwLVLx7EoTtLfJN5cqlgMxMX\nD7ULPG7vsQWieNYqEXJMXToVXktmt4PlglHXq5JIyMJxSTosVYWzXQgUYHD6MwLa\ntnq5p1N7zKbOGRk8CvrgryUwGElW9knwFsbRsyVzyQKBgQD/Hxnt5yUwfftMrF58\nEhP8Ts8RY0v03GrYsVCNYI9+EmCzlafLAv/7tVCyo2jgo2UVB0dnN/V+62/VO9iL\n5EFlfyKQgFuWkFgqTIKvrHe6/fKep92HTEJZrsUs/+PknWHgJVMFjsr+ErvI3X0S\n1NVz4koyrHvVU11DzH3AaXisiQKBgQDBt8knTBVzf9/ULWEAhHu1XwFYLOZHUADL\nCBlDaiyk5W7KSVmwDCNyRwZIpzdeocM0cTRw8V+V6Lq4ifLhuL78u59c5qAD7wb0\nkfs4KthdBH4HuxS5M4G/lxW98V8hrtp6m9t5lWNfaltMfbmGYZiWHeLkRdnj7gBF\n/E2s+z9NlwKBgQCV7T8dskMGtmKicoGsRCt+kQnbXBFdOtOHuNxv2Rk6q64sm3xa\nL88jqzBbavDNYviaXQa3QAmDpvS3yU2/GEreTNKRPP2WBAnsAb6jYqWSPH8CggAL\n7OLpNW4mvdK9nUfRo92gXIQqv/OfXZqNIIq4aXnVQcwcV7tthpU4KEOEkQKBgFvK\niEhUpv15pEfX0NT78dp5UBvF+r/jytxp0/67urfP93Vm2FaxCcLGoNWgVn5CzEp3\nMdAqr45LHt7+jtYiQm1jQho1NbFne7Y7zfxJYQiCbm2fWix/mYV9Q9IjI17EicX7\nQD4WaWX28ZgHIvfoGvNW6gIcjyiPyBVhfjTvQIm1AoGBAMUcod79SHcPI5kBmxNR\n1WcBqjxe14jPOMFCfBdJ9zkLSbLBvDDvZVf68uCybUg0vjs4jM3DIbW6eEWUipBr\nybV3Qp86oVUVyGCnlPgz0wd65fCsgiKESDTStxVX73Fayd1uxOB+yLG396O3JGl0\nfW3ASok8wPxEL+6Cmb0eD+Wa\n-----END PRIVATE KEY-----\n',
  'client_email': 'starting-account-zh0eflfeaief@my-project-1568712612462.iam.gserviceaccount.com',
  'client_id': '106655476572248497772',
  'auth_uri': 'https://accounts.google.com/o/oauth2/auth',
  'token_uri': 'https://oauth2.googleapis.com/token',
  'auth_provider_x509_cert_url': 'https://www.googleapis.com/oauth2/v1/certs',
  'client_x509_cert_url': 'https://www.googleapis.com/robot/v1/metadata/x509/starting-account-zh0eflfeaief%40my-project-1568712612462.iam.gserviceaccount.com'
}";
            _googleCred = GoogleCredential.FromJson(acctInfo);

            _client = TranslationClient.Create(_googleCred);
        }

        public static List<string> Translate(List<string> textToTranslate, string targetLanguageCode)
        {
            List<string> translatedText = new List<string>();

            foreach(string text in textToTranslate)
            {
                translatedText.Add(Translate(text, targetLanguageCode));
            }
            
            return translatedText;
        }

        public static string Translate(string textToTranslate, string targetLanguageCode)
        {
            var response = _client.TranslateText(textToTranslate, targetLanguageCode);

            return response.TranslatedText;
        }
    }
}
