using System;
using UnityEngine;

namespace UniFan
{
    [System.Serializable]
    public class VariableElement
    {
        [SerializeField]
        protected string _name;

        [SerializeField]
        protected UnityEngine.Object _objectValue;

        [SerializeField]
        protected string _dataValue;


        public virtual string name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        public object GetValue(VariableType variableType)
        {
            switch (variableType)
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
                    throw new System.NotSupportedException();
                default:
                    throw new System.NotSupportedException();
            }
        }


#if UNITY_EDITOR

        [NonSerialized]
        public VariableArray editorVaiableArray;

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

        public bool EditorCheckVariableElementValid(VariableType arrayType)
        {
            switch (arrayType)
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
                    return false;
                default:
                    throw new System.NotSupportedException();
            }
            return true;
        }
#endif
    }
}