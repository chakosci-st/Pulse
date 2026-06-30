using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Web.Http.Cors;
using System.Web.Http.Routing;
using Pulse.Api.Handler;

namespace Pulse.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // ---------- CORS ----------
            var allowedOriginsRaw = System.Configuration.ConfigurationManager.AppSettings["cors-Origins"];

            if (string.IsNullOrWhiteSpace(allowedOriginsRaw))
            {
                throw new InvalidOperationException("AppSetting 'cors-Origins' is not configured.");
            }

            // Normalize and filter origins
            var origins = allowedOriginsRaw
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .Where(o => !string.IsNullOrEmpty(o))
                .ToArray();

            var cors = new EnableCorsAttribute(
                origins: string.Join(",", origins),
                headers: "Origin, Content-Type, Accept, Authorization",
                methods: "GET, POST, PUT, DELETE, OPTIONS")
            {
                SupportsCredentials = false
            };

            // CORS must be enabled early
            config.EnableCors(cors);

            // Global preflight handling (message handler should be early)
            config.MessageHandlers.Add(new PreflightRequestsHandler());

            // ---------- Formatters ----------
            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
            jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonSettings.Formatting = Formatting.Indented;

            // JSON via ?type=json
            config.Formatters.JsonFormatter.MediaTypeMappings.Add(
                new QueryStringMapping("type", "json", new MediaTypeHeaderValue("application/json")));

            // XML via ?type=xml
            config.Formatters.XmlFormatter.MediaTypeMappings.Add(
                new QueryStringMapping("type", "xml", new MediaTypeHeaderValue("application/xml")));

            // Allow JSON for text/html (useful for browsers)
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(
                new MediaTypeHeaderValue("text/html"));

            // ---------- Routing ----------
            var constraintResolver = new DefaultInlineConstraintResolver();
            constraintResolver.ConstraintMap.Add("string", typeof(StringConstraint));

            // Attribute routes with custom constraint resolver
            config.MapHttpAttributeRoutes(constraintResolver);



            //// config.Routes.MapHttpRoute(
            ////    name: "DefaultApiActionKey",
            ////    routeTemplate: "api/{controller}/{action}/{key}",
            ////    defaults: new { key = RouteParameter.Optional }
            ////);

            //// config.Routes.MapHttpRoute(
            ////     name: "DefaultApi_key",
            ////     routeTemplate: "api/{controller}/{key}",
            ////     defaults: new { code = RouteParameter.Optional }
            //// );

            //// config.Routes.MapHttpRoute(
            ////     name: "DefaultApi_code",
            ////     routeTemplate: "api/{controller}/{code}",
            ////     defaults: new { code = RouteParameter.Optional }
            //// );
            ///


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

        }
    }
}
