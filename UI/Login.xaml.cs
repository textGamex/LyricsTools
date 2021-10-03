using Lyricss;
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
using Lyrics.Baidu;
using Lyrics;
using System.ComponentModel;

namespace LyricsTools.UI
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            if (Properties.Settings.Default.isAutoFill)
            { 
                AutoFillMessageCheckBox.IsChecked = true;
                AppId.Text = Properties.Settings.Default.AppId;
                SecretKey.Text = Properties.Settings.Default.SecretKey;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            MyData myData = new MyData();
            MainWindow mainwindow = new MainWindow(new TranslateApi(myData.AppId, myData.SecretKey));
            mainwindow.Show();
            Close();
#else
            if (AppId.Text == "" || SecretKey.Text == "")
            {
                _ = MessageBox.Show("请输入APP ID和秘钥!");
                return;
            }
            
            TranslateApi api = new TranslateApi(AppId.Text.Trim(), SecretKey.Text.Trim());
            if (api.VerifyAccount(out _, out _))
            {
                if (Properties.Settings.Default.isAutoFill)
                {
                    SaveUserAccountMessage();
                }
                MainWindow mainwindow = new MainWindow(api);
                MessageBox.Show("登录成功");
                mainwindow.DebugButton.Visibility = Visibility.Collapsed;
                mainwindow.Show();
                Close();
            }
            else
            {
                MessageBox.Show("APP ID或秘钥错误!");
            }
#endif
        }

        private void MainGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MessageBoxResult code = MessageBox.Show("确定要退出吗?", "退出", MessageBoxButton.OKCancel);
                if (code == MessageBoxResult.OK)
                {
                    Close();
                }
            }
        }

        private void AutoFillMessageCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.isAutoFill = true;
        }

        private void AutoFillMessageCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.isAutoFill = false;
        }

        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void SaveUserAccountMessage()
        {
            Properties.Settings.Default.AppId = AppId.Text;
            Properties.Settings.Default.SecretKey = SecretKey.Text;
        }
    }
}