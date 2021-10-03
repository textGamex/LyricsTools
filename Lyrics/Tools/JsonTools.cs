using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
//using System.Collections.Generic;

namespace Lyrics.Baidu
{
    public partial class TranslateApi
    {
        #region 解析 https://fanyi-api.baidu.com/api/trans/vip/translate 返回的json格式
        public static class JsonTools
        {
            public static Dictionary<string, string> GetTranslatedMap(string serverReturnedJson)
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                JObject rawJson = JObject.Parse(serverReturnedJson);
                JToken translatedTextArray = rawJson.GetValue("trans_result");

                foreach (JToken translated in translatedTextArray)
                {
                    JObject single = JObject.Parse(translated.ToString());

                    string rawText = single.GetValue("src").ToString();
                    string translatedText = single.GetValue("dst").ToString();
                    if (!data.ContainsKey(rawText))
                    {
                        data.Add(rawText, translatedText);
                    }          
                }

                return data;
            }
        }
    #endregion
}

}
