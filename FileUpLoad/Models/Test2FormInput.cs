using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileUpLoad.Models
{
    public class Test2FormInput : FormFileInfo
    {
        public string Param1 { get; set; }
        public Test2FormInput()
        {
            FileBlockData = HttpContext.Current.Request.Files[0];
            //配置保存地址
            FileSavePath = @"D:\UploadTest\Temp\";
        }
    }
}