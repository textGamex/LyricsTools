using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyrics.Translation.Baidu
{
    public partial class BaiduTranslationApi
    {
        public class ErrorCode
        {
            public static readonly int NONE = 0;
            public static readonly int UNAUTHORIZED_USER = 52003; 
        }
    }
}
