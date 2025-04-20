using UnityEngine;
using UnityEngine.UI;

namespace HotCode.Framework
{
    [RequireComponent(typeof(Image))]
    public class UIImageItemInfo : MonoBehaviour
    {
        private Image _image;

        public Image image
        {
            get
            {
                if (_image == null)
                {
                    _image = this.GetComponent<Image>();
                }
                return _image;
            }
        }

        public ImageItem[] dataArray;

        public bool isHasDiffColor;

        public void SetIndex(int index)
        {
            Sprite tex = null;
            if (dataArray != null && dataArray.Length > index && index >= 0)
            {
                var data = dataArray[index];
                tex = data.icon;
                if (isHasDiffColor)
                {
                    image.color = data.SpriteColor;
                }
            }
            else
            {
                Debug.LogWarning($"{this.gameObject}:当前数组为空或者索引({index})越界，请检查");
            }
            image.sprite = tex;
        }

        public Sprite GetIndex(int index)
        {
            Sprite tex = null;
            if (dataArray != null && dataArray.Length > index && index >= 0)
            {
                var data = dataArray[index];
                tex = data.icon;
            }
            else
            {
                Debug.LogWarning($"{this.gameObject}:当前数组为空或者索引({index})越界，请检查");
            }

            return tex;
        }

        public Color GetIndexColor(int index)
        {
            Color color = image.color;
            if (dataArray != null && dataArray.Length > index && index >= 0)
            {
                var data = dataArray[index];
                color = data.SpriteColor;
            }
            else
            {
                Debug.LogWarning($"{this.gameObject}:当前数组为空或者索引({index})越界，请检查");
            }
            return color;
        }
    }

    [System.Serializable]
    public class ImageItem
    {
        [SerializeField]
        protected Sprite _icon = null;

        [SerializeField]
        private Color _spriteColor = Color.white;

        public Sprite icon
        {
            get => _icon;
            set => _icon = value;
        }

        public Color SpriteColor
        {
            get => _spriteColor;
            set => _spriteColor = value;
        }

        ImageItem(Sprite Icon)
        {
            _icon = Icon;
        }

        ImageItem() : this(null)
        {
        }
    }


}
