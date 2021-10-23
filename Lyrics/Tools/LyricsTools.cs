using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Lyrics
{
    public static class LyricsTools
    {
        /// <summary>
        /// 传入一行带时间标签的歌词, 返回其歌词部分
        /// </summary>
        /// <param name="text">lrc文件中的一行歌词</param>
        /// <returns>其歌词部分</returns>
        public static string GetLineLyric(string text)
        {
            return text.Split(']')[1].Trim();
        }

        /// <summary>
        /// 返回字符串, 其中包含了集合中的所有字符串,并用换行符分隔开, 会删除重复的字符串
        /// </summary>
        /// <param name="lyrics">LRC文件中的所有歌词, 不能带时间标签</param>
        /// <returns>一个字符串, 其中包含了集合中的所有字符串,并用换行符分隔开</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ProcessingLyrics(in string[] lyrics)
        {                       
            return ProcessingLyrics(new List<string>(lyrics));
        }

        /// <summary>
        /// 返回字符串, 其中包含了集合中的所有字符串,并用换行符分隔开, 会删除重复的字符串
        /// </summary>
        /// <param name="lyrics">LRC文件中的所有歌词, 不能带时间标签</param>
        /// <returns>一个字符串, 其中包含了集合中的所有字符串,并用换行符分隔开</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ProcessingLyrics(List<string> lyrics)
        {
            if (lyrics == null)
                throw new ArgumentNullException(nameof(lyrics));

            //去除重复字符串, 以减少翻译字符数
            lyrics = lyrics.Distinct().ToList();

            StringBuilder sb = new StringBuilder(60);
            for (int i = 0, max = lyrics.Count - 1; i < max; ++i)
            {               
                sb.Append(lyrics[i]).Append(Environment.NewLine);
            }
            _ = sb.Append(lyrics[lyrics.Count - 1]);
            return sb.ToString();
        }

        /// <summary>
        /// 把<c>sourceText</c>中的歌词部分替换成<c>replacedText</c>
        /// </summary>
        /// <param name="sourceText">标准歌词</param>
        /// <param name="replacedText">要替换的字符串</param>
        /// <returns>被替换后的字符串</returns>
        /// <exception cref="ArgumentNullException">
        /// 如果<c>sourceText</c>或
        /// <c>replacedText</c>为null       
        /// </exception>
        public static string ReplaceLyric(string sourceText, string replacedText)
        {
            if (sourceText == null)
            {
                throw new ArgumentNullException(nameof(sourceText));
            }
            if (replacedText == null)
            {
                throw new ArgumentNullException(nameof(replacedText));
            }
            string result = sourceText.Split(']')[0];
            return result + ']' + replacedText;
        }
    }
}
