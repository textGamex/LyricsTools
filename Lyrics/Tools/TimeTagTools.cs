using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyrics
{
    public static class TimeTagTools
    {
        /// <summary>
        /// 返回总和的毫秒
        /// </summary>
        /// <param name="minute">分</param>
        /// <param name="second">秒</param>
        /// <param name="millisecond">毫秒</param>
        /// <returns>传入的时间换算成毫秒的数</returns>
        public static uint ToMillisecond(uint minute, uint second, uint millisecond)
        {
            return (minute * 60 + second) * 1000 + millisecond;
        }
    }
}
