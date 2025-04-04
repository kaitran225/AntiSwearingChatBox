using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AntiSwearingChatBox.Server.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip detailed logging for SignalR requests
            bool isSignalRRequest = context.Request.Path.StartsWithSegments("/chatHub", StringComparison.OrdinalIgnoreCase);
            
            if (!isSignalRRequest)
            {
                // Log only essential request information
                LogRequestBasicInfo(context);
            }

            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;

            try
            {
                // Only create a memory stream for non-SignalR requests that we want to log
                if (!isSignalRRequest)
                {
                    // Create a new memory stream for the response
                    using var responseBody = new MemoryStream();
                    context.Response.Body = responseBody;

                    // Continue down the middleware pipeline
                    await _next(context);

                    // Log response basics 
                    LogResponseBasicInfo(context);

                    // Copy the response stream back to the original body
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
                else
                {
                    // For SignalR requests, just continue without any logging overhead
                    await _next(context);
                }
            }
            finally
            {
                // Ensure the response body is restored
                if (!isSignalRRequest)
                {
                    context.Response.Body = originalBodyStream;
                }
            }
        }

        private void LogRequestBasicInfo(HttpContext context)
        {
            // Log only method, path and query string
            _logger.LogDebug(
                "REQUEST: {Method} {Path}{QueryString}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString);
        }

        private void LogResponseBasicInfo(HttpContext context)
        {
            // Log only status code
            _logger.LogDebug(
                "RESPONSE: {StatusCode} for {Method} {Path}",
                context.Response.StatusCode,
                context.Request.Method,
                context.Request.Path);
        }
    }
} 