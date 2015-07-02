#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.34011
     * 类 名 称：       AuthMessage
     * 机器名称：       JASON_PC
     * 命名空间：       Benz.Areas.Authorize.Models
     * 文 件 名：       AuthMessage
     * 创建时间：       2015/05/05 17:03:17
     * 计算机名：       Administrator
     * 作    者：       Jason.Yang(yangxing1002@gmail.com)
     * 说    明： 
     * 修改时间：
     * 修 改 人：
**************************************************************************************/
#endregion

namespace SSO.Model
{
    public class AuthMessage
    {
        public bool IsError { get; set; }
        public bool Allow { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public object Data { get; set; }
        public object Status { get; set; }

    }
}