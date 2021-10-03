#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Lyrics
{
    public class MyData
    {
        public string AppId { get; }
        public string SecretKey { get; }

        public MyData()
        {
            FileStream fileStream = new FileStream(@"D:\IDE\Project\C#\LyricsTools\Lyrics\MyData\data.txt", FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            AppId = streamReader.ReadLine();
            SecretKey = streamReader.ReadLine();
        }
    }
}
#endif