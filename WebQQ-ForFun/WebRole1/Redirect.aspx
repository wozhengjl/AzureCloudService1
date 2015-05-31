<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Redirect.aspx.cs" Inherits="WebRole1.Redirect" %>

<!DOCTYPE html>

<html> 
   <head> 
     <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
	 <title> QQConnect JSDK - redirectURI </title>
	 <style type="text/css">
		html, body{font-size:14px; line-height:180%;}
	 </style>

   </head> 
   <body> 
	    <div>
		    <h3>数据传输中，请稍后...</h3>
	    </div>
        <script>
            var paramsStr = window.location.hash;
            if (paramsStr)
            {
                paramsStr = paramsStr.replace('#', '');
            }
            //使用Access Token来获取用户的OpenID
            var url = "http://qqoauth2.cloudapp.net/?" + paramsStr;
            window.location.href = url;
        </script>
   </body> 
</html>