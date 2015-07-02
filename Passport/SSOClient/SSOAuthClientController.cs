using System.Web.Mvc;

using SSO;
using SSO.Util;
using SSOClient.Attributes;
using SSOClient.Model;
using SSOClient.Util;

namespace SSOClient
{
    public class SSOAuthClientController : Controller
    {
     
        /// <summary>
        /// 回调方法
        /// </summary>
        /// <returns></returns>
        [NeedUrlAuthorize(true)]
        public ActionResult CallBack()
        {
            //验证回调地址  
            var verifyPass = Request.Url != null && CJUtils.VerifyResponse(Request.Url.AbsoluteUri, Stands.SIGN_SECRET);
            if (!verifyPass) return Content("签名验证不通过,非法请求！");

            //通过令牌去拿凭证
            var token = Request["token"];
            var web = new WebUtils();
            var cacheInfo = web.DoGet(Stands.AUTH_HOST + "/Authorize/TokenGetCredence/", new CJDictionary { { "projectCode", Stands.PROJECT_CODE }, { "token", token } });
            var cacheLogin = Tools.JsonDeserialize<CacheLoginModel>(cacheInfo);
            if (!cacheLogin.IsLogin)
            {
                return Content("您还没有登录，请重新登录！");
            }
            //将uid、token 写入cookie
            Tools.SetCookie(Stands.UID, cacheLogin.Info.Id);
            Tools.SetCookie(Stands.TOKEN, token);

            //请求之前的url
            var beq = Request[Stands.BEFORE_REQUEST_URL];
         
            if (string.IsNullOrEmpty(beq))
            {
                return Content("缺少返回地址：" + Request.Url.AbsoluteUri);
            }
            //执行自定义回调
            CallBacking(Request.Url.AbsoluteUri, cacheLogin.Info);
            beq = WebUtils.UrlDecode(beq);
          
            return Redirect(beq);
        }


        /// <summary>
        /// 回调函数中执行的用户自定义方法
        /// </summary>
        /// <param name="url"></param>
        /// <param name="loginInfo"></param>
     
        public virtual void CallBacking(string url, SSOLoginInfo loginInfo)
        {

        }
        [NeedUrlAuthorize(true)]
        public ActionResult LoginOutFromSSO()
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
    }
}
