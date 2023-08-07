using UniFan;
using UnityEngine;
using UnityEngine.UI;

namespace HotCode.Framework
{
    [RequireComponent(typeof(ExText))]
    public class UITextItemInfo : MonoBehaviour
    {
        private Text _text = null;

        public Text text
        {
            get
            {
                if (_text == null)
                {
                    _text = this.gameObject.GetComponent<Text>();
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
            if (text is ExText exText)
            {
                exText.SetTextSafe(content);
            }
            else
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

    [System.Serializable]
    public class TextItem : IExText
    {
        [SerializeField]
        private long _tid = 0;
        [SerializeField]
        private string _strContent = "";
        [SerializeField]
        private Color _strColor = Color.black;

        public long tid
        {
            get => _tid;
            set => _tid = value;
        }
        //与ExText不同 TextItemInfo 只有在SetIndex时才会拿一次Word
        public string Content
        {
            get
            {
                if (_tid <= 0)
                {
                    return _strContent;
                }

                return _strContent;

                //TODO 本地化
                //var content = LanguageHelper.Instance.QueryWordById(ID);
                //if (content == null)
                //{
                //    content = _strContent;
                //}
                //return content;
            }
        }
        public Color TextColor
        {
            get => _strColor;
            set => _strColor = value;
        }

        public string GetText()
        {
            return _strContent;
        }
    }


}
