#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.18444
     * 类 名 称：       CJClient
     * 机器名称：       YANG-PC
     * 命名空间：       SSO
     * 文 件 名：       CJClient
     * 创建时间：       2015/4/17 15:57:25
     * 计算机名：       Yang
     * 作    者：       Jason.Yang(yangxing1002@gmail.com)
     * 说    明： 
     * 修改时间：
     * 修 改 人：
**************************************************************************************/
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using SSO.Util;

namespace SSO
{
    public class CJClient
    {

        public const string FORMAT = "format";
        public const string METHOD = "method";
        public const string TIMESTAMP = "timestamp";
        public const string VERSION = "v";
        public const string SIGN = "sign";
        public const string PARTNER_ID = "partner_id";
        public const string TOKEN = "token";
        public const string FORMAT_XML = "xml";
        public const string SDK_VERSION = "0000"; // SDK自动生成会替换成真实的版本

        public const string OPEN_ID = "openId";
        public const string CALL_BACK = "callback";
        public const string PROJECT_CODE = "projectCode";
        public const string AVATAR = "avatar";
        public const string AUTH_TYPE = "auth_type";
        //public const string SESSION_LOGININFO_KEY = "__LoginInfo";

        private string serverUrl;
        private string appKey;
        private string appSecret;
        private string format = FORMAT_XML;
        public string BuildAuthUrl(string url, string openId, string secret, string callback, CJDictionary txtParams, DateTime timestamp)
        {
            txtParams.Add(CALL_BACK, callback);
            //   txtParams.Add(VERSION, "1.0");
            txtParams.Add(OPEN_ID, openId);
            // txtParams.Add(FORMAT, format);
            //  txtParams.Add(PARTNER_ID, SDK_VERSION);
            txtParams.Add(TIMESTAMP, timestamp);
            // txtParams.Add(TOKEN, session);
            // txtParams.AddAll(this.systemParameters);

            // 添加签名参数
            txtParams.Add(SIGN, CJUtils.SignRequest(txtParams, secret));
            var webUtils = new WebUtils();
            return webUtils.BuildGetUrl(url, txtParams);
        }
        public string BuildAuthUrl(string url, CJDictionary txtParams, DateTime timestamp)
        {
            var webUtils = new WebUtils();
            return webUtils.BuildGetUrl(url, txtParams);
        }
        public string BuildUrl(string url, IDictionary<string, string> txtParams)
        {
            var webUtils = new WebUtils();
            return webUtils.BuildGetUrl(url, txtParams);
        }
        public string BuildUrl(string url, string secret, IDictionary<string, string> txtParams)
        {
            txtParams.Add(SIGN, CJUtils.SignRequest(txtParams, secret));
            var webUtils = new WebUtils();
            return webUtils.BuildGetUrl(url, txtParams);
        }

        /// <summary>
        /// 生成返回地址
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="token"></param>
        /// <param name="sign"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public string BuildReturnUrl(string callBack, CJDictionary dic)
        {
            dic = dic ?? new CJDictionary();
            callBack = RefactorUrl(callBack, dic);
            return BuildUrl(HttpUtility.UrlDecode(callBack), Stands.SIGN_SECRET, dic);
        }
        /// <summary>
        /// 获取微信授权地址
        /// </summary>
        /// <returns></returns>
        public string GetWeixinAuthUrl(string appId, string state)
        {
            return OAuthApi.GetAuthorizeUrl(appId, WebUtils.CurrentHost() + "/Authorize/UserInfoCallback", state, OAuthScope.snsapi_userinfo);
        }

        //解析并分离回调地址中的参数 ps:签名时会根据所有参数进行编码
        public string RefactorUrl(string callBack, CJDictionary dic)
        {
            if (string.IsNullOrEmpty(callBack)) return string.Empty;
            Uri uri = new Uri(WebUtils.UrlDecode(callBack));

            string query = uri.Query;
            if (!string.IsNullOrEmpty(query)) // 没有回调参数
            {
                query = query.Trim(new char[] { '?', ' ' });
                if (query.Length > 0) // 没有回调参数
                {
                    callBack = uri.AbsoluteUri.Replace(uri.Query, "");
                    dic.AddAll(SplitUrlQuery(query));
                }
            }
            return callBack;
        }

        public IDictionary<string, string> GetParamter(string url)
        {
            Uri uri = new Uri(url);

            string query = uri.Query;
            if (string.IsNullOrEmpty(query)) // 没有回调参数
            {
                return new Dictionary<string, string>();
            }

            query = query.Trim(new[] { '?', ' ' });
            if (query.Length == 0) // 没有回调参数
            {
                return new Dictionary<string, string>();
            }

            return SplitUrlQuery(query);
        }

        private IDictionary<string, string> SplitUrlQuery(string query)
        {
            IDictionary<string, string> result = new Dictionary<string, string>();

            string[] pairs = query.Split(new char[] { '&' });
            if (pairs != null && pairs.Length > 0)
            {
                foreach (string pair in pairs)
                {
                    string[] oneParam = pair.Split(new char[] { '=' }, 2);
                    if (oneParam != null && oneParam.Length == 2)
                    {
                        if (result.ContainsKey(oneParam[0])) throw new Exception(string.Format("地址异常,出现多个相同的参数 ：{0}", oneParam[0]));
                        result.Add(oneParam[0], WebUtils.UrlDecode(oneParam[1]));
                    }
                }
            }

            return result;
        }

    }
}
