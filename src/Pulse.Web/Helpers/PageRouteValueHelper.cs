using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Pulse.Web.Helpers
{
    public static class PageRouteValueHelper
    {
        public static ActionResult ResolveStringRouteValue(Controller controller, string value, string routeKey, params string[] queryKeys)
        {
            var resolvedValue = value;

            if (string.IsNullOrWhiteSpace(resolvedValue))
            {
                resolvedValue = GetQueryValue(controller, routeKey, queryKeys);
            }
            else
            {
                resolvedValue = resolvedValue.Trim();
            }

            if (string.IsNullOrWhiteSpace(resolvedValue))
            {
                return RedirectToIndex(controller);
            }

            if (HasAnyQueryValue(controller, routeKey, queryKeys))
            {
                return RedirectToCurrentAction(controller, routeKey, resolvedValue);
            }

            controller.ViewBag.Id = resolvedValue;
            return null;
        }

        public static ActionResult ResolveIntRouteValue(Controller controller, int value, string routeKey, params string[] queryKeys)
        {
            var resolvedValue = value;

            if (resolvedValue <= 0)
            {
                var queryValue = GetQueryValue(controller, routeKey, queryKeys);
                if (!int.TryParse(queryValue, out resolvedValue) || resolvedValue <= 0)
                {
                    return RedirectToIndex(controller);
                }
            }

            if (HasAnyQueryValue(controller, routeKey, queryKeys))
            {
                return RedirectToCurrentAction(controller, routeKey, resolvedValue);
            }

            controller.ViewBag.Id = resolvedValue;
            return null;
        }

        private static string GetQueryValue(Controller controller, string routeKey, string[] queryKeys)
        {
            var keys = new[] { routeKey };
            if (queryKeys != null && queryKeys.Length > 0)
            {
                keys = CombineKeys(routeKey, queryKeys);
            }

            foreach (var key in keys)
            {
                var value = controller.Request?.QueryString[key];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private static bool HasAnyQueryValue(Controller controller, string routeKey, string[] queryKeys)
        {
            var keys = new[] { routeKey };
            if (queryKeys != null && queryKeys.Length > 0)
            {
                keys = CombineKeys(routeKey, queryKeys);
            }

            foreach (var key in keys)
            {
                if (!string.IsNullOrWhiteSpace(controller.Request?.QueryString[key]))
                {
                    return true;
                }
            }

            return false;
        }

        private static string[] CombineKeys(string routeKey, string[] queryKeys)
        {
            var keys = new string[queryKeys.Length + 1];
            keys[0] = routeKey;
            Array.Copy(queryKeys, 0, keys, 1, queryKeys.Length);
            return keys;
        }

        private static RedirectToRouteResult RedirectToCurrentAction(Controller controller, string routeKey, object routeValue)
        {
            var area = controller.RouteData.DataTokens["area"]?.ToString();
            var controllerName = controller.RouteData.Values["controller"]?.ToString();
            var actionName = controller.RouteData.Values["action"]?.ToString();
            var routeValues = new RouteValueDictionary
            {
                ["action"] = actionName,
                ["controller"] = controllerName,
                [routeKey] = routeValue
            };

            if (!string.IsNullOrWhiteSpace(area))
            {
                routeValues["area"] = area;
            }

            return new RedirectToRouteResult(routeValues);
        }

        private static RedirectToRouteResult RedirectToIndex(Controller controller)
        {
            var area = controller.RouteData.DataTokens["area"]?.ToString();
            var controllerName = controller.RouteData.Values["controller"]?.ToString();
            var routeValues = new RouteValueDictionary
            {
                ["action"] = "Index",
                ["controller"] = controllerName
            };

            if (!string.IsNullOrWhiteSpace(area))
            {
                routeValues["area"] = area;
            }

            return new RedirectToRouteResult(routeValues);
        }
    }
}