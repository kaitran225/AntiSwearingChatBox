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
            // Log the request
            await LogRequest(context);

            // Capture the response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                // Continue down the middleware pipeline
                        await _next(context);

                // Log the response
                await LogResponse(context, responseBody, originalBodyStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request");
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            // Keep track of the request body
            var requestBodyText = string.Empty;
            if (context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);
                
                requestBodyText = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            // Log the request details
            var message = $@"
===== HTTP REQUEST =====
{context.Request.Method} {context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}
Content-Type: {context.Request.ContentType}
{(string.IsNullOrEmpty(requestBodyText) ? "No Body" : $"Body: {requestBodyText}")}
======================";

            _logger.LogInformation(message);
        }

        private async Task LogResponse(HttpContext context, MemoryStream responseBody, Stream originalBodyStream)
        {
            responseBody.Position = 0;

            // Read the response body
            var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
            
            // Copy the response body back to the original stream
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalBodyStream);

            // Log the response details
            var message = $@"
===== HTTP RESPONSE =====
Status: {context.Response.StatusCode}
Content-Type: {context.Response.ContentType}
{(string.IsNullOrEmpty(responseBodyText) ? "No Body" : $"Body: {responseBodyText}")}
======================";

            _logger.LogInformation(message);
        }
    }
} 