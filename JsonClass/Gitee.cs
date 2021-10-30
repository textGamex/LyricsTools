using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsTools.JsonClass
{
    public class GiteeReleases
    {
        /// <summary>
        /// 版本名称
        /// </summary>
        public string Tag_name { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 更新内容
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime Created_at { get; set; }
        public Author Author { get; set; }
    }

    /// <summary>
    /// 作者信息
    /// </summary>
    public class Author
    {
        public string Name { get; set; }
        /// <summary>
        /// 头像链接
        /// </summary>
        public string Avatar_url { get; set; }
    }
}
