#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.Serialization;
#endif
using UnityEngine;

namespace UniFan
{

    [System.Serializable]
    public enum VariableType
    {
        Object,
        GameObject,
        Component,
        Boolean,
        Integer,
        Float,
        String,
        Color,
        Vector2,
        Vector3,
        Vector4,
        Array,
    }

    [System.Serializable]
    public class Variable
    {
        [SerializeField]
        protected string _name = "";

        [SerializeField]
        protected UnityEngine.Object _objectValue;

        [SerializeField]
        protected string _dataValue;

        [SerializeField]
        protected VariableType _variableType;

#if UNITY_EDITOR
        /// <summary>
        /// 隐藏类选择框
        /// </summary>
        [HideReferenceObjectPicker]
#endif
        [SerializeReference]
        protected VariableArray _variableArray;

        public virtual string name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        public virtual VariableType variableType
        {
            get { return this._variableType; }
        }

        public VariableArray variableArray => _variableArray;

        public virtual void SetValue<T>(T value)
        {
            this.SetValue((object)value);
        }

        public virtual T GetValue<T>()
        {
            return (T)GetValue();
        }

        public virtual void SetValue(object value)
        {
            switch (this._variableType)
            {
                case VariableType.Boolean:
                    this._dataValue = DataConverter.GetString((bool)value);
                    break;
                case VariableType.Float:
                    this._dataValue = DataConverter.GetString((float)value);
                    break;
                case VariableType.Integer:
                    this._dataValue = DataConverter.GetString((int)value);
                    break;
                case VariableType.String:
                    this._dataValue = DataConverter.GetString((string)value);
                    break;
                case VariableType.Color:
                    this._dataValue = DataConverter.GetString((Color)value);
                    break;
                case VariableType.Vector2:
                    this._dataValue = DataConverter.GetString((Vector2)value);
                    break;
                case VariableType.Vector3:
                    this._dataValue = DataConverter.GetString((Vector3)value);
                    break;
                case VariableType.Vector4:
                    this._dataValue = DataConverter.GetString((Vector4)value);
                    break;
                case VariableType.Object:
                    this._objectValue = (UnityEngine.Object)value;
                    break;
                case VariableType.GameObject:
                    this._objectValue = (GameObject)value;
                    break;
                case VariableType.Component:
                    this._objectValue = (Component)value;
                    break;
                case VariableType.Array:
                    _variableArray = (VariableArray)value;
                    break;
                default:
                    throw new System.NotSupportedException();
            }
        }

        public virtual object GetValue()
        {
            switch (this._variableType)
            {
                case VariableType.Boolean:
                    return DataConverter.ToBoolean(this._dataValue);
                case VariableType.Float:
                    return DataConverter.ToSingle(this._dataValue);
                case VariableType.Integer:
                    return DataConverter.ToInt32(this._dataValue);
                case VariableType.String:
                    return DataConverter.ToString(this._dataValue);
                case VariableType.Color:
                    return DataConverter.ToColor(this._dataValue);
                case VariableType.Vector2:
                    return DataConverter.ToVector2(this._dataValue);
                case VariableType.Vector3:
                    return DataConverter.ToVector3(this._dataValue);
                case VariableType.Vector4:
                    return DataConverter.ToVector4(this._dataValue);
                case VariableType.Object:
                    return this._objectValue;
                case VariableType.GameObject:
                    return this._objectValue;
                case VariableType.Component:
                    return this._objectValue;
                case VariableType.Array:
                    return _variableArray;
                default:
                    throw new System.NotSupportedException();
            }
        }

#if UNITY_EDITOR
        public bool editorError { set; get; } = false;

        public void EditorSetVariableType(VariableType valueType)
        {
            this._variableType = valueType;
        }

        public UnityEngine.Object editorObjectValue
        {
            get
            {
                return _objectValue;
            }
            set
            {
                _objectValue = value;
            }
        }

        public string editorDataValue
        {
            get
            {
                return _dataValue;
            }
            set
            {
                _dataValue = value;
            }
        }

        public VariableArray editorVariableArray
        {
            get
            {
                return _variableArray;
            }
            set
            {
                _variableArray = value;
            }
        }

        public bool EditorCheckVariableValid()
        {
            switch (this._variableType)
            {
                case VariableType.Boolean:
                case VariableType.Float:
                case VariableType.Integer:
                case VariableType.String:
                case VariableType.Color:
                case VariableType.Vector2:
                case VariableType.Vector3:
                case VariableType.Vector4:
                    if (string.IsNullOrEmpty(_dataValue))
                    {
                        return false;
                    }
                    break;
                case VariableType.Object:
                    return _objectValue != null;
                case VariableType.GameObject:
                    return _objectValue is GameObject;
                case VariableType.Component:
                    return _objectValue is Component;
                case VariableType.Array:
                    if (_variableArray == null || _variableArray.editorArrayType == VariableType.Array)
                    {
                        return false;
                    }
                    foreach (var ele in _variableArray.editorArrayValue)
                    {
                        if (!ele.EditorCheckVariableElementValid(_variableArray.arrayType, _variableArray.componentType))
                        {
                            return false;
                        }
                    }
                    return true;
                default:
                    throw new System.NotSupportedException();
            }
            return true;
        }
#endif

    }

}