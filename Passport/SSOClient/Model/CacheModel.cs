#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.34011
     * 类 名 称：       CacheModel
     * 机器名称：       JASON_PC
     * 命名空间：       SSO.Model
     * 文 件 名：       CacheModel
     * 创建时间：       2015/05/12 14:05:28
     * 计算机名：       Administrator
     * 作    者：       Jason.Yang(yangxing1002@gmail.com)
     * 说    明： 
     * 修改时间：
     * 修 改 人：
**************************************************************************************/
#endregion

namespace SSOClient.Model
{
    public class CacheLoginModel
    {
        public bool IsLogin { get; set; }
        public SSOLoginInfo Info { get; set; }
    }
}
