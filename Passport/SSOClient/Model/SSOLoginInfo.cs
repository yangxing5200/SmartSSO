#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.34011
     * 类 名 称：       SSOLoginInfo
     * 机器名称：       JASON_PC
     * 命名空间：       Benz.Areas.Authorize.Models
     * 文 件 名：       SSOLoginInfo
     * 创建时间：       2015/05/05 17:02:47
     * 计算机名：       Administrator
     * 作    者：       Jason.Yang(yangxing1002@gmail.com)
     * 说    明： 
     * 修改时间：
     * 修 改 人：
**************************************************************************************/
#endregion

using System.Collections.Generic;

namespace SSOClient.Model
{
    public class SSOLoginInfo
    {
        public string Id { get; set; }
        public string Uid { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string OpenId { get; set; }
        public string AppSecret { get; set; }
        public string CallBackUrl { get; set; }
        public string Sign { get; set; }
        public string Format { get; set; }
        public string Token { get; set; }
        public string ProjectCode { get; set; }
        public string Avatar { get; set; }
        public object Status { get; set; }
        public Dictionary<string, object> UserLoginDic()
        {
            return new Dictionary<string, object>
            {
                {"CardNO", UserName},
                {"Password", Password}
            };
        }
        public bool IsRedirect { get; set; }
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public object Field6 { get; set; }
        public object Field7 { get; set; }
        public object Field8 { get; set; }
        public override string ToString()
        {
            string str = string.Format("Id:{0},Uid:{1},UserName:{2},OpenId:{3},ProjectCode:{4}", Id, Uid, UserName,OpenId,ProjectCode);
            return str;
        }
    }
}