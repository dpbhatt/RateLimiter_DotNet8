namespace RateLimiterDotNet.Models;

public class RateLimitOptions
{
    public const string RateLimit = "RateLimit";
    public int PermitLimit { get; set; }
    public int AnonymousPermitLimit { get; set; }
    public int Window { get; set; }
    public int QueueLimit { get; set; }
}