using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using WebRole1.Module;

namespace WebRole1.Chat
{
    public class MessageInfo
    {
        public MessageInfo()
        {
            SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 发送账号
        /// </summary>
        public string SendUserId { get; set; }

        public string SendUserName { get; set; }
        /// <summary>
        /// 接受账号
        /// </summary>
        public string ReciveUserId { get; set; }

        public string ReciveUserName { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public object Content { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public string SendTime { get; set; }
        /// <summary>
        /// 接收时间
        /// </summary>
        public string ReciveTime { get; set; }
    }

    public class ResponseResult
    {
        public object ResponseData { get; set; }
        public string ResponseDetails { get; set; }
        public int ResponseStatus { get; set; }
        public string ResultString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class ServerPushHandler
    {
        #region 全局变量
        HttpContext m_Context;
        //推送结果
        ServerPushResult _IAsyncResult;

        FormsPrincipal<UserInfo> m_CurrentUser;
        //声明一个集合
        static Dictionary<string, ServerPushResult> dict = new Dictionary<string, ServerPushResult>();
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造方法
        /// </summary>
        public ServerPushHandler(HttpContext context, ServerPushResult _IAsyncResult)
        {
            this.m_Context = context;
            this._IAsyncResult = _IAsyncResult;
            m_CurrentUser = (FormsPrincipal<UserInfo>)m_Context.User;
        }
        #endregion

        #region 执行操作
        /// <summary>
        /// 根据Action判断执行方法
        /// </summary>
        /// <returns></returns>
        public ServerPushResult ExecAction()
        {
            switch (m_Context.Request["Action"])
            {
                case "Keepline":
                    Keepline();
                    break;
                case "SendMsg":
                    //SendMsg();
                    BroadCastMsg();
                    break;
                default:
                    break;
            }
            return _IAsyncResult;
        }
        #endregion

        #region 保持联接
        private void Keepline()
        {
            if (!dict.ContainsKey(m_CurrentUser.UserData.OpenId))
                dict.Add(m_CurrentUser.UserData.OpenId, _IAsyncResult);
            else //登录时虽然保存了当前用户的连接，但是登录完后异步向客户端推送了数据，此时这个客户端连接已经失效，那么在connect时相当于才是此客户端与服务器端真正的连接，需要重新更新ServerPushResult的值
                dict[m_CurrentUser.UserData.OpenId] = _IAsyncResult;
        }
        #endregion


        private string GetResponseData(MessageInfo message)
        {
            ResponseResult responseResult = new ResponseResult();
            responseResult.ResponseData = message;
            responseResult.ResponseDetails = "消息发送成功！";
            responseResult.ResponseStatus = 1;
            return responseResult.ResultString();
        }

        #region 发送消息
        private void SendMsg()
        {
            MessageInfo message = new MessageInfo()
            {
                SendUserId = m_CurrentUser.UserData.OpenId,
                SendUserName = m_CurrentUser.UserData.UserName,
                ReciveUserId = m_Context.Request["ReciveUserId"],
                ReciveUserName = m_Context.Request["ReciveUserName"],
                Content = m_Context.Request["Content"]
            };

            string result = GetResponseData(message);

            if (dict.ContainsKey(message.ReciveUserId))
            {
                dict[message.ReciveUserId].Result = result;
                dict[message.ReciveUserId].Send();
            }
            _IAsyncResult.Result = result;
            _IAsyncResult.Send();
        }
        #endregion

        #region 全站广播
        public void BroadCastMsg()
        {
            MessageInfo message = new MessageInfo()
            {
                SendUserId = m_CurrentUser.UserData.OpenId,
                SendUserName = m_CurrentUser.UserData.UserName,
                Content = m_Context.Request["Content"]
            };

            string result = GetResponseData(message);

            foreach (string key in dict.Keys)
            {
                if (!string.Equals(m_CurrentUser.UserData.OpenId, key, StringComparison.OrdinalIgnoreCase))
                {
                    ServerPushResult IAsyncResult = dict[key];
                    IAsyncResult.Result = result;
                    IAsyncResult.Send();
                }
            }

            _IAsyncResult.Result = result;
            _IAsyncResult.Send();
        }
        #endregion
    }
}