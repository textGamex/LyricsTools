using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyrics
{
    [Flags]
    public enum LanguageCode
    {
        NONE = 1 << 0,
        /// <summary>
        /// 中文
        /// </summary>
        ZH   = 1 << 1,
        /// <summary>
        /// 英语
        /// </summary>
        EN   = 1 << 2,
        /// <summary>
        /// 日语
        /// </summary>
        JP   = 1 << 3,
        /// <summary>
        /// 俄语
        /// </summary>
        RU   = 1 << 4,
        /// <summary>
        /// 繁体中文
        /// </summary>
        CHT  = 1 << 5,
        /// <summary>
        /// 德语
        /// </summary>
        DE   = 1 << 6,
        /// <summary>
        /// 法语
        /// </summary>
        FRA  = 1 << 7,
        /// <summary>
        /// 西班牙语
        /// </summary>
        SPA  = 1 << 8,
    }
}
