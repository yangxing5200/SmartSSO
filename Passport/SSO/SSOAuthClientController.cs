using System.Web.Mvc;
using Common.Logging;
using SSO.Attributes;
using SSO.Model;
using SSO.Util;

namespace SSO
{
    public class SSOAuthClientController : Controller
    {
        private ILog _logSSO = LogManager.GetLogger("SSOURL");
        /// <summary>
        /// 回调方法
        /// </summary>
        /// <returns></returns>
        [NeedUrlAuthorize(true)]
        public ActionResult CallBack()
        {
            try
            {
                //验证回调地址  
                var verifyPass = Request.Url != null && CJUtils.VerifyResponse(Request.Url.AbsoluteUri, Stands.SIGN_SECRET);
                if (!verifyPass) return Content("签名验证不通过,非法请求！");

                _logSSO.Info(Request.Url.AbsoluteUri);
                //通过令牌去拿凭证
                var token = Request["token"];
                var web = new WebUtils();
                var cacheInfo = web.DoGet(Stands.AUTH_HOST + "/Authorize/TokenGetCredence/", new CJDictionary { { "projectCode", Stands.PROJECT_CODE }, { "token", token } });
                var cacheLogin = Tools.JsonDeserialize<SSOData>(cacheInfo);
                if (!cacheLogin.IsLogin)
                {
                    return Content("您还没有登录，请重新登录！");
                }
                //将uid、token 写入cookie
                Tools.SetCookie(Stands.UID, cacheLogin.User.Uid);
                Tools.SetCookie(Stands.TOKEN, token);

                //请求之前的url
                var beq = Request[Stands.BEFORE_REQUEST_URL];
                _logSSO.Info("decode:" + beq);
                if (string.IsNullOrEmpty(beq))
                {
                    return Content("缺少返回地址：" + Request.Url.AbsoluteUri);
                }
                //执行自定义回调
                CallBacking(Request.Url.AbsoluteUri, cacheLogin.User);
                beq = WebUtils.UrlDecode(beq);
                _logSSO.Info(beq);
                return Redirect(beq);
            }
            catch (System.Exception ex)
            {
                _logSSO.Error(ex.Message);
                 return Content("服务器繁忙...");
               
            }
        }


        /// <summary>
        /// 回调函数中执行的用户自定义方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="user"></param>
     
        public virtual void CallBacking(string url, SSOUser user)
        {

        }
        [NeedUrlAuthorize(true)]
        public ActionResult LoginOutFromSSO()
        {
            try
            {
                //取出当前token 准备删掉服务器中对应的凭证
                var token = Tools.GetCookie(Stands.TOKEN);
                if (string.IsNullOrEmpty(token))
                {
                    return View();
                }
                var web = new WebUtils();
                var result = web.DoGet(Stands.AUTH_HOST + "/Authorize/ClearToken/", new CJDictionary { { "projectCode", Stands.PROJECT_CODE }, { "token", token } });
                if (result != "success") return Content("注销出现错误");
                Tools.ClearCookie(Stands.UID);
                Tools.ClearCookie(Stands.TOKEN);
                Tools.ClearCookie(Stands.CURRENT_PROJECT_CODE_KEY);
                return View();
            }
            catch (System.Exception ex)
            {
                _logSSO.Error(ex);
               return View();
            }
        }
    }
}
