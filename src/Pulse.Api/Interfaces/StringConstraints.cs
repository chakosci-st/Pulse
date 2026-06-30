using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;

public class StringConstraint : IHttpRouteConstraint
{
    public bool Match(
        HttpRequestMessage request,
        IHttpRoute route,
        string parameterName,
        IDictionary<string, object> values,
        HttpRouteDirection routeDirection)
    {
        if (values.TryGetValue(parameterName, out var value) && value != null)
        {
            return value is string; // Ensure the value is a string
        }
        return false;
    }
}