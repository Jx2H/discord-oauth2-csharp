using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace oauth2 {
    class Json {
        public string access_token {get; set;}
    }

    class end_Json {
        public string id {get; set;}
        public string username {get; set;}
        public string discriminator {get; set;}
        public string email {get; set;}
    }

    // public class testException : SystemException {
    //     public testException(string message): base(message) => Console.WriteLine(message);
    // }

    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            Process.Start("chrome.exe", "https://discord.com/oauth2/authorize?client_id=707719365134516357&redirect_uri=http%3A%2F%2F127.0.0.1%3A56400%2Fcallback&response_type=code&scope=identify%20email%20guilds%20connections");

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://*:56400/");
            listener.Start();
            httpsel(listener);
            // listener.Stop();
        }

        static void httpsel(HttpListener listener) {
            while (true) {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                void bye() {
                    using (StreamWriter a = new StreamWriter(response.OutputStream))
                        a.Write("Bye~");
                }
                string path = request.RawUrl;
                if (path.StartsWith("/callback")) {
                    var query = request.QueryString;
                    if (query.AllKeys.Length != 0 && query["code"] != "") {
                        Console.WriteLine("인증시도: " + query["code"]);
                        user_info(query["code"]);
                        bye();
                    } else bye();
                } else bye();
            }
        }

        static string discord_access(string code) {
            string CLIENT_ID = "";
            string CLIENT_SECRET = "";

            HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create("https://discord.com/api/oauth2/token");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            using (StreamWriter requestStream = new StreamWriter(request.GetRequestStream())) {
                requestStream.Write($"client_id={CLIENT_ID}&client_secret={CLIENT_SECRET}&grant_type=authorization_code&code={code}&redirect_uri={WebUtility.UrlEncode("http://127.0.0.1:56400/callback")}&scope={WebUtility.UrlEncode("identify email guilds connections")}");
            }
            try {
                return (new StreamReader(request.GetResponse().GetResponseStream())).ReadToEnd();
            } catch (WebException e) {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse) response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data)) {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                        return null;
                    }
                }
            }
        }

        static void user_info(string code) {
            string html = discord_access(code);
            if (html == null) {
                Console.WriteLine("올바른 접근이 아닙니다.");
                return;
            }
            Json json = JsonConvert.DeserializeObject<Json>(html);
            HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create("https://discord.com/api/users/@me");
            request.Headers.Add("Authorization", "Bearer " + json.access_token);
            string end = (new StreamReader(request.GetResponse().GetResponseStream())).ReadToEnd();
            end_Json a = JsonConvert.DeserializeObject<end_Json>(end);
            Console.WriteLine("ID: " + a.id);
            Console.WriteLine("NAME: " + a.username + "#" + a.discriminator);
            Console.WriteLine("EMAIL: " + a.email + "\n");
        }
    }
}