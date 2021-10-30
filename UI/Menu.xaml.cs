using Lyrics;
using LyricsTools.JsonClass;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LyricsTools.UI
{
    /// <summary>
    /// Menu.xaml 的交互逻辑
    /// </summary>
    public partial class Menu : Window
    {
        public const string VERSION = "v1.2.0.0";
        public Menu()
        {
            InitializeComponent();
        }

        private void Translation_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            Close();            
        }

        private void Processing_Click(object sender, RoutedEventArgs e)
        {
            Processing processing = new Processing();
            processing.Show();
            Close();
        }

        //打开仓库
        private void ProjectUrl_Click(object sender, RoutedEventArgs e)
        {
            _ = System.Diagnostics.Process.Start("https://github.com/textGamex/LyricsTools");
            _ = System.Diagnostics.Process.Start("https://gitee.com/mengxin_C/LyricsTools");
        }

        //检查更新
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://gitee.com/api/v5/repos/mengxin_C/LyricsTools/releases/latest?access_token=" + MyData.GiteeKey;

            string result = "";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);            
            req.Method = "GET";
            req.ContentType = "application/x-www-form-urlencoded";
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            using (Stream stream = resp.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
            }                       

            GiteeReleases gitInfo = JsonConvert.DeserializeObject<GiteeReleases>(result);
            if (IsUpdateVersion(gitInfo.Tag_name))
            {
                var value = MessageBox.Show("发现新版本, 是否更新?", "更新", MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Asterisk);
                if (value == MessageBoxResult.Yes)
                {
                    var da = new UpdateWindow(gitInfo, result);
                    da.ShowDialog();
                }
                else
                {
                    return;
                }
            }
            else
            {
                MessageBox.Show("暂无新版本更新");
                return;
            }
        }

        /// <summary>
        /// 对比两个版本号
        /// </summary>
        /// <param name="newVersion"></param>
        /// <returns>如果Gitee上有新版本, 返回true, 否则返回false</returns>
        private bool IsUpdateVersion(string newVersion)
        {
            newVersion = newVersion.ToLower();
            if (newVersion == VERSION.ToLower())
            {
                return false;
            }

            var newVersionArray = GetVersionCode(newVersion);
            var thisVersionArray = GetVersionCode(VERSION.ToLower());

            for (int i = 0; i < newVersionArray.Length; ++i)
            {
                if (newVersionArray[i] > thisVersionArray[i])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获得版本号
        /// </summary>
        /// <param name="version">字符串形式的版本号</param>
        /// <returns></returns>
        private byte[] GetVersionCode(string version)
        {
            version = version.Split('v')[1];
            var info = version.Split('.');
            byte[] versionArray = new byte[info.Length];

            versionArray[0] = byte.Parse(info[0]);
            versionArray[1] = byte.Parse(info[1]);
            versionArray[2] = byte.Parse(info[2]);
            versionArray[3] = byte.Parse(info[3]);

            return versionArray;
        }
    }
}
