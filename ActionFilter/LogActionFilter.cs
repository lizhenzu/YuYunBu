using BulletinBoard.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

public class LogActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerName = context.RouteData.Values["controller"].ToString();
        var actionName = context.RouteData.Values["action"].ToString();
        var userName = context.HttpContext.User.Identity.Name ?? "Anonymous";

        // 클라이언트의 IP 주소 가져오기
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();

        // 로그 메시지 생성
        var logMessage = $"Request to {controllerName}.{actionName} made by {userName}, IP: {ipAddress}";

        // 로그 출력 (실제 프로덕션에서는 로깅 프레임워크 사용 권장)
        Console.WriteLine(logMessage);

        // 동기 DB 쓰기를 백그라운드로 처리 — 요청 스레드를 블록하지 않음
        _ = System.Threading.Tasks.Task.Run(() =>
            SharedComm.AddToLog(controllerName, actionName, userName, ipAddress));

        base.OnActionExecuting(context);
    }
}
