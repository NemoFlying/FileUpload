using Newtonsoft.Json;
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
    public class FormFileUploadInput
    {

        public FormFileUploadInput()
        {
            var files = HttpContext.Current.Request.Files;
            _fileInfo = JsonConvert.DeserializeObject<FormFileInfo>(HttpContext.Current.Request.Form["FileInfo"]);
            if (files != null && files.Count > 0)
            {
                //表示有文件上传
                _fileInfo.FileBlockData = files[0];
                //设置文件保存路径
                _fileInfo.SetFormFileSavePath(SetFileSavePath(files.Keys[0]), SetFileTmpSavePath(files.Keys[0]));
            }
        }

        /// <summary>
        /// 子类实现文件保存路径
        /// </summary>
        /// <param name="Key">和前台File Input Name属性保持一致</param>
        /// <returns></returns>
        public virtual string SetFileSavePath(string Key)
        {
            return Environment.CurrentDirectory + @"\UpLoad\";
        }

        /// <summary>
        /// 子类实现文件临时保存路径
        /// 默认和文件保存路径一致
        /// </summary>
        /// <param name="Key">和前台File Input Name属性保持一致</param>
        /// <returns></returns>
        public virtual string SetFileTmpSavePath(string Key)
        {
            return SetFileSavePath(Key);
        }

        /// <summary>
        /// 处理类型
        /// FILESAVE：继续保存文件
        /// FILEDELETE:文件保存失败，删除所有问题件
        /// FORMSAVE:  文件保存成功，保存表单信息
        /// </summary>
        public string HandleStatus { get; set; }

        ///// <summary>
        ///// 序列化后的文件信息
        ///// </summary>
        //public string FileInfo { get; set; }

        /// <summary>
        /// 文件信息
        /// </summary>
        public FormFileInfo _fileInfo
        {
            get; set;
        }
        
        /// <summary>
        /// 当所有文件上传完成后ajax传入所有保存成功文件信息
        /// </summary>
        public List<FormFileSaveStatus> FileListInfo { get; set; }

        /// <summary>
        /// 保存当前传入的文件块
        /// </summary>
        /// <returns></returns>
        public FormFileSaveStatus SaveFile()
        {
            return _fileInfo.SaveFile();
        }

        /// <summary>
        /// 删除已上传文件&临时数据
        /// </summary>
        public void DelFile()
        {
            foreach(var item in FileListInfo)
            {
                var fileFullPath = Path.Combine(SetFileSavePath(item.Key), item.FileSaveName);
                var tmpFullPath = $@"{SetFileTmpSavePath(item.Key)}{item.UpLoadId}\";
                File.Delete(fileFullPath);
                Directory.Delete(tmpFullPath, true);
            }
        }

    }
    /// <summary>
    /// 文件保存返回信息
    /// </summary>
    public class FormFileSaveStatus
    {
        /// <summary>
        /// 前端 File Name 属性
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 臨時編號
        /// </summary>
        public string UpLoadId { get; set; }

        /// <summary>
        /// 另存为文件名
        /// </summary>
        public string FileSaveName { get; set; }

        /// <summary>
        /// 临时文件夹
        /// </summary>
        public string BlockTempSavePath { get; set; }

        /// <summary>
        /// Web访问地址
        /// </summary>
        public string WebUrl { get; set; }

        /// <summary>
        /// 當前塊數
        /// </summary>
        public int BlockIndex { get; set; }

        /// <summary>
        /// Block保存状态
        /// </summary>
        public bool BlockSaveStatus { get; set; } = false;

        /// <summary>
        /// 文件保存状态
        /// </summary>
        public bool FileSaveStatus { get; set; } = false;

        /// <summary>
        /// 文件保存错误信息
        /// </summary>
        public string FileSaveErrMsg { get; set; }


    }

    /// <summary>
    /// ajax传入后台表单中文件的基本信息
    /// </summary>
    [Serializable]
    public class FormFileInfo
    {
        /// <summary>
        /// 文件保存路径
        /// </summary>
        /// 默认路径为程式运行目录下UpLoad
        private string _fileSavePath { get; set; }

        /// <summary>
        /// block临时保存路径
        /// </summary>
        private string _blockTempSavePath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileSavePath">文件保存路径</param>
        /// <param name="fileTempSavePath">临时文件保存路径</param>
        public void SetFormFileSavePath(string fileSavePath,string fileTempSavePath)
        {
            _blockTempSavePath = fileTempSavePath;
            _fileSavePath = fileSavePath;

        }

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
        public HttpPostedFile FileBlockData { get; set; }

        /// <summary>
        /// 文件名称
        /// 默认为原文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 另存为文件名用户配置
        /// </summary>
        public string FileSaveName { get; set; }

        /// <summary>
        /// 文件实际保存名称
        /// </summary>
        private string _fileSaveName
        {
            get { return FileSaveName == null ? FileName : FileSaveName; }
        }


        /// <summary>
        /// 保存Block
        /// </summary>
        public FormFileSaveStatus SaveFile()
        {
            var output = new FormFileSaveStatus() {
                BlockIndex = BlockIndex
            };
            try
            {
                var blockName = ""; //块的名称 UploadId_BlockIndex
                var blockFullPath = "";//块的完整路径+名称
                var fileFullPath = "" ;//文件保存的完整路径
                var blockTempSavePath = "";
                if(BlockIndex>1&&string.IsNullOrEmpty(UpLoadId))
                {
                    //接受的不是第一块且没有创建唯一标识
                    output.BlockSaveStatus = false;
                    output.FileSaveErrMsg = "当前不是第一块，且没有创建临时编号，请重新上传！";  //////////////////////////ERROR1
                    return output;
                }
                //处理Block
                if (BlockIndex == 1) //第一个Block,创建唯一临时编号
                {
                    if(string.IsNullOrEmpty(UpLoadId))
                    {
                        //有可能第一个包就上传失败，但是UpLoadId已经产生
                        UpLoadId = Guid.NewGuid().ToString().Replace("-", "");
                    }
                    blockTempSavePath = $@"{_blockTempSavePath}{UpLoadId}\";
                    //创建临时数据保存文件夹
                    if (!Directory.Exists(blockTempSavePath))
                    {
                        Directory.CreateDirectory(blockTempSavePath);
                    }
                }
                output.UpLoadId = UpLoadId;
                blockTempSavePath = $@"{_blockTempSavePath}{UpLoadId}\";

                blockName = $"{UpLoadId}_{BlockIndex}.block";                
                blockFullPath = Path.Combine(blockTempSavePath, blockName);
                if (File.Exists(blockFullPath))
                {
                    //已经上传该模块
                    output.BlockSaveStatus = true;
                    return output;
                }
                FileBlockData.SaveAs(blockFullPath);
                output.BlockSaveStatus = true;
                //判断是否最后一个Block【该方法只是适合Block依次传】
                if (BlockTotal == BlockIndex)
                {
                    var tmpBlock = Directory.GetFiles(blockTempSavePath, $"{UpLoadId}*");
                    if (tmpBlock.Count() == BlockTotal)
                    {
                        //创建文件夹
                        if (!Directory.Exists(_fileSavePath))
                        {
                            Directory.CreateDirectory(_fileSavePath);
                        }
                        fileFullPath = Path.Combine(_fileSavePath, _fileSaveName);
                        using (var fs = new FileStream(fileFullPath, FileMode.Create))
                        {
                            for (var i = 1; i <= BlockTotal; i++)
                            {
                                var path = Path.Combine(blockTempSavePath, $"{UpLoadId}_{i}.block");
                                var bytes = File.ReadAllBytes(path);
                                fs.Write(bytes, 0, bytes.Length);
                            }
                        }
                        output.FileSaveStatus = true;
                    }
                    else
                    {
                        //文件上传失败【数据包丢失】
                        output.FileSaveStatus = false;
                        output.FileSaveErrMsg = "数据包丢失!";
                    }
                    output.FileSaveName = _fileSaveName;
                    //清除临时数据
                    Directory.Delete(blockTempSavePath, true);
                }
            }
            catch(Exception ex)
            {
                output.FileSaveErrMsg = ex.ToString();
            }
            return output;
        }

        /// <summary>
        /// 删除文件&临时文件
        /// </summary>
        public void DeleteFileAndTemp()
        {

        }



    }




}