# Rate limiter demo
In this repository, I'm illustrating the implementation of rate limiting in a specific scenario:

## Scenario:
You possess public APIs developed in .NET 8 that cater to both authenticated and anonymous users. It is necessary to impose API access restrictions for both user types, with anonymous users having a lower limit than authenticated users. Additionally, when the APIs are accessed via localhost (loopback address), there should be no limits imposed, catering to developers in the local environment.
In the demonstration application, authenticated users are subjected to a rate limit of 10 requests per minute, while anonymous users are limited to 5 requests per minute. Requests originating from localhost, meant for local development, are exempt from any rate limits.

In the application, GlobalLimiter is used which globally limits the application but if you want different rate limits in certain endpoints, it is also possible by creating a Chained limiter.
