(function ($){
    //默认参数
    var defaults = {
        url: '', //请求地址
        method: 'POST',//请求方式,默认POST
        contentType: false,
        processData: false,//避免对formData序列化处理
        dataType: "json", //返回值类型
        success: function () { },
        error: function () { },
        async: true,
        beforeSend: function () { },
        cache: true,
        complete: function () { },
        form: null,                         //上传的表单jQuery 对象 或者 input File对象
        ProgressHandler: ProgressHandler
    }

    //进度条类
    //可以定制 传入对象为被控制对象
    //该方法须订阅事件['sendChange']事件
    function ProgressHandler(fileHandler) {
        var progress = $("div.progress-total").clone();
        progress.removeClass("progress-total");
        progress.find("h4").text(fileHandler.FileName);
        $("div.progress-group").append(progress);
        progress.find("button.pause").click(function () {
            fileHandler.Pause();
        });
        progress.find("button.play").click(function () {
            fileHandler.Continue();
        });
        //订阅事件
        fileHandler.on('sendChange', function (data) {
            switch (fileHandler.SendStatus) {
                case 0:
                    break;
                case 1:   //文件上传完成
                    break;
                case -1:  //发生异常
                    break;
            }
            progress.find(".progress-bar")
                .css("width", Math.ceil((fileHandler.FinishedSize / fileHandler.TotalSize) * 100) + "%");
        });
    }

    //单个文件上传类
    function FileHandler(opt) {
        this.EventHandlers = {};                                                        //事件容器
        this.InputFileName = opt.inputFileName;                                         // input file name 属性
        this.file = opt.file;                                                           //上传文件
        this.FileName = this.file.name;                                                  // 文件名称
        this.FileSaveName = null;                                                        // 文件保存名称【后台传回】
        this.WebUrl = null;                                                               //文件Web访问地址
        this.TotalSize = this.file.size;                                                 //文件大小
        this.UpLoadId = null;                                                           //上传数据块唯一标示
        this.FinishedSize = 0;                                                          //已经上传文件大小
        //上传状态
        this.PauseStatus = false;                                                       //暂停状态
        this.SendStatus = 0;                                                            // 0表示上传中；-1 表示中断；1上传完成
        this.ErrMsg = null;                                                             // 异常信息
        this.BlockIndex = 1;                                                            //上传块索引
        this.BlockTotal = Math.ceil(this.TotalSize / this.BlockSize);                   //数据块总数

        this.FormFileInfo = {}                                                           //构建上传文件信息
        this.FormFileInfo.UpLoadId = '';
        this.FormFileInfo.BlockTotal = this.BlockTotal;
        this.FormFileInfo.BlockIndex = this.BlockIndex;
        this.FormFileInfo.FileName = this.FileName;
        this.FormFileData = new FormData();                                                  //创建表单
        this.FormFileData.append("FileInfo", JSON.stringify(this.FormFileInfo));
        this.FormFileData.append("HandleStatus", "FILESAVE");                                    //告诉后台文件保存
        this.FormFileData.append(this.InputFileName, null);
    }
    FileHandler.prototype = {
        BlockSize: 1024 * 1024 * 3, //设置每次上传块的大小
        //订阅时间[当前支持 类型：sendChange=>文件传送改变时间]
        on: function (eventType, handler) {
            var self = this;
            if (!(eventType in self.EventHandlers)) {
                self.EventHandlers[eventType] = [];
            }
            self.EventHandlers[eventType].push(handler);
            return self;
        },
        //触发时间【发布事件】
        emit: function (eventType) {
            
            var self = this;
            var handlerArgs = Array.prototype.slice.call(arguments, 1);
            for (var i = 0; i < self.EventHandlers[eventType].length; i++) {
                self.EventHandlers[eventType][i].apply(self, handlerArgs);
            }
            return self;
        },
        //取消订阅事件
        off: function (eventType, handler) {
            var currentEvent = this.EventHandlers[eventType];
            var len = 0;
            if (currentEvent) {
                len = currentEvent.length;
                for (var i = len - 1; i >= 0; i--) {
                    if (currentEvent[i] === handler) {
                        currentEvent.splice(i, 1);
                    }
                }
            }
            return this;
        },

        //暂停发送数据
        Pause: function () {
            var self = this;
            self.PauseStatus = true;
            return self;
        },
        //继续发送数据
        Continue: function () {
            var self = this;
            if (self.PauseStatus) {
                self.PauseStatus = false;
                self.Send();
            }
            return self;
        },
        //发送数据
        Send: function () {
            var self = this;
            if (self.PauseStatus) {
                //暂停
                return false;
            }
            self.SendStatus = 0;  //表示传送中
            var start = (self.BlockIndex - 1) * self.BlockSize;
            var end = Math.min(self.TotalSize, start + self.BlockSize);
            self.FormFileData.set(self.InputFileName, self.file.slice(start, end));
            self.FormFileInfo.UpLoadId = self.UpLoadId;
            self.FormFileInfo.BlockIndex = self.BlockIndex;
            self.FormFileData.set("FileInfo", JSON.stringify(self.FormFileInfo));
            $.ajax({
                url: "../FileUpLoad/ReciveFile4",
                method: 'POST',
                data: self.FormFileData,
                contentType: false,
                processData: false,
                dataType: 'json',
                success: function (reData) {
                    //保存反馈回来信息
                    self.ErrMsg = reData.FileSaveErrMsg;
                    self.FileSaveName = reData.FileSaveName;
                    self.WebUrl = reData.WebUrl;
                    if (reData.BlockSaveStatus) {
                        self.BlockIndex++;
                        if (self.BlockIndex == 2) {
                            self.UpLoadId = reData.UpLoadId;
                        }
                        if (self.BlockIndex <= self.BlockTotal) {
                            self.Send();
                        }
                        else {
                            if (reData.FileSaveStatus) {
                                self.SendStatus = 1;
                                //文件保存成功后操作
                            } else {
                                self.SendStatus = -1;
                                self.ErrMsg = reData.FileSaveErrMsg;
                            }

                        }
                    } else {
                        //后台传来异常
                        self.SendStatus = -1;
                        self.ErrMsg = reData.FileSaveErrMsg;
                    }
                },
                error: function (e) {
                    self.SendStatus = -1;
                    self.ErrMsg = e;
                },
                complete: function () {
                    self.FinishedSize =
                        (self.BlockIndex - 1) * self.BlockSize > self.TotalSize ? self.TotalSize : (self.BlockIndex - 1) * self.BlockSize;
                    //触发事件
                    self.emit("sendChange", self);
                }
            });
        }
    }

    //所有文件管理类
    //包含当前上传的所有文件
    function AllFileHandler() {
        this.EventHandlers = {};    //事件容器
        this.Files = [];            //文件列表
        this.FileName = "总进度";
        this.FinishedSize = 0;
        this.TotalSize = 0;
        this.TotalFileCnt = 0;
        this.FinishedFileCnt = 0;
        this.SendStatus = 0;
    }
    AllFileHandler.prototype = {
        on: function (eventType, handler) {
            var self = this;
            if (!(eventType in self.EventHandlers)) {
                self.EventHandlers[eventType] = [];
            }
            self.EventHandlers[eventType].push(handler);
            return self;
        },
        //触发时间【发布事件】
        emit: function (eventType) {
            var self = this;
            var handlerArgs = Array.prototype.slice.call(arguments, 1);
            for (var i = 0; self.EventHandlers[eventType] && i < self.EventHandlers[eventType].length; i++) {
                self.EventHandlers[eventType][i].apply(self, handlerArgs);
            }
            return self;
        },
        //取消订阅事件
        off: function (eventType, handler) {
            var currentEvent = this.EventHandlers[eventType];
            var len = 0;
            if (currentEvent) {
                len = currentEvent.length;
                for (var i = len - 1; i >= 0; i--) {
                    if (currentEvent[i] === handler) {
                        currentEvent.splice(i, 1);
                    }
                }
            }
            return this;
        },
        //暂停发送数据
        Pause: function () {
            var self = this;
            for (let i = 0; i < self.Files.length; i++) {
                self.Files[i].Pause();
            }
            return self;
        },
        //继续发送数据
        Continue: function () {
            var self = this;
            for (let i = 0; i < self.Files.length; i++) {
                self.Files[i].Continue();
            }
            return self;
        }
    }

    //上传表单数据（包含文件）
    function SendWithFile() {
        var $form = defaults.form;
        var $fileInput = $form.find("input[type=file]");
        if ($fileInput.length <= 0) {
            SendFormNoFile($form);
            return;
        }
        SendFileOnly($fileInput, SendWithOutFile);//单独将文件上传后调用保存表单信息数据
    }

    //上传表单数据不包含文件
    //若表单中包含 file 自动剔除
    function SendWithOutFile(allFiles) {
        //console.log("SendWithOutFile");
        var $form = defaults.form;
        var newForm = new FormData($form[0]);
        //删除表单中 file 元素
        $form.find("input[type=file]").each(function () {
            newForm.delete(this.name);
        });
        //后台根据HandleStatus来判断是保存文件还是保存表单非文件信息
        newForm.append("HandleStatus", "FORMSAVE");
        if (allFiles != null && allFiles.length>0) {
            //添加已经上传文件信息
            let filesInput = [];
            $(allFiles).each(function () {
                let obj = {};
                obj.Key = this.InputFileName;
                obj.UpLoadId = this.UpLoadId;
                obj.FileSaveName = this.FileSaveName;
                obj.WebUrl = this.WebUrl;
                filesInput.push(obj);
            });
            newForm.append("FileListInfoJson", JSON.stringify(filesInput)); //后台反序列化为想要的对象
        }
        //保存表单非文件信息
        $.ajax({
            url: defaults.url,
            method: 'POST',
            data: newForm,
            contentType: false,
            processData: false,
            dataType: 'json',
            success: defaults.success,
            error: defaults.error,
            complete: defaults.complete
        });
    }

    //上传文件单个input file 对应
    function SendFileOnly($fileInput, endSendCallback) {
        //console.log("SendFileOnly");
        $fileInput = $fileInput == undefined ? defaults.form : $fileInput;
        var allFileInfo = new AllFileHandler();
        allFileInfo.FileName = "总进度";
        allFileInfo.FinishedSize = 0;
        allFileInfo.TotalSize = 0;
        allFileInfo.TotalFileCnt = 0;
        allFileInfo.FinishedFileCnt = 0;
        //循环input表单中所有的File文件
        $fileInput.each(function () {
            $(this.files).each(function () {
                allFileInfo.TotalFileCnt += 1;
                allFileInfo.TotalSize += this.size;
            });
        });
        if (allFileInfo.TotalFileCnt > 1) {
            //当文件个数大于1的时候创建总进度条
            defaults.ProgressHandler(allFileInfo);
        }
        //对表单中包含文件依次上传
        $fileInput.each(function () {
            var inputFileName = this.name;
            $(this.files).each(function () {
                var fileHandler = new FileHandler({ file: this, inputFileName: inputFileName });
                defaults.ProgressHandler(fileHandler);                                                //创建一个该文件的控制

                //订阅sendChange事件，获取每个文件上传信息
                fileHandler.on('sendChange', function () {
                    switch (this.SendStatus) {
                        case 0:
                            //传送中
                            allFileInfo.FinishedSize += this.BlockSize;
                            break;
                        case 1:
                            //完成传送
                            allFileInfo.FinishedSize += this.TotalSize % this.BlockSize;
                            allFileInfo.FinishedFileCnt += 1;
                            if (allFileInfo.FinishedFileCnt == allFileInfo.TotalFileCnt) {
                                allFileInfo.SendStatus = this.SendStatus;
                                if (typeof (endSendCallback) == "function") {
                                    console.log("***********");
                                    endSendCallback(allFileInfo.Files);
                                } else {
                                    defaults.success(allFileInfo.Files);
                                }
                                //console.log("上传完成！");
                            }
                            break;
                        case -1:
                            allFileInfo.SendStatus = this.SendStatus;
                            break;
                    }
                    //触发总文件上传进度改变事件
                    allFileInfo.emit("sendChange", allFileInfo);
                });
                allFileInfo.Files.push(fileHandler);
                fileHandler.Send.apply(fileHandler); //一定要带上作用域
            });
        });
    }

    $.nemoajax = function (opt) {
        $.extend(defaults, opt);
        return {
            SendWithFile: SendWithFile,
            SendWithOutFile: SendWithOutFile,
            SendFileOnly: SendFileOnly
        };
    }

})(jQuery)