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
            var files = HttpContext.Current.Request.Files;
            FileBlockData = files[0];//一个From包含一个文件
            //配置保存地址
            if(files.Keys[0]=="file1")
            {
                FileSavePath = @"E:\UploadTest\Temp1\";
            }
            else
            {
                FileSavePath = @"E:\UploadTest\Temp2\";
            }
            
        }
    }
}