<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LogOn2.aspx.cs" Inherits="WebRole1.LogOn2" %>

<!DOCTYPE html>

  <html>
     <head id="Head1" runat="server">
        <title>Client Flow Example</title> 
     </head>
     <body>
        <form id="form" runat="server">
            <script>
                function callback(user) {
                    var userName = document.getElementById('userName');
                    var greetingText = document.createTextNode('Greetings, ' + user.openid + '.');
                    userName.appendChild(greetingText);
                }

                function urlencode(str) {
                    str = (str + '').toString();

                    return encodeURIComponent(str).replace(/!/g, '%21').replace(/'/g, '%27').replace(/\(/g, '%28').
                    replace(/\)/g, '%29').replace(/\*/g, '%2A').replace(/%20/g, '+');
                }

                //应用的APPID，请改为你自己的
                var appID = "101216287";
                //成功授权后的回调地址，请改为你自己的
                //var redirectURI = "www.qq.com";
                var redirectURI = urlencode("http://qqoauth2.cloudapp.net/");

                //window.location.href = "http://localhost:23606/LogOn2.aspx/?#access_token=D3E1822215AD9D71545165A5F322CEFE&expires_in=7776000";

                //构造请求
                if (window.location.hash.length == 0) {
                    var path = 'https://graph.qq.com/oauth2.0/authorize?';
                    var queryParams = ['client_id=' + appID, 'redirect_uri=' + redirectURI, 'scope=' + 'get_user_info,list_album,upload_pic,add_feeds,do_like', 'response_type=token'];
                    //var queryParams = ['client_id=' + appID, 'scope=' + 'get_user_info,list_album,upload_pic,add_feeds,do_like', 'response_type=token'];

                    var query = queryParams.join('&');
                    var url = path + query;
                    window.location.href = url;
                    //window.open(url);
                }
                else {
                    //获取access token
                    var accessToken = window.location.hash.substring(1);
                    //使用Access Token来获取用户的OpenID
                    var path = "https://graph.qq.com/oauth2.0/me?";
                    var queryParams = [accessToken, 'callback=callback'];
                    var query = queryParams.join('&');
                    var url = path + query;
                    var script = document.createElement('script');
                    script.src = url;
                    document.body.appendChild(script);
                }
            </script>
        </form>
     </body>
 </html>