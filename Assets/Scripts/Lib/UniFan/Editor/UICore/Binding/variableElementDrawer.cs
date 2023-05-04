using Sirenix.OdinInspector.Editor;
using System;
using UniFan;
using UnityEditor;
using UnityEngine;

namespace UniFanEditor
{
    public class variableElementDrawer : OdinValueDrawer<VariableElement>
    {

        protected override void DrawPropertyLayout(GUIContent label)
        {

            // 获取属性的引用
            var entry = this.ValueEntry;
            var property = this.Property;
            var variableListEle = entry.SmartValue;
            if (variableListEle.editorVaiableArray == null)
            {
                return;
            }
            EditorGUILayout.BeginHorizontal();
            {

                EditorGUILayout.LabelField(property.Index.ToString(), EditorBindingUtil.IndexTextStyle, GUILayout.MaxWidth(30), EditorBindingUtil.MinWidthStyle);

                switch (variableListEle.editorVaiableArray.arrayType)
                {
                    case VariableType.Component:
                        Type type = variableListEle.editorVaiableArray.EditorGetComponentType();
                        EditorGUI.BeginChangeCheck();
                        variableListEle.editorObjectValue = EditorGUILayout.ObjectField(GUIContent.none, variableListEle.editorObjectValue, type, true, EditorBindingUtil.MinWidthStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (string.IsNullOrEmpty(variableListEle.name) && variableListEle.editorObjectValue != null)
                                variableListEle.name = EditorBindingUtil.NormalizeName(variableListEle.editorObjectValue.name);
                        }
                        break;
                    case VariableType.GameObject:
                        EditorGUI.BeginChangeCheck();
                        variableListEle.editorObjectValue = EditorGUILayout.ObjectField(GUIContent.none, variableListEle.editorObjectValue, typeof(UnityEngine.GameObject), true, EditorBindingUtil.MinWidthStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (string.IsNullOrEmpty(variableListEle.name) && variableListEle.editorObjectValue != null)
                                variableListEle.name = EditorBindingUtil.NormalizeName(variableListEle.editorObjectValue.name);
                        }
                        break;
                    case VariableType.Object:
                        EditorGUI.BeginChangeCheck();
                        variableListEle.editorObjectValue = EditorGUILayout.ObjectField(GUIContent.none, variableListEle.editorObjectValue, typeof(UnityEngine.Object), true, EditorBindingUtil.MinWidthStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (string.IsNullOrEmpty(variableListEle.name) && variableListEle.editorObjectValue != null)
                                variableListEle.name = EditorBindingUtil.NormalizeName(variableListEle.editorObjectValue.name);
                        }
                        break;
                    case VariableType.Color:
                        Color color = DataConverter.ToColor(variableListEle.editorDataValue);
                        EditorGUI.BeginChangeCheck();
                        color = EditorGUILayout.ColorField(GUIContent.none, color, EditorBindingUtil.MinWidthStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            variableListEle.editorDataValue = DataConverter.GetString(color);
                        }
                        break;
                    case VariableType.Vector2:
                        Vector2 vector2 = DataConverter.ToVector2(variableListEle.editorDataValue);
                        EditorGUI.BeginChangeCheck();
                        vector2 = EditorGUILayout.Vector2Field(GUIContent.none, vector2, EditorBindingUtil.MinWidthStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            variableListEle.editorDataValue = DataConverter.GetString(vector2);
                        }
                        break;
                    case VariableType.Vector3:
                        Vector3 vector3 = DataConverter.ToVector3(variableListEle.editorDataValue);
                        EditorGUI.BeginChangeCheck();
                        vector3 = EditorGUILayout.Vector3Field(GUIContent.none, vector3, EditorBindingUtil.MinWidthStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            variableListEle.editorDataValue = DataConverter.GetString(vector3);
                        }
                        break;
                    case VariableType.Vector4:
                        Vector4 vector4 = DataConverter.ToVector4(variableListEle.editorDataValue);
                        EditorGUI.BeginChangeCheck();
                        vector4 = EditorGUILayout.Vector4Field(GUIContent.none, vector4, EditorBindingUtil.MinWidthStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            variableListEle.editorDataValue = DataConverter.GetString(vector4);
                        }
                        break;
                    case VariableType.Boolean:
                        bool b = DataConverter.ToBoolean(variableListEle.editorDataValue);
                        EditorGUI.BeginChangeCheck();
                        b = EditorGUILayout.Toggle(GUIContent.none, b, EditorBindingUtil.MinWidthStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            variableListEle.editorDataValue = DataConverter.GetString(b);
                        }
                        break;
                    case VariableType.Float:
                        float f = DataConverter.ToSingle(variableListEle.editorDataValue);
                        EditorGUI.BeginChangeCheck();
                        f = EditorGUILayout.FloatField(GUIContent.none, f, EditorBindingUtil.MinWidthStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            variableListEle.editorDataValue = DataConverter.GetString(f);
                        }
                        break;
                    case VariableType.Integer:
                        int i = DataConverter.ToInt32(variableListEle.editorDataValue);
                        EditorGUI.BeginChangeCheck();
                        i = EditorGUILayout.IntField(GUIContent.none, i, EditorBindingUtil.MinWidthStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            variableListEle.editorDataValue = DataConverter.GetString(i);
                        }
                        break;
                    case VariableType.String:
                        string s = DataConverter.ToString(variableListEle.editorDataValue);
                        EditorGUI.BeginChangeCheck();
                        s = EditorGUILayout.TextField(GUIContent.none, s, EditorBindingUtil.MinWidthStyle);
                        if (EditorGUI.EndChangeCheck())
                        {
                            variableListEle.editorDataValue = DataConverter.GetString(s);
                        }
                        break;

                    default:
                        break;
                }
            }
            EditorGUILayout.EndHorizontal();

            this.ValueEntry.SmartValue = variableListEle;
        }

    }
}
