﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Lyrics.Translation.Baidu;
using Lyrics.Translation;
using Microsoft.Win32;
using System.Windows.Forms;
using static LyricsTools.Tools.Debug;

namespace Lyrics
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private LyricsFile lyricsFile;
        private readonly ITranslation api;
        private LanguageFlags languageCode;
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
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
                    FolderBrowserDialog UserSelectedPath = new FolderBrowserDialog()
                    {
                        Description = "选择保存文件夹"
                    };
                    var state = UserSelectedPath.ShowDialog();
                    if (state == System.Windows.Forms.DialogResult.OK)
                    {
                        StartTranslate(UserSelectedPath);
                        _ = System.Windows.MessageBox.Show("完成");
                    }
                }
                else
                {
                    _ = System.Windows.MessageBox.Show("请选择要翻译到哪种语言");
                }
            }                       
        }

        /// <summary>
        /// 开始翻译, 并把开始按钮设为隐藏, 翻译完成后重新显示
        /// </summary>
        /// <param name="SaveFolderPath">翻译好的歌词要保存到的文件夹路径</param>
        private void StartTranslate(FolderBrowserDialog SaveFolderPath)
        {
            IsNotNull(SaveFolderPath);
            getLrcPathButton.Visibility = Visibility.Hidden;

            uint totalNumber = GetChoicesNumber();
            double completedNumber = 0;
            UnifiedLanguageCode[] languages = GetUnifiedLanguageCode(languageCode);

            foreach (var language in languages)
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

        private UnifiedLanguageCode[] GetUnifiedLanguageCode(LanguageFlags language)
        {
            var list = new List<UnifiedLanguageCode>(8);
            if (language.HasFlag(LanguageFlags.EN))
            {
                list.Add(UnifiedLanguageCode.English);
            }
            if (language.HasFlag(LanguageFlags.ZH))
            {
                list.Add(UnifiedLanguageCode.Chinese);
            }
            if (language.HasFlag(LanguageFlags.JP))
            {
                list.Add(UnifiedLanguageCode.Japanese);
            }
            if (language.HasFlag(LanguageFlags.DE))
            {
                list.Add(UnifiedLanguageCode.German);
            }
            if (language.HasFlag(LanguageFlags.CHT))
            {
                list.Add(UnifiedLanguageCode.TraditionalChinese);
            }
            if (language.HasFlag(LanguageFlags.SPA))
            {
                list.Add(UnifiedLanguageCode.Spanish);
            }
            if (language.HasFlag(LanguageFlags.RU))
            {
                list.Add(UnifiedLanguageCode.Russian);
            }
            if (language.HasFlag(LanguageFlags.FRA))
            {
                list.Add(UnifiedLanguageCode.French);
            }
            return list.ToArray();
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
            systenMessage.Content = languageCode.ToString();
        }
        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

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
            languageCode |= LanguageFlags.EN;
        }

        private void EnglishSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode &= ~LanguageFlags.EN;
        }

        private void JapaneseSwitch_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageFlags.JP;
        }

        private void JapaneseSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode &= ~LanguageFlags.JP;
        }
       
        private void ChineseSwich_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageFlags.ZH;
        }

        private void ChineseSwich_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode &= ~LanguageFlags.ZH;
        }

        private void TraditionalChineseSwich_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageFlags.CHT;
        }

        private void TraditionalChineseSwich_UnChecked(object sender, RoutedEventArgs e)
        {
            languageCode &= ~LanguageFlags.CHT;
        }               

        private void GermanSwitch_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageFlags.DE;
        }

        private void GermanSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode &= ~LanguageFlags.DE;
        }

        private void RussianSwitch_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageFlags.RU;
        }

        private void RussianSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode &= ~LanguageFlags.RU;
        }

        private void FrenchSwitch_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageFlags.FRA;
        }

        private void FrenchSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode &= ~LanguageFlags.FRA;
        }
       

        private void SpanishSwitch_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageFlags.SPA;
        }

        private void SpanishSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode &= ~LanguageFlags.SPA;
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

        private void ProjectUrl_Click(object sender, RoutedEventArgs e)
        {
            _ = System.Diagnostics.Process.Start("https://github.com/textGamex/LyricsTools");
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
    }
}
