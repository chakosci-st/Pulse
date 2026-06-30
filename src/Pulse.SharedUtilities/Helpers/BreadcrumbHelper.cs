using Pulse.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.SharedUtilities.Helpers
{
    public class Breadcrumb
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }

    /*
     Sample usage:
     *** With Area and Route Values ***
        var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
            area: "Admin",
            controller: "Plant",
            controllerTitle: "Plants",
            action: "Edit",
            actionTitle: "Edit Plant",
            areaTitle: "Administration",
            routeValues: new Dictionary<string, object> { { "id", 5 } }
        );
        // Output: /Admin/Plant/Edit?id=5

     *** With Area and Route Values ***
        var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
            area: null,
            controller: "Project",
            controllerTitle: "Projects",
            action: "Details",
            actionTitle: "Project Details",
            routeValues: new Dictionary<string, object> { { "id", 42 } }
        );
        // Output: /Project/Details?id=42         
         
         
     *** Home/Index (Root) ***
        var breadcrumbs = BreadcrumbHelper.GenerateBreadcrumbs(
            area: null,
            controller: "Home",
            controllerTitle: "Home",
            action: "Index",
            actionTitle: "Home"
        );
        // Output: Only Home breadcrumb         
         
         */


    public static class BreadcrumbHelper
    {
        /// <summary>
        /// Generates a breadcrumb trail for MVC areas, controllers, and actions.
        /// </summary>
        /// <param name="area">Area name (null or empty for root)</param>
        /// <param name="controller">Controller name</param>
        /// <param name="controllerTitle">Display title for controller</param>
        /// <param name="action">Action name</param>
        /// <param name="actionTitle">Display title for action</param>
        /// <param name="areaTitle">Optional display title for area</param>
        /// <param name="routeValues">Optional route values (e.g., id)</param>
        /// <returns>List of Breadcrumb objects</returns>
        public static List<Breadcrumb> GenerateBreadcrumbs(
            string area,
            string controller,
            string controllerTitle,
            string action,
            string actionTitle,
            string areaTitle = null,
            IDictionary<string, object> routeValues = null)
        {
            var breadcrumbs = new List<Breadcrumb>
        {
            new Breadcrumb { Title = "Home", Url = "/" }
        };

            // If Home/Index (root), return only Home
            bool isRoot = string.IsNullOrEmpty(area) &&
                          controller.Equals("Home", StringComparison.OrdinalIgnoreCase) &&
                          action.Equals("Index", StringComparison.OrdinalIgnoreCase);

            if (isRoot)
                return breadcrumbs;

            // Area breadcrumb
            if (!string.IsNullOrEmpty(area))
            {
                breadcrumbs.Add(new Breadcrumb
                {
                    Title = areaTitle ?? area,
                    Url = $"/{area}"
                });
            }

            // Controller breadcrumb
            if (!(controller.Equals("Home", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(area)))
            {
                breadcrumbs.Add(new Breadcrumb
                {
                    Title = controllerTitle,
                    Url = !string.IsNullOrEmpty(area) ? $"/{area}/{controller}" : $"/{controller}"
                });
            }

            // Action breadcrumb (if not Index)
            if (!action.Equals("Index", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(action))
            {
                string url = !string.IsNullOrEmpty(area)
                    ? $"/{area}/{controller}/{action}"
                    : $"/{controller}/{action}";

                if (routeValues != null && routeValues.Count > 0)
                {
                    var canonicalRouteKey = GetCanonicalRouteKey(routeValues);

                    if (canonicalRouteKey != null)
                    {
                        var routeValue = routeValues[canonicalRouteKey]?.ToString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(routeValue))
                        {
                            url = BuildCanonicalUrl(area, controller, action, canonicalRouteKey, routeValue);
                        }
                    }
                    else
                    {
                        var query = string.Join("&", routeValues.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value?.ToString() ?? "")}"));
                        url += "?" + query;
                    }
                }

                breadcrumbs.Add(new Breadcrumb
                {
                    Title = actionTitle,
                    Url = url
                });
            }

            return breadcrumbs;
        }

        private static string GetCanonicalRouteKey(IDictionary<string, object> routeValues)
        {
            if (routeValues.Count != 1)
            {
                return null;
            }

            var key = routeValues.Keys.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            return key.Equals("code", StringComparison.OrdinalIgnoreCase)
                || key.Equals("id", StringComparison.OrdinalIgnoreCase)
                || key.Equals("projectno", StringComparison.OrdinalIgnoreCase)
                ? key
                : null;
        }

        private static string BuildCanonicalUrl(string area, string controller, string action, string routeKey, string routeValue)
        {
            if (string.Equals(area, "Projects", StringComparison.OrdinalIgnoreCase)
                && string.Equals(controller, "Projects", StringComparison.OrdinalIgnoreCase)
                && string.Equals(routeKey, "projectno", StringComparison.OrdinalIgnoreCase))
            {
                return $"/{area}/{Uri.EscapeDataString(routeValue)}/{action}";
            }

            var baseUrl = !string.IsNullOrEmpty(area)
                ? $"/{area}/{controller}/{action}"
                : $"/{controller}/{action}";

            return $"{baseUrl}/{Uri.EscapeDataString(routeValue)}";
        }
    }
}
