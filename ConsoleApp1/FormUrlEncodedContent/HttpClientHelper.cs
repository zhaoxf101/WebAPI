
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MarketingPlatform.Common
{
    public class HttpClientHelper
    {
        static string _ClientVersion = "V1.0.0.0";
        static string _DeviceType = "Win 7";
        static string _DeviceId;

        static string _LastToken;

        internal static string LastToken
        {
            get { return _LastToken; }
            set { _LastToken = value; }
        }

        static HttpClientHelper()
        {
            //_DeviceId = Util.GetSystemDiskNo();
        }

        public static string PrefixUrl
        {
            get
            {
                //string url = ConfigurationManager.AppSettings["neturl"];
                var url = "";
                return url;
            }
        }

        public static T GetResponse<T>(string url)
                  where T : class, new()
        {
            if (PrefixUrl.StartsWith("https"))
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            }

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(PrefixUrl, uriKind: UriKind.Absolute);
            httpClient.DefaultRequestHeaders.Add("XinYunHui-DeviceId", _DeviceId);
            httpClient.DefaultRequestHeaders.Add("XinYunHui-DeviceType", _DeviceType);
            httpClient.DefaultRequestHeaders.Add("XinYunHui-Version", _ClientVersion);

            if (!string.IsNullOrEmpty(_LastToken))
            {
                //httpClient.DefaultRequestHeaders.Add(HeadersHelper.ToKen, _LastToken);
            }

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = default(T);
            HttpResponseMessage response = null;
            try
            {
                response = httpClient.GetAsync(url).Result;
            }
            catch
            {
                return result;
            }

            if (response.IsSuccessStatusCode)
            {
                IEnumerable<string> values;
                //if (response.Headers.TryGetValues(HeadersHelper.ToKen, out values))
                {
                    //_LastToken = values.First();
                }

                Task<string> t = response.Content.ReadAsStringAsync();
                string s = t.Result;

                //result = JsonConvert.DeserializeObject<T>(s);
            }

            return result;
        }

        //public static JsonFormatResult<T> GetResponse<T>(string url) where T : class, new()
        //{
        //    if (url.StartsWith("https"))
        //    {
        //        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
        //    }

        //    HttpClient httpClient = new HttpClient();
        //    httpClient.BaseAddress = new Uri(PrefixUrl, uriKind: UriKind.Absolute);

        //    httpClient.DefaultRequestHeaders.Add("XinYunHui-DeviceId", _DeviceId);
        //    httpClient.DefaultRequestHeaders.Add("XinYunHui-DeviceType", _DeviceType);
        //    httpClient.DefaultRequestHeaders.Add("XinYunHui-Version", _ClientVersion);

        //    httpClient.DefaultRequestHeaders.Accept.Add(
        //       new MediaTypeWithQualityHeaderValue("application/json"));
        //    HttpResponseMessage response = httpClient.GetAsync(url).Result;

        //    var result = new JsonFormatResult<T>();

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var responseString = response.Content.ReadAsStringAsync().Result;
        //        result.Content = JsonConvert.DeserializeObject<T>(responseString);
        //    }
        //    else
        //    {
        //        result.IsSuccess = false;
        //        result.ErrorMsg = "访问服务器失败！";
        //    }
        //    return result;
        //}

        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData">post数据</param>
        /// <returns></returns>
        //public static string PostResponse(string url, string postData)
        //{
        //    url = PrefixUrl + url;
        //    if (url.StartsWith("https"))
        //        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

        //    HttpContent httpContent = new StringContent(postData);
        //    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //    HttpClient httpClient = new HttpClient();

        //    HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;

        //    if (response.IsSuccessStatusCode)
        //    {
        //        string result = response.Content.ReadAsStringAsync().Result;
        //        return result;
        //    }
        //    return null;
        //}

        /// <summary>
        /// 发起post请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">url</param>
        /// <param name="postData">post数据</param>
        /// <returns></returns>
        public static T PostResponse<T>(string url, IEnumerable<KeyValuePair<string, string>> data)
            where T : class, new()
        {
            if (PrefixUrl.StartsWith("https"))
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            }

            var httpContent = new FormUrlEncodedContent(data);

            //var contentBuilder = new StringBuilder();
            //foreach (var item in data)
            //{
            //    contentBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
            //}

            //if (contentBuilder.Length > 0)
            //{
            //    contentBuilder.Remove(contentBuilder.Length - 1, 1);
            //}

            //var httpContent = new StringContent(contentBuilder.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");

            //var httpContent = new StringContent(contentBuilder.ToString());
            //httpContent.Headers.Remove("Content-Type");
            //httpContent.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(PrefixUrl, uriKind: UriKind.Absolute);

            //httpClient.DefaultRequestHeaders.Add(HeadersHelper.DeviceId, _DeviceId);
            //httpClient.DefaultRequestHeaders.Add(HeadersHelper.DeviceType, _DeviceType);
            //httpClient.DefaultRequestHeaders.Add(HeadersHelper.DeviceType, _ClientVersion);
            
            if (!string.IsNullOrEmpty(_LastToken))
            {
                //httpClient.DefaultRequestHeaders.Add(HeadersHelper.ToKen, _LastToken);
            }

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            var result = default(T);
            HttpResponseMessage response = null;
            try
            {
                response = httpClient.PostAsync(url, httpContent).Result;
            }
            catch
            {
                return result;
            }

            if (response.IsSuccessStatusCode)
            {
                IEnumerable<string> values;
                //if (response.Headers.TryGetValues(HeadersHelper.ToKen, out values))
                {
                    //_LastToken = values.First();
                }

                Task<string> t = response.Content.ReadAsStringAsync();
                string s = t.Result;

                //result = JsonConvert.DeserializeObject<T>(s);
            }

            return result;
        }


        public static T Post<T>(string url, string param, string type = "application/x-www-form-urlencoded")
        {
            url = PrefixUrl + url;

            string json = string.Empty;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            Encoding encoding = Encoding.UTF8;
            //encoding.GetBytes(postData);
            byte[] bs = Encoding.UTF8.GetBytes(param);
            req.Method = "Post";

            req.Headers.Add("XinYunHui-DeviceId", _DeviceId);
            req.Headers.Add("XinYunHui-DeviceType", _DeviceType);
            req.Headers.Add("XinYunHui-Version", _ClientVersion);

            if (!string.IsNullOrEmpty(_LastToken))
            {
                //req.Headers.Add(HeadersHelper.ToKen, _LastToken);
            }

            req.ContentType = type;
            req.ContentLength = bs.Length;
            req.Timeout = 10 * 60 * 1000;

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
                reqStream.Close();
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        json = reader.ReadToEnd();

                        //return JsonConvert.DeserializeObject<T>(json);
                    }
                }
            }
            catch
            {
                
            }

            return default(T);
        }


        /// <summary>
        /// V3接口全部为Xml形式，故有此方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static T PostXmlResponse<T>(string url, string xmlString)
            where T : class, new()
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            HttpContent httpContent = new StringContent(xmlString);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();

            T result = default(T);

            HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;

            if (response.IsSuccessStatusCode)
            {
                Task<string> t = response.Content.ReadAsStringAsync();
                string s = t.Result;

                result = XmlDeserialize<T>(s);
            }
            return result;
        }

        /// <summary>
        /// 反序列化Xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static T XmlDeserialize<T>(string xmlString)
            where T : class, new()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                using (StringReader reader = new StringReader(xmlString))
                {
                    return (T)ser.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("XmlDeserialize发生异常：xmlString:" + xmlString + "异常信息：" + ex.Message);
            }

        }
    }
}
