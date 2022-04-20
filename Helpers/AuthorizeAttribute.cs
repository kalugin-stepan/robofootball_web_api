using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Helpers;

public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var isAuth = (bool?)context.HttpContext.Items["is_auth"];
        if (isAuth == null || isAuth == false)
        {
            var rez = new StatusCodeResult(403);
            context.Result = rez;
        }
    }
}