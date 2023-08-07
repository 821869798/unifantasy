using UniFan;
using UnityEditor;
using UnityEditor.UI;

namespace HotCode.Framework.Editor
{
    [CustomEditor(typeof(ExButton))]
    [CanEditMultipleObjects]
    public class ExButtonEditor : ButtonEditor
    {
        private SerializedProperty _isBackButton;

        protected override void OnEnable()
        {
            base.OnEnable();
            _isBackButton = serializedObject.FindProperty(nameof(_isBackButton));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(_isBackButton);

            serializedObject.ApplyModifiedProperties();
        }
    }
}