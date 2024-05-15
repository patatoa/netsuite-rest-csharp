# Netsuite REST C\#

This is a budding Netsuite Rest Api SDK

## Prerequisites

> You'll need your Netsuite REST API credentials handy:

> - CONSUMER KEY
> - CONSUMER SECRET
> - TOKEN
> - TOKEN SECRET
> - REALM

At the moment you'll also need to know what endpoints and actions you'll want to reach. Some knowledge of he Netsuite REST API will be needed.

## How to use

See the unit test for example.

```cs
NetsuiteRequest netsuiteRequest = new NetsuiteRequest(consumerKey, consumerSecret, token, tokenSecret, realm);
string urlString = $"/customer/{testId}";
HttpMethod httpMethod = HttpMethod.Get;

JsonDocument result = await netsuiteRequest.Request(urlString, httpMethod);
```

### SuiteQL Example

```cs
JsonObject body = new();
body.Add("q", $"SELECT * FROM customer where email = '{testEmail}'");
var result = await netsuiteRequest.SuiteQlRequest(body, 1);
```

The hope is by returning a `JsonDocument` it provides a flexible format that the caller can then box however they need.

## History

This is _nominally_ a fork of [alejandrofierro/netsuite-rest-csharp](https://github.com/alejandrofierro/netsuite-rest-csharp) though I'm not sure any original code remains.
