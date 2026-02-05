using POC_Analyse_2.Models.Dto;
using Serilog;
using System;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace POC_Analyse_2.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly HttpClient client;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger) 
        {
            _logger = logger;
            _next = next;
            client = new HttpClient();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    "Logs/tracking-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} - {Path} - {UrlReferrer} - {Action} - {SessionId} - {UserAgent}{NewLine}"
                )
                .CreateLogger();
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == "GET" || context.Request.Method == "POST")
            {
                string? action = context.Request.Method == "POST"
                    ? (await GetListOfStringsFromStream(context.Request.Body)).FirstOrDefault()
                    : null;

                var log = new RequestLogDto(
                    DateTimeOffset.Now,
                    Url: $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}",
                    UrlReferrer: context.Request.Headers.Referer.ToString() == "" ? "null" : context.Request.Headers.Referer.ToString(),
                    Action: action,
                    SessionId: context.Session.Id,
                    UserAgent: context.Request.Headers["User-Agent"].FirstOrDefault() ?? "null"
                );

                var json = JsonSerializer.Serialize(log);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync("http://localhost:5188/TrackingLogs", content);
            }

            await _next(context);
        }

        private async Task<List<string>> GetListOfStringsFromStream(Stream requestBody)
        {
            // Build up the request body in a string builder.
            StringBuilder builder = new StringBuilder();

            // Rent a shared buffer to write the request body into.
            byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);

            while (true)
            {
                var bytesRemaining = await requestBody.ReadAsync(buffer, offset: 0, buffer.Length);
                if (bytesRemaining == 0)
                {
                    break;
                }

                // Append the encoded string into the string builder.
                var encodedString = Encoding.UTF8.GetString(buffer, 0, bytesRemaining);
                builder.Append(encodedString);
            }

            ArrayPool<byte>.Shared.Return(buffer);

            var entireRequestBody = builder.ToString();

            // Split on \n in the string.
            return new List<string>(entireRequestBody.Split("\n"));
        }
    }
}
