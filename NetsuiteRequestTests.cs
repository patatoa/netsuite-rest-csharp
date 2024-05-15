// using xunit;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using NetsuiteRequest;

namespace NetsuiteRequest.Tests
{
	public class NetsuiteRequestTests
	{
		[Xunit.Fact]
		public async Task RequestTest()
		{
			// Arrange
			var netsuiteRequest = GenerateNetsuiteRequest();
			string testId = Environment.GetEnvironmentVariable("TEST_ID") ?? throw new ArgumentNullException("TEST_ID");
			string urlString = $"/customer/{testId}";
			HttpMethod httpMethod = HttpMethod.Get;

			// Act
			var result = await netsuiteRequest.Request(urlString, httpMethod);

			// Assert
			Assert.NotNull(result);
		}
		[Xunit.Fact]
		public async Task SuiteQlRequestTest()
		{
			// Arrange
			var netsuiteRequest = GenerateNetsuiteRequest();
			var testEmail = Environment.GetEnvironmentVariable("TEST_EMAIL") ?? throw new ArgumentNullException("TEST_EMAIL");
			JsonObject body = new();
			body.Add("q", $"SELECT * FROM customer where email = '{testEmail}'");

			// Act
			var result = await netsuiteRequest.SuiteQlRequest(body, 1);

			// Assert
			Assert.NotNull(result);
		}

		private static NetsuiteRequest GenerateNetsuiteRequest()
		{
			string consumerKey = Environment.GetEnvironmentVariable("CONSUMER_KEY") ?? throw new ArgumentNullException("CONSUMER_KEY");
			string consumerSecret = Environment.GetEnvironmentVariable("CONSUMER_SECRET") ?? throw new ArgumentNullException("CONSUMER_SECRET");
			string token = Environment.GetEnvironmentVariable("TOKEN") ?? throw new ArgumentNullException("TOKEN");
			string tokenSecret = Environment.GetEnvironmentVariable("TOKEN_SECRET") ?? throw new ArgumentNullException("Token_SECRET");
			string realm = Environment.GetEnvironmentVariable("REALM") ?? throw new ArgumentNullException("REALM");

			return new NetsuiteRequest(consumerKey, consumerSecret, token, tokenSecret, realm);

		}
	}
}