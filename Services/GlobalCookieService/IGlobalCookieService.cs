using System.Collections.Generic;

public interface IGlobalCookieService
{
    Task<bool> ShouldShowAsync(string cookieKey, HttpContext httpContext);
    Task SetHideAsync(string cookieKey, string durationCode, HttpContext httpContext);
}
