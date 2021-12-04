using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsTools.Lyrics.Bili
{
    public class Credential
    {
        //上传用的信息
        public string SessData { get; }
        public string Bili_jct { get; }
        public string Buvid3 { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessData"></param>
        /// <param name="bili_jct"></param>
        /// <param name="buvid3"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Credential(string sessData, string bili_jct, string buvid3)
        {
            Bili_jct = bili_jct ?? throw new ArgumentNullException(nameof(bili_jct));
            Buvid3 = buvid3 ?? throw new ArgumentNullException(nameof(buvid3));
            SessData = sessData ?? throw new ArgumentNullException(nameof(sessData));
        }
    }
}
