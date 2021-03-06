using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyrics.Translation
{
    /// <summary>
    /// 最基础的翻译接口
    /// </summary>
    public interface ITranslation
    {
        /// <summary>
        /// 把<c>统一语言枚举</c>翻译成API对应的<c>to</c>字符串
        /// </summary>
        /// <param name="standardLanguageParameters">统一语言枚举</param>
        /// <returns>API对应的<c>to</c>参数</returns>
        string GetStandardTranslationLanguageParameters(UnifiedLanguageCode standardLanguageParameters);

        /// <summary>
        /// 检测Id和秘钥是否正确
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>如果正确, 返回true, 否则返回false</returns>
        bool IsCorrectAccount(out int errorCode, out string errorMessage);

        /// <summary>
        /// 翻译字符串, 如果要一次性翻译多个字符串, 用换行符分隔每个字符串.
        /// </summary>
        /// <param name="query">要翻译的字符串</param>
        /// <param name="from">字符串的语言</param>
        /// <param name="to">目标语言</param>
        /// <returns>一个key为原始字符串, value为翻译后字符串的Dictionary</returns>
        Dictionary<string, string> GetTransResultAndRawDataMap(string query, string from, string to);
    }
}
