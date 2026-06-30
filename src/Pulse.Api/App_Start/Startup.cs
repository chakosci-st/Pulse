using Microsoft.Owin;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using Owin;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Pulse.Api.Middleware;
using System.Security.Claims;

[assembly: OwinStartup(typeof(Pulse.Api.Startup))]

namespace Pulse.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.Use<JwtCookieAuthMiddleware>();

            var secret = Encoding.UTF8.GetBytes(System.Configuration.ConfigurationManager.AppSettings["Jwt:SecretKey"].ToString());
            var issuer = System.Configuration.ConfigurationManager.AppSettings["Jwt:Issuer"].ToString();
            var audience = System.Configuration.ConfigurationManager.AppSettings["Jwt:Audience"].ToString();
 
            // Configure JWT Bearer Authentication
            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(secret),
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                }
            });
        }
    }
}