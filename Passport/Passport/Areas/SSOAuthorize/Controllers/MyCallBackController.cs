using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SSO;
using SSO.Model;

namespace Passport.Areas.SSOAuthorize.Controllers
{
    public class MyCallBackController : SSOAuthClientController
    {
        //
        // GET: /SSOAuthorize/MyCallBack/
        public override void CallBacking(string url, SSOUser user)
        {
            Session["User"] = user;
            base.CallBacking(url, user);
        }
    }
}
