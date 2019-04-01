using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileUpLoad.Models
{
    public class Test2FormInput : FormFileUploadInput
    {
        public string Param1 { get; set; }

        /// <summary>
        /// 配置文件保存路径
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public override string SetFileSavePath(string Key)
        {
            if (Key == "file1")
            {
                return @"E:\UploadTest\Temp1\"; ;
            }
            else
            {
                return  @"E:\UploadTest\Temp2\";
            }
        }

        public Test2FormInput()
        {  

        }




    }
}