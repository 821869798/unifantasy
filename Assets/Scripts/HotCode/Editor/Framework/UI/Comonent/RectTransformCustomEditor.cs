using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HotCode.Framework
{
    [CustomEditor(typeof(RectTransform), true)]
    public class RectTransformCustomEditor : UnityEditor.Editor
    {
        private GUIStyle guiStyle = new GUIStyle();

        private UnityEditor.Editor _editorInstance;

        private static Vector3 copiedPosition;
        private static Vector2 copiedSizeDelta;

        private void OnEnable()
        {
            if (serializedObject == null)
            {
                return;
            }

            guiStyle.normal.textColor = Color.white;
            guiStyle.fontSize = 12;
            guiStyle.alignment = TextAnchor.MiddleCenter;

            Assembly ass = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            Type rtEditor = ass.GetType("UnityEditor.RectTransformEditor");
            _editorInstance = CreateEditor(target, rtEditor);

            // 调用OnEnable
            //MethodInfo OnEnable_Method = _editorInstance.GetType().GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
            //OnEnable_Method.Invoke(_editorInstance, null);
        }

        private void OnDisable()
        {
            // 调用_editorInstance的OnDisable
            if (_editorInstance == null)
            {
                return;
            }
            MethodInfo OnDisable_Method = _editorInstance.GetType().GetMethod("OnDisable", BindingFlags.NonPublic | BindingFlags.Instance);
            OnDisable_Method.Invoke(_editorInstance, null);

            // Destory Editor 
            UnityEngine.Object.DestroyImmediate(_editorInstance);
            _editorInstance = null;
        }

        public override void OnInspectorGUI()
        {
            if (_editorInstance == null)
            {
                return;
            }

            _editorInstance.OnInspectorGUI();

            // 自定义部分
            var rectTransform = (RectTransform)target;

            EditorGUILayout.Space();
            GUILayout.BeginVertical("GroupBox");
            {
                GUILayout.Box("快捷工具栏", guiStyle);

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Copy Component Values"))
                    {
                        UnityEditorInternal.ComponentUtility.CopyComponent(rectTransform);
                    }

                    if (GUILayout.Button("Paste Component Values"))
                    {
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(rectTransform);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Copy Position"))
                    {
                        copiedPosition = rectTransform.anchoredPosition3D;
                    }
                    if (GUILayout.Button("Paste Position"))
                    {
                        Undo.RecordObject(target, "Paste Position");
                        rectTransform.anchoredPosition3D = copiedPosition;
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Copy SizeDelta"))
                    {
                        copiedSizeDelta = rectTransform.sizeDelta;
                    }

                    if (GUILayout.Button("Paste SizeDelta"))
                    {
                        Undo.RecordObject(target, "Paste Size");
                        rectTransform.sizeDelta = copiedSizeDelta;
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Reset Scale"))
                    {
                        Undo.RecordObject(target, "Reset Scale");
                        rectTransform.localScale = Vector3.one;
                    }
                    if (GUILayout.Button("Reset Rotation"))
                    {
                        Undo.RecordObject(target, "Reset Rotation");
                        rectTransform.localRotation = Quaternion.identity;
                    }
                }
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();


            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (_editorInstance == null)
            {
                return;
            }

            MethodInfo onSceneGUI_Method = _editorInstance.GetType().GetMethod("OnSceneGUI", BindingFlags.NonPublic | BindingFlags.Instance);
            onSceneGUI_Method.Invoke(_editorInstance, null);
        }
    }
}

