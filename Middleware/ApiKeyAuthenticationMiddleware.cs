namespace TempMonitor.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only check API key for POST endpoints that require authentication
        if (context.Request.Method == "POST" &&
            (context.Request.Path.StartsWithSegments("/api/sensor-data") ||
             context.Request.Path.StartsWithSegments("/api/errors")))
        {
            var expectedApiKey = _configuration["Authentication:ApiKey"];

            if (string.IsNullOrEmpty(expectedApiKey))
            {
                _logger.LogError("API key is not configured in settings");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "Server configuration error" });
                return;
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                _logger.LogWarning("Missing Authorization header from {RemoteIp}", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Missing Authorization header" });
                return;
            }

            var headerValue = authHeader.ToString();
            if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid Authorization header format from {RemoteIp}", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid Authorization header format" });
                return;
            }

            var providedApiKey = headerValue.Substring("Bearer ".Length).Trim();
            if (providedApiKey != expectedApiKey)
            {
                _logger.LogWarning("Invalid API key provided from {RemoteIp}", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
                return;
            }

            _logger.LogDebug("API key validated successfully for {Path}", context.Request.Path);
        }

        await _next(context);
    }
}

// Extension method to make it easy to add the middleware to the pipeline
public static class ApiKeyAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyAuthenticationMiddleware>();
    }
}
