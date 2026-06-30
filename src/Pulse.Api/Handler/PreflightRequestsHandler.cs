using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Pulse.Api.Handler
{
    public class PreflightRequestsHandler : DelegatingHandler
    {
        private readonly HashSet<string> _allowedOrigins;

        public PreflightRequestsHandler()
        {
            var allowedOrigins = System.Configuration.ConfigurationManager.AppSettings["cors-Origins"];
            _allowedOrigins = new HashSet<string>(
                allowedOrigins.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(o => o.Trim()),
                StringComparer.OrdinalIgnoreCase
            );
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. If it's a preflight request, respond immediately
            if (request.Method == HttpMethod.Options)
            {
                var preflightResponse = new HttpResponseMessage(HttpStatusCode.OK);

                AddCorsHeaders(request, preflightResponse);

                // Preflight typically doesn’t need a body
                return preflightResponse;
            }

            // 2. For non-OPTIONS, let the pipeline handle it
            var response = await base.SendAsync(request, cancellationToken);

            // 3. Add CORS headers to normal responses (incl. 401, 403, 500)
            AddCorsHeaders(request, response);

            return response;
        }

        private void AddCorsHeaders(HttpRequestMessage request, HttpResponseMessage response)
        {
            if (request.Headers.Contains("Origin"))
            {
                var origin = request.Headers.GetValues("Origin").FirstOrDefault();
                if (!string.IsNullOrEmpty(origin) && _allowedOrigins.Contains(origin))
                {
                    // make sure we only have one
                    response.Headers.Remove("Access-Control-Allow-Origin");
                    response.Headers.Add("Access-Control-Allow-Origin", origin);

                    // If you use credentials (cookies/Authorization header)
                    // response.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
            }

            // These are needed for preflight, but harmless on other responses
            response.Headers.Remove("Access-Control-Allow-Headers");
            response.Headers.Remove("Access-Control-Allow-Methods");
            response.Headers.Add("Access-Control-Allow-Headers", "Origin, Content-Type, Accept, Authorization, cache-control, x-requested-with");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Max-Age", "1728000");
        }
    }
}