using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyrics.Translation.Youdao
{
    public partial class YoudaoTranslateApi
    {
        private static class JsonTools
        {
            public const int NoError = 0;
            public static string GetErrorMessage(in int errorCode)
            {
                switch (errorCode)
                {
                    case NoError: return "NONE";
                    case 101: return "程序错误, 请联系程序员";
                    case 102: return "不支持的语言类型";
                    case 103: return "翻译文本过长";
                    case 108: return "应用ID无效";
                    case 202: return "应用ID或密钥无效";
                    case 203: return "访问IP地址不在可访问IP列表";
                    case 401: return "账户已经欠费，请进行账户充值";
                    default: return "未知错误";
                }
            }

            public static Dictionary<string, string> JsonToTranslatedMap(string serverReturnedJson)
            {
                var map = new Dictionary<string, string>();
                var json = JObject.Parse(serverReturnedJson);
                Console.WriteLine(json.ToString());
                string[] rawTextArray = json.GetValue("query").ToString().Split('\n');
                string[] translationTextArray = json.GetValue("translation")[0].ToString().Split('\n');

                if (rawTextArray.Length != translationTextArray.Length)
                    throw new Exception("奇怪的错误");

                for (uint i = 0; i < rawTextArray.Length; ++i)
                {
                    map[rawTextArray[i]] = translationTextArray[i];
                }
                return map;
            }
        }
    }
}
