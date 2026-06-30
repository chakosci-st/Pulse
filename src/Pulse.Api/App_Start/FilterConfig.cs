using Pulse.Api.Filters;
using System.Web;
using System.Web.Mvc;

namespace Pulse.Api
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute()); 
        }
    }
}
