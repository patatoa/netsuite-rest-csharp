using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using NetsuiteRequest.OAuth;
using System.Text.Json;

namespace NetsuiteRequest
{
	public class NetsuiteRequest
	{
		private readonly string _consumerKey;
		private readonly string _consumerSecret;
		private readonly string _token;
		private readonly string _tokenSecret;
		private readonly string _realm;
		private readonly string _baseUrl;
		private readonly string _suiteQlBaseUrl;

		public NetsuiteRequest(string consumerKey, string consumerSecret, string token, string tokenSecret, string realm)
		{
			_consumerKey = consumerKey;
			_consumerSecret = consumerSecret;
			_token = token;
			_tokenSecret = tokenSecret;
			_realm = realm;
			_baseUrl = $"https://{_realm.ToLower().Replace("_", "-")}.suitetalk.api.netsuite.com/services/rest/record/v1";
			_suiteQlBaseUrl = $"https://{_realm.ToLower().Replace("_", "-")}.suitetalk.api.netsuite.com/services/rest/query/v1/suiteql";
		}
		public async Task<JsonDocument> Request(string urlString, HttpMethod httpMethod, JsonObject? body = null)
		{
			var url = _baseUrl + urlString;
			using HttpClient client = new();
			var oAuth1HeaderGenerator = new OAuth1HeaderGenerator(_consumerKey, _consumerSecret, _token, _tokenSecret, _realm, httpMethod, url);
			var header = oAuth1HeaderGenerator.CreateAuthenticationHeaderValue();
			Console.WriteLine(header);
			Console.WriteLine(url);
			client.DefaultRequestHeaders.Authorization = header;
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			HttpResponseMessage response;
			switch (httpMethod.Method)
			{
				case "GET":
					response = await client.GetAsync(url);
					break;
				case "POST":
					if (body == null)
					{
						throw new Exception("Error: Body is required for POST requests");
					}
					response = await client.PostAsync(url, new StringContent(body.ToString(), Encoding.UTF8, "application/json"));
					break;
				case "PUT":
					if (body == null)
					{
						throw new Exception("Error: Body is required for PUT requests");
					}
					response = await client.PutAsync(url, new StringContent(body.ToString(), Encoding.UTF8, "application/json"));
					break;
				case "DELETE":
					response = await client.DeleteAsync(url);
					break;
				default:
					throw new Exception("Error: Invalid HTTP Method");
			}

			if (response.IsSuccessStatusCode)
			{
				var jsonString = await response.Content.ReadAsStringAsync();
				return JsonDocument.Parse(jsonString);
			}
			else
			{
				throw response.StatusCode switch
				{
					HttpStatusCode.NotFound => new Exception("Error: Not Found"),
					HttpStatusCode.BadRequest => new Exception("Error: Bad Request"),
					HttpStatusCode.Unauthorized => new Exception("Error: Unauthorized" + response.Content.ReadAsStringAsync().Result),
					HttpStatusCode.Forbidden => new Exception("Error: Forbidden"),
					HttpStatusCode.InternalServerError => new Exception("Error: Internal Server Error"),
					_ => new Exception($"Error: {response.StatusCode}"),
				};
			}
		}
		public async Task<JsonDocument> SuiteQlRequest(JsonObject body, Nullable<int> limit)
		{
			var url = _suiteQlBaseUrl;
			if (limit != null)
			{
				url += $"?limit={limit}";
			}
			using HttpClient client = new();
			var oAuth1HeaderGenerator = new OAuth1HeaderGenerator(_consumerKey, _consumerSecret, _token, _tokenSecret, _realm, HttpMethod.Post, url);
			var header = oAuth1HeaderGenerator.CreateAuthenticationHeaderValue();
			Console.WriteLine(header);
			Console.WriteLine(url);
			client.DefaultRequestHeaders.Authorization = header;
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("Prefer", "transient");

			HttpResponseMessage response;
	
			if (body == null)
			{
				throw new Exception("Error: Body is required for POST requests");
			}
			
			response = await client.PostAsync(url, new StringContent(body.ToString(), Encoding.UTF8, "application/json"));


			if (response.IsSuccessStatusCode)
			{
				var jsonString = await response.Content.ReadAsStringAsync();
				return JsonDocument.Parse(jsonString);
			}
			else
			{
				throw response.StatusCode switch
				{
					HttpStatusCode.NotFound => new Exception("Error: Not Found"),
					HttpStatusCode.BadRequest => new Exception("Error: Bad Request"),
					HttpStatusCode.Unauthorized => new Exception("Error: Unauthorized" + response.Content.ReadAsStringAsync().Result),
					HttpStatusCode.Forbidden => new Exception("Error: Forbidden"),
					HttpStatusCode.InternalServerError => new Exception("Error: Internal Server Error"),
					_ => new Exception($"Error: {response.StatusCode}"),
				};
			}
		}
	}
}