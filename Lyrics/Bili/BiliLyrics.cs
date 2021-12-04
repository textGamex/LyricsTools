using Lyrics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LyricsTools.Lyrics.Bili
{
    public class BiliLyrics : LyricsFile
    {
        private const string _API = "https://api.bilibili.com/x/v2/dm/subtitle/draft/save";
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// B站Cookie信息
        /// </summary>
        private readonly Credential _credential;
        public BiliLyrics(string filePath, Credential credential) : base(filePath)
        {
            _credential = credential ?? throw new ArgumentNullException(nameof(credential)); ;
        }

        public BiliLyrics(IEnumerable<string> lrcFileRawData, Credential credential) : base(lrcFileRawData)
        {
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        }

        public int SubmitSubtitle(long uid, string lan, string bvid, int type = 1, bool sign = true, bool submit = true)
        {
            var subtitle = GetSubtitleData();
            var map = GetPostSubtitleData(subtitle, uid, lan, bvid, _credential.Bili_jct, type, sign, submit);
            var result = Post(map);
            log.Debug(result);
            return 0;
        }

        /// <summary>
        /// 指定Post地址使用Get 方式获取全部字符串
        /// </summary>
        /// <param name="dic">提交的数据</param>
        /// <returns></returns>
        private string Post(Dictionary<string, string> dic)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_API);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Headers.Add("cookie", GetBiliCookie());

            #region 添加Post 参数
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dic)
            {
                if (i > 0)
                    builder.Append('&');
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                ++i;
            }
            byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
            req.ContentLength = data.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
            }
            #endregion

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            //获取响应内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        /// <summary>
        /// 得到POST的字幕数据
        /// </summary>
        /// <param name="data">字幕数据</param>
        /// <param name="uid">UID</param>
        /// <param name="lan">字幕语言</param>
        /// <param name="bvid">B站BV号</param>
        /// <param name="csrf">Bili_jct</param>
        /// <param name="type">类型, 默认为1</param>
        /// <param name="sign">是否署名, 默认为true</param>
        /// <param name="submit">是否提交, 如果为false, 则保存为草稿, 默认为true</param>
        /// <returns></returns>
        private static Dictionary<string, string> GetPostSubtitleData(SubtitleData data, long uid, string lan, string bvid, string csrf, int type = 1, bool sign = true, bool submit = true)
        {
            var map = new Dictionary<string, string>(8)
            {
                ["type"] = type.ToString(),
                ["oid"] = uid.ToString(),
                ["sign"] = sign.ToString(),
                ["submit"] = submit.ToString(),
                ["lan"] = lan,
                ["bvid"] = bvid,
                ["csrf"] = csrf,
                ["data"] = JsonConvert.SerializeObject(data),
            };
            return map;
        }

        /// <summary>
        /// 把歌词打包成<see cref="SubtitleData"/>
        /// </summary>
        /// <returns>一个<see cref="SubtitleData"/>, 包含所有的数据</returns>
        private SubtitleData GetSubtitleData()
        {
            var subtitle = new SubtitleData();
            var list = new List<SubtitleBody>(Count);
            for (var node = _data.First; node != null; node = node.Next)
            {
                var body = new SubtitleBody();
                body.Content = node.Value.lyrics;
                //每一段的结束时间就是下一段的开始时间
                body.From = node.Value.timeTag.GetBiliFormatTimeTag();

                if (node.Next != null)
                {
                    body.To = node.Next.Value.timeTag.GetBiliFormatTimeTag();
                }
                else
                {
                    //? 无法知道歌曲结束时间, 先这样处理. 等接入了网易云API, 在末尾加一个标志符, 针对性处理
                    body.To = node.Value.timeTag.GetBiliFormatTimeTag() + 0.03;
                }
                log.Debug(body);
                list.Add(body);
            }
            subtitle.Body = list;
            return subtitle;
        }


        /// <summary>
        /// 得到Bili的Cookie
        /// </summary>
        /// <returns></returns>
        private string GetBiliCookie()
        {
            var sb = new StringBuilder(155);
            sb.Append("SESSDATA=").Append(_credential.SessData).Append(';');
            sb.Append("buvid3=").Append(_credential.Buvid3).Append(';');
            sb.Append("bili_jct=").Append(_credential.Bili_jct).Append(';');
            return sb.ToString();
        }
    }
}
