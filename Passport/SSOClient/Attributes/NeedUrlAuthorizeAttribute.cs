#region
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

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;
using System.Web.Mvc;

using SSO.Util;
using SSOClient.Model;
using SSOClient.Util;
using System.Web.Http.Filters;
namespace SSOClient.Attributes
{
    public class NeedUrlAuthorizeAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        public NeedUrlAuthorizeAttribute()
        {
        }

        public bool NotCheck { get; set; }

        /// <summary>
        /// url验证：要求登录并且有权访问
        /// </summary>
        /// <param name="notCheck">false 不验证</param>
        public NeedUrlAuthorizeAttribute(bool notCheck)
        {
            NotCheck = notCheck;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null
                || filterContext.HttpContext == null
                || filterContext.HttpContext.Request == null
                || filterContext.HttpContext.Request.Url == null) { return; }
            var url = filterContext.HttpContext.Request.Url.AbsoluteUri;
            //不需要验证直接跳过
            if (NotCheck) return;

            var uid = Tools.GetCookie(Stands.UID);
            var token = Tools.GetCookie(Stands.TOKEN);

            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(token))
            {
                //ajax 判断
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.HttpContext.Response.Write("您还没有登录，请先登录");
                    return;
                }

                filterContext.Result = new RedirectResult(WebUtils.GetAuthUrl(url));
                return;
            }

            WebUtils web = new WebUtils();
            var cacheInfo = web.DoGet(Stands.AUTH_HOST + "/Authorize/TokenGetCredence/", new CJDictionary { { "projectCode", Stands.PROJECT_CODE }, { "token", token } });
            var loginCache = Tools.JsonDeserialize<CacheLoginModel>(cacheInfo);

            if (!loginCache.IsLogin)
            {
                //保险起见删除本地cookie
                Tools.ClearCookie(Stands.UID);
                Tools.ClearCookie(Stands.TOKEN);
                Tools.ClearCookie(Stands.PROJECT_CODE);
                //ajax 判断
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new ContentResult { Content = "无权访问" };
                    return;
                }
                filterContext.Result = new RedirectResult(WebUtils.GetAuthUrl(url));
                return;
            }

            //状态被修改为限制状态后跳转到指定页面
            if (loginCache.Info.IsRedirect)
            {
                filterContext.Result = new RedirectResult(loginCache.Info.CallBackUrl);
                return;
            }
            //更新cookie
            Tools.SetCookie(Stands.UID, loginCache.Info.Id);
            Tools.SetCookie(Stands.TOKEN, token);

            base.OnActionExecuting(filterContext);
        }
      

    }

    public class NeedUrlAuthorizeApiAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public NeedUrlAuthorizeApiAttribute()
        {
        }

        public bool NotCheck { get; set; }

        /// <summary>
        /// url验证：要求登录并且有权访问
        /// </summary>
        /// <param name="notCheck">false 不验证</param>
        public NeedUrlAuthorizeApiAttribute(bool notCheck)
        {
            NotCheck = notCheck;
        }
        
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null
                || actionContext.Request == null
                || actionContext.Request.RequestUri == null
            ) { return; }
            var url = actionContext.Request.RequestUri.AbsoluteUri;
            //不需要验证直接跳过
            if (NotCheck) return;

            var uid = Tools.GetCookie(Stands.UID);
            var token = Tools.GetCookie(Stands.TOKEN);

            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(token))
            {
                var response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Redirect,
                    "Unauthorized");
                response.Headers.Add("Location", WebUtils.GetAuthUrl(url));
                actionContext.Response = response;
              
                return;
            }

            WebUtils web = new WebUtils();
            var cacheInfo = web.DoGet(Stands.AUTH_HOST + "/Authorize/TokenGetCredence/", new CJDictionary { { "projectCode", Stands.PROJECT_CODE }, { "token", token } });
            var loginCache = Tools.JsonDeserialize<CacheLoginModel>(cacheInfo);

            if (!loginCache.IsLogin)
            {
                //保险起见删除本地cookie
                Tools.ClearCookie(Stands.UID);
                Tools.ClearCookie(Stands.TOKEN);
                Tools.ClearCookie(Stands.PROJECT_CODE);
                //ajax 判断
                var response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Redirect,
                     "Unauthorized");
                response.Headers.Add("Location", WebUtils.GetAuthUrl(url));
                actionContext.Response = response;
                return;
            }

            //状态被修改为限制状态后跳转到指定页面
            if (loginCache.Info.IsRedirect)
            {
                var response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Redirect,
                  "Unauthorized");
                response.Headers.Add("Location", loginCache.Info.CallBackUrl);
                actionContext.Response = response;
                return;
            }
            //更新cookie
            Tools.SetCookie(Stands.UID, loginCache.Info.Id);
            Tools.SetCookie(Stands.TOKEN, token);
            base.OnActionExecuting(actionContext);
        }

        
    }
}
