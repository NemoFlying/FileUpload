(function ($) {
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
        form: null

    }

    $.fn.nemoAjaxForm = function (opt) {
        $.extend(defaults, opt);
        //获取表单中 file 标签
        var $inputFiles = defaults.form.find("input[type=file]");
        if ($inputFiles.length != 0) {
            $inputFiles.each(function () {
                //循环处理每个 file 空间 信息
                var $this = $(this);
                var file;               //file 控件选择的文件
                var accept;             //用户配置可上传文件类型    "*" 表示类型无限制
                var maxFileSize;        //用户配置允许上传文件大小  0表示文件大小无限制
                var fileName;           //上传文件系统名称
                var fileExtension;      //上传文件扩展名称
                if ($this.attr("required") != undefined && this.files.length == 0) {
                    alert("请选择要上传的文件!");
                    return false;
                }
                if (this.files.length > 0) {
                    //进行文件判定
                    file = this.files[0];//暂时支持一个文件
                    //上传文件扩展名限制【当用户换一个扩展名还是可以上传Bug】
                    var accept = $this.attr("accept");
                    accept = accept == undefined ? "*" : accept.toLowerCase();
                    fileName = file.name;
                    fileExtension = $.trim(fileName.substring(fileName.lastIndexOf(".")).toLowerCase());
                    if (accept != "*" && accept.indexOf(fileExtension) == -1) {
                        alert("上传文件 【" + file.name + "】 类型不符合要求【" + accept + "】");
                        return false;
                    }
                    //上传问价大小限制
                    var maxFileSize = $this.attr("maxFilesSize");
                    maxFileSize = maxFileSize == undefined ? 0 : eval(maxFileSize);
                    if (maxFileSize != 0 && file.size > maxFileSize) {
                        alert("上传文件 【" + file.name + "】 大小超出限制【 " + (maxFileSize / 1024 / 1024) + "M】 ");
                        return false;
                    }
                }
            })
        }
        var formData = new FormData(defaults.form[0]);
        $.ajax({
            url: defaults.url,
            method: defaults.method,
            data: formData,
            async: defaults.async,
            contentType: defaults.contentType,
            processData: defaults.processData,
            dataType: defaults.dataType,
            success: defaults.success,
            error: defaults.error
        });
    };
})(jQuery);