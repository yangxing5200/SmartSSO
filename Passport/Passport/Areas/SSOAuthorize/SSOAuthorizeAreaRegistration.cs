using System.Web.Mvc;

namespace Passport.Areas.SSOAuthorize
{
    public class SSOAuthorizeAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SSOAuthorize";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "SSOAuthorize_default",
                "SSOAuthorize/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
