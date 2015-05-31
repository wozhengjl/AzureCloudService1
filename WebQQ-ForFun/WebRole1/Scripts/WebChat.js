function PHPArray(Action) {
    this.params = new Object();
    this.length = 1;
    this.params["Action"] = Action;
}

PHPArray.prototype.Add = function (key, value) {
    this.params[key] = value;
    this.length++;
}

PHPArray.prototype.ToJson = function () {
    var tempstr = "{";
    for (var key in this.params) {
        tempstr += key + ":\"" + this.params[key] + "\",";
    }
    tempstr = tempstr.substring(0, tempstr.length - 1);
    tempstr += "}";
    return (new Function("return " + tempstr))();
}

function PostSubmit(params, success) {
    $.post("comet_broadcast.asyn", params, success, "json");
}

$(document).ready(function () {
    //初始化事件
    InitEvent();
    Keepline();
});
//初始化事件
function InitEvent() {
    $("#btnSendMsg").click(function () { SendMsg(); });
}

function Keepline() {
    var array = new PHPArray("Keepline");
    var success = function (data, status) {
        if (data.ResponseStatus == 1) {
            ShowMessage(data.ResponseData, "recive");
        }
        Keepline();
    }
    PostSubmit(array.ToJson(), success);
    //PostSubmit($.toJSON(array.params), success);
}

function SendMsg() {
    var Content = $("#txtSendMsg").val();
    var array = new PHPArray("SendMsg");
    //array.Add("ReciveUserId", Global.FriendInfo.UserId);
    array.Add("Content", Content);
    var success = function (data, status) {
        ShowMessage(data.ResponseData, "send");
    }
    PostSubmit(array.ToJson(), success);
    //PostSubmit($.toJSON(array.params), success);
}

function ShowMessage(message, type) {
    var tabClass;
    if (type == "send") {
        tabClass = "sendmsg";
    } else if (type == "recive") {
        tabClass = "recivemsg";
    }
    var tempstr = "<table class='" + tabClass + "' cellpadding='0' cellspacing='0'>";
    tempstr += "<tr>";
    tempstr += "<td class='user'>" + message.SendUserName + " " + message.SendTime + "</td>";
    tempstr += "</tr>";
    tempstr += "<tr>";
    tempstr += "<td class='msg'>" + message.Content + "</td>";
    tempstr += "</tr>";
    tempstr += "</table>";
    $("#Messages .chat .message").append(tempstr);
}