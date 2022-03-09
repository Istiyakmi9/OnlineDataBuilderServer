using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ModalLayer.Modal;
using Newtonsoft.Json;
using OnlineDataBuilder.ContextHandler;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SchoolInMindServer.MiddlewareServices
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration configuration;

        public ExceptionHandlerMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.configuration = configuration;
            _next = next;
        }

        public async Task Invoke(HttpContext context, CurrentSession currentSession)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (HiringBellException exception)
            {
                await HandleHiringBellExceptionMessageAsync(context, exception);
            }
            catch (Exception ex)
            {
                await HandleExceptionMessageAsync(context, ex);
            }
        }

        private static Task HandleHiringBellExceptionMessageAsync(HttpContext context, HiringBellException e)
        {
            context.Response.ContentType = "application/json";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            var result = JsonConvert.SerializeObject(new ApiResponse
            {
                AuthenticationToken = string.Empty,
                HttpStatusCode = e.HttpStatusCode,
                HttpStatusMessage = e.UserMessage,
                ResponseBody = new { e.Message, InnerMessage = e.InnerException?.Message }
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsync(result);
        }

        private static Task HandleExceptionMessageAsync(HttpContext context, Exception e)
        {
            context.Response.ContentType = "application/json";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            var result = JsonConvert.SerializeObject(new ApiResponse
            {
                AuthenticationToken = string.Empty,
                HttpStatusCode = HttpStatusCode.BadRequest,
                HttpStatusMessage = e.Message,
                ResponseBody = new { e.Message, InnerMessage = e.InnerException?.Message }
            });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsync(result);
        }
    }
}
