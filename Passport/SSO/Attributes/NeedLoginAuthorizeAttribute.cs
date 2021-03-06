﻿#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.34011
     * 类 名 称：       NeedUrlAuthorizeAttribute
     * 机器名称：       JASON_PC
     * 命名空间：       SSO
     * 文 件 名：       NeedUrlAuthorizeAttribute
     * 创建时间：       2015/05/06 10:13:20
     * 计算机名：       Administrator
     * 作    者：       Jason.Yang(yangxing1002@gmail.com)
     * 说    明： 
     * 修改时间：
     * 修 改 人：
**************************************************************************************/
#endregion

using System.Web;
using System.Web.Mvc;
using SSO.Util;

namespace SSO.Attributes
{
    public class NeedLoginAuthorizeAttribute : ActionFilterAttribute
    {
        public NeedLoginAuthorizeAttribute()
        {
        }

        public NeedLoginAuthorizeAttribute(bool isNotCheck)
        {
            IsNotCheck = isNotCheck;
        }
        public bool IsNotCheck { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (IsNotCheck)
            {
                var uid = filterContext.HttpContext.Request.Cookies[Stands.UID];
                var token = filterContext.HttpContext.Request.Cookies[Stands.TOKEN];
                if (uid == null || uid.Value == null || token == null || token.Value == null)
                {
                    filterContext.Result = new RedirectResult(RouteUtils.GetAuthUrl(filterContext.HttpContext.Request.Url.AbsoluteUri));
                    return;
                }
                base.OnActionExecuting(filterContext);
            }
        }
    }
}
