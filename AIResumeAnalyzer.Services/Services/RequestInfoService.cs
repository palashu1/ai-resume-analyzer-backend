using AIResumeAnalyzer.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Services.Services
{
    public class RequestInfoService : IRequestInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestInfoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?
                       .Request
                       .Headers["User-Agent"]
                       .ToString()
                   ?? "Unknown";
        }

        public string GetIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return "Unknown";

            // Production (Azure, Nginx, Cloudflare, IIS Reverse Proxy)
            var forwardedIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(forwardedIp))
            {
                return forwardedIp.Split(',')[0].Trim();
            }

            // Development (Kestrel / Localhost)
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
