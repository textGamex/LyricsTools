using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.IO;

namespace Lyrics.Translation.Youdao
{
    class YoudaoTranslateApi : ITranslation
    {
        private readonly string appId;
        private readonly string secretKey;

        public YoudaoTranslateApi(string newAppId, string newSecretKey)
        {
            appId = newAppId;
            secretKey = newSecretKey;
        }

        public Dictionary<string, string> GetTransResultAndRawDataMap(string query, string from, string to)
        {
            var json = GetReturnResult(GetUrlMap(query, from, to));
            return JsonToTranslatedMap(json);
        }

        private static Dictionary<string, string> JsonToTranslatedMap(string serverReturnedJson)
        {
            var map = new Dictionary<string, string>();
            var json = JObject.Parse(serverReturnedJson);
            string[] rawTextArray = json.GetValue("query").ToString().Split('\n');
            string[] translationTextArray = json.GetValue("translation")[0].ToString().Split('\n');

            if (rawTextArray.Length != translationTextArray.Length)
                throw new Exception("奇怪的错误");

            for (int i = 0; i < rawTextArray.Length; ++i)
            {
                map[rawTextArray[i]] = translationTextArray[i];
            }
            return map;
        }

        public Dictionary<string, string> GetUrlMap(string query, string from, string to)
        {
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

        public string GetReturnResult(Dictionary<string, string> dic)
        {
            string result = "";

            StringBuilder builder = new StringBuilder("https://openapi.youdao.com/api").Append("/?");
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append("&");
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(builder.ToString());
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            Stream stream = resp.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        protected static string ComputeHash(string input, HashAlgorithm algorithm)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }

        protected static string Truncate(string q)
        {
            if (q == null)
            {
                return null;
            }
            int len = q.Length;
            return len <= 20 ? q : (q.Substring(0, 10) + len + q.Substring(len - 10, 10));
        }

        private static bool SaveBinaryFile(WebResponse response, string FileName)
        {
            string FilePath = FileName + DateTime.Now.Millisecond.ToString() + ".mp3";
            bool Value = true;
            byte[] buffer = new byte[1024];

            try
            {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
                Stream outStream = System.IO.File.Create(FilePath);
                Stream inStream = response.GetResponseStream();

                int l;
                do
                {
                    l = inStream.Read(buffer, 0, buffer.Length);
                    if (l > 0)
                        outStream.Write(buffer, 0, l);
                }
                while (l > 0);

                outStream.Close();
                inStream.Close();
            }
            catch
            {
                Value = false;
            }
            return Value;
        }

        public string GetStandardTranslationLanguageParameters(UnifiedLanguageCode standardLanguageParameters)
        {
            throw new NotImplementedException();
        }
    }
}
