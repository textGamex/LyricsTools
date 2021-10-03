using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Lyrics.Baidu;
using Lyrics;
using Microsoft.Win32;
using System.Windows.Forms;

namespace Lyricss
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<string> rawData = new List<string>(64);
//#if DEBUG
        private readonly TranslateApi api;
//#endif
        private LanguageCode languageCode;
        private StateCode stateCode = StateCode.NONE;

        public MainWindow(TranslateApi newApi)
        {
            api = newApi;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "歌词文件|*.lrc;*.txt"
            };
            if (stateCode == StateCode.NONE)
            {
                if (dialog.ShowDialog() == true)
                {
                    FileStream stream = new FileStream(dialog.FileName, FileMode.Open);
                    using (StreamReader file = new StreamReader(stream))
                    {
                        while (!file.EndOfStream)
                        {
                            rawData.Add(file.ReadLine());
                        }
                    }
                    stream.Close();

                    foreach (string s in rawData)
                    {
                        textOut.Text += s + Environment.NewLine;
                    }
                }
                getLrcPathButton.Content = "再次点击开始翻译";
                stateCode = StateCode.USER_HAS_SELECTED_LYRIC_FILES;
            }
            else if (stateCode == StateCode.USER_HAS_SELECTED_LYRIC_FILES && GetChoicesNumber() != 0)
            {
                FolderBrowserDialog fileSave = new FolderBrowserDialog()
                {
                    Description = "保存文件夹"
                };
                if (fileSave.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    uint totalNumber = GetChoicesNumber();
                    double completedNumber = 0;
                    string[] languages = languageCode.ToString().Split(',');
                    string[] rawDataAyyar = rawData.ToArray();
                    foreach (string language in languages)
                    {
                        Console.WriteLine(language);
                        FileStream fileStream = new FileStream(fileSave.SelectedPath + $@"\{language}.lrc", FileMode.Create);
                        string[] data = api.GetTransResultArray(rawDataAyyar, language.Trim());                        
                        using (StreamWriter file = new StreamWriter(fileStream))
                        {
                            foreach (string s in data)
                            {
                                file.WriteLine(s);
                            }
                        }
                        
                        ++completedNumber;
                        System.Diagnostics.Debug.Assert(totalNumber > 0);
                        double value = completedNumber / totalNumber * 100;
                        translationProgressBar.Dispatcher.Invoke(new Action<DependencyProperty, object>(translationProgressBar.SetValue), System.Windows.Threading.DispatcherPriority.Input, System.Windows.Controls.ProgressBar.ValueProperty, value);
                        systenMessage.Content = $"{language}翻译完成";

                        fileStream.Close();
                        System.Threading.Thread.Sleep(1200);
                    }                   
                }                
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("未选择语言!");
            }
            
        }

        private uint GetChoicesNumber()
        {
            uint count = 0;
            var list = languageComboBox.Items;
            foreach (System.Windows.Controls.CheckBox item in list)
            {
                if (item.IsChecked ?? false)
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

#region 获得翻译语言

        private void EnglishSwitch_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageCode.EN;
        }

        private void EnglishSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode ^= LanguageCode.EN;
        }

        private void JapaneseSwitch_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageCode.JP;
        }

        private void JapaneseSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode ^= LanguageCode.JP;
        }
       
        private void ChineseSwich_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageCode.ZH;
        }

        private void ChineseSwich_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode ^= LanguageCode.ZH;
        }

        private void TraditionalChineseSwich_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageCode.CHT;
        }

        private void TraditionalChineseSwich_UnChecked(object sender, RoutedEventArgs e)
        {
            languageCode ^= LanguageCode.CHT;
        }               

        private void GermanSwitch_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageCode.DE;
        }

        private void GermanSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode ^= LanguageCode.DE;
        }

        private void RussianSwitch_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageCode.RU;
        }

        private void RussianSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode ^= LanguageCode.RU;
        }

        private void FrenchSwitch_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageCode.FRA;
        }

        private void FrenchSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode ^= LanguageCode.FRA;
        }
       

        private void SpanishSwitch_Checked(object sender, RoutedEventArgs e)
        {
            languageCode |= LanguageCode.SPA;
        }

        private void SpanishSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            languageCode ^= LanguageCode.SPA;
        }

#endregion

        private void ProjectUrl_Click(object sender, RoutedEventArgs e)
        {
            _ = System.Diagnostics.Process.Start("https://github.com/textGamex/LyricsTools");
        }
    }
}
