#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.34011
     * 类 名 称：       RouteUtils
     * 机器名称：       JASON_PC
     * 命名空间：       SSO.Util
     * 文 件 名：       RouteUtils
     * 创建时间：       2015/05/11 10:52:48
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using ServiceStack.Logging;
using SSO.Cache;
using SSO.Model;
using UApiService;

namespace SSO.Util
{
    public class RouteUtils
    {
        private static ILog _log = LogManager.GetLogger("logSample");

        /// <summary>
        /// url 权限路由
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="userInfo">用户信息</param>
        /// <returns></returns>
        public static AuthMessage RouteUrl(string callBack, SSOUser userInfo)
        {
            return RouteUrl(callBack, userInfo, null);
        }

        /// <summary>
        /// url 权限路由
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="userInfo">用户信息</param>
        /// <param name="dic">其他信息 该信息会通过Url返回到分站的callback</param>
        /// <returns></returns>
        public static AuthMessage RouteUrl(string callBack, SSOUser userInfo, CJDictionary dic)
        {

            CJClient _client = new CJClient();
            return RouteUrl(callBack, _client, userInfo, dic);
        }

        /// <summary>
        /// url 权限路由
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="_biz">数据请求对象</param>
        /// <param name="_client">url编辑对象</param>
        /// <param name="userInfo">用户信息</param>
        /// <param name="dic">其他信息</param>
        /// <returns></returns>
        public static AuthMessage RouteUrl(string callBack, CJClient _client, SSOUser userInfo, CJDictionary dic)
        {
            try
            {
                userInfo.Password = null;
                dic = dic ?? new CJDictionary();
                //尝试从cookie中拿openid
                var openId = Tools.GetCookie(Stands.OpenIdCookie);
                //生成主站令牌 ps:如果有openid 使用openid作为令牌，如果没有openid  使用id
                var token = string.IsNullOrEmpty(openId) ? Guid.NewGuid().ToString().ToUpper() : openId;
                dic[CJClient.TOKEN] = token;


                var list = CacheHelper.Item_Get<List<Filters>>(Stands.FILTERS);
                //存储用户信息
                SaveLoginInfo(userInfo, token);
                try
                {
                    string configMsg;
                    if (list != null && list.Any(x => x.FlagValue == userInfo.FlagValue))
                    {
                        var redirect = list.First(x => x.FlagValue == userInfo.FlagValue);
                        callBack = redirect.Url;
                        configMsg = redirect.Message;
                    }
                    else
                    {
                        callBack = _client.BuildReturnUrl(callBack, dic);
                        configMsg = "登录成功！";
                    }
                    return new AuthMessage
                    {
                        Message = configMsg,
                        Url = callBack,
                        Status = userInfo.FlagValue
                    };

                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                    throw new Exception("请检查xml文件:" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return new AuthMessage
                {
                    IsError = true,
                    Message = ex.Message
                };
            }
        }

        public static void SaveLoginInfo(SSOUser userInfo, string token)
        {
            //token = token.ToUpper();
            //主站存Cookie
            Tools.SetCookie(Stands.UID, userInfo.Uid);
            Tools.SetCookie(Stands.TOKEN, token);
            Tools.SetCookie(Stands.CURRENT_PROJECT_CODE_KEY, userInfo.ProjectCode);

            //Redis 存储 
            try
            {

                var key = userInfo.ProjectCode + "_" + token;
                _log.Info(String.Format(" SET {0} {1} RouteUtils>SaveLoginInfo", key, userInfo));
                CacheHelper.Item_Set(key, userInfo);
                CacheHelper.SortedSet_SetExpire(key, DateTime.Now.AddDays(1)); //one day
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                throw new Exception("缓存服务器启动异常：" + ex.Message);
            }
            /*当前版本的redis 不支持expire key field 接下来可能会更新*/
            //CacheHelper.Hash_Set(userInfo.ProjectCode, token, userInfo);
            //CacheHelper.Hash_SetExpire(userInfo.ProjectCode, DateTime.Now.AddDays(7));

        }




        /// <summary>
        /// url 已经登录的用户 如果状态变更为不可用状态即跳转到指定页面
        /// </summary>
        /// <param name="userInfo">用户信息</param>
        /// <returns></returns>
        public static SSOData LoginRoute(SSOUser userInfo)
        {
            try
            {

                var list = CacheHelper.Item_Get<List<Filters>>(Stands.FILTERS);
                if (list != null && list.Any(x => x.FlagValue == userInfo.FlagValue))
                {
                    var redirect = list.First(x => x.FlagValue == userInfo.FlagValue);
                    return new SSOData { IsLogin = true, IsRedirect = true, Url = redirect.Url, User = userInfo };
                }

                return new SSOData { User = userInfo, IsLogin = true };
            }
            catch (Exception ex)
            {
                return new SSOData
                {
                    ErrorCode = "error",
                    ErrorMessage = ex.Message,
                    Url = "/"
                };
            }
        }

        /// <summary>
        /// 获取Web授权地址
        /// </summary>
        /// <returns></returns>
        public static string GetAuthUrl(string callback)
        {
            string url = Stands.AUTH_HOST + "/Authorize/Index";
            //回调地址
            string callBackUrl = WebUtils.CurrentHost() + "/" + Stands.CallBackController + "/CallBack/?" + Stands.BEFORE_REQUEST_URL + "=" + HttpUtility.UrlEncode(callback);

            CJClient client = new CJClient();
            CJDictionary dic = new CJDictionary { { "projectCode", Stands.PROJECT_CODE }, { "auth_type", (int)Stands.AuthType } };
            DateTime timestamp = DateTime.Now;
            return client.BuildAuthUrl(url, "", Stands.SIGN_SECRET, callBackUrl, dic, timestamp);
        }
    }
}
