using System.Web;
using System.Web.Mvc;
using SSO.Attributes;

namespace Passport
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new NeedUrlAuthorizeAttribute());
        }
    }
}