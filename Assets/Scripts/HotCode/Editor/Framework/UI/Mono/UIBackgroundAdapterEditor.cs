using HotCode.Framework;
using UnityEditor;
using UnityEngine;

namespace HotCode.FrameworkEditor
{
    [CustomEditor(typeof(UIBackgroundAdapter))]
    public class UIBackgroundAdapterEditor : UnityEditor.Editor
    {
        SerializedProperty m_onlyWidth;
        SerializedProperty m_halfScreenWidth;

        void OnEnable()
        {
            m_onlyWidth = serializedObject.FindProperty(nameof(m_onlyWidth));
            m_halfScreenWidth = serializedObject.FindProperty(nameof(m_halfScreenWidth));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var targetAdapter = target as UIBackgroundAdapter;
            if (targetAdapter.adapterType == UIBackgroundAdapter.BGAdapter.Stretch)
            {
                GUILayout.BeginVertical();
                {
                    m_onlyWidth.boolValue = EditorGUILayout.Toggle("Only Width", m_onlyWidth.boolValue);
                    m_halfScreenWidth.boolValue = EditorGUILayout.Toggle("Half Screen Width", m_halfScreenWidth.boolValue);
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("自动左半屏幕"))
                {
                    m_halfScreenWidth.boolValue = true;
                    var rectTransform = targetAdapter.transform as RectTransform;
                    rectTransform.pivot = new Vector2(0, 0.5f);
                    rectTransform.sizeDelta = new Vector2(UIAdaptation.StandardResolution.x / 2, UIAdaptation.StandardResolution.y);
                    rectTransform.anchorMin = new Vector2(0, 0.5f);
                    rectTransform.anchorMax = new Vector2(0, 0.5f);
                    rectTransform.anchoredPosition3D = Vector3.zero;
                }
                if (GUILayout.Button("自动右半屏幕"))
                {
                    m_halfScreenWidth.boolValue = true;
                    var rectTransform = targetAdapter.transform as RectTransform;
                    rectTransform.pivot = new Vector2(1, 0.5f);
                    rectTransform.sizeDelta = new Vector2(UIAdaptation.StandardResolution.x / 2, UIAdaptation.StandardResolution.y);
                    rectTransform.anchorMin = new Vector2(1, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                    rectTransform.anchoredPosition3D = Vector3.zero;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }

}