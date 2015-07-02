#region
/*************************************************************************************
     * CLR 版本：       4.0.30319.34011
     * 类 名 称：       ProjectCode
     * 机器名称：       JASON_PC
     * 命名空间：       SSO.Model
     * 文 件 名：       ProjectCode
     * 创建时间：       2015/07/02 17:08:48
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

namespace SSO.Model
{
   public class Project
    {
       public int Id { get; set; }
       public string ProjectCode { get; set; }
       public string ProjectName { get; set; }
       public string AppKey { get; set; }
       public string AppSecret { get; set; }
       public string Avatar { get; set; }
       public string Attr1 { get; set; }
       public string Attr2 { get; set; }
       public string Attr3 { get; set; }
       public string Attr4 { get; set; }
       public string Attr5 { get; set; }
       public string Attr6 { get; set; }
       public DateTime? CreateTime { get; set; }
       public DateTime? UpdateTime { get; set; }

    }
}
