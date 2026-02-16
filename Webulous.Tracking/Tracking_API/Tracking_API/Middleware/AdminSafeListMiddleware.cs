using System.Net;
using Tracking_API.Model;

namespace Tracking_API.Middleware
{
    public class AdminSafeListMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminSafeListMiddleware> _logger;
        private readonly IPManager _ipManager;

        public AdminSafeListMiddleware(
            RequestDelegate next,
            ILogger<AdminSafeListMiddleware> logger,
            IPManager ipManager)
        {
            _ipManager = ipManager;
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            _logger.LogDebug("Request from Remote IP address: {RemoteIp}", remoteIp);
            var badIp = true;

            try
            {
                if(remoteIp != null) badIp = !_ipManager.IsInSafeList(remoteIp.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                badIp = true;
            }
            

            if (badIp)
            {
                _logger.LogWarning(
                    "Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            await _next.Invoke(context);
        }
    }
}
