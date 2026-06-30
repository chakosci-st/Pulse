using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AutoMapper;


namespace Pulse.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            System.Diagnostics.Debug.WriteLine(">>> Application_Start from Pulse.Api.WebApiApplication <<<");

            Mapper.Initialize(c => c.AddProfile<MappingProfile>());
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AutofacConfig.RegisterDependencies();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            ////HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            ////if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            ////{
            ////    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
            ////    HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
            ////    HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
            ////    HttpContext.Current.Response.End();
            ////}

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            System.Diagnostics.Debug.WriteLine("ERROR:" + exception.Message);

            // Log the exception using your logging framework of choice.

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
                    case 404:
                        Response.Redirect("~/Error/NotFound");
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
