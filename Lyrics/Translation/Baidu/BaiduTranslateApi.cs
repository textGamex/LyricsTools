using System;
using System.IO;
using System.Text;
using System.Net;
using System.Web;
using System.Security.Cryptography;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static LyricsTools.Tools.Debug;

namespace Lyrics.Translation.Baidu
{ 
    public partial class BaiduTranslationApi : ITranslation
    {
        private readonly string appId;
        private readonly string secretKey;
        private static readonly Random random = new Random();

        public BaiduTranslationApi(string newAppId, string newSecretKey)
        {
            appId = newAppId ?? throw new ArgumentNullException(nameof(newAppId));
            secretKey = newSecretKey ?? throw new ArgumentNullException(nameof(newSecretKey));
        }

        public bool VerifyAccount(out int errorCode, out string errorMessage)
        {
            string message = GetTransResult("1", "en");
            JObject json = JObject.Parse(message);
            if (json.TryGetValue("error_code", out JToken getErrorCode))
            {
                errorCode = int.Parse(getErrorCode.ToString());
                errorMessage = json.GetValue("error_msg").ToString();
                return false;
            }
            else
            {
                errorCode = 0;
                errorMessage = string.Empty;
                return true;
            }
        }

        public string GetTransResult(string query, string from, string to)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));
            
            string randomNumber = random.Next(100000).ToString();
            string sign = GetSign(query, randomNumber);
            string url = GetUrl(query, from.ToLower(), to.ToLower(), randomNumber, sign);
            return TranslateText(url);
        }

        public string GetTransResult(string query, string to)
        {
            return GetTransResult(query, "auto", to);
        }

        public string[] GetTransResultArray(string[] rawDatas, string targetLanguage)
        {
            return GetTransResultArray(rawDatas, "auto", targetLanguage);
        }

        /// <summary>
        /// 得到翻译后的集合
        /// </summary>
        /// <param name="rawDatas">原始数据</param>
        /// <param name="targetLanguage">目标语言</param>
        /// <param name="from">原始数据的语言</param>
        /// <returns>一个翻译完成的数组</returns>
        /// <exception cref="ArgumentNullException">如果rawDatas为null</exception>
        public string[] GetTransResultArray(string[] rawDatas, string from, string targetLanguage)
        {
            if (rawDatas == null)
                throw new ArgumentNullException(nameof(rawDatas));

            foreach (string rawData in rawDatas)
            {
                Console.WriteLine(rawData);
            }
            ///去除时间标签的歌词集合
            List<string> processedData = new List<string>();

            foreach (string query in rawDatas)
            {
                processedData.Add(LyricsTools.GetLineLyric(query));
            }
            string one = LyricsTools.ProcessingLyrics(processedData.ToArray());
            string rawJson = GetTransResult(one, from, targetLanguage.ToLower());
            Dictionary<string, string> map = JsonTools.GetTranslatedMap(rawJson);
            List<string> rawDataArray = new List<string>(rawDatas);

            //用翻译好的歌词替换原来的歌词
            foreach (string rawLyric in map.Keys)
            {
                for (int index = 0, max = rawDataArray.Count; index < max; ++index)
                {
                    if (rawDataArray[index].Contains(rawLyric))
                    {
                        rawDataArray[index] = LyricsTools.ReplaceLyric(rawDataArray[index], map[rawLyric]);
                    }
                }
            }
            return rawDataArray.ToArray();
        }

        public string GetStandardTranslationLanguageParameters(LanguageCode languageCode)
        {
            switch (languageCode)
            {
                case LanguageCode.Chinese: return "zh";
                case LanguageCode.English: return "en";
                    default: throw new ArgumentException($"{languageCode}未实现");
            }
        }

        private string GetSign(string query, string randomNumber)
        {
            StringBuilder sb = new StringBuilder();

            _ = sb.Append(appId).Append(query).Append(randomNumber).Append(secretKey);
            return EncryptString(sb.ToString());
        }

        private string GetUrl(string query, string from, string to, string randomNumber, string sign)
        {
            StringBuilder url = new StringBuilder("http://api.fanyi.baidu.com/api/trans/vip/translate");
            url.Append("?q=").Append(HttpUtility.UrlEncode(query));
            url.Append("&from=").Append(from);
            url.Append("&to=").Append(to);
            url.Append("&appid=").Append(appId);
            url.Append("&salt=").Append(randomNumber);
            url.Append("&sign=").Append(sign);

            return url.ToString();
        }

        private string TranslateText(string url)
        {
            IsNotNull(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string result =  myStreamReader.ReadToEnd();

            myStreamReader.Close();
            myResponseStream.Close();

            return result;
        }        

        // 计算MD5值
        private static string EncryptString(string str)
        {
            IsNotNull(str);
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
    }
}
