using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace SSO.Util
{
    public class Stands
    {
        public const string OpenIdCookie = "WX_OPEN_ID";
        public const string AccessToken = "WX_TOKEND";
        public const string SIGN_SECRET = "yangxing1002@gmail.com";
        public static string CURRENT_PROJECT_CODE_KEY = "CurrentProjectCode";
        public static string CALLBACK_CONTROLLER = "SSOAuthClient";
        public static string BEFORE_REQUEST_URL = "before_request_url";

        public static string FILTERS = "__Filter__";

        public static string PROJECT_CODE = ConfigurationManager.AppSettings["PROJECT_CODE"];
        public static string AUTH_HOST = ConfigurationManager.AppSettings["AUTH_HOST"];
        public static string AUTH_TYPE = ConfigurationManager.AppSettings["AUTH_TYPE"];
        public static string UID = ConfigurationManager.AppSettings["USER_COOKIE_ID"];
        public static string TOKEN = ConfigurationManager.AppSettings["TOKEN_COOKIE_ID"];
        public static AuthType AuthType = (AuthType)Enum.Parse(typeof(AuthType), AUTH_TYPE);

        public static string CallBackController
        {
            get
            {
                string callback = ConfigurationManager.AppSettings["CALLBACK_CONTROLLER"];
                if (!string.IsNullOrEmpty(callback)) CALLBACK_CONTROLLER = callback;
                return CALLBACK_CONTROLLER;
            }
        }
    }
    public enum AuthType
    {
        /// <summary>
        /// Web授权
        /// </summary>
        Web = 0,
        /// <summary>
        /// 微信授权
        /// </summary>
        Weixin = 1,
        /// <summary>
        /// 其他类型授权
        /// </summary>
        Other = 999
    }

    public enum SSOError
    {
        
        缺少必要的参数=1001,
        参数签名错误 = 1002,
        缓存服务器启动异常 = 1003,

    }

    public class SSOMessage
    {
        
    }

}
