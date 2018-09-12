function ConvertJSONDateToJSDateObject(jsondate) {
    if (jsondate != null) {
        var date = new Date(parseInt(jsondate.replace("/Date(", "").replace(")/", ""), 10));
    }
    return date;
}
function getDate(date) {
    date = ConvertJSONDateToJSDateObject(date);
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var day = date.getDate();
    return year + "-" + month + "-" + day;
}
function getDateTime(date) {
    date = ConvertJSONDateToJSDateObject(date);
    if (date == null || date == undefined)
        return "";
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var day = date.getDate();
    var hh = date.getHours();
    var mm = date.getMinutes();
    var ss = date.getSeconds();
    return year + "/" + pad2(month, 2) + "/" + pad2(day, 2) + " " + pad2(hh, 2) + ":" + pad2(mm, 2) + ":" + pad2(ss, 2);
}
function pad2(num, n) {
    if ((num + "").length >= n) return num;
    return pad2("0" + num, n);
}
function ShowError(data) {
    if (data.Result == 401) {
        LoginError(data.Message);
    }
    else {
        GetDataError(data.Message);
    }
}
function ShowMessage(str) {
    window.top.art.dialog({
        title: '提示',
        content: str,
        icon: 'succeed',
        lock: true,
        ok: true
    });
}
function GetDataError(str) {
    window.top.art.dialog({
        title: '提示',
        content: str,
        icon: 'warning',
        lock: true,
        ok: true
    });
}
function LoginError(str) {
    window.top.art.dialog({
        title: '提示',
        content: str,
        icon: 'warning',
        lock: true,
        ok: function () {
            window.location.href = "/";
        },
        cancel: true
    });
}
function tronmouseenter(obj) {
    obj.style.backgroundColor = '#DFEBF2';
}
function tronmouseleave(obj) {
    obj.style.backgroundColor = '';
}  