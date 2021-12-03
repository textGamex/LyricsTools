using System;
using System.Collections.Generic;
using System.Linq;
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
using LyricsTools.JsonClass;
using Newtonsoft.Json.Linq;

namespace LyricsTools.UI
{
    /// <summary>
    /// UpdateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private readonly GiteeReleases giteeInfo;
        private readonly string rawJson;

        public UpdateWindow(GiteeReleases newGiteeInfo, string json)
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            giteeInfo = newGiteeInfo;
            rawJson = json;
            UpdateContent.Text = giteeInfo.Body;
            TextOut.Content = $"发布时间:{newGiteeInfo.Created_at} 版本号:{newGiteeInfo.Tag_name}";
            AuthorName.Content = $"{giteeInfo.Author.Name}出品";

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(newGiteeInfo.Author.Avatar_url);
            image.DecodePixelWidth = 248;
            image.EndInit();
            AuthorImage.Source = image;
        }

        private void StateUpdate_Click(object sender, RoutedEventArgs e)
        {
            var assets = JObject.Parse(rawJson).GetValue("assets")[0].ToString();
            string downloadURL = JObject.Parse(assets).GetValue("browser_download_url").ToString();
            System.Diagnostics.Process.Start(downloadURL);
        }        
    }
}
