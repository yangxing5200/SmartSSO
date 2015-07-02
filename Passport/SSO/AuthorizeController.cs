#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.34011
     * 类 名 称：       AuthorizeController
     * 机器名称：       JASON_PC
     * 命名空间：       SSO
     * 文 件 名：       AuthorizeController
     * 创建时间：       2015/05/10 15:10:00
     * 计算机名：       Administrator
     * 作    者：       Jason.Yang(yangxing1002@gmail.com)
     * 说    明： 
     * 修改时间：
     * 修 改 人：
**************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using Common.Logging;

using Senparc.Weixin;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using SSO.Attributes;
using SSO.Cache;
using SSO.Model;
using SSO.Util;
using UApiService;

namespace SSO
{
    public class AuthorizeController : Controller
    {
        private readonly string _appId;
        private readonly string _secret;
        private readonly CJClient _client;
        private ILog _log = LogManager.GetLogger("logSample");

        private string loginPage = "/SSOAuthorize/Account/Index";

        public AuthorizeController()
        {
          
            _client = _client ?? (_client = new CJClient());
            _appId = ConfigurationManager.AppSettings["AppId"];
            _secret = ConfigurationManager.AppSettings["AppSecret"];

        }

        [NeedUrlAuthorize(NotCheck = true)]
        public ActionResult Index()
        {
            try
            {
                #region 检查参数有效性

                if (!CJUtils.VerifyResponse(Request.Url.AbsoluteUri, Stands.SIGN_SECRET))
                {
                    return Json("非法访问，错误的签名。", JsonRequestBehavior.AllowGet);
                }
                var projectCode = Request[CJClient.PROJECT_CODE];

                //授权类型 如果是其他则自动识别当前环境
                var strAuthType = Request[CJClient.AUTH_TYPE];
                int authType = int.Parse(strAuthType);
                if (authType == 999)
                {
                    if (Request.UserAgent != null &&
                        Request.UserAgent.IndexOf("MicroMessenger", StringComparison.Ordinal) > 0)
                    {
                        authType = 1;
                    }
                }
                var callBack = Request[CJClient.CALL_BACK];
                if (string.IsNullOrEmpty(projectCode) || string.IsNullOrEmpty(callBack))
                {
                    return Content("非法访问，错误的参数。");
                }
                #endregion

                var token = Tools.GetCookie(Stands.TOKEN);
                //检查是否登录 （token 是否为空）
                #region 如果已经登录
                if (!string.IsNullOrEmpty(token))
                {
                    var key = projectCode + "_" + token;
                    var model = CacheHelper.Item_Get<SSOUser>(key);

                    if (model != null)
                    {
                        var url = RouteUtils.RouteUrl(callBack,  _client, model, null).Url;
                        //没有回调地址则删除redis信息重新登录
                        if (url == null)
                        {
                            _log.Warn("Url is null model:" + model);
                            //删除redis中的信息
                            CacheHelper.Item_Remove(key);
                            return Content("服务器繁忙请重试！");
                        }
                        return Redirect(url);
                    }
                }
                #endregion


                //微信授权访问
                #region 微信授权访问
                var openId = Request[CJClient.OPEN_ID];
                //openId 为了减少请求微信api次数 第一次获取后就存放到用户cookie中 存放时间为 1 年 
                if (string.IsNullOrEmpty(openId))
                {
                    openId = Tools.GetCookie(Stands.OpenIdCookie);
                }

                //需要微信授权登录（1，登录方式auth_type=1 2，openid 为空）
                if (authType == 1 && string.IsNullOrEmpty(openId))
                {
                    Tools.SetCookie(Stands.CURRENT_PROJECT_CODE_KEY, projectCode);
                    Tools.SetCookie(projectCode + "_CallBack", callBack);
                    //微信授权
                    return Redirect(_client.GetWeixinAuthUrl(_appId, "cj_jason_sso"));
                }

                if (!string.IsNullOrEmpty(openId))
                {
                    var key = projectCode + "_" + openId.ToUpper();
                    var model = CacheHelper.Item_Get<SSOUser>(key);

                    if (model == null)
                    {
                        var dics = new CJDictionary { 
                        { CJClient.PROJECT_CODE, projectCode }, 
                        { CJClient.OPEN_ID,openId.ToUpper()},
                        { CJClient.CALL_BACK, callBack}};

                        return Redirect(_client.BuildUrl(Stands.AUTH_HOST + loginPage, Stands.SIGN_SECRET, dics));
                    }

                    var authMessage = RouteUtils.RouteUrl(callBack,  _client, model, null);
                    return Redirect(authMessage.Url);
                }
                #endregion

                //拿到传递的参数转向到登录页面，此处没有对参数进行再次签名
                var dic = _client.GetParamter(Request.Url.AbsoluteUri);
                return Redirect(_client.BuildUrl(Stands.AUTH_HOST + loginPage, dic));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Content("服务器繁忙...");
            }
        }

        /// <summary>
        /// 微信回调函数
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [NeedUrlAuthorize(NotCheck = true)]
        public ActionResult UserInfoCallback(string code, string state)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return Content("您拒绝了授权！");
                }

                if (state == null || state != "cj_jason_sso")
                {
                    return Content("验证失败！请从正规途径进入！");
                }
                OAuthAccessTokenResult result = null;

                //通过，用code换取access_token
                try
                {
                    result = OAuthApi.GetAccessToken(_appId, _secret, code);
                    Tools.SetCookie(Stands.OpenIdCookie, result.openid, DateTime.Now.AddYears(1));
                    Tools.SetCookie(Stands.AccessToken, result.access_token, DateTime.Now.AddYears(1));
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    return Content("服务器繁忙");
                }
                if (result.errcode != ReturnCode.请求成功)
                {
                    _log.Error(result.errmsg);
                    return Content("错误：" + result.errmsg);
                }
                try
                {
                    //callback&projectcode

                    ////因为第一步选择的是OAuthScope.snsapi_userinfo，这里可以进一步获取用户详细信息
                    OAuthUserInfo userInfo = OAuthApi.GetUserInfo(result.access_token, result.openid);
                    SSOUser info = new SSOUser { Avatar = userInfo.headimgurl };
                    var projectCode = Tools.GetCookie(Stands.CURRENT_PROJECT_CODE_KEY);
                    var key = projectCode + "_" + userInfo.openid.ToUpper();

                    var model = CacheHelper.Item_Get<SSOUser>(key);
                    // Tools.Log(String.Format(" GET {0} {1} AuthorizeController>UserInfoCallback", key, model));
                    if (model == null)
                    {
                        var dic = new CJDictionary { 
                    { CJClient.PROJECT_CODE, projectCode }, 
                    { CJClient.AUTH_TYPE, (int)AuthType.Weixin }, 
                    { CJClient.AVATAR, info.Avatar },
                    { CJClient.OPEN_ID,userInfo.openid}};

                        //把参数存到session
                        return Redirect(_client.BuildUrl(Stands.AUTH_HOST + loginPage, Stands.SIGN_SECRET, dic));
                    }
                    var callBack = Tools.GetCookie(projectCode + "_CallBack");
                    var authMessage = RouteUtils.RouteUrl(callBack,  _client, model, null);
                    return Redirect(authMessage.Url);
                }
                catch (ErrorJsonResultException ex)
                {
                    _log.Error(ex);
                    return Content(ex.Message);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Content("服务器繁忙。。。");
            }
        }

        /// <summary>
        /// 根据令牌获取用户凭证
        /// </summary>
        /// <param name="token"></param>
        /// <param name="projectCode"></param>
        /// <returns></returns>
        [HttpGet]
        [NeedUrlAuthorize(true)]
        public object TokenGetCredence(string projectCode, string token)
        {
            try
            {
                token = token.ToUpper();
                var key = projectCode + "_" + token;
                var model = CacheHelper.Item_Get<SSOUser>(key);
                SSOData cache = new SSOData();

                if (model != null)
                {
                    cache.IsLogin = true;
                    //检查权限
                    cache= RouteUtils.LoginRoute(model);
                }
                else
                {
                    cache.User = new SSOUser();
                }
                return Tools.JsonSerializer(cache);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return Tools.JsonSerializer(new SSOData());
            }
        }

        [HttpGet]
        [NeedUrlAuthorize(true)]
        public object TokenUpdate(string projectCode, string token, string status)
        {
            token = token.ToUpper();
            var key = projectCode + "_" + token;
            var model = CacheHelper.Item_Get<SSOUser>(key);

            // Tools.Log(String.Format(" GET {0} {1} AuthorizeController>TokenUpdate", key, model));

            if (model != null)
            {
                model.FlagValue = status;
                // Tools.Log(String.Format(" UPDATE {0} {1} AuthorizeController>TokenUpdate", key, model));
                return CacheHelper.Item_Set(key, model);
            }
            return string.Empty;
        }



        /// <summary>
        /// 清除令牌
        /// </summary>
        /// <param name="projectCode"></param>
        /// <param name="token"></param>
        [HttpGet]
        [NeedUrlAuthorize(true)]
        public object ClearToken(string projectCode, string token)
        {
            try
            {
                token = token.ToUpper();
                var key = projectCode + "_" + token;
                Tools.ClearCookie(Stands.UID);
                Tools.ClearCookie(Stands.TOKEN);
                Tools.ClearCookie(Stands.PROJECT_CODE);
                CacheHelper.Item_Remove(key);
                return "success";
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            return "error";
        }

    }
}
