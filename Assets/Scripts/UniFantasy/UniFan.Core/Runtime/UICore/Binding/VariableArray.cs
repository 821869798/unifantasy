#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UniFan
{

    [Serializable]
    public class VariableArray
    {
        [SerializeField]
        protected VariableType _arrayType;

        public VariableType arrayType => _arrayType;

        //[SerializeField]
        //protected VariableComponentType _componentType;

        //public VariableComponentType componentType => _componentType;

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

        //public VariableComponentType editorComponentType
        //{
        //    get
        //    {
        //        return _componentType;
        //    }
        //    set
        //    {
        //        this._componentType = value;
        //    }
        //}

        public Type EditorGetComponentType()
        {
            // 使用第一个非空的元素的类型，否则返回Transform
            foreach (var element in _arrayValue)
            {
                if (element.editorObjectValue != null)
                {
                    return element.editorObjectValue.GetType();
                }
            }
            return typeof(Transform);
        }

        /// <summary>
        /// 获取可选择的组件类型，从第一个非空的元素中获取
        /// </summary>
        /// <returns></returns>
        public List<Type> EditorGetSelectableComponentType(out int selectIndex)
        {
            selectIndex = 0;
            List<Type> selectList = new List<Type>();
            foreach (var element in _arrayValue)
            {
                if (element.editorObjectValue != null && element.editorObjectValue is Component component)
                {
                    var typeSet = new HashSet<Type>();
                    GameObject go = component.gameObject;
                    var components = go.GetComponents<Component>();
                    for (var i = 0; i < components.Length; i++)
                    {
                        var c = components[i];
                        var tc = c.GetType();
                        if (typeSet.Contains(tc))
                        {
                            continue;
                        }
                        typeSet.Add(tc);
                        selectList.Add(tc);
                        if (c == component)
                        {
                            selectIndex = i;
                        }
                    }
                }
            }
            if (selectList.Count == 0)
            {
                selectList.Add(typeof(Transform));
            }
            return selectList;
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

        public void EditorUpdateComponntType(Type type)
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
                element.editorObjectValue = go.GetComponent(type);
            }
        }
#endif

    }
}