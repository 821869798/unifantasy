using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(ExLoopHorizontalScrollRect))]
    public class ExLoopHorizontalScrollRectEditor : LoopScrollRectInspector
    {

        private void OnEnable()
        {
            var source = serializedObject.FindProperty("m_source");
            source.isExpanded = true;
        }
    }
}

