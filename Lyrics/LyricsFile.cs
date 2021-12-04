using Lyrics.Translation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static LyricsTools.Tools.Debug;

namespace Lyrics
{
    /// <summary>
    /// 处理LRC文件数据的类
    /// </summary>
    public partial class LyricsFile : ICollection<(TimeTag, string)>, ICloneable
    {
        /// <summary>
        /// 结束标志
        /// </summary>
        public const string END_FLAG = "-----END-----";
        protected readonly LinkedList<(TimeTag timeTag, string lyrics)> _data =
            new LinkedList<(TimeTag timeTag, string lyrics)>();
        private UnifiedLanguageCode? _contentLanguage = null;
        public string MusicName{ get; private set; }
        public int Count => _data.Count;

        /// <summary>
        /// 集合中每个元素为LRC文件中的一行
        /// </summary>
        /// <param name="lrcFileRawData"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LyricsFile(IEnumerable<string> lrcFileRawData)
        {
            if (lrcFileRawData == null)
                throw new ArgumentNullException(nameof(lrcFileRawData));

            var list = new List<string>();
            foreach (var item in lrcFileRawData)
            {
                var result = GetTimeTagAndLyrics(item);
                list.Add(result.lyrics);
                _data.AddLast(result);
            }

            MusicName = "Music";
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
                        _data.AddLast(GetTimeTagAndLyrics(streamReader.ReadLine()));
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
            
            int index = filePath.LastIndexOf('\\') + 1;            
            string newString = filePath.Substring(index);            
            string songName = newString.Split('.')[0];
            return songName;
        }

        /// <summary>
        /// 翻译到指定语种
        /// </summary>
        /// <param name="api">翻译API</param>
        /// <param name="targetLanguage">翻译到的语言</param>
        /// <returns>一个新的<see cref="LyricsFile"/>对象</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public LyricsFile TranslateTo(ITranslation api, UnifiedLanguageCode targetLanguage)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));

            LyricsFile newLyricsFile = (LyricsFile)((ICloneable)this).Clone();
            newLyricsFile._contentLanguage = targetLanguage;

            string rawLyrscs = GetAllLyrics();
            string correspondingLanguageCode = api.GetStandardTranslationLanguageParameters(targetLanguage);
            var translation = api.GetTransResultAndRawDataMap(rawLyrscs, "auto", correspondingLanguageCode);
            foreach (string rawLyrics in translation.Keys)
            {
                for (var node = newLyricsFile._data.First; node != null; node = node.Next)
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
        /// 把对象写入到<c>saveFolderPath</c>
        /// </summary>
        /// <param name="saveFolderPath">写入路径</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void WriteFileTo(string saveFolderPath)
        {
            if (saveFolderPath == null)
                throw new ArgumentNullException(nameof(saveFolderPath));

            string language;
            if (_contentLanguage == null)
            {
                language = "Unknown";
            }
            else
            {
                language = _contentLanguage.ToString();
            }
            using (FileStream fileStream = new FileStream(saveFolderPath + $"\\{MusicName}-{language}.lrc", FileMode.Create))
            {
                using (StreamWriter file = new StreamWriter(fileStream))
                {
                    foreach (var (timeTag, lyrics) in _data)
                    {
                        file.WriteLine($"{timeTag.ToTimeTag()}{lyrics}");
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
            for (var node = _data.First; node != null; node = node.Next)
            {
                if (node.Value.timeTag > removeTimeTag)
                {
                    node = node.Previous;
                    _data.AddAfter(node, (removeTimeTag, node.Value.lyrics));
                    //删除node及所有node前的数据
                    for (LinkedListNode<(TimeTag timeTag, string lyrics)> previous; node != null; node = previous)
                    {
                        previous = node.Previous;
                        _data.Remove(node);
                    }
                    //前移时间标签
                    for (var needRemoveNode = _data.First; needRemoveNode != null; needRemoveNode = needRemoveNode.Next)
                    {
                        needRemoveNode.Value = (needRemoveNode.Value.timeTag - removeTimeTag, needRemoveNode.Value.lyrics);
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
            for (var node = _data.First; node != null; node = node.Next)
            {
                if (node.Value.timeTag > removeTimeTag || node.Value.timeTag == removeTimeTag)
                {
                    _data.AddBefore(node, (removeTimeTag, "---END---"));
                    //删除node及所有node前的数据
                    for (LinkedListNode<(TimeTag timeTag, string lyrics)> next; node != null; node = next)
                    {
                        //Console.WriteLine("移除" + node.Value);
                        next = node.Next;
                        _data.Remove(node);
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
        /// <exception cref="ArgumentException">如果statr 大于 end</exception>
        public void InterceptTime(in TimeTag statr, in TimeTag end)
        {
            if (statr < end)
            {
                throw new ArgumentException();
            }
            RemoveBefore(statr);
            RemoveAfter(end);
        }



        /// <summary>
        /// 得到data里的所有lyrics
        /// </summary>
        /// <returns>data里的所有lyrics组成的一个字符串</returns>
        private string GetAllLyrics()
        {
            List<string> lyrics = new List<string>();
            foreach (var item in _data)
            {
                lyrics.Add(item.lyrics);
            }
            return LyricsTool.ProcessingLyrics(lyrics);
        }

        //protected Dictionary<TimeTag, string> GetData()
        //{
        //    var map = new Dictionary<TimeTag, string>(data.Count);
        //    foreach (var (timeTag, lyrics) in data)
        //    {
        //        map[timeTag] = lyrics;
        //    }
        //    return map;
        //}

        /// <summary>
        /// 得到标准格式的LRC文件内容数组, 每个元素对应文件中的一行
        /// </summary>
        /// <returns>标准格式的LRC文件内容数组, 每个元素对应文件中的一行</returns>
        public string[] GetLrcFileTypeArray()
        {
            List<string> list = new List<string>();

            foreach (var (timeTag, lyrics) in _data)
            {
                list.Add(string.Format($"{timeTag.ToTimeTag()}{lyrics}"));
            }
            return list.ToArray();
        }

        

        /// <summary>
        /// 歌词使用的语言
        /// </summary>
        public UnifiedLanguageCode? LyricsLanguage => _contentLanguage;

        #region ICollection接口实现

        public IEnumerator<(TimeTag, string)> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains((TimeTag, string) item)
        {
            return _data.Contains(item);
        }

        public bool Remove((TimeTag, string) item)
        {
            return _data.Remove(item);
        }

        bool ICollection<(TimeTag, string)>.IsReadOnly => false;

        void ICollection<(TimeTag, string)>.Add((TimeTag, string) item)
        {
            _data.AddLast(item);
        }

        void ICollection<(TimeTag, string)>.CopyTo((TimeTag, string)[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }
        #endregion

        object ICloneable.Clone()
        {
            (TimeTag, string)[] array = new (TimeTag, string)[_data.Count];
            _data.CopyTo(array, 0);
            var clone = new LyricsFile(array);
            clone.MusicName = this.MusicName;
            return clone;
        }

        //public virtual LyricsFile Clone()
        //{

        //}

        private LyricsFile((TimeTag, string)[] datas)
        {
            foreach (var itme in datas)
            {
                _data.AddLast(itme);
            }
        }
    }
}
