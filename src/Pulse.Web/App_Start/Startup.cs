using Autofac;
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(typeof(Pulse.Web.Startup))]
namespace Pulse.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Cookie auth
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Auth/Login")
            });

            // Build Autofac container
            IContainer container = ContainerConfig.Configure();

            // Set SignalR to use Autofac
            var signalRResolver = new AutofacDependencyResolver(container);
            GlobalHost.DependencyResolver = signalRResolver;

            // Map SignalR
            app.MapSignalR();
        }
    }
}