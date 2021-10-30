using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lyrics;

namespace Lyrics
{
    public partial class LyricsFile
    {
        /// <summary>
        /// 统计<see cref="LyricsFile"/>信息
        /// </summary>
        public class Logger
        {
            public uint TotalLine { get; set; } = 0;
            public uint TotalLineLength { get; set; } = 0;
            public double LineAverageLength
            {
                get
                {
                    if (TotalLineLength == 0)
                        return 0.0;
                    return (double)TotalLine / TotalLineLength;
                }
            }
            /// <summary>
            /// 重复字符串数量
            /// </summary>
            public uint RepeatingItemNumber { get; set; } = 0;
        }
    }
    
}
