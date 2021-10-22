using Lyrics.Translation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
            (LyricTimeTag timeTag, string lyrics) lineData;
            lineData.timeTag = new LyricTimeTag(rawLine);
            lineData.lyrics = LyricsTools.GetLineLyric(rawLine);
            return lineData;
        }

        private static string GetFileName(string filePath)
        {
            Console.WriteLine(filePath);
            int index = filePath.LastIndexOf('\\') + 1;
            Console.WriteLine(index.ToString());
            string newString = filePath.Substring(index);
            Console.WriteLine(newString);
            return newString.Split('.')[0];
        }
        
        public void FileWriteTO(string savePath)
        {
            if (savePath == null)
                throw new ArgumentNullException(nameof(savePath));

            using (FileStream fileStream = new FileStream(savePath + $@"\{fileName}-{language}.lrc", FileMode.Create))
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
        /// <param name="removeTime">移除此时间标签前的所有时间标签</param>
        public void RemoveBefore(LyricTimeTag removeTime)
        {
            for (var node = data.First; node != null; node = node.Next)
            {
                if (node.Value.timeTag > removeTime)
                {
                    node = node.Previous;
                    data.AddAfter(node, (removeTime, node.Value.lyrics));
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

        public void RemoveAfter(LyricTimeTag removeTime)
        {

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
