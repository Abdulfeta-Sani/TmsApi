using Microsoft.AspNetCore.Mvc.Filters;

namespace TmsApi.Filters;

public class AuditLogFilter(ILogger<AuditLogFilter> logger) : IActionFilter, IResultFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var route = context.HttpContext.Request.Path;
        var method = context.HttpContext.Request.Method;

        logger.LogInformation("TMS API call: {Method} {Route}", method, route);
    }

        public void OnActionExecuted(ActionExecutedContext context)
    {
    }

        public void OnResultExecuting(ResultExecutingContext context)
    {
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        var status = context.HttpContext.Response.StatusCode;

        logger.LogInformation("TMS API response: {StatusCode}", status);
    }
}