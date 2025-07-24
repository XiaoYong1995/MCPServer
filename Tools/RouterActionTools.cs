using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ModelContextProtocol.Server;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SampleMcpServer.Tools
{

    internal class RouterActionTools
    {
        // 1. 创建 CookieContainer
        static CookieContainer cookieContainer = new CookieContainer();

        // 2. 创建 HttpClientHandler 并赋值
        static HttpClientHandler handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            // AllowAutoRedirect = false
        };
        static HttpClient client = new HttpClient(handler);
        // [McpServerResource]
        [McpServerTool]
        [Description("使用此方法登录路由器,返回其他接口需要的token")]
        public string login(
           [Description("BaseUrl")] string baseUrl = "192.168.31.1",
           [Description("UserId")] string user = "",
           [Description("Password")] string password = "")
        {
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri("http://" + baseUrl);
            }
            FormUrlEncodedContent formUrlEncoded = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "username",user},
                { "psd",password},
            });
            HttpResponseMessage result = client.PostAsync("/cgi-bin/luci", formUrlEncoded).GetAwaiter().GetResult();
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string htmlConent = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (htmlConent.IndexOf("WiFi设置") > -1)
                {
                    string pattern = @"token:\s*'([^']*)'";

                    // 执行匹配
                    Match match = Regex.Match(htmlConent, pattern);

                    if (match.Success)
                    {
                        string token = match.Groups[1].Value;
                        return token;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        [McpServerTool]
        [Description("使用此方法获取路由器映射列表,需要先使用login登录系统")]
        public async Task<PortMappingConfig?> getMappingList()
        {
            if (cookieContainer.Count == 0)
            {
                throw new Exception("没有登录");
            }
            var result = await client.GetAsync("/cgi-bin/luci/admin/settings/pmDisplay");
            string jsonString = await result.Content.ReadAsStringAsync();
            var ports=JsonConvert.DeserializeObject<PortMappingConfig>(jsonString);
            return ports;
        }
        [McpServerTool]
        [Description("使用此方法设置或删除映射,需要先使用login登录系统")]
        public async Task<string> operationMapping(
            [Description("token,从login方法获取")] string token = "",
            [Description("operation 操作类型,添加则为 add 、删除则为 del")] string op = "add",
            [Description("srvname 映射的名称,这里的名称是getMappingList中的desp字段.")] string srvname = "",
            [Description("client 内网IP 在删除时无需提供")] string client_ip = "",
            [Description("protocol 有TCP、UDP、BOTH,在删除时无需提供")] string protocol = "",
            [Description("in_port 内部端口,在删除时无需提供")] string in_port = "",
            [Description("ex_port 外部端口,在删除时无需提供")] string ex_port = "")
        {
            if (token == "")
            {
                return "您没有登录";
            }
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
               { "token",token},
               { "op",op},
               { "srvname",srvname},
            };

            if (op == "add")
            {
                dic.Add("client", client_ip);
                dic.Add("protocol", protocol);
                dic.Add("inPort", in_port);
                dic.Add("exPort", ex_port);
            }
            string opName = op == "add" ? "设置" : "删除";
            FormUrlEncodedContent formUrlEncoded = new FormUrlEncodedContent(dic);
            var result = await client.PostAsync("/cgi-bin/luci/admin/settings/pmSetSingle", formUrlEncoded);
            string u = await result.Content.ReadAsStringAsync();
            if (u.Trim() == "")
            {
                return "程序接口调用失败";
            }
            try
            {
                dynamic? jsonResult = JsonConvert.DeserializeObject<dynamic>(u);
                if (jsonResult != null)
                {
                    if (jsonResult.retVal == 0)
                    {
                        return opName + "成功";
                    }
                    else
                    {
                        return opName + "失败";
                    }
                }
                return opName + "失败";
            }
            catch
            {
                return "返回结果不正确,请尝试重新登录";
            }
            //return u;
        }





    }
}
