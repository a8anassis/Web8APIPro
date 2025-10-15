﻿using Microsoft.Extensions.Logging;
using SchoolApp.Exceptions;
using Serilog;
using System.Net;

namespace SchoolApp.Helpers
{
    public class ErrorHandlerMiddleware
    {
        private readonly ILogger<ErrorHandlerMiddleware> logger = 
            new LoggerFactory().AddSerilog().CreateLogger<ErrorHandlerMiddleware>();

        private readonly RequestDelegate next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                var logContext = new
                {
                    ExceptionType = exception.GetType().Name,
                    EndPoint = context.Request.Path,
                    Method = context.Request.Method,
                    User = context.User.Identity?.Name ?? "Anonymous",
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    TraceId = context.TraceIdentifier
                };

                logger.LogError("{ExceptionType} at {Endpoint} {Method} by {User} | Trace={TraceId}",
                    logContext.ExceptionType, logContext.EndPoint, logContext.Method, logContext.User, logContext.TraceId);

                var response = context.Response;
                response.ContentType = "application/json";

                response.StatusCode = exception switch
                {
                    InvalidRegistrationException or
                    EntityAlreadyExistsException => (int)HttpStatusCode.BadRequest, // 400
                    EntityNotAuthorizedException => (int)HttpStatusCode.Unauthorized,    // 401
                    EntityForbiddenException => (int)HttpStatusCode.Forbidden,          // 403
                    EntityNotFoundException => (int)HttpStatusCode.NotFound,        // 404
                    _ => (int)HttpStatusCode.InternalServerError,                     // 500    
                };

                var result = System.Text.Json.JsonSerializer.Serialize(new { code = response.StatusCode, message = exception?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}
