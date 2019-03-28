using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FileUpLoad.Models
{
    /// <summary>
    /// 含有文件上传的表单提交【ajax提交】
    /// 若ajax中包含多个文件，则依次上传文件
    /// </summary>
    public abstract class FormFileUploadInput
    {
        /// <summary>
        /// 上传文件总个数
        /// </summary>
        public int FileTotal { get; set; }

        /// <summary>
        /// 当前处理的文件索引
        /// </summary>
        public int FileIndex { get; set; }

        /// <summary>
        /// 记录当前所上传的文件保存路径
        /// </summary>
        public List<string> filesPath { get; set; } = new List<string>();



    }

    public class FormFileInfo2
    {
        /// <summary>
        /// 臨時編號
        /// </summary>
        public string UpLoadId { get; set; }

        /// <summary>
        /// 文件塊數
        /// </summary>
        public int BlockTotal { get; set; }

        /// <summary>
        /// 當前塊數
        /// </summary>
        public int BlockIndex { get; set; }

        /// <summary>
        /// 单个Block數據
        /// </summary>
        private HttpPostedFile _fileBlockData { get; set; }

        public FormFileInfo2(HttpPostedFile fileData)
        {
            _fileBlockData = fileData;
            FileSaveName = _fileBlockData.FileName;
        }

        /// <summary>
        /// 另存为文件名称
        /// 默认为原文件名
        /// </summary>
        public string FileSaveName { get; set; }

        /// <summary>
        /// 文件保存路径
        /// </summary>
        /// 默认路径为程式运行目录下UpLoad
        public string FileSavePath { get; set; } = Environment.CurrentDirectory + @"\UpLoad\";

        /// <summary>
        /// 临时文件夹
        /// </summary>
        public string BlockTempSavePath { get { return FileSavePath + @"fileTemp\"; } }

        /// <summary>
        /// 保存Block
        /// </summary>
        public void Save()
        {
            var blockName = ""; //块的名称 UploadId_BlockIndex
            var blockFullPath = "";//块的完整路径+名称
            var fileFullPath = Path.Combine(FileSavePath, FileSaveName); ;//文件保存的完整路径
            if (BlockTotal == BlockIndex && BlockIndex == 1)
            {
                //单个文件
                if(!Directory.Exists(FileSavePath))
                {
                    Directory.CreateDirectory(FileSavePath);
                }
                _fileBlockData.SaveAs(fileFullPath);
                return;
            }

            //处理Block
            if(BlockIndex == 1) //第一个Block,创建唯一临时编号
            {
                UpLoadId = Guid.NewGuid().ToString().Replace("-", "");
            }
            blockName = $"{UpLoadId}_{BlockIndex}";
            blockFullPath = Path.Combine(BlockTempSavePath, blockName);
            if(!File.Exists(blockFullPath))
            {
                //已经上传该模块
                return;
            }
            _fileBlockData.SaveAs(blockFullPath);

            //判断是否最后一个Block【该方法只是适合Block依次传】
            if(BlockTotal == BlockIndex)
            {
                var tmpBlock = Directory.GetFiles(BlockTempSavePath, $"{UpLoadId}*");
                if (tmpBlock.Count()==BlockTotal)
                {
                    //创建文件夹
                    if(!Directory.Exists(FileSavePath))
                    {
                        Directory.CreateDirectory(FileSavePath);
                    }
                    using (var fs = new FileStream(fileFullPath, FileMode.Create))
                    {
                        for (var i = 1; i <= BlockTotal; i++)
                        {
                            var path = Path.Combine(BlockTempSavePath, $"{UpLoadId}_{i}.block");
                            var bytes = File.ReadAllBytes(path);
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                else
                {
                    //文件上传失败【数据包丢失】
                    //Directory.Delete(BlockTempSavePath, true);
                }
                //清除临时数据
                foreach(var filePath in tmpBlock)
                {
                    File.Delete(filePath);
                }
            }


        }



    }




}