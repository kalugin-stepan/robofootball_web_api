namespace Helpers;

public class AuthenticationMiddleware {
    private readonly RequestDelegate next;
    private readonly string secret;
    public AuthenticationMiddleware(RequestDelegate next, string secret) {
        this.next = next;
        this.secret = secret;
    }

    public async Task Invoke(HttpContext context)
    {
        string? secret = context.Request.Headers["secret"];
        if (secret == null)
        {
            context.Items["is_auth"] = false;
            await next(context);
            return;
        }
        context.Items["is_auth"] = this.secret == secret;
        await next(context);
    }
}