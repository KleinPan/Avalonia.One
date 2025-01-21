using One.Base.ExtensionMethods;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace One.Base.Helpers.HttpHelper
{
    /// <summary>主要用于get post请求</summary>
    public class HTTPClientHelper
    {
        private static readonly HttpClient HttpClient;

        static HTTPClientHelper()
        {
            //var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.None, ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true };
            var handler = new SocketsHttpHandler() { AutomaticDecompression = DecompressionMethods.None };

            HttpClient = new HttpClient(handler);
        }

        #region Get

        public static async Task<string> GetAsync(string url, Dictionary<string, string> paramArray = null)
        {
            string result = "";

            var httpclient = HTTPClientHelper.HttpClient;

            if (paramArray != null)
            {
                url = url + "?" + BuildParam(paramArray);
            }

            var response = await httpclient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                Stream myResponseStream = await response.Content.ReadAsStreamAsync();
                //获取流的内容
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                result = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
            }

            return result;
        }

        /// <summary>对原生方法的封装</summary>
        /// <param name="url"></param>
        /// <param name="paramArray"></param>
        /// <returns></returns>
        public static async Task<string> GetStringAsync(string url, Dictionary<string, string> paramArray = null)
        {
            var httpclient = HTTPClientHelper.HttpClient;

            if (paramArray != null)
            {
                url = url + "?" + BuildParam(paramArray);
            }

            var result = await httpclient.GetStringAsync(url);
            return result;
        }

        #endregion Get

        #region Post

        public static async Task<string> HttpPostDictionaryRequestAsync(string Url, Dictionary<string, string> paramArray)
        {
            try
            {
                var postData = BuildParam(paramArray);
                var message = await PostAsync(Url, postData, ContentType: "application/x-www-form-urlencoded");
                message.EnsureSuccessStatusCode();

                return await message.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                return exception.ToString();
            }
        }

        public static async Task<string> HttpPostJsonRequestAsync(string Url, string postData)
        {
            try
            {
                var message = await PostAsync(Url, postData, ContentType: "application/json");

                message.EnsureSuccessStatusCode();

                return await message.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                return exception.ToString();
            }
        }

        /// <summary> 异步POST请求 </summary> <param name="Url">234</param> <param
        /// name="paramArray">324</param> <param
        /// name="ContentType"><para>POST请求的两种编码格式:</para>"application/x-www-urlencoded"是浏览器默认的编码格式,用于键值对参数,参数之间用&（&amp;）用间隔；<para>"multipart/form-data"常用于文件等二进制，也可用于键值对参数，最后连接成一串字符传输(参考Java
        /// OK HTTP)。</para><para>除了这两个编码格式，还有"application/json"也经常使用。</para></param> <returns></returns>
        private static Task<HttpResponseMessage> PostAsync(string Url, string postData, string ContentType = "application/x-www-form-urlencoded")//"application/x-www-form-urlencoded"
        {
            System.Diagnostics.Debug.WriteLine("发送内容:\n" + postData);

            var data = Encoding.ASCII.GetBytes(postData);

            try
            {
                //HttpClient.DefaultRequestHeaders.Add("User-Agent", @"SmartSwitchCabinet.Client");
                //HttpClient.DefaultRequestHeaders.Add("Accept", @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

                HttpResponseMessage message = null;
                using (Stream dataStream = new MemoryStream(data ?? new byte[0]))
                {
                    using (HttpContent content = new StreamContent(dataStream))
                    {
                        content.Headers.Add("Content-Type", ContentType);
                        //content.Headers.Add("Test", ContentType);

                        var task = HttpClient.PostAsync(Url, content);
                        //var task2=  task.ContinueWith(GetTimelyReturnMessages);
                        //  return task2;

                        return task;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // return Task.FromResult<string>("函数执行异常!");
                return Task.FromResult<HttpResponseMessage>(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }

        public static async Task<string> PostKeyValuePair(string url, string method, string content)
        {
            var response = await HttpClient.PostAsync(url + method, new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Par",content),
            }));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();

            //var request = new HttpRequestMessage(HttpMethod.Post, url + method);

            //var collection = new List<KeyValuePair<string, string>>();
            //collection.Add(new("Par", content));

            //request.Content = new FormUrlEncodedContent(collection);
            //var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            //return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> PostJson(string url, string method, string content)
        {
            var response = await HttpClient.PostAsync(url + method, new StringContent(content, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        #endregion Post

        private static string BuildParam(Dictionary<string, string> paramArray, Encoding encode = null)
        {
            string url = "";

            if (encode == null) encode = Encoding.UTF8;

            if (paramArray != null && paramArray.Count > 0)
            {
                var parms = "";
                foreach (var item in paramArray)
                {
                    parms += string.Format("{0}={1}&", item.Key.Encode(encode), item.Value.Encode(encode));
                }
                if (parms != "")
                {
                    parms = parms.TrimEnd('&');
                }
                url += parms;
            }
            return url;
        }

        #region 文件下载

        private static readonly object lockObj = new();

        /// <summary>下载文件</summary>
        /// <param name="url">文件下载地址</param>
        /// <param name="savePath">本地保存路径+名称</param>
        /// <param name="downloadCallBack">下载回调（总长度,已下载,进度）</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task DownloadFileAsync(string url, string savePath, Action<long, long, double>? downloadCallBack = null)
        {
            try
            {
                Console.WriteLine($"文件【{url}】开始下载！");
                HttpResponseMessage? response = null;
                using (HttpClient client = new HttpClient())
                    response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (response == null)
                    throw new Exception("文件获取失败");

                var total = response.Content.Headers.ContentLength ?? 0;
                var stream = await response.Content.ReadAsStreamAsync();
                var file = new FileInfo(savePath);
                using (var fileStream = file.Create())
                using (stream)
                {
                    if (downloadCallBack == null)
                    {
                        await stream.CopyToAsync(fileStream);
                        Console.WriteLine($"文件【{url}】下载完成！");
                    }
                    else
                    {
                        byte[] buffer = new byte[1024];
                        long readLength = 0;
                        int length;
                        while ((length = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            // 写入到文件
                            fileStream.Write(buffer, 0, length);

                            //更新进度
                            readLength += length;
                            double? progress = Math.Round((double)readLength / total * 100, 2, MidpointRounding.ToZero);
                            lock (lockObj)
                            {
                                //下载完毕立刻关闭释放文件流
                                if (total == readLength && progress == 100)
                                {
                                    fileStream.Close();
                                    fileStream.Dispose();
                                }
                                downloadCallBack?.Invoke(total, readLength, progress ?? 0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"下载文件失败：{ex.Message}!");
            }
        }

        #endregion 文件下载

        public static HttpClient CreateSimpleHttpProxyClient(string proxyHost, string proxyPort)
        {
            var proxy = new WebProxy
            {
                Address = new Uri($"http://{proxyHost}:{proxyPort}"),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,

                // *** These creds are given to the proxy server, not the web server ***
                //Credentials = new NetworkCredential(
                //    userName: proxyUserName,
                //    password: proxyPassword)
            };

            // Now create a client handler which uses that proxy
            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy,
            };

            // Omit this part if you don't need to authenticate with the web server:
            if (false)
            {
                httpClientHandler.PreAuthenticate = true;
                httpClientHandler.UseDefaultCredentials = false;

                // *** These creds are given to the web server, not the proxy server ***
                //httpClientHandler.Credentials = new NetworkCredential(
                //    userName: serverUserName,
                //    password: serverPassword);
            }

            // Finally, create the HTTP client object
            return new HttpClient(handler: httpClientHandler, disposeHandler: true);
        }

        public static HttpClient CreateSimpleHttpClient(string userName, string password)
        {
            HttpClient client = new HttpClient();
            // 创建身份认证 using System.Net.Http.Headers;
            AuthenticationHeaderValue authentication = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}")));

            client.DefaultRequestHeaders.Authorization = authentication;

            return client;
        }
    }
}