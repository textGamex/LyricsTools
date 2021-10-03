using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyrics
{
    public enum StateCode : ushort
    {        
        NONE,
        /// <summary>
        /// 用户已选择歌词文件
        /// </summary>
        USER_HAS_SELECTED_LYRIC_FILES,
    }
}
