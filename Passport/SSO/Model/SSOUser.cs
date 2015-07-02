#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.34011
     * 类 名 称：       SSOUser
     * 机器名称：       JASON_PC
     * 命名空间：       Benz.Areas.Authorize.Models
     * 文 件 名：       SSOUser
     * 创建时间：       2015/05/05 17:02:47
     * 计算机名：       Administrator
     * 作    者：       Jason.Yang(yangxing1002@gmail.com)
     * 说    明： 
     * 修改时间：
     * 修 改 人：
**************************************************************************************/
#endregion

using System;
using System.Collections.Generic;

namespace SSO.Model
{
    public class SSOUser
    {
        public string Uid { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string ProjectCode { get; set; }
        public string IPAddress { get; set; }
        public string FlagValue { get; set; }
        public string Attr1 { get; set; }
        public string Attr2 { get; set; }
        public string Attr3 { get; set; }
        public string Attr4 { get; set; }
        public string Attr5 { get; set; }
        public string Attr6 { get; set; }
        public string Attr7 { get; set; }
        public string Attr8 { get; set; }
        public string Attr9 { get; set; }
        public string Attr10 { get; set; }
        public string CreateTime { get; set; }
        public string UpdateTime { get; set; }
    }
}