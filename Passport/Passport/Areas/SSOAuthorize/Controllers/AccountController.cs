using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SSO.Attributes;
using SSO.Model;
using SSO.Util;
using UApiService;

namespace Passport.Areas.SSOAuthorize.Controllers
{
    [NeedUrlAuthorize(NotCheck = true)]
    public class AccountController : Controller
    {
        #region 页面初始化

        private readonly BizCenter _biz;

        public AccountController()
        {
            _biz = _biz ?? (_biz = new BizCenter());


        }
        #endregion
        //
        // GET: /SSOAuthorize/Account/

        public ActionResult Index()
        {
            if (!CJUtils.VerifyResponse(Request.Url.AbsoluteUri, Stands.SIGN_SECRET))
            {
                return Content("签名验证不通过，请勿非法请求！ 您的IP：" + Tools.GetIP4Address() + "已记录。");
            }
            var callBack = Request.QueryString["callback"];
            var openId = Request.QueryString["openId"];
            var projectCode = Request.QueryString["projectCode"];
            var avatar = Request.QueryString["avatar"];
            if (string.IsNullOrEmpty(callBack) || string.IsNullOrEmpty(projectCode))
            {

                return Json(new AuthMessage
                {
                    Code = (int)SSOError.缺少必要的参数,
                    Message = SSOError.缺少必要的参数.ToString()
                });
            }
            Tools.SetCookie("_CallBack",callBack);
            Tools.SetCookie("_OpenId",openId);
            Tools.SetCookie("_ProjectCode",projectCode);
            Tools.SetCookie("_Avatar", avatar);
            var proj = _biz.ApiGet<Project>(null, "GetProject", new Dictionary<string, object> {{"ProjectCode", projectCode}}, true);
            ViewBag.Logo = proj.List.First().Avatar;
            return View();
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        [NeedUrlAuthorize(true)]
        public ActionResult Login(string account, string password)
        {
            var result = _biz.ApiGet<SSOUser>(UserInfoEx.GetUserInfoEx(), "UserLogin", new Dictionary<string, object> { { "Account", account },{"ProjectCode",Tools.GetCookie("_ProjectCode")} }, true);
            if (!result.Success) return Json(new AuthMessage { Message = "未知错误，已发送邮件通知管理员，请耐心等待！" });

            if (result.Count == 0)
            {
                return Json(new AuthMessage { Message = "用户名不存在！" });
            }

            var ssoUser = result.List.First();

            if (ssoUser == null) return Json(new AuthMessage { Message = "用户名不存在！" });
            if (ssoUser.Password.ToUpper() != password.ToUpper())
            {
                return Json(new AuthMessage { Message = "密码错误！" });
            }

            ssoUser.IPAddress = Tools.GetIP4Address();
            if (string.IsNullOrEmpty(ssoUser.Avatar))
            {
                if (string.IsNullOrEmpty(Tools.GetCookie("_Avatar")))
                {
                    var avatar = WebUtils.UrlDecode(Tools.GetCookie("_Avatar"));
                   var api= _biz.ApiGet(UserInfoEx.GetUserInfoEx(), "UpdateAvatar", new Dictionary<string, object> { { "Id", ssoUser.Uid }, { "Avatar", avatar} }, true, true);
                    if (api.Success)
                    {
                        ssoUser.Avatar = avatar;
                    }
                }
                
            }
            return Json(RouteUtils.RouteUrl(Tools.GetCookie("_CallBack"), ssoUser, null));
        }
    }
}
