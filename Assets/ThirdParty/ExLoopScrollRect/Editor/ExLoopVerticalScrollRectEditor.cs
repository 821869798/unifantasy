using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(ExLoopVerticalScrollRect))]
    public class ExLoopVerticalScrollRectEditor : LoopScrollRectInspector
    {
        private void OnEnable()
        {
            var source = serializedObject.FindProperty("m_source");
            source.isExpanded = true;
        }
    }
}


