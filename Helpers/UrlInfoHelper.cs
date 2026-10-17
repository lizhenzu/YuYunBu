using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;



namespace BulletinBoard.Helpers
{
    public static class UrlInfoHelper
    {
        public static (string Domain, string ControllerName, string ActionName) GetUrlInfo(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var routeData = httpContext.GetRouteData();

            string domain = $"{request.Scheme}://{request.Host}";
            string controllerName = routeData.Values["controller"]?.ToString();
            string actionName = routeData.Values["action"]?.ToString();

            return (domain, controllerName, actionName);
        }
    }

}
