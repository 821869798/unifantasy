using TMPro;
using UnityEngine;

namespace HotCode.Framework
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UITMPItemInfo : MonoBehaviour
    {
        private TextMeshProUGUI _text = null;

        public TextMeshProUGUI text
        {
            get
            {
                if (_text == null)
                {
                    _text = this.gameObject.GetComponent<TextMeshProUGUI>();
#if UNITY_EDITOR
                    initedText = true;
#endif
                }
                return _text;
            }
        }
        public TextItem[] dataArray;
        public bool isHasImage;
        public bool isHasDiffColor;
        public UIImageItemInfo imageItemInfo;

        public void SetIndex(int index)
        {
            var content = GetIndex(index);
            SetText(content);
            SetColor(GetTextColorByIndex(index));
            if (isHasImage && imageItemInfo != null)
            {
                imageItemInfo.SetIndex(index);
            }
        }
        public void SetIndex(int index, params string[] strFormat)
        {
            var content = GetIndex(index, strFormat);
            SetText(content);
            SetColor(GetTextColorByIndex(index));
            if (isHasImage && imageItemInfo != null)
            {
                imageItemInfo.SetIndex(index);
            }
        }

        public void SetIndex(int index, int arg)
        {
            SetIndex(index, arg.ToString());
        }

        public string GetIndex(int index)
        {
            string content = null;
            if (dataArray != null && dataArray.Length > index && index >= 0)
            {
                var data = dataArray[index];
                content = data.Content;
            }
            else
            {
                Debug.LogWarning($"{this.gameObject}:当前数组为空或者索引{index}越界，请检查");
            }

            return content;
        }

        public string GetIndex(int index, params string[] strFormat)
        {
            var content = GetIndex(index);
            return string.Format(content, strFormat);
        }

        private Color? GetTextColorByIndex(int index)
        {
            if (dataArray != null && dataArray.Length > index && index >= 0)
            {
                var data = dataArray[index];
                return data.TextColor;
            }
            else
            {
                return null;
            }
        }

        public void SetText(string content)
        {
            //if (text is ExText exText)
            //{
            //    exText.SetTextSafe(content);
            //}
            //else
            {
                text.text = content;
            }

#if UNITY_EDITOR
            initedText = true;
#endif
        }

        public void SetColor(Color? textColor)
        {
            if (isHasDiffColor && textColor != null)
            {
                text.color = (Color)textColor;
            }
        }

        public void ClearText()
        {
            text.text = string.Empty;
        }

#if UNITY_EDITOR
        bool initedText = false;

        private void Awake()
        {
            //清除文本，防止做功能的时候，有人没有调用SetIndex
            if (!initedText)
                ClearText();
        }
#endif
    }

}

