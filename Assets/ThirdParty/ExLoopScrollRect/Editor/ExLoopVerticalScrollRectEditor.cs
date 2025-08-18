using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(ExLoopVerticalScrollRect))]
    public class ExLoopVerticalScrollRectEditor : LoopScrollRectInspector
    {
        SerializedProperty m_source;
        protected override void OnEnable()
        {
            m_source = serializedObject.FindProperty("m_source");
            m_source.isExpanded = true;
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 绘制 m_source
            EditorGUILayout.PropertyField(m_source);

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}


