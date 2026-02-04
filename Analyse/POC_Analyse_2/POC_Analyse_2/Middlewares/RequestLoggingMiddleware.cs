using Serilog;
using System.Buffers;
using System.Text;

namespace POC_Analyse_2.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger) 
        {
            _logger = logger;
            _next = next;

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
            if (context.Request.Method == "GET")
            {
                Log.Information(
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} - {Path} - {UrlReferrer} - {Action} - {SessionId} - {UserAgent}",
                    DateTimeOffset.Now,
                    context.Request.Path,
                    context.Request.Headers.Referer.ToString(),
                    null,
                    context.Session.Id,
                    context.Request.Headers.UserAgent
                );
            }
            else if (context.Request.Method == "POST")
            {
                var bodyStream = await GetListOfStringsFromStream( context.Request.Body );


                Log.Information(
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} - {Path} - {UrlReferrer} - {Action} - {SessionId} - {UserAgent}",
                    DateTimeOffset.Now,
                    context.Request.Path,
                    context.Request.Headers.Referer.ToString(),
                    bodyStream.First(),
                    context.Session.Id,
                    context.Request.Headers.UserAgent
                );
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
