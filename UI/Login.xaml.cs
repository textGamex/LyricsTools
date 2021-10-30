using Lyrics;
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
using Lyrics.Translation.Baidu;
using Lyrics.Translation.Youdao;
using Lyrics.Translation;
using System.ComponentModel;

namespace LyricsTools.UI
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public const int NoErrer = 0;
        public Login()
        {
            InitializeComponent();
            if (Properties.Settings.Default.IsAutoFill)
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
            MainWindow mainwindow = new MainWindow(new BaiduTranslationApi(myData.AppId, myData.SecretKey));
            mainwindow.Show();
            Close();
#else
            if (AppId.Text == "" || SecretKey.Text == "")
            {
                _ = MessageBox.Show("请输入APP ID和秘钥!");
                return;
            }

            ITranslation api = null;
            bool isEffectiveAccount = false;
            var result = (Label) ApiOption.SelectedItem;

            if (result == BaiduTranslation)
            {
                api = new BaiduTranslationApi(AppId.Text, SecretKey.Text);        
            }
            if (result == YoudaoTranslation)
            {
                api = new YoudaoTranslateApi(AppId.Text, SecretKey.Text);                                            
            }
            isEffectiveAccount = api.IsCorrectAccount(out int errorCode, out string errorMessage);
            if (isEffectiveAccount)
            {
                if (Properties.Settings.Default.IsAutoFill)
                {
                    SaveUserAccountMessage();
                }
            }
            else if (errorCode != NoErrer)
            {
                MessageBox.Show($"错误码:{errorCode} 错误信息:{errorMessage}", "错误", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MainWindow mainwindow = new MainWindow(api);
            MessageBox.Show("登录成功");
            mainwindow.DebugButton.Visibility = Visibility.Collapsed;
            mainwindow.Show();
            Close();              
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
            Properties.Settings.Default.IsAutoFill = true;
        }

        private void AutoFillMessageCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsAutoFill = false;
        }

        private void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!Properties.Settings.Default.IsAutoFill)
            {
                ClearUserAccountMessage();
            }
            Properties.Settings.Default.Save();
        }

        private void SaveUserAccountMessage()
        {
            Properties.Settings.Default.AppId = AppId.Text;
            Properties.Settings.Default.SecretKey = SecretKey.Text;
        }

        private void ClearUserAccountMessage()
        {
            Properties.Settings.Default.AppId = "";
            Properties.Settings.Default.SecretKey = "";
        }
    }
}