using eCommerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace eCommerce.SharedLibrary.Middleware
{
    public class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            string message = "internal server error occurred.";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";

            try
            {
                await next(context);

                if(context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "Too many request";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);
                }

                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "Not Authenticated";
                    statusCode = (int)StatusCodes.Status401Unauthorized;

                    await ModifyHeader(context, title, message, statusCode);
                }

                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out of Access";
                    message = "Not Authorized level";
                    statusCode = (int)StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);
                }

            }catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                if (ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = "out of time";
                    message = "out of time";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }

                await ModifyHeader(context, title, message, statusCode);
            }

        }

        private async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails() { 
                Detail = message,
                Status = statusCode,
                Title = title
            } ), CancellationToken.None);
            return;
        }
    }
}
