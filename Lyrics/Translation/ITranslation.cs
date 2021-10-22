using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lyrics.Translation
{
    /// <summary>
    /// 翻译的接口
    /// </summary>
    public interface ITranslation
    {
        /// <summary>
        /// 把<c>标准语言参数</c>翻译成API对应的<c>to</c>参数
        /// </summary>
        /// <param name="standardLanguageParameters">标准语言参数</param>
        /// <returns>API对应的<c>to</c>参数</returns>
        string GetStandardTranslationLanguageParameters(LanguageCode standardLanguageParameters);

        /// <summary>
        /// 得到翻译后的集合
        /// </summary>
        /// <param name="rawDatas">原始数据</param>
        /// <param name="from">原始数据的语言</param>
        /// <param name="targetLanguage">目标语言</param>
        /// <returns>一个翻译完成的数组</returns>
        string[] GetTransResultArray(string[] rawDatas, string from, string targetLanguage);
        //bool VerifyAccount(out int errorCode, out string errorMessage);
    }
}
