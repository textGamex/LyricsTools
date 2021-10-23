using Lyrics.Translation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using static System.Diagnostics.Debug;

namespace Lyrics
{
    public class LyricsFile : IEnumerable<(LyricTimeTag, string)>
    {
        private LinkedList<(LyricTimeTag timeTag, string lyrics)> data =
            new LinkedList<(LyricTimeTag timeTag, string lyrics)>();
        private readonly string fileName = "Music";
        private readonly LanguageType language = LanguageType.Unknown;

        public LyricsFile(string[] lrcFileRawData)
        {
            if (lrcFileRawData == null)
                throw new ArgumentNullException(nameof(lrcFileRawData));

            foreach (var item in lrcFileRawData)
            {
                data.AddLast(GetTimeTagAndLyrics(item));
            }
        }

        public LyricsFile(List<string> listData) : this(listData.ToArray())
        {
        }

        public LyricsFile(LinkedList<string> linkData) : this(new List<string>(linkData))
        {
        }

        public LyricsFile(FileStream readFileStream)
        {
            if (readFileStream == null)
                throw new ArgumentNullException(nameof(readFileStream));

            using (StreamReader streamReader = new StreamReader(readFileStream))
            {
                while (!streamReader.EndOfStream)
                {
                    data.AddLast(GetTimeTagAndLyrics(streamReader.ReadLine()));
                }
            }
            fileName = GetFileName(readFileStream.Name);
        }

        private static (LyricTimeTag timeTag, string lyrics) GetTimeTagAndLyrics(string rawLine)
        {
            Assert(rawLine != null);

            (LyricTimeTag timeTag, string lyrics) lineData;
            lineData.timeTag = new LyricTimeTag(rawLine);
            lineData.lyrics = LyricsTools.GetLineLyric(rawLine);
            return lineData;
        }

        private static string GetFileName(string filePath)
        {
            Assert(filePath != null);

            Console.WriteLine(filePath);
            int index = filePath.LastIndexOf('\\') + 1;
            Console.WriteLine(index.ToString());
            string newString = filePath.Substring(index);
            Console.WriteLine(newString);
            return newString.Split('.')[0];
        }
        
        public void FileWriteTO(string saveFolderPath)
        {
            if (saveFolderPath == null)
                throw new ArgumentNullException(nameof(saveFolderPath));

            using (FileStream fileStream = new FileStream(saveFolderPath + $@"\{fileName}-{language}.lrc", FileMode.Create))
            {
                using (StreamWriter file = new StreamWriter(fileStream))
                {
                    foreach (var lrcData in data)
                    {
                        file.WriteLine($"{lrcData.timeTag.ToTimeTag()}{lrcData.lyrics}");
                    }
                }
            }
        }

        /// <summary>
        /// 移除<c>removeTime</c>之前的timeTag及lyrics
        /// </summary>
        /// <param name="removeTimeTag">移除此时间标签前的所有时间标签</param>
        public void RemoveBefore(in LyricTimeTag removeTimeTag)
        {
            for (var node = data.First; node != null; node = node.Next)
            {
                if (node.Value.timeTag > removeTimeTag)
                {
                    node = node.Previous;
                    data.AddAfter(node, (removeTimeTag, node.Value.lyrics));
                    //删除node及所有node前的数据
                    for (LinkedListNode<(LyricTimeTag timeTag, string lyrics)> previous; node != null; node = previous)
                    {
                        //Console.WriteLine("移除" + node.Value);
                        previous = node.Previous;
                        data.Remove(node);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 移除<c>removeTime</c>之后的timeTag及lyrics
        /// </summary>
        /// <param name="removeTimeTag">时间标签</param>
        public void RemoveAfter(in LyricTimeTag removeTimeTag)
        {           
            for (var node = data.First; node != null; node = node.Next)
            {
                if (node.Value.timeTag > removeTimeTag || node.Value.timeTag == removeTimeTag)
                {
                    data.AddBefore(node, (removeTimeTag, "---END---"));
                    //删除node及所有node前的数据
                    for (LinkedListNode<(LyricTimeTag timeTag, string lyrics)> next; node != null; node = next)
                    {
                        //Console.WriteLine("移除" + node.Value);
                        next = node.Next;
                        data.Remove(node);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 截取其中一段时间
        /// </summary>
        /// <param name="statr">开始</param>
        /// <param name="end">结束</param>
        public void Intercept(in LyricTimeTag statr, in LyricTimeTag end)
        {
            RemoveBefore(statr);
            RemoveAfter(end);
        }
        //public bool 

        public int Count => data.Count;
        public string FileName => fileName;
        /// <summary>
        /// 歌词使用的语言
        /// </summary>
        public LanguageType LyricsLanguage => language;       
        #region foreach实现

        public IEnumerator<(LyricTimeTag, string)> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
