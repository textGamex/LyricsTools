﻿using Lyrics.Translation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LyricsTools.Tools.Debug;

namespace Lyrics
{
    public class LyricsFile : ICollection<(TimeTag, string)>, ICloneable
    {
        private LinkedList<(TimeTag timeTag, string lyrics)> data =
            new LinkedList<(TimeTag timeTag, string lyrics)>();
        public string MusicName
        {
            get; private set; 
        }
        private LyricsLanguageType language = LyricsLanguageType.Unknown;

        public LyricsFile(string[] lrcFileRawData)
        {
            if (lrcFileRawData == null)
                throw new ArgumentNullException(nameof(lrcFileRawData));

            foreach (var item in lrcFileRawData)
            {
                data.AddLast(GetTimeTagAndLyrics(item));
            }

            MusicName = "Music";
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
            MusicName = GetFileName(readFileStream.Name);
        }

        public LyricsFile(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));
            
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        data.AddLast(GetTimeTagAndLyrics(streamReader.ReadLine()));
                    }
                }
            }
            MusicName = GetFileName(filePath);
        }

        private static (TimeTag timeTag, string lyrics) GetTimeTagAndLyrics(string rawLine)
        {            
            (TimeTag timeTag, string lyrics) lineData;
            lineData.timeTag = new TimeTag(rawLine);
            lineData.lyrics = LyricsTool.GetLineLyric(rawLine);
            return lineData;
        }

        private static string GetFileName(string filePath)
        {
            IsNotNull(filePath);

            //Console.WriteLine("文件路径" + filePath);
            int index = filePath.LastIndexOf('\\') + 1;
            //Console.WriteLine(index.ToString());
            string newString = filePath.Substring(index);
            //Console.WriteLine(newString.Split('.')[0]);
            string songName = newString.Split('.')[0];
            return songName;
        }
        
        public void WriteFileTo(string saveFolderPath)
        {
            if (saveFolderPath == null)
                throw new ArgumentNullException(nameof(saveFolderPath));

            using (FileStream fileStream = new FileStream(saveFolderPath + $"\\{MusicName}-{language}.lrc", FileMode.Create))
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
        public void RemoveBefore(in TimeTag removeTimeTag)
        {
            for (var node = data.First; node != null; node = node.Next)
            {
                if (node.Value.timeTag > removeTimeTag)
                {
                    node = node.Previous;
                    data.AddAfter(node, (removeTimeTag, node.Value.lyrics));
                    //删除node及所有node前的数据
                    for (LinkedListNode<(TimeTag timeTag, string lyrics)> previous; node != null; node = previous)
                    {
                        //Console.WriteLine("移除" + node.Value);
                        previous = node.Previous;
                        data.Remove(node);
                    }
                    //前移时间标签
                    for (var n = data.First; n != null; n = n.Next)
                    {                        
                        n.Value = (n.Value.timeTag - removeTimeTag, n.Value.lyrics);
                    }
                    break;
                }
            }

        }

        /// <summary>
        /// 移除<c>removeTime</c>之后的timeTag及lyrics
        /// </summary>
        /// <param name="removeTimeTag">时间标签</param>
        public void RemoveAfter(in TimeTag removeTimeTag)
        {
            for (var node = data.First; node != null; node = node.Next)
            {
                if (node.Value.timeTag > removeTimeTag || node.Value.timeTag == removeTimeTag)
                {
                    data.AddBefore(node, (removeTimeTag, "---END---"));
                    //删除node及所有node前的数据
                    for (LinkedListNode<(TimeTag timeTag, string lyrics)> next; node != null; node = next)
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
        public void InterceptTime(in TimeTag statr, in TimeTag end)
        {
            RemoveBefore(statr);
            RemoveAfter(end);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="api">翻译API</param>
        /// <param name="targetLanguage">翻译到的语言</param>
        /// <returns>一个新的<see cref="LyricsFile"/>对象</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public LyricsFile TranslateTo(ITranslation api, UnifiedLanguageCode targetLanguage)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));
                        
            LyricsFile newLyricsFile = (LyricsFile) ((ICloneable)this).Clone();
            newLyricsFile.language = LanguageEnumConversion(targetLanguage);

            string rawLyrscs = GetAllLyrics();
            string correspondingLanguageCode = api.GetStandardTranslationLanguageParameters(targetLanguage);
            var translation = api.GetTransResultAndRawDataMap(rawLyrscs, "auto", correspondingLanguageCode);
            foreach (string rawLyrics in translation.Keys)
            {
                for (var node = newLyricsFile.data.First; node != null; node = node.Next)
                {
                    if (node.Value.lyrics == rawLyrics)
                    {
                        node.Value = (node.Value.timeTag, translation[rawLyrics]);
                    }
                }
            }
            return newLyricsFile;
        }

        /// <summary>
        /// 把<see cref="UnifiedLanguageCode"/>转换为等价的<see cref="LyricsLanguageType"/>
        /// </summary>
        /// <param name="unifiedLanguageCode"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private LyricsLanguageType LanguageEnumConversion(UnifiedLanguageCode unifiedLanguageCode)
        {
            switch (unifiedLanguageCode)
            {
                case UnifiedLanguageCode.English: return LyricsLanguageType.English;
                case UnifiedLanguageCode.Chinese: return LyricsLanguageType.Chinese;
                case UnifiedLanguageCode.Japanese: return LyricsLanguageType.Japanese;
                case UnifiedLanguageCode.German: return LyricsLanguageType.German;
                case UnifiedLanguageCode.TraditionalChinese: return LyricsLanguageType.TraditionalChinese;
                case UnifiedLanguageCode.Russian: return LyricsLanguageType.Russian;
                case UnifiedLanguageCode.French: return LyricsLanguageType.French;
                case UnifiedLanguageCode.Spanish: return LyricsLanguageType.Spanish;

                default: throw new ArgumentException(nameof(unifiedLanguageCode));
            }
        }

        /// <summary>
        /// 得到data里的所有lyrics
        /// </summary>
        /// <returns>data里的所有lyrics组成的一个字符串</returns>
        private string GetAllLyrics()
        {
            List<string> lyrics = new List<string>();
            foreach (var item in data)
            {
                lyrics.Add(item.lyrics);
            }
            return LyricsTool.ProcessingLyrics(lyrics);
        }

        /// <summary>
        /// 得到标准格式的LRC文件内容数组, 每个元素对应文件中的一行
        /// </summary>
        /// <returns>标准格式的LRC文件内容数组, 每个元素对应文件中的一行</returns>
        public string[] GetLrcFileTypeArray()
        {
            List<string> list = new List<string>();

            foreach (var (timeTag, lyrics) in data)
            {
                list.Add(string.Format($"{timeTag.ToTimeTag()}{lyrics}"));
            }
            return list.ToArray();
        }

        public int Count => data.Count;

        /// <summary>
        /// 歌词使用的语言
        /// </summary>
        public LyricsLanguageType LyricsLanguage => language;

        #region ICollection接口实现

        public IEnumerator<(TimeTag, string)> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public void Clear()
        {
            data.Clear();
        }

        public bool Contains((TimeTag, string) item)
        {
            return data.Contains(item);
        }

        public bool Remove((TimeTag, string) item)
        {
            return data.Remove(item);
        }

        bool ICollection<(TimeTag, string)>.IsReadOnly => false;

        void ICollection<(TimeTag, string)>.Add((TimeTag, string) item)
        {
            data.AddLast(item);
        }

        void ICollection<(TimeTag, string)>.CopyTo((TimeTag, string)[] array, int arrayIndex)
        {
            data.CopyTo(array, arrayIndex);
        }
        #endregion

        object ICloneable.Clone()
        {
            (TimeTag, string)[] array = new (TimeTag, string)[data.Count];
            data.CopyTo(array, 0);
            var clone = new LyricsFile(array);
            clone.MusicName = this.MusicName;
            return clone;
        }

        private LyricsFile((TimeTag, string)[] datas)
        {
            foreach (var itme in datas)
            {
                data.AddLast(itme);
            }
        }
    }
}
