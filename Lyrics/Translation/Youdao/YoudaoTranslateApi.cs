using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.IO;

namespace Lyrics.Translation.Youdao
{
    public partial class YoudaoTranslateApi : ITranslation
    {
        private readonly string appId;
        private readonly string secretKey;

        public YoudaoTranslateApi(string newAppId, string newSecretKey)
        {
            appId = newAppId.Trim() ?? throw new ArgumentNullException(nameof(newAppId));
            secretKey = newSecretKey.Trim() ?? throw new ArgumentNullException(nameof(newSecretKey));
        }

        public bool IsCorrectAccount(out int errorCode, out string errorMessage)
        {
            var json = JObject.Parse(GetReturnResult("一", "auto", "en"));
            var stateCode = json.GetValue("errorCode").ToString();
            errorCode = int.Parse(stateCode);
            errorMessage = JsonTools.GetErrorMessage(errorCode);
           if (errorCode == JsonTools.NoError)
                return true;
           else
                return false;
        }

        public Dictionary<string, string> GetTransResultAndRawDataMap(string query, string from, string to)
        {
            var json = GetReturnResult(query, from, to);
            return JsonTools.JsonToTranslatedMap(json);
        }

        public string GetStandardTranslationLanguageParameters(UnifiedLanguageCode standardLanguageParameters)
        {
            switch (standardLanguageParameters)
            {
                case UnifiedLanguageCode.English: return "en";
                case UnifiedLanguageCode.Chinese: return "zh-CHS";
                case UnifiedLanguageCode.TraditionalChinese: return "zh-CHS";
                case UnifiedLanguageCode.Japanese: return "ja";
                case UnifiedLanguageCode.German: return "de";
                case UnifiedLanguageCode.French: return "fr";
                case UnifiedLanguageCode.Russian: return "ru";
                case UnifiedLanguageCode.Spanish: return "es";
                default: throw new ArgumentException();
            }
        }        
        
        private Dictionary<string, string> GetUrlMap(string query, string from, string to)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (from == null)
                throw new ArgumentNullException(nameof(from));
            if (to == null)
                throw new ArgumentNullException(nameof(to));

            Dictionary<string, string> dic = new Dictionary<string, string>();
            string salt = DateTime.Now.Millisecond.ToString();
            dic.Add("from", from);
            dic.Add("to", to);
            dic.Add("signType", "v3");

            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            long millis = (long)ts.TotalMilliseconds;
            string curtime = Convert.ToString(millis / 1000);
            dic.Add("curtime", curtime);

            string signStr = appId + Truncate(query) + salt + curtime + secretKey;
            string sign = ComputeHash(signStr, new SHA256CryptoServiceProvider());
            dic.Add("q", System.Web.HttpUtility.UrlEncode(query));
            dic.Add("appKey", appId);
            dic.Add("salt", salt);
            dic.Add("sign", sign);

            return dic;
        }

        private string GetReturnResult(string query, string from, string to)
        {
            return GetReturnResult(GetUrlMap(query, from, to));
        }

        private string GetReturnResult(Dictionary<string, string> dic)
        {
            string result = "";

            StringBuilder builder = new StringBuilder("https://openapi.youdao.com/api").Append("/?");
            builder.Append("from=").Append(dic["from"]).Append("&");
            builder.Append("to=").Append(dic["to"]).Append("&");
            builder.Append("signType=").Append(dic["signType"]).Append("&");
            builder.Append("curtime=").Append(dic["curtime"]).Append("&");
            builder.Append("q=").Append(dic["q"]).Append("&");
            builder.Append("appKey=").Append(dic["appKey"]).Append("&");
            builder.Append("salt=").Append(dic["salt"]).Append("&");
            builder.Append("sign=").Append(dic["sign"]);

            //Console.WriteLine(builder.ToString());

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(builder.ToString());
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            Stream stream = resp.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            stream.Close();
            return result;
        }
        
        private static string ComputeHash(string input, HashAlgorithm algorithm)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }

        private static string Truncate(string q)
        {
            if (q == null)
            {
                return null;
            }
            int len = q.Length;
            return len <= 20 ? q : (q.Substring(0, 10) + len + q.Substring(len - 10, 10));
        }
        
    }
}
