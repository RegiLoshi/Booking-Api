using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace BookingApp.Middleware;

public class GlobalExceptionHandler(RequestDelegate _next, ILogger<GlobalExceptionHandler> _logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                message = "An unhandled exception occurred",
                detail = ex.Message,
            };
            
            var json = JsonSerializer.Serialize(response);
            await httpContext.Response.WriteAsync(json);
        }
    }
}