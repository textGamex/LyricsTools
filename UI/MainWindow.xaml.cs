using System;
using LyricsTools.UI;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Lyrics.Translation.Baidu;
using Lyrics.Translation;
using Microsoft.Win32;
using static LyricsTools.Tools.Debug;
using System.Net;
using Newtonsoft.Json.Linq;
using LyricsTools.JsonClass;
using Newtonsoft.Json;

namespace Lyrics
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {       
        private LyricsFile lyricsFile;
        private readonly ITranslation api;
        private readonly List<UnifiedLanguageCode> translationToLanguage = new List<UnifiedLanguageCode>(GetEnumSize());
        private StateCode stateCode = StateCode.NONE;
        
        public MainWindow(ITranslation newApi)
        {
            if (newApi == null)
            {
                System.Windows.Forms.MessageBox.Show("未知翻译API");
                Close();
            }
                
            api = newApi;
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "歌词文件|*.lrc;*.txt"
            };
            bool? ops = null;
            if (stateCode == StateCode.NONE)
                ops = dialog.ShowDialog();
            if (stateCode == StateCode.NONE)
            {
                if (ops == null)
                    ops = dialog.ShowDialog();
                if (ops == true)
                {
                    lyricsFile = new LyricsFile(dialog.FileName);
                    var outText = new StringBuilder();
                    foreach (string line in lyricsFile.GetLrcFileTypeArray())
                    {
                        outText.Append(line).Append(Environment.NewLine);
                    }
                    textOut.Text = outText.ToString();
                    getLrcPathButton.Content = "再次点击开始翻译";
                    stateCode = StateCode.USER_HAS_SELECTED_LYRIC_FILES;
                    return;
                }                                
            }

            //TODO 做成两个按钮, 选择歌词文件和保存翻译后歌词文件分开
            if (stateCode == StateCode.USER_HAS_SELECTED_LYRIC_FILES)
            {
                if (GetChoicesNumber() != 0)
                {
                    System.Windows.Forms.FolderBrowserDialog UserSelectedPath = new System.Windows.Forms.FolderBrowserDialog()
                    {
                        Description = "选择保存文件夹"
                    };
                    var state = UserSelectedPath.ShowDialog();
                    if (state == System.Windows.Forms.DialogResult.OK)
                    {
                        StartTranslate(UserSelectedPath);
                        _ = MessageBox.Show("完成");
                    }
                }
                else
                {
                    _ = MessageBox.Show("请选择要翻译到哪种语言");
                }
            }                       
        }

        /// <summary>
        /// 开始翻译, 并把开始按钮设为隐藏, 翻译完成后重新显示
        /// </summary>
        /// <param name="SaveFolderPath">翻译好的歌词要保存到的文件夹路径</param>
        private void StartTranslate(System.Windows.Forms.FolderBrowserDialog SaveFolderPath)
        {
            IsNotNull(SaveFolderPath);
            getLrcPathButton.Visibility = Visibility.Hidden;

            uint totalNumber = GetChoicesNumber();
            double completedNumber = 0;

            foreach (var language in translationToLanguage)
            {
                var newLyrics = lyricsFile.TranslateTo(api, language);
                newLyrics.WriteFileTo(SaveFolderPath.SelectedPath);

                ++completedNumber;
                System.Diagnostics.Debug.Assert(totalNumber > 0);
                double value = completedNumber / totalNumber * 100;
                UpdateProgressBar(value);
                systenMessage.Content = $"{language}翻译完成";

                System.Threading.Thread.Sleep(1100);
            }
            getLrcPathButton.Visibility = Visibility.Hidden;
        }
       
        /// <summary>
        /// 刷新进度条
        /// </summary>
        /// <param name="percentComplete">完成百分比</param>
        private void UpdateProgressBar(double percentComplete)
        {                       
            translationProgressBar.Dispatcher.Invoke(new Action<DependencyProperty, object>
                (translationProgressBar.SetValue), System.Windows.Threading.DispatcherPriority.Input,
                System.Windows.Controls.ProgressBar.ValueProperty, percentComplete);            
        }

        /// <summary>
        /// 用户选中的语言数量
        /// </summary>
        /// <returns>用户选中的语言数量</returns>
        private uint GetChoicesNumber()
        {
            uint count = 0;
            var list = languageComboBox.Items;
            foreach (System.Windows.Controls.CheckBox item in list)
            {
                if ((item.IsChecked ?? false) && item != SelectAllSwitch)
                {
                    ++count;
                }
            }
            return count;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            systenMessage.Content = translationToLanguage.ToString();
        }
        #region 获得需要翻译的语言

        private void SelectAllSwitch_Checked(object sender, RoutedEventArgs e)
        {
            EnglishSwitch_Checked(sender, e);
            JapaneseSwitch_Checked(sender, e);
            ChineseSwich_Checked(sender, e);
            TraditionalChineseSwich_Checked(sender, e);
            GermanSwitch_Checked(sender, e);
            FrenchSwitch_Checked(sender, e);
            RussianSwitch_Checked(sender, e);
            SpanishSwitch_Checked(sender, e);
            CheckedAllLanguageCheck();
        }   

        private void SelectAllSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            EnglishSwitch_Unchecked(sender, e);
            JapaneseSwitch_Unchecked(sender, e);
            ChineseSwich_Unchecked(sender, e);
            TraditionalChineseSwich_UnChecked(sender, e);
            GermanSwitch_Unchecked(sender, e);
            FrenchSwitch_Unchecked(sender, e);
            RussianSwitch_Unchecked(sender, e);
            SpanishSwitch_Unchecked(sender, e);
            UnCheckedAllLanguageCheck();
        }

        private void EnglishSwitch_Checked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Add(UnifiedLanguageCode.English);
        }

        private void EnglishSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Remove(UnifiedLanguageCode.English);
        }

        private void JapaneseSwitch_Checked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Add(UnifiedLanguageCode.Japanese);
        }

        private void JapaneseSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Remove(UnifiedLanguageCode.Japanese);
        }

        private void ChineseSwich_Checked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Add(UnifiedLanguageCode.Chinese);
        }

        private void ChineseSwich_Unchecked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Remove(UnifiedLanguageCode.Chinese);
        }

        private void TraditionalChineseSwich_Checked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Add(UnifiedLanguageCode.TraditionalChinese);
        }

        private void TraditionalChineseSwich_UnChecked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Remove(UnifiedLanguageCode.TraditionalChinese);
        }

        private void GermanSwitch_Checked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Add(UnifiedLanguageCode.German);
        }

        private void GermanSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Remove(UnifiedLanguageCode.German);
        }

        private void RussianSwitch_Checked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Add(UnifiedLanguageCode.Russian);
        }

        private void RussianSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Remove(UnifiedLanguageCode.Russian);
        }

        private void FrenchSwitch_Checked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Add(UnifiedLanguageCode.French);
        }

        private void FrenchSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Remove(UnifiedLanguageCode.French);
        }


        private void SpanishSwitch_Checked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Add(UnifiedLanguageCode.Spanish);
        }

        private void SpanishSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            translationToLanguage.Remove(UnifiedLanguageCode.Spanish);
        }

        private void CheckedAllLanguageCheck()
        {
            var list = languageComboBox.Items;
            foreach (System.Windows.Controls.CheckBox item in list)
            {
                item.IsChecked = true;
            }
        }

        private void UnCheckedAllLanguageCheck()
        {
            var list = languageComboBox.Items;
            foreach (System.Windows.Controls.CheckBox item in list)
            {
                item.IsChecked = false;
            }
        }

        #endregion    
        
        private static int GetEnumSize()
        {
            return Enum.GetNames(new UnifiedLanguageCode().GetType()).Length;
        }
    }
}
