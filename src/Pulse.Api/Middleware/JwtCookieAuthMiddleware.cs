using System.Threading.Tasks;
using Microsoft.Owin;

namespace Pulse.Api.Middleware
{
    public class JwtCookieAuthMiddleware : OwinMiddleware
    {
        public JwtCookieAuthMiddleware(OwinMiddleware next) : base(next) { }

        public override async Task Invoke(IOwinContext context)
        {
            System.Diagnostics.Debug.WriteLine("JwtCookieAuthMiddleware invoked!");
            // Check if the cookie exists
            var cookie = context.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(cookie))
            {
                // Set the Authorization header so JWT middleware can see it
                context.Request.Headers.Append("Authorization", "Bearer " + cookie);
            }

            await Next.Invoke(context);
        }
    }
}