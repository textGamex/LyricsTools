using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LyricsTools.Music
{
    /// <summary>
    /// 提供音乐的信息
    /// </summary>
    public class Music163Info
    {
        static void Main(string[] args)
        {
            Console.WriteLine(GetRequestUrl("1865167375", RequestType.Song));
        }
        public const string Api = "https://api.imjad.cn/cloudmusic/";

        /// <summary>  
        /// 
        /// </summary>  
        /// <param name="MusicId">网易云音乐Id</param>  
        /// <exception cref="ArgumentNullException">如果<c>MusicId</c>为null
        /// </exception>
        /// <exception cref="ArgumentException">MusicId不正确时</exception>
        public Music163Info(string MusicId)
        {
            if (MusicId == null)
                throw new ArgumentNullException(nameof(MusicId));
            Id = ulong.Parse(MusicId);

            //todo 整合网易云歌词和Lyrics(写个子类)
            string result = GetApiReturnInfo(GetRequestUrl(MusicId, RequestType.Detail));            
            JObject rawJson = JObject.Parse(result);
            //错误检测
            var otherInfo = JObject.Parse(rawJson.GetValue("privileges")[0].ToString());
            if (otherInfo.GetValue("st").ToString() == "-200")
            {
                throw new ArgumentException("无效id=" + MusicId);
            }

            var musicMainInfo = JObject.Parse(rawJson.GetValue("songs")[0].ToString());
            Duration = uint.Parse(musicMainInfo.GetValue("dt").ToString());
            MusicName = musicMainInfo.GetValue("name").ToString();
            //歌手名称
            var singerInfo = JObject.Parse(musicMainInfo.GetValue("ar")[0].ToString());
            SingerName = singerInfo.GetValue("name").ToString();
        }

        /// <summary>
        ///  网易云歌曲Id
        /// </summary>
        public ulong Id
        {
            get;
        }

        /// <summary>
        /// 音乐名称
        /// </summary>
        public string MusicName
        {
            get;
        }        

        /// <summary>
        /// 歌手名称
        /// </summary>
        public string SingerName
        {
            get;
        }

        /// <summary>  
        /// 音乐时长, 单位为毫秒
        /// </summary>  
        public uint Duration
        {
            get; private set;
        }

        ///// <summary>  
        ///// 大小, 单位为字节
        ///// </summary>  
        //public string FileSize
        //{
        //    get; private set;
        //}

        const int GB = 1024 * 1024 * 1024;
        const int MB = 1024 * 1024;
        const int KB = 1024;

        /// <summary>
        /// 文件大小单位转换
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        private string SizeFormat(long len)
        {
            if (len / GB >= 1)
            {
                return Math.Round(len / (float)GB, 2) + " GB";
            }
            if (len / MB >= 1)
            {
                return Math.Round(len / (float)MB, 2) + " MB";
            }
            if (len / KB >= 1)
            {
                return Math.Round(len / (float)KB, 2) + " KB";
            }
            return "--";
        }

        private static string GetRequestUrl(string MusicId, RequestType requestType)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Api);
            stringBuilder.Append("?type=").Append(requestType.ToString().ToLower());
            stringBuilder.Append("&id=").Append(MusicId);
            return stringBuilder.ToString();
        }        

        private static string GetApiReturnInfo(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 6000;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string result = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            myResponseStream.Close();
            return result;
        }
    }
}
