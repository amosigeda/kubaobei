function ShowPages(name, back) { 	//初始化属性
    this.name = name;      //对象名称
    this.page = 1;         //当前页数
    this.pageCount = 1;    //总页数
    this.back = back;
}

ShowPages.prototype.checkPages = function () { 	//进行当前页数和总页数的验证
    if (isNaN(parseInt(this.page))) this.page = 1;
    if (isNaN(parseInt(this.pageCount))) this.pageCount = 1;
    if (this.page < 1) this.page = 1;
    if (this.pageCount < 1) this.pageCount = 1;
    if (this.page > this.pageCount) this.page = this.pageCount;
    this.page = parseInt(this.page);
    this.pageCount = parseInt(this.pageCount);
}

ShowPages.prototype.createHtml = function () { //生成html代码
    var strHtml = '', prevPage = this.page - 1, nextPage = this.page + 1;

//    strHtml += '<span class="count">页码:' + this.page + '/' + this.pageCount + '</span>';
//    strHtml += '<span class="number">';
//    if (prevPage < 1) {
    //        strHtml += '<span title="首页">«</span>';
    //        strHtml += '<span title="上一页">‹</span>';
//    }
//    else {
//        strHtml += '<span title="首页"><a href="javascript:' + this.name + '.toPage(1);">«</a></span>';
//        strHtml += '<span title="上一页"><a href="javascript:' + this.name + '.toPage(' + prevPage + ');">‹</a></span>';
//    }
//    for (var i = 1; i <= this.pageCount; i++) {
//        if (i > 0) {
//            if (i == this.page) {
//                strHtml += '<span title="第' + i + '页">[<strong>' + i + '</strong>]</span>';
//            }
//            else {
//                strHtml += '<span title="第' + i + '页"><a href="javascript:' + this.name + '.toPage(' + i + ');">[' + i + ']</a></span>';
//            }
//        }
//    }
//    if (nextPage > this.pageCount) {
//        strHtml += '<span title="下一页">›</span>';
//        strHtml += '<span title="尾页">»</span>';
//    }
//    else {
//        strHtml += '<span title="下一页"><a href="javascript:' + this.name + '.toPage(' + nextPage + ');">›</a></span>';
//        strHtml += '<span title="尾页"><a href="javascript:' + this.name + '.toPage(' + this.pageCount + ');">»</a></span>';
//    }
//    strHtml += '</span><br>';

    strHtml += '<span class="count">页码: ' + this.page + ' / ' + this.pageCount + '</span>';
    strHtml += '<span class="number">';
    if (prevPage < 1) {
        strHtml += '<span title="首页">首页</span>';
        strHtml += '<span title="上一页">上一页</span>';
    } else {
        strHtml += '<span title="首页"><a href="javascript:' + this.name + '.toPage(1);">首页</a></span>';
        strHtml += '<span title="上一页"><a href="javascript:' + this.name + '.toPage(' + prevPage + ');">上一页</a></span>';
    }
    if (this.page != 1) strHtml += '<span title="第 1 页"><a href="javascript:' + this.name + '.toPage(1);">[1]</a></span>';
    if (this.page >= 7) strHtml += '<span>...</span>';
    if (this.pageCount > this.page + 4) {
        var endPage = this.page + 4;
    } else {
        var endPage = this.pageCount;
    }
    for (var i = this.page - 4; i <= endPage; i++) {
        if (i > 0) {
            if (i == this.page) {
                strHtml += '<span title="第 ' + i + ' 页">[<strong>' + i + '</strong>]</span>';
            } else {
                if (i != 1 && i != this.pageCount) {
                    strHtml += '<span title="第 ' + i + ' 页"><a href="javascript:' + this.name + '.toPage(' + i + ');">[' + i + ']</a></span>';
                }
            }
        }
    }
    if (this.page + 5 < this.pageCount) strHtml += '<span>...</span>';
    if (this.page != this.pageCount) strHtml += '<span title="第 ' + this.pageCount + ' 页"><a href="javascript:' + this.name + '.toPage(' + this.pageCount + ');">[' + this.pageCount + ']</a></span>';
    if (nextPage > this.pageCount) {
        strHtml += '<span title="下一页">下一页</span>';
        strHtml += '<span title="尾页">尾页</span>';
    } else {
        strHtml += '<span title="下一页"><a href="javascript:' + this.name + '.toPage(' + nextPage + ');">下一页</a></span>';
        strHtml += '<span title="尾页"><a href="javascript:' + this.name + '.toPage(' + this.pageCount + ');">尾页</a></span>';
    }
    strHtml += '</span><br />';
    return strHtml;
}

ShowPages.prototype.toPage = function (page) {
    var pageindex;
    if (typeof (page) == 'object') {
        pageindex = page.options[page.selectedIndex].value;
    }
    else {
        pageindex = page;
    }
    this.back(pageindex);
}

ShowPages.prototype.getHtml = function () { 	//显示html代码
    this.checkPages();
    return '<div id="pages_' + this.name + '" class="pages">' + this.createHtml() + '</div>';
}

    