#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.18444
     * 类 名 称：       CJUtils
     * 机器名称：       YANG-PC
     * 命名空间：       SSO.Util
     * 文 件 名：       CJUtils
     * 创建时间：       2015/4/17 15:30:54
     * 计算机名：       Yang
     * 作    者：       Jason.Yang(yangxing1002@gmail.com)
     * 说    明： 
     * 修改时间：
     * 修 改 人：
**************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SSO.Util
{
    public class CJUtils
    {
        public const string WX_AUTH_URL = "";
        public const string CJ_AUTH_URL = "";
        /// <summary>
        /// 给请求签名。
        /// </summary>
        /// <param name="parameters">所有字符型的请求参数</param>
        /// <param name="secret">签名密钥</param>
        /// <returns>签名</returns>
        public static string SignRequest(IDictionary<string, string> parameters, string secret)
        {
            return SignRequest(parameters, secret, false);
        }

        /// <summary>
        /// 给请求签名。
        /// </summary>
        /// <param name="parameters">所有字符型的请求参数</param>
        /// <param name="secret">签名密钥</param>
        /// <param name="qhs">是否前后都加密钥进行签名</param>
        /// <returns>签名</returns>
        public static string SignRequest(IDictionary<string, string> parameters, string secret, bool qhs)
        {
            // 第一步：把字典按Key的字母顺序排序
            IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parameters, StringComparer.Ordinal);
            IEnumerator<KeyValuePair<string, string>> dem = sortedParams.GetEnumerator();

            // 第二步：把所有参数名和参数值串在一起
            StringBuilder query = new StringBuilder(secret);
            while (dem.MoveNext())
            {
                string key = dem.Current.Key;
                string value = dem.Current.Value;
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    query.Append(key).Append(value);
                }
            }
            if (qhs)
            {
                query.Append(secret);
            }

            // 第三步：使用MD5加密
            MD5 md5 = MD5.Create();
            byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query.ToString()));

            // 第四步：把二进制转化为大写的十六进制
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                result.Append(bytes[i].ToString("X2"));
            }

            return result.ToString();
        }

        /// <summary>
        /// 验证回调地址的签名是否合法。
        /// </summary>
        /// <param name="callbackUrl">回调地址</param>
        /// <param name="appSecret">应用密钥</param>
        /// <returns>验证成功返回True，否则返回False</returns>
        public static bool VerifyResponse(string callbackUrl, string appSecret)
        {
           
            Uri uri = new Uri(callbackUrl);

            string query = uri.Query;
            if (string.IsNullOrEmpty(query)) // 没有回调参数
            {
                return false;
            }

            query = query.Trim(new char[] { '?', ' ' });
            if (query.Length == 0) // 没有回调参数
            {
                return false;
            }

            IDictionary<string, string> queryDict = SplitUrlQuery(query);

            
            string Params;
            queryDict.TryGetValue("parameters", out Params);
            string Session;
            queryDict.TryGetValue(CJClient.TOKEN, out Session);
            string Sign;
            queryDict.TryGetValue(CJClient.SIGN, out Sign);
            string openId;
            queryDict.TryGetValue(CJClient.OPEN_ID, out openId);

            Sign = (Sign == null ? null : Uri.UnescapeDataString(Sign));
            queryDict.Remove(CJClient.SIGN);
            return Sign == SignRequest(queryDict, appSecret);
            return VerifyResponse(Params, Session, Sign, openId, appSecret);
        }

        /// <summary>
        /// 验证回调地址的签名是否合法。要求所有参数均为已URL反编码的。
        /// </summary>
        /// <param name="Params">私有参数（未经BASE64解密后的）</param>
        /// <param name="Session">私有会话码</param>
        /// <param name="Sign">回调签名（经过URL反编码的）</param>
        /// <param name="openId">应用公钥</param>
        /// <param name="appSecret">应用密钥</param>
        /// <returns>验证成功返回True，否则返回False</returns>
        public static bool VerifyResponse(string Params, string Session, string Sign, string openId, string appSecret)
        {
            StringBuilder result = new StringBuilder();
            result.Append(openId).Append(Params).Append(Session).Append(appSecret);
            byte[] bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(result.ToString()));
            return Convert.ToBase64String(bytes) == Sign;
        }

        private static IDictionary<string, string> SplitUrlQuery(string query)
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
                        result.Add(oneParam[0], WebUtils.UrlDecode(oneParam[1]));
                    }
                }
            }

            return result;
        }
        /// <summary>
        /// 获取文件的真实媒体类型。目前只支持JPG, GIF, PNG, BMP四种图片文件。
        /// </summary>
        /// <param name="fileData">文件字节流</param>
        /// <returns>媒体类型</returns>
        public static string GetMimeType(byte[] fileData)
        {
            string suffix = GetFileSuffix(fileData);
            string mimeType;

            switch (suffix)
            {
                case "JPG": mimeType = "image/jpeg"; break;
                case "GIF": mimeType = "image/gif"; break;
                case "PNG": mimeType = "image/png"; break;
                case "BMP": mimeType = "image/bmp"; break;
                default: mimeType = "application/octet-stream"; break;
            }

            return mimeType;
        }

        /// <summary>
        /// 根据文件后缀名获取文件的媒体类型。
        /// </summary>
        /// <param name="fileName">带后缀的文件名或文件全名</param>
        /// <returns>媒体类型</returns>
        public static string GetMimeType(string fileName)
        {
            string mimeType;
            fileName = fileName.ToLower();

            if (fileName.EndsWith(".bmp", StringComparison.CurrentCulture))
            {
                mimeType = "image/bmp";
            }
            else if (fileName.EndsWith(".gif", StringComparison.CurrentCulture))
            {
                mimeType = "image/gif";
            }
            else if (fileName.EndsWith(".jpg", StringComparison.CurrentCulture) || fileName.EndsWith(".jpeg", StringComparison.CurrentCulture))
            {
                mimeType = "image/jpeg";
            }
            else if (fileName.EndsWith(".png", StringComparison.CurrentCulture))
            {
                mimeType = "image/png";
            }
            else
            {
                mimeType = "application/octet-stream";
            }

            return mimeType;
        }

        /// <summary>
        /// 根据API名称获取响应根节点名称。
        /// </summary>
        /// <param name="api">API名称</param>
        /// <returns></returns>
        public static string GetRootElement(string api)
        {
            int pos = api.IndexOf(".");
            if (pos != -1 && api.Length > pos)
            {
                api = api.Substring(pos + 1).Replace('.', '_');
            }
            return api + "_response";
        }
        /// <summary>
        /// 获取文件的真实后缀名。目前只支持JPG, GIF, PNG, BMP四种图片文件。
        /// </summary>
        /// <param name="fileData">文件字节流</param>
        /// <returns>JPG, GIF, PNG or null</returns>
        public static string GetFileSuffix(byte[] fileData)
        {
            if (fileData == null || fileData.Length < 10)
            {
                return null;
            }

            if (fileData[0] == 'G' && fileData[1] == 'I' && fileData[2] == 'F')
            {
                return "GIF";
            }
            else if (fileData[1] == 'P' && fileData[2] == 'N' && fileData[3] == 'G')
            {
                return "PNG";
            }
            else if (fileData[6] == 'J' && fileData[7] == 'F' && fileData[8] == 'I' && fileData[9] == 'F')
            {
                return "JPG";
            }
            else if (fileData[0] == 'B' && fileData[1] == 'M')
            {
                return "BMP";
            }
            else
            {
                return null;
            }
        }
    }
}
