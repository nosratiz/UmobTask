namespace GbfsQuiz.Web.Common.Middleware;

/// <summary>Adds baseline security response headers (CSP, nosniff, HSTS).</summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        headers["Content-Security-Policy"] =
            "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; " +
            "connect-src 'self'; base-uri 'self'; frame-ancestors 'none'";

        await next(context);
    }
}
