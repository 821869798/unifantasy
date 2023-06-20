using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using UniFan;
using UnityEditor;
using UnityEngine;

namespace UniFanEditor
{
    public class VariableDrawer : OdinValueDrawer<Variable>
    {


        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            // 获取属性的引用
            var entry = this.ValueEntry;
            var property = this.Property;
            var variable = entry.SmartValue;

            Color prevColor = GUI.backgroundColor;
            if (variable.editorError)
            {
                GUI.backgroundColor = Color.red;
            }

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            {
                // 绘制名称字段
                EditorGUILayout.LabelField(property.Index.ToString(), EditorBindingUtil.IndexTextStyle, GUILayout.Width(30));
                variable.name = EditorGUILayout.TextField(variable.name, EditorBindingUtil.InputBoldStyle);

                if (variable.variableType == VariableType.Component)
                {
                    int index = 0;
                    List<System.Type> types = new List<System.Type>();
                    var component = variable.editorObjectValue as Component;
                    if (component != null)
                    {
                        GameObject go = component.gameObject;
                        foreach (var c in go.GetComponents<Component>())
                        {
                            if (!types.Contains(c.GetType()))
                                types.Add(c.GetType());
                        }

                        for (int i = 0; i < types.Count; i++)
                        {
                            if (component.GetType().Equals(types[i]))
                            {
                                index = i;
                                break;
                            }
                        }
                    }

                    if (types.Count <= 0)
                        types.Add(typeof(Transform));

                    List<GUIContent> contents = new List<GUIContent>();
                    foreach (var t in types)
                    {
                        contents.Add(new GUIContent(t.Name, t.FullName));
                    }

                    EditorGUI.BeginChangeCheck();
                    var newIndex = EditorGUILayout.Popup(GUIContent.none, index, contents.ToArray(), EditorBindingUtil.ValueTypeStyle, GUILayout.ExpandWidth(true), EditorBindingUtil.MinWidthStyle);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (component != null)
                            variable.editorObjectValue = component.gameObject.GetComponent(types[newIndex]);
                        else
                            variable.editorObjectValue = null;
                    }
                }
                else if (variable.variableType == VariableType.Array)
                {
                    //绘制Array数据
                    var arrayValue = variable.editorVariableArray;
                    if (arrayValue == null)
                    {
                        arrayValue = new VariableArray();
                        variable.editorVariableArray = arrayValue;
                    }
                    var arrayType = arrayValue.editorArrayType;
                    EditorGUI.BeginChangeCheck();
                    var vType = (VariableType)EditorGUILayout.EnumPopup(arrayType, EditorBindingUtil.ValueTypeStyle, GUILayout.MaxWidth(100), EditorBindingUtil.MinWidthStyle);

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (vType != VariableType.Array)
                        {
                            arrayValue.editorArrayType = vType;
                            arrayValue.editorArrayValue = new VariableElement[0];
                        }
                    }
                    if (vType == VariableType.Component)
                    {
                        //绘制组件类型选项，Array是固定的,所以比较特殊需要先选
                        var componentType = arrayValue.componentType;
                        var newComponnetType = (VariableComponentType)EditorGUILayout.EnumPopup(componentType, EditorBindingUtil.ValueTypeStyle, GUILayout.MaxWidth(100), EditorBindingUtil.MinWidthStyle);
                        if (newComponnetType != componentType)
                        {
                            arrayValue.editorComponentType = newComponnetType;
                            arrayValue.EditorUpdateComponntType();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(variable.variableType.ToString(), EditorBindingUtil.ValueTypeConstStyle, GUILayout.ExpandWidth(true), EditorBindingUtil.MinWidthStyle);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorBindingUtil.BindValueStyle);
            if (variable.variableType != VariableType.Array)
            {
                variable.editorVariableArray = null;
            }
            switch (variable.variableType)
            {
                case VariableType.Component:
                    EditorGUI.BeginChangeCheck();
                    variable.editorObjectValue = EditorGUILayout.ObjectField(GUIContent.none, variable.editorObjectValue, typeof(UnityEngine.Component), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (string.IsNullOrEmpty(variable.name) && variable.editorObjectValue != null)
                            variable.name = EditorBindingUtil.NormalizeName(variable.editorObjectValue.name);
                    }
                    break;
                case VariableType.GameObject:
                    EditorGUI.BeginChangeCheck();
                    variable.editorObjectValue = EditorGUILayout.ObjectField(GUIContent.none, variable.editorObjectValue, typeof(UnityEngine.GameObject), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (string.IsNullOrEmpty(variable.name) && variable.editorObjectValue != null)
                            variable.name = EditorBindingUtil.NormalizeName(variable.editorObjectValue.name);
                    }
                    break;
                case VariableType.Object:
                    EditorGUI.BeginChangeCheck();
                    variable.editorObjectValue = EditorGUILayout.ObjectField(GUIContent.none, variable.editorObjectValue, typeof(UnityEngine.Object), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (string.IsNullOrEmpty(variable.name) && variable.editorObjectValue != null)
                            variable.name = EditorBindingUtil.NormalizeName(variable.editorObjectValue.name);
                    }
                    break;
                case VariableType.Color:
                    Color color = DataConverter.ToColor(variable.editorDataValue);
                    EditorGUI.BeginChangeCheck();
                    color = EditorGUILayout.ColorField(GUIContent.none, color);
                    if (EditorGUI.EndChangeCheck())
                    {
                        variable.editorDataValue = DataConverter.GetString(color);
                    }
                    break;
                case VariableType.Vector2:
                    Vector2 vector2 = DataConverter.ToVector2(variable.editorDataValue);
                    EditorGUI.BeginChangeCheck();
                    vector2 = EditorGUILayout.Vector2Field(GUIContent.none, vector2);
                    if (EditorGUI.EndChangeCheck())
                    {
                        variable.editorDataValue = DataConverter.GetString(vector2);
                    }
                    break;
                case VariableType.Vector3:
                    Vector3 vector3 = DataConverter.ToVector3(variable.editorDataValue);
                    EditorGUI.BeginChangeCheck();
                    vector3 = EditorGUILayout.Vector3Field(GUIContent.none, vector3);
                    if (EditorGUI.EndChangeCheck())
                    {
                        variable.editorDataValue = DataConverter.GetString(vector3);
                    }
                    break;
                case VariableType.Vector4:
                    Vector4 vector4 = DataConverter.ToVector4(variable.editorDataValue);
                    EditorGUI.BeginChangeCheck();
                    vector4 = EditorGUILayout.Vector4Field(GUIContent.none, vector4);
                    if (EditorGUI.EndChangeCheck())
                    {
                        variable.editorDataValue = DataConverter.GetString(vector4);
                    }
                    break;
                case VariableType.Boolean:
                    bool b = DataConverter.ToBoolean(variable.editorDataValue);
                    EditorGUI.BeginChangeCheck();
                    b = EditorGUILayout.Toggle(GUIContent.none, b);
                    if (EditorGUI.EndChangeCheck())
                    {
                        variable.editorDataValue = DataConverter.GetString(b);
                    }
                    break;
                case VariableType.Float:
                    float f = DataConverter.ToSingle(variable.editorDataValue);
                    EditorGUI.BeginChangeCheck();
                    f = EditorGUILayout.FloatField(GUIContent.none, f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        variable.editorDataValue = DataConverter.GetString(f);
                    }
                    break;
                case VariableType.Integer:
                    int i = DataConverter.ToInt32(variable.editorDataValue);
                    EditorGUI.BeginChangeCheck();
                    i = EditorGUILayout.IntField(GUIContent.none, i);
                    if (EditorGUI.EndChangeCheck())
                    {
                        variable.editorDataValue = DataConverter.GetString(i);
                    }
                    break;
                case VariableType.String:
                    string s = DataConverter.ToString(variable.editorDataValue);
                    EditorGUI.BeginChangeCheck();
                    s = EditorGUILayout.TextField(GUIContent.none, s);
                    if (EditorGUI.EndChangeCheck())
                    {
                        variable.editorDataValue = DataConverter.GetString(s);
                    }
                    break;
                case VariableType.Array:

                    ValueEntry.Property.Children["_variableArray"].Draw(label);

                    break;
                default:
                    break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();


            GUI.backgroundColor = prevColor;

            // 更新属性的值
            this.ValueEntry.SmartValue = variable;
        }


    }



}
