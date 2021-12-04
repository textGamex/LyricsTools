using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Lyrics;

namespace LyricsTools.UI
{
    /// <summary>
    /// Processing.xaml 的交互逻辑
    /// </summary>
    public partial class Processing : Window
    {
        private LyricsFile lyricsFile;
        private LyricsFile cloneLyricsFile;

        public Processing()
        {
            InitializeComponent();
            //隐藏面板
            SetAllItem(Visibility.Hidden);
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void GetLrcPathButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "歌词文件|*.lrc;*.txt"
            };
            if (dialog.ShowDialog() == true)
            {

                lyricsFile = new LyricsFile(dialog.FileName);
                cloneLyricsFile = (LyricsFile) ((ICloneable)lyricsFile).Clone();
                
                GetLrcPathButton.Visibility = Visibility.Collapsed;
                FunctionSelection.Visibility = Visibility.Visible;

                UpdatePreviewBox(lyricsFile);
            }
        }

        private void UpdatePreviewBox(LyricsFile lyricsFile)
        {
            var stringBulider = new StringBuilder();
            foreach (string s in lyricsFile.GetLrcFileTypeArray())
            {
                stringBulider.Append(s).Append(Environment.NewLine);
            }
            PreviewBox.Text = stringBulider.ToString();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            var label = (Label)FunctionSelection.SelectedItem;
            TimeTag timeTag;
            try
            {
                if (label == RemoveBeforeLabel)
                {
                    timeTag = GetTimeTag(StartMinute.Text, StartSecond.Text, StartMillisecond.Text);
                    lyricsFile.RemoveBefore(timeTag);
                }
                else if (label == RemoveAfterLabel)
                {
                    timeTag = GetTimeTag(EndMinute.Text, EndSecond.Text, EndMillisecond.Text);
                    lyricsFile.RemoveAfter(timeTag);
                }
                else
                {
                    timeTag = GetTimeTag(StartMinute.Text, StartSecond.Text, StartMillisecond.Text);
                    var end = GetTimeTag(EndMinute.Text, EndSecond.Text, EndMillisecond.Text);
                    lyricsFile.InterceptTime(timeTag, end);
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("时间格式错误");
                return;
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("时间大于最大值");
                return;
            }
            UpdatePreviewBox(lyricsFile);
        }

        private TimeTag GetTimeTag(string minuteString, string secondString, string millisecondString)
        {
            uint minute;
            uint second;
            uint millisecond;
            TimeTag timeTag;
            try
            {
                minute = uint.Parse(minuteString);
                second = uint.Parse(secondString);
                millisecond = uint.Parse(millisecondString);
                timeTag = new TimeTag(minute, second, millisecond);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw;
            }
            return timeTag;
        }

        private void FunctionSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartButton.Visibility != Visibility.Visible)
            {
                StartButton.Visibility = Visibility.Visible;
            }
            if (ResetButton.Visibility != Visibility.Visible)
            {
                ResetButton.Visibility = Visibility.Visible;
            }
            SaveButton.Visibility = Visibility.Visible;

            foreach (Label l in e.AddedItems)
            {
                if (l == RemoveBeforeLabel)
                {
                    SetBefore(Visibility.Visible);
                    SetAfter(Visibility.Hidden);
                }
                else if (l == RemoveAfterLabel)
                {
                    SetBefore(Visibility.Hidden);
                    SetAfter(Visibility.Visible);
                }
                else
                {
                    SetAllItem(Visibility.Visible);
                }
            }
        }

        private void SetAllItem(Visibility visibility)
        {
            FunctionSelection.Visibility = visibility;
            StartMinute.Visibility = visibility;
            StartSecond.Visibility = visibility;
            StartMillisecond.Visibility = visibility;
            EndMinute.Visibility = visibility;
            EndSecond.Visibility = visibility;
            EndMillisecond.Visibility = visibility;
            
            Info1.Visibility = visibility;
            Info2.Visibility = visibility;
            Info3.Visibility = visibility;
            Info4.Visibility = visibility;

            StartButton.Visibility = visibility;
            ResetButton.Visibility = visibility;       
            SaveButton.Visibility = visibility;
        }

        private void SetBefore(Visibility visibility)
        {
            StartMinute.Visibility = visibility;
            StartSecond.Visibility = visibility;
            StartMillisecond.Visibility = visibility;
            Info1.Visibility = visibility;
            Info3.Visibility = visibility;
        }

        private void SetAfter(Visibility visibility)
        {
            EndMinute.Visibility = visibility;
            EndSecond.Visibility = visibility;
            EndMillisecond.Visibility = visibility;
            Info2.Visibility = visibility;
            Info4.Visibility = visibility;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            lyricsFile = cloneLyricsFile;
            UpdatePreviewBox(lyricsFile);
            cloneLyricsFile = (LyricsFile) ((ICloneable)lyricsFile).Clone();
        }

        private void NumberPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var UserSelectedPath = new System.Windows.Forms.FolderBrowserDialog()
            {
                Description = "选择保存文件夹"
            };
            var state = UserSelectedPath.ShowDialog();
            if (state == System.Windows.Forms.DialogResult.OK)
            {
                lyricsFile.WriteFileTo(UserSelectedPath.SelectedPath);
                _ = MessageBox.Show("完成");
            }
        }
    }
}
