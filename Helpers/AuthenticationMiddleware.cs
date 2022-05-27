namespace Helpers;

public class AuthenticationMiddleware {
    private readonly RequestDelegate next;
    public AuthenticationMiddleware(RequestDelegate next) {
        this.next = next;
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
        context.Items["is_auth"] = Globals.secret == secret;
        await next(context);
    }
}