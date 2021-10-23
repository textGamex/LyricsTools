using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsTools.Tools
{
    /// <summary>
    /// 提供一组辅助调试的静态方法
    /// </summary>
    public static class Debug
    {
        /// <summary>
        /// 检查<c>obj</c>是否为null, 如果是, 则显示一个消息框,其中显示调用堆栈
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="obj">要检测是否为null的对象</param>
        public static void IsNotNull<T>(T obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);
        }
    }
}
