
using UnityEngine;

namespace HotCode.FrameworkEditor
{
    public static class TemplateGUIStyles
    {
        public static readonly GUIStyle NoInputStyle = new GUIStyle("AnimItemBackground");
        public static readonly GUIStyle InputStyle = new GUIStyle("BoldTextField");

        public static readonly GUILayoutOption MaxWidthNormalStyle = GUILayout.MaxWidth(400);
        public static readonly GUILayoutOption MaxWidthLargeStyle = GUILayout.MaxWidth(800);

        static TemplateGUIStyles()
        {
            var temp = new GUIStyle("CN StatusWarn");
            InputStyle.normal.textColor = temp.normal.textColor;
        }
    }
}
