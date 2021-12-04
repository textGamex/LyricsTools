using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsTools.Lyrics.Bili
{
    public class SubtitleData
    {
        [JsonProperty("font_size")]
        public double FontSize { get; set; } = 0.4;

        [JsonProperty("font_color")]
        public string FontColor { get; set; } = "#FFFFFF";

        [JsonProperty("background_alpha")]
        public double BackgroundAlpha { get; set; } = 0.5;

        [JsonProperty("background_color")]
        public string BackgroundColor { get; set; } = "#9C27B0";

        [JsonProperty("Stroke")]
        public string Stroke { get; set; } = "none";

        /// <summary>
        /// 全部歌词
        /// </summary>
        [JsonProperty("body")]
        public List<SubtitleBody> Body { get; set; }
    }

    public class SubtitleBody
    {
        [JsonProperty("from")]
        public double From { get; set; }

        [JsonProperty("to")]
        public double To { get; set; }

        [JsonProperty("location")]
        public int Location { get; set; } = 2;

        [JsonProperty("content")]
        public string Content { get; set; }

        public override string ToString()
        {
            return $"From={From}, To={To}, Location={Location}, Content={Content}";
        }
    }
}
