using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lyrics.Translation;

namespace LyricsTools.Lyrics.Translation
{
    /// <summary>
    /// 提供比<see cref="ITranslation"/>更多的服务
    /// </summary>
    public interface IAutoTranslation : ITranslation
    {
        /// <summary>
        /// 得到翻译后的集合
        /// </summary>
        /// <param name="rawDatas">从lrc文件中读取的数据</param>
        /// <param name="from">原始数据的语言</param>
        /// <param name="targetLanguage">目标语言</param>
        /// <returns>一个翻译完成的数组</returns>
        string[] GetTransResultArray(string[] rawDatas, string from, string targetLanguage);
    }
}
