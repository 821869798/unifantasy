
namespace UniFan
{
    public interface IExText
    {
        /// <summary>
        /// 文本id，本地化使用
        /// </summary>
        long tid { get; set; }

        string GetText();
    }
}
