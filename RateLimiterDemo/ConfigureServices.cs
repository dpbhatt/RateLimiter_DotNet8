using RateLimiterDotNet.Models;
using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;

namespace IdentityDemo;

public static class ConfigureServices
{
    public static IServiceCollection AddWebAPIServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        var rateLimitOptions = new RateLimitOptions();
        configuration.GetSection(RateLimitOptions.RateLimit).Bind(rateLimitOptions);

        services.AddRateLimiter(rateLimiterOptions =>
        {
            rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                if (httpContext.User.Identity?.IsAuthenticated is true)
                {
                    string username = httpContext.User.ToString()!;

                    return RateLimitPartition.GetFixedWindowLimiter(username,
                           _ => new FixedWindowRateLimiterOptions
                           {
                               PermitLimit = rateLimitOptions.PermitLimit,
                               QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                               QueueLimit = rateLimitOptions.QueueLimit,
                               Window = TimeSpan.FromMinutes(rateLimitOptions.Window),
                           });
                }

                IPAddress? remoteIpAddress = httpContext.Connection.RemoteIpAddress;

                if (!IPAddress.IsLoopback(remoteIpAddress!))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(remoteIpAddress!.ToString(),
                            _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = rateLimitOptions.AnonymousPermitLimit,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = rateLimitOptions.QueueLimit,
                                Window = TimeSpan.FromMinutes(rateLimitOptions.Window),
                            });
                }

                return RateLimitPartition.GetNoLimiter(IPAddress.Loopback.ToString());
            });

            // When rate limit reached this callback responds
            rateLimiterOptions.OnRejected = (context, token) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                    }

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);

                    return new ValueTask();
                };
        });

        return services;
    }
}