//using NAudio.Wave;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace LyricsTools.Music
//{
//    /// <summary>
//    /// 提供音乐的信息
//    /// </summary>
//    public class MusicInfo
//    {
//        /// <summary>  
//        /// Mp3文件地址  
//        /// </summary>  
//        /// <param name="fileName">Mp3文件地址</param>  
//        /// <exception cref="ArgumentNullException">如果<c>fileName</c>为null
//        /// </exception>
//        public MusicInfo(string fileName)
//        {
//            if (fileName == null)
//                throw new ArgumentNullException(nameof(fileName));

//            using (Mp3FileReader mp3FileReader = new Mp3FileReader(fileName))
//            {
//                Duration = mp3FileReader.TotalTime;
//                mp3FileReader.Dispose();
//            }
//            FileInfo fileInfo = new FileInfo(fileName);
//            FileSize = SizeFormat(fileInfo.Length);
//        }

//        /// <summary>  
//        /// 音乐时长  
//        /// </summary>  
//        public TimeSpan Duration
//        {
//            get; private set;
//        }

//        /// <summary>  
//        /// 大小  
//        /// </summary>  
//        public string FileSize
//        {
//            get; private set;
//        }

//        const int GB = 1024 * 1024 * 1024;
//        const int MB = 1024 * 1024;
//        const int KB = 1024;

//        /// <summary>
//        /// 文件大小单位转换
//        /// </summary>
//        /// <param name="len"></param>
//        /// <returns></returns>
//        private string SizeFormat(long len)
//        {
//            if (len / GB >= 1)
//            {
//                return Math.Round(len / (float)GB, 2) + " GB";
//            }
//            if (len / MB >= 1)
//            {
//                return Math.Round(len / (float)MB, 2) + " MB";
//            }
//            if (len / KB >= 1)
//            {
//                return Math.Round(len / (float)KB, 2) + " KB";
//            }
//            return "--";
//        }
//    }
//}
