using System;
using System.IO;
using System.Text;
using System.Net;
using System.Web;
using System.Security.Cryptography;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static LyricsTools.Tools.Debug;
using UtilityLib;

namespace Lyrics.Translation.Baidu
{ 
    public partial class BaiduTranslationApi : ITranslation
    {
        private readonly string _appId;
        private readonly string _secretKey;
        private static readonly Random _random = new Random();

        public BaiduTranslationApi(string newAppId, string newSecretKey)
        {
            _appId = newAppId.Trim() ?? throw new ArgumentNullException(nameof(newAppId));
            _secretKey = newSecretKey.Trim() ?? throw new ArgumentNullException(nameof(newSecretKey));
        }

        /// <summary>
        /// 检测ID和秘钥是否正确
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        /// <returns>如果正确, 返回true, 否则返回false</returns>
        public bool IsCorrectAccount(out int errorCode, out string errorMessage)
        {
            string message = GetTransResult("1", "en");
            JObject json = JObject.Parse(message);
            if (json.TryGetValue("error_code", out JToken getErrorCode))
            {
                errorCode = int.Parse(getErrorCode.ToString());
                errorMessage = JsonTools.GetErrorMessage(errorCode);
                //errorMessage = json.GetValue("error_msg").ToString();
                return false;
            }
            else
            {
                errorCode = JsonTools.NoError;
                errorMessage = JsonTools.GetErrorMessage(errorCode);
                return true;
            }
        }

        public string GetTransResult(string query, string to)
        {
            return GetTransResult(query, "auto", to);
        }

        public string GetTransResult(string query, string from, string to)
        {
            Console.WriteLine(query);
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));
            
            string randomNumber = _random.Next().ToString();
            string sign = GetSign(query, randomNumber);
            string url = GetUrl(query, from.ToLower(), to.ToLower(), randomNumber, sign);
            return TranslateText(url);
        }        

        public Dictionary<string, string> GetTransResultAndRawDataMap(string query, string from, string to)
        {
            Console.WriteLine(query);
            string rawJson = GetTransResult(query, from, to.ToLower());
            return JsonTools.GetTranslatedMap(rawJson);
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
            
            //去除时间标签的歌词集合
            List<string> processedData = new List<string>(rawDatas.Length);

            foreach (string query in rawDatas)
            {
                processedData.Add(LyricsTool.GetLineLyric(query));
            }
            string rusalt = LyricsTool.ProcessingLyrics(processedData.ToArray());
            processedData.Clear();
            string rawJson = GetTransResult(rusalt, from, targetLanguage.ToLower());
            Dictionary<string, string> translation = JsonTools.GetTranslatedMap(rawJson);

            List<string> rawDataArray = new List<string>(rawDatas);
            //用翻译好的歌词替换原来的歌词
            foreach (string rawLyrics in translation.Keys)
            {
                for (int index = 0, max = rawDataArray.Count; index < max; ++index)
                {
                    if (rawDataArray[index] == rawLyrics)
                    {
                        rawDataArray[index] = LyricsTool.ReplaceLyric(rawDataArray[index], translation[rawLyrics]);
                    }
                }
            }
            return rawDataArray.ToArray();
        }       

        public string GetStandardTranslationLanguageParameters(UnifiedLanguageCode languageCode)
        {
            switch (languageCode)
            {
                case UnifiedLanguageCode.Chinese: return "zh";
                case UnifiedLanguageCode.English: return "en";
                case UnifiedLanguageCode.Japanese: return "jp";
                case UnifiedLanguageCode.German: return "de";
                case UnifiedLanguageCode.TraditionalChinese: return "cht";
                case UnifiedLanguageCode.Russian: return "ru";
                case UnifiedLanguageCode.French: return "fra";
                case UnifiedLanguageCode.Spanish: return "spa";
                default: throw new ArgumentException($"{languageCode}未实现");
            }
        }

        private string GetSign(string query, string randomNumber)
        {
            StringBuilder sb = new StringBuilder();

            _ = sb.Append(_appId).Append(query).Append(randomNumber).Append(_secretKey);
            return EncryptString(sb.ToString());
        }

        private string GetUrl(string query, string from, string to, string randomNumber, string sign)
        {
            const string url = "http://api.fanyi.baidu.com/api/trans/vip/translate";
            var map = new Dictionary<string, string>()
            {
                ["q"] = HttpUtility.UrlEncode(query),
                ["from"] = from,
                ["to"] = to,
                ["appid"] = _appId,
                ["salt"] = randomNumber,
                ["sign"] = sign,
            };

            return url.GetCompleteUrl(map);
        }

        private string TranslateText(string url)
        {
            IsNotNull(url);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "text/html;charset=UTF-8";
            request.Timeout = 6000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string result;
            using (Stream myResponseStream = response.GetResponseStream())
            {
                using (StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8")))
                {
                    result = myStreamReader.ReadToEnd();
                }                
            }                      

            return result;
        }

        ///<summary>
        ///计算MD5值
        ///</summary> 
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
