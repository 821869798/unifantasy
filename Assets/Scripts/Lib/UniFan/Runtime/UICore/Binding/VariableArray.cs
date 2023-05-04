using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace UniFan
{
    public enum VariableComponentType
    {
        Transform = 0,
        RectTransform,
        ExButton,
        ExText,
        ExImage,
        ExRawImage,
        ExToggle,
        ExSlider,
        Canvas,
    }


    [Serializable]
    public class VariableArray
    {
        [SerializeField]
        protected VariableType _arrayType;

        public VariableType arrayType => _arrayType;

        [SerializeField]
        protected VariableComponentType _componentType;

        public VariableComponentType componentType => _componentType;

#if UNITY_EDITOR
        [ListDrawerSettings()]
        [LabelText("Array")]
#endif
        [SerializeField]
        protected VariableElement[] _arrayValue = new VariableElement[0];

        public VariableElement[] ArrayValue => _arrayValue;

        public virtual int ArrayValueCount => ArrayValue.Length;

        public virtual T GetValue<T>(int index)
        {
            return (T)GetValue(index);
        }

        public virtual object GetValue(int index)
        {
            if (index < 0 || index >= _arrayValue.Length)
            {
                return null;
            }
            return _arrayValue[index].GetValue(arrayType);
        }


#if UNITY_EDITOR
        /// <summary>
        /// 给List Inspector添加自定义
        /// </summary>
        //private void DrawTitleBarGUI()
        //{
        //    if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
        //    {
        //        var array = new VariableElement[this._arrayValue.Length + 1];
        //        array[array.Length - 1] = new VariableElement();
        //        System.Array.Copy(this._arrayValue, array, this._arrayValue.Length);

        //        this._arrayValue = array;
        //    }
        //}

        public VariableType editorArrayType
        {
            get
            {
                return this._arrayType;
            }
            set
            {
                this._arrayType = value;
            }
        }

        public VariableComponentType editorComponentType
        {
            get
            {
                return _componentType;
            }
            set
            {
                this._componentType = value;
            }
        }

        public Type EditorGetComponentType()
        {
            switch (componentType)
            {
                case VariableComponentType.Transform:
                    return typeof(Transform);
                case VariableComponentType.RectTransform:
                    return typeof(RectTransform);
                case VariableComponentType.ExButton:
                    return typeof(ExButton);
                case VariableComponentType.ExText:
                    return typeof(ExText);
                case VariableComponentType.ExImage:
                    return typeof(ExImage);
                case VariableComponentType.ExRawImage:
                    return typeof(ExRawImage);
                case VariableComponentType.ExToggle:
                    return typeof(ExToggle);
                case VariableComponentType.ExSlider:
                    return typeof(ExSlider);
                case VariableComponentType.Canvas:
                    return typeof(Canvas);
                default:
                    throw new NotImplementedException();
            }
        }

        public VariableElement[] editorArrayValue
        {
            get
            {
                return _arrayValue;
            }
            set
            {
                this._arrayValue = value;
            }
        }

        public void EditorUpdateComponntType()
        {
            if (this.arrayType != VariableType.Component)
            {
                return;
            }
            foreach (VariableElement element in _arrayValue)
            {
                var component = element.editorObjectValue as Component;
                GameObject go = null;
                if (component != null)
                {
                    go = component.gameObject;
                }
                if (go == null)
                {
                    element.editorObjectValue = null;
                    continue;
                }
                element.editorObjectValue = go.GetComponent(this.componentType.ToString());
            }
        }
#endif

    }
}