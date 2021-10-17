using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Lyrics.Translation;

namespace Lyrics
{
    public class LyricsCollection : IEnumerable<(LyricTimeTag, string)>
    {
        private LinkedList<(LyricTimeTag timeTag, string lyrics)> data =
            new LinkedList<(LyricTimeTag timeTag, string lyrics)>();
        private string fileName = "Music";
        private LanguageCode language = LanguageCode.Unknown;

        public LyricsCollection(string[] lrcFileRawData)
        {
            if (lrcFileRawData == null)
                throw new ArgumentNullException(nameof(lrcFileRawData));

            foreach (var item in lrcFileRawData)
            {
                data.AddLast(GetTimeTagAndLyrics(item));
            }
        }

        public LyricsCollection(List<string> listData) : this(listData.ToArray())
        {
        }

        public LyricsCollection(LinkedList<string> linkData) : this(new List<string>(linkData))
        {
        }

        public LyricsCollection(FileStream readFileStream)
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
                        file.WriteLine($"{lrcData.timeTag}{lrcData.lyrics}");
                    }
                }
            }
        }
        
        public int Count => data.Count;
        public string FileName => fileName;
        public LanguageCode LyricsLanguage => language;       
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
