using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net;
using System.Web.Http; //<--Microsoft.AspNet.WebApi.WebHost
namespace Pulse.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MvcApplication));

        protected void Application_Start()
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "employeeid";
            ContainerConfig.RegisterContainer(GlobalConfiguration.Configuration);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private void LogException(Exception ex)
        {
            log4net.Config.XmlConfigurator.Configure();
            int level = 0;
            while (ex != null)
            {
                // You can replace this with your logging framework (log4net, NLog, etc.)
                System.Diagnostics.Debug.WriteLine(
                    $"[Exception Level {level}] {ex.GetType().FullName}: {ex.Message}\nStackTrace: {ex.StackTrace}"
                );

                log.Error($"[Exception Level {level}] {ex.GetType().FullName}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                ex = ex.InnerException;
                level++;
            }
        }
      

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            // Log the exception using your logging framework of choice.

            System.Diagnostics.Debug.WriteLine("Unhandled error: " + exception.Message);

            // 2. Store error message in a cookie  
            HttpCookie errorCookie = new HttpCookie("AppErrorMessage", exception.Message)
            {
                Expires = DateTime.Now.AddMinutes(1)
            };
            Response.Cookies.Add(errorCookie);


            // 3. add to logfile  
            LogException(exception);





            //log.Info("Application is starting...");
            //log.Debug("This is a debug message.");
            //log.Warn("This is a warning message.");


            //log.Fatal("This is a fatal message.");


            // Clear the error from the server.
            Server.ClearError();

            // Check if the exception is an HttpException.
            if (exception is HttpException httpException)
            {
                // Check the status code 400 (BadRequest).
                switch (httpException.GetHttpCode())
                {
                    // Redirect to a custom BadRequest error page.
                    case 400:
                        Response.Redirect("~/Error/BadRequest");
                        break;
                    // Redirect to a custom AccessDenied error page.
                    case 401:
                        Response.Redirect("~/Error/AccessDenied");
                        break;

                    case 403:
                        Response.Redirect("~/Error/Forbidden");
                        break;
                    case 404:
                        Response.Redirect("~/Error/NotFound");
                        break;
                    case 500:
                        Response.Redirect("~/Error/ServerError");
                        break;
                    case 503:
                        Response.Redirect("~/Error/ServiceUnavailable");
                        break;
                    default:
                        Response.Redirect("~/Error/General");
                        break;
                }


            }

            else
            {
                // Redirect to a general error page for other types of errors.
                Response.Redirect("~/Error/General");
            }
        }
    }
}
