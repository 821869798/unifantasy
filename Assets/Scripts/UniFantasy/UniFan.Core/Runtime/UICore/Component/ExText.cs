using UnityEngine;
using UnityEngine.UI;

namespace UniFan
{
    public class ExText : Text, IExText
    {
        [SerializeField]
        private long _tid;
        public long tid { set; get; }

        public string GetText()
        {
            return text;
        }

        /// <summary>
        /// 安全的设置内容
        /// </summary>
        /// <param name="content"></param>
        public void SetTextSafe(string content)
        {
            text = content;
        }

    }

}
