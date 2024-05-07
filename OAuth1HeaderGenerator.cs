using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;

// This is essentially a fork of https://github.com/ericpopivker/entech-blog-netsuite-rest-api-tba-demo/blob/main/NetSuiteRestApiTbaDemo/NetSuiteRestApiTbaDemo.Core/OAuth1HeaderGenerator.cs
// The original OAuth code here did not support the HMAC-SHA256 signature method, so I had to look elsewhere for a solution.
namespace NetsuiteRequest.OAuth
{
    public class OAuth1HeaderGenerator
    {
        private HttpMethod _httpMethod;
        private string _requestUrl;
		private string _consumerKey;
		private string _consumerSecret;
		private string _token;
		private string _tokenSecret;
		private string _realm;

        public OAuth1HeaderGenerator(string consumerKey, string consumerSecret, string token, string tokenSecret, string realm, HttpMethod httpMethod, string requestUrl)
        {
			_consumerKey = consumerKey;
			_consumerSecret = consumerSecret;
			_token = token;
			_tokenSecret = tokenSecret;
			_realm = realm;
            _httpMethod= httpMethod;
            _requestUrl= requestUrl;
        }

        public AuthenticationHeaderValue CreateAuthenticationHeaderValue()
        {
            var nonce = GenerateNonce();
            var timestamp = GenerateTimeStamp();

            var parameter = GetAuthenticationHeaderValueParameter(nonce, timestamp);
            return new AuthenticationHeaderValue("OAuth", parameter);
        }

        public string GetAuthenticationHeaderValueParameter(string nonce, string timestamp)
        {
            var signature = GenerateSignature(nonce, timestamp);

            //NetSuite doc:
            //https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/section_1534941295.html

            var parameters = new OrderedDictionary
            {
                { "realm", _realm },
                { "oauth_token", _token },
                { "oauth_consumer_key", _consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_timestamp", timestamp },
                { "oauth_signature_method", "HMAC-SHA256" },
                { "oauth_version", "1.0" },
                { "oauth_signature", signature }
            };

            var combinedOauthParams = CombineOAuthHeaderParams(parameters);
            Debug.WriteLine($"CreateAuthenticationHeaderValue: combinedOauthParams: {combinedOauthParams}");

            return combinedOauthParams;
        }


        private string CombineOAuthHeaderParams(OrderedDictionary parameters)
        {
            var sb = new StringBuilder();
            var first = true;

            foreach (var key in parameters.Keys)
            {
                if (!first)
                    sb.Append(", ");

                var value = parameters[key].ToString();
                value = Uri.EscapeDataString(value);
                sb.Append($"{key}=\"{value}\"");
                first = false;
            }

            return sb.ToString();
        }


        // From NetSuite doc:
        // https://docs.oracle.com/en/cloud/saas/netsuite/ns-online-help/section_1534941088.html
        public string GenerateSignature(string nonce, string timestamp)
        {
            var baseString = GenerateSignatureBaseString(nonce, timestamp);           
            string key = GenerateSignatureKey();

            Debug.WriteLine($"GenerateSignature: baseString: {baseString}");
            Debug.WriteLine($"GenerateSignature: key: {key}");

            string signature = "";

            var encoding = new ASCIIEncoding();

            byte[] keyBytes = encoding.GetBytes(key);
            byte[] messageBytes = encoding.GetBytes(baseString);

            using (var hmaCsha256 = new HMACSHA256(keyBytes))
            {
                byte[] hash = hmaCsha256.ComputeHash(messageBytes);
                signature = Convert.ToBase64String(hash);
            }
            
            return signature;
        }

        public string GenerateSignatureKey()
        {
            var keyParams = new List<string>
                {
                    _consumerSecret,
                    _tokenSecret
                };

            var key = CombineKeyParams(keyParams);
            return key;
        }

        public string GenerateSignatureBaseString(string nonce, string timestamp)
        {
            var requestUri = new Uri(_requestUrl);
            var requestUrlPath = requestUri.GetLeftPart(UriPartial.Path);
            
            string baseString = _httpMethod.ToString();
            baseString += "&";
            baseString += Uri.EscapeDataString(requestUrlPath);
            baseString += "&";

            var baseStringParams = new Dictionary<string, string>()
            {
                { "oauth_consumer_key", _consumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA256" },
                { "oauth_timestamp", timestamp },
                { "oauth_token", _token },
                { "oauth_version", "1.0" }
            };

            //Handle query string
            //https://www.rfc-editor.org/rfc/rfc5849#section-3.4.1

            var requestUriQuery = requestUri.Query;
            if (requestUriQuery != string.Empty)
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(requestUri.Query);

                foreach (var key in queryParams.Keys)
                    baseStringParams.Add(key.ToString(), queryParams[key.ToString()].ToString());
            }

            var combinedBaseStringParams = CombineBaseStringParams(baseStringParams);
            baseString += Uri.EscapeDataString(combinedBaseStringParams);
            return baseString;
        }



        // https://www.rfc-editor.org/rfc/rfc5849#section-3.4.1
        // 3.4.1.3.2.  Parameters Normalization

        private string CombineBaseStringParams(Dictionary<string, string> parameters)
        {
            var sortedPairs = new List<string>();
            foreach (var key in parameters.Keys)
            {
                var pair = Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(parameters[key]);
                sortedPairs.Add(pair);
            }
            sortedPairs.Sort();

            var sb = new StringBuilder();
            var first = true;
            var separator = "&";

            foreach (var pair in sortedPairs)
            {
                if (!first)
                    sb.Append(separator);

                sb.Append(pair);

                first = false;
            }

            return sb.ToString();
        }

        private string CombineKeyParams(List<string> parameters)
        {
            var sb = new StringBuilder();
            var first = true;

            foreach (var param in parameters)
            {
                if (!first)
                    sb.Append("&");

                sb.Append(Uri.EscapeDataString(param));
                first = false;
            }

            return sb.ToString();
        }

        public string GenerateTimeStamp()
        {
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        public string GenerateNonce()
        {
            var random = new Random();

            // Just a simple implementation of a random number between 123400 and 9999999
            return random.Next(123400, 9999999).ToString();
        }
    }
}