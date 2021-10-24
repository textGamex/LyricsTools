using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsTools.Music
{
    public enum RequestType
    {
        /// <summary>
        /// 歌曲的下载地址信息
        /// </summary>
        Song,
        /// <summary>
        /// 歌曲的歌词信息
        /// </summary>
        Lyric,
        /// <summary>
        /// 歌曲的基本的信息
        /// </summary>
        Detail,
    }
}
