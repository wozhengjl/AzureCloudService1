namespace WebRole1.Module
{
    using System;
    using System.IO;
    using System.Security;
    using System.Net;
    using System.Web;
    using System.Web.Script.Serialization;
    using System.Web.Security;
    using System.Security.Principal;

    public class UserInfo
    {
        public string AccessToken { get; set; }

        public string OpenId { get; set; }

        public string UserName { get; set; }
    }

    public class FormsPrincipal<TUserData> : IPrincipal
            where TUserData : class, new()
    {
        private IIdentity m_identity;
        private TUserData m_userData;

        public FormsPrincipal(FormsAuthenticationTicket ticket, TUserData userData)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }
            if (userData == null)
            {
                throw new ArgumentNullException("userData");
            }

            m_identity = new FormsIdentity(ticket);
            m_userData = userData;
        }

        public TUserData UserData
        {
            get { return m_userData; }
        }

        public IIdentity Identity
        {
            get { return m_identity; }
        }

        public bool IsInRole(string role)
        {
            return true;
        }
    }

    public class AuthenticationModule : IHttpModule
    {
        private const string appID = "101221922";

        public void Init(HttpApplication application)
        {
            application.AuthenticateRequest += new EventHandler(Application_AuthenticateRequest);
        }

        public void Dispose()
        { 
        }

        private void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpApplication application = sender as HttpApplication;
            HttpContext context = application.Context;
            HttpResponse response = application.Response;
            HttpRequest request = application.Request;

            if (request.RawUrl.Contains("Redirect.aspx"))
            {
                return;
            }

            HttpCookie tokenCookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (tokenCookie == null || string.IsNullOrEmpty(tokenCookie.Value))
            {
                //?#access_token=D3E1822215AD9D71545165A5F322CEFE&expires_in=7776000
                string token = request["access_token"];
                string expireStr = request["expires_in"];

                if (token == null)
                {
                    string redirectURI = HttpUtility.UrlEncode("http://qqoauth2.cloudapp.net/Redirect.aspx");
                    string path = "https://graph.qq.com/oauth2.0/authorize?";
                    string[] queryParams = { "client_id=" + appID, "redirect_uri=" + redirectURI, "cope=" + "get_user_info,list_album,upload_pic,add_feeds,do_like", "response_type=token" };

                    string query = string.Join("&", queryParams);
                    var url = path + query;
                    response.Write("<script> top.location.href='" + url + "'</script>");
                }
                else
                {
                    try
                    {
                        string openID_url = string.Format("https://graph.qq.com/oauth2.0/me?access_token={0}", token);
                        HttpWebRequest openIDRequest = WebRequest.Create(openID_url) as HttpWebRequest;
                        openIDRequest.Method = "GET";
                        HttpWebResponse openIDresponse = openIDRequest.GetResponse() as HttpWebResponse;
                        Stream stream = openIDresponse.GetResponseStream();
                        StreamReader sr = new StreamReader(stream);
                        string html = sr.ReadToEnd();
                        //"callback( {\"client_id\":\"101216287\",\"openid\":\"05F713879CC79D5F0E3E13393FFD55FE\"} );\n"
                        string queryStr = "\"openid\":";
                        string openID = html.Substring(html.IndexOf(queryStr) + queryStr.Length).Split('\"')[1];
                        string qqName = getUserName(token, appID, openID);

                        signIn(new UserInfo
                        {
                            AccessToken = token,
                            OpenId = openID,
                            UserName = qqName,
                        }, int.Parse(expireStr));
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            else
            {
                UserInfo userData = null;
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(tokenCookie.Value);

                if (ticket != null && string.IsNullOrEmpty(ticket.UserData) == false)
                    userData = (new JavaScriptSerializer()).Deserialize<UserInfo>(ticket.UserData);

                if (ticket != null && userData != null)
                    context.User = new FormsPrincipal<UserInfo>(ticket, userData);
            }
        }

        /*
        {
            "ret": 0,
            "msg": "",
            "is_lost":0,
            "nickname": "郑建磊",
            "gender": "男",
            "province": "北京",
            "city": "",
            "year": "1987",
            "figureurl": "http:\/\/qzapp.qlogo.cn\/qzapp\/101216287\/05F713879CC79D5F0E3E13393FFD55FE\/30",
            "figureurl_1": "http:\/\/qzapp.qlogo.cn\/qzapp\/101216287\/05F713879CC79D5F0E3E13393FFD55FE\/50",
            "figureurl_2": "http:\/\/qzapp.qlogo.cn\/qzapp\/101216287\/05F713879CC79D5F0E3E13393FFD55FE\/100",
            "figureurl_qq_1": "http:\/\/q.qlogo.cn\/qqapp\/101216287\/05F713879CC79D5F0E3E13393FFD55FE\/40",
            "figureurl_qq_2": "http:\/\/q.qlogo.cn\/qqapp\/101216287\/05F713879CC79D5F0E3E13393FFD55FE\/100",
            "is_yellow_vip": "0",
            "vip": "0",
            "yellow_vip_level": "0",
            "level": "0",
            "is_yellow_year_vip": "0"
        }
        */
        private string getUserName(string token, string appID, string openID)
        {
            string getUserInfo_url = string.Format("https://graph.qq.com/user/get_user_info?access_token={0}&oauth_consumer_key={1}&openid={2}", token, appID, openID);
            HttpWebRequest openIDRequest = WebRequest.Create(getUserInfo_url) as HttpWebRequest;
            openIDRequest.Method = "GET";
            HttpWebResponse openIDresponse = openIDRequest.GetResponse() as HttpWebResponse;
            Stream stream = openIDresponse.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string html = sr.ReadToEnd();
            string queryStr = "\"nickname\":";
            string nickName = html.Substring(html.IndexOf(queryStr) + queryStr.Length).Split('\"')[1];
            return nickName;
        }

        private void signIn(UserInfo userData, int expiration)
        {
            var data = (new JavaScriptSerializer()).Serialize(userData);

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1, userData.UserName, DateTime.Now, DateTime.Now.AddDays(1), true, data);

            HttpContext context = HttpContext.Current;
            if (context == null)
                throw new InvalidOperationException();
            context.User = new FormsPrincipal<UserInfo>(ticket, userData);

            string cookieValue = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, cookieValue);
            cookie.HttpOnly = true;
            cookie.Secure = FormsAuthentication.RequireSSL;
            cookie.Domain = FormsAuthentication.CookieDomain;
            cookie.Path = FormsAuthentication.FormsCookiePath;
            if (expiration > 0)
                cookie.Expires = DateTime.Now.AddMinutes(expiration);

            context.Response.Cookies.Remove(cookie.Name);
            context.Response.Cookies.Add(cookie);
        }
    }
}