#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.34011
     * 类 名 称：       IAuthorizeBase
     * 机器名称：       JASON_PC
     * 命名空间：       SSO.interfaces
     * 文 件 名：       IAuthorizeBase
     * 创建时间：       2015/05/05 15:22:02
     * 计算机名：       Administrator
     * 作    者：       Jason.Yang(yangxing1002@gmail.com)
     * 说    明： 
     * 修改时间：
     * 修 改 人：
**************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.interfaces
{
   public interface IAuthorizeBase
    {
       //授权检查
       bool CheckAuthorize();

       

    }
}
