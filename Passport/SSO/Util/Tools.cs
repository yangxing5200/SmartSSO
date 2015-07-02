#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.34011
     * 类 名 称：       Tools
     * 机器名称：       JASON_PC
     * 命名空间：       Benz.Common
     * 文 件 名：       Tools
     * 创建时间：       2015/04/28 11:42:03
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
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SSO.Util
{
    public class Tools
    {
        #region Cookie

        public static void ClearCookie(string key)
        {
            if (HttpContext.Current.Request.Cookies[key] != null)
            {
                HttpCookie mycookie = HttpContext.Current.Request.Cookies[key];
                TimeSpan ts = new TimeSpan(0, 0, 0, 0); //时间跨度 
                mycookie.Expires = DateTime.Now.Add(ts); //立即过期 
                mycookie.Value = null;
                HttpContext.Current.Response.Cookies.Remove(key); //清除 
                HttpContext.Current.Response.Cookies.Add(mycookie); //写入立即过期的*/
            }
        }
        /// <summary>
        /// 检查cookie是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CheckCookie(string key)
        {
            var cookie = HttpContext.Current.Request.Cookies[key];
            if (cookie == null) return false;
            if (cookie.Value == null) return false;
            if (string.IsNullOrEmpty(cookie.Value)) return false;
            return true;
        }
        /// <summary>
        /// 设置会话cookie
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetCookie(string key, string value)
        {
            var tokenCookie = new HttpCookie(key) { Value = value };
            HttpContext.Current.Response.SetCookie(tokenCookie);
        }
        /// <summary>
        /// 设置cookie 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expires"></param>
        public static void SetCookie(string key, string value, DateTime expires)
        {
            var httpCookie = HttpContext.Current.Response.Cookies[key];
            if (httpCookie != null)
            {
                httpCookie.Value = value;
                httpCookie.Expires = expires;
            }
            else
            {
                httpCookie = new HttpCookie(key)
                {
                    Value = value,
                    Expires = expires
                };
                HttpContext.Current.Response.SetCookie(httpCookie);
            }
        }
        /// <summary>
        /// 获取cookie
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetCookie(string key)
        {
            if (CheckCookie(key))
            {
                var cookie = HttpContext.Current.Request.Cookies[key];
                return cookie.Value;
            }
            return string.Empty;
        }
        #endregion

        public static string GetIP4Address()
        {
            string IP4Address = String.Empty;


            foreach (IPAddress IPA in Dns.GetHostAddresses(HttpContext.Current.Request.UserHostAddress))
            {
                if (IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    IP4Address = IPA.ToString();
                    break;
                }
            }

            if (IP4Address != String.Empty)
            {
                return IP4Address;
            }

            foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    IP4Address = IPA.ToString();
                    break;
                }
            }

            return IP4Address;
        }

        /// <summary>
        ///
        /// 将对象属性转换为key-value对
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static Dictionary<String, Object> ToMap<T>(T o) where T : class
        {
            Dictionary<String, Object> map = new Dictionary<string, object>();

            Type t = o.GetType();

            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo p in pi)
            {
                MethodInfo mi = p.GetGetMethod();

                if (mi != null && mi.IsPublic)
                {
                    map.Add(p.Name, mi.Invoke(o, new Object[] { }));
                }
            }

            return map;
        }
        /// <summary>
        /// JSON反序列化
        /// </summary>
        public static T JsonDeserialize<T>(string jsonString)
        {
            //将"yyyy-MM-dd HH:mm:ss"格式的字符串转为"\/Date(1294499956278+0800)\/"格式
            string p = @"\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}";
            MatchEvaluator matchEvaluator = new MatchEvaluator(ConvertDateStringToJsonDate);
            Regex reg = new Regex(p);
            jsonString = reg.Replace(jsonString, matchEvaluator);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(ms);
            return obj;
        }
        /// <summary>
        /// 将时间字符串转为Json时间
        /// </summary>
        private static string ConvertDateStringToJsonDate(Match m)
        {
            string result = string.Empty;
            DateTime dt = DateTime.Parse(m.Groups[0].Value);
            dt = dt.ToUniversalTime();
            TimeSpan ts = dt - DateTime.Parse("1970-01-01");
            result = string.Format("\\/Date({0}+0800)\\/", ts.TotalMilliseconds);
            return result;
        }
        /// <summary>
        /// JSON序列化
        /// </summary>
        public static string JsonSerializer<T>(T t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            //替换Json的Date字符串
            string p = @"\\/Date\((\d+)\+\d+\)\\/";
            MatchEvaluator matchEvaluator = new MatchEvaluator(ConvertJsonDateToDateString);
            Regex reg = new Regex(p);
            jsonString = reg.Replace(jsonString, matchEvaluator);
            return jsonString;
        }
        /// <summary>
        /// 将Json序列化的时间由/Date(1294499956278+0800)转为字符串
        /// </summary>
        private static string ConvertJsonDateToDateString(Match m)
        {
            string result = string.Empty;
            DateTime dt = new DateTime(1970, 1, 1);
            dt = dt.AddMilliseconds(long.Parse(m.Groups[1].Value));
            dt = dt.ToLocalTime();
            result = dt.ToString("yyyy-MM-dd HH:mm:ss");
            return result;
        }
    }
}