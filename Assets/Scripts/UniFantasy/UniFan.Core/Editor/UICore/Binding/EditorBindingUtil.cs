using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UniFan;
using UnityEditor;
using UnityEngine;

namespace UniFanEditor
{
    public static class EditorBindingUtil
    {
        public static readonly GUIStyle IndexTextStyle = new GUIStyle();

        public static readonly GUIStyle BindValueStyle = new GUIStyle("ProgressBarBar");
        public static readonly GUIStyle InputBoldStyle = new GUIStyle("BoldTextField");

        public static readonly GUIStyle ValueTypeStyle = new GUIStyle("PreviewPackageInUse");

        public static readonly GUIStyle ValueTypeConstStyle = new GUIStyle("OL box");

        public static readonly GUIStyle DragHighlightStyle = new GUIStyle("flow node 0 on");
        public static readonly GUIStyle DragNormalStyle = new GUIStyle("flow node 0");

        public static readonly GUIStyle DragTitleStyle = new GUIStyle("flow node 3 on");
        public static readonly GUIStyle DragObjEleStyle = new GUIStyle("EditModeSingleButton");
        public static readonly GUIStyle DragObjSkipStyle = new GUIStyle("flow node hex 5");

        public static readonly GUILayoutOption MinWidthStyle = GUILayout.MinWidth(10);

        static EditorBindingUtil()
        {
            ValueTypeStyle.alignment = TextAnchor.MiddleCenter;
            ValueTypeStyle.margin = new RectOffset(0, 0, 0, 0);

            IndexTextStyle.normal.textColor = Color.yellow;

            DragNormalStyle.alignment = TextAnchor.MiddleCenter;
            DragHighlightStyle.alignment = TextAnchor.MiddleCenter;

            DragTitleStyle.alignment = TextAnchor.MiddleCenter;
            DragObjEleStyle.alignment = TextAnchor.MiddleCenter;

            ValueTypeConstStyle.normal.textColor = ValueTypeStyle.normal.textColor;
            ValueTypeConstStyle.alignment = TextAnchor.MiddleCenter;
        }

        public static readonly Regex RegexNormalizeName = new Regex(@"[^\w]");

        public static string NormalizeName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";

            name = name.Replace(" ", "");
            string result = char.ToLower(name[0]) + name.Substring(1);
            string output = RegexNormalizeName.Replace(result, "");
            return output;
        }

        public static void ShowNotificationInInspector(string message)
        {
            System.Type inspectorWindowType = System.Type.GetType("UnityEditor.InspectorWindow, UnityEditor");
            EditorWindow inspectorWindow = EditorWindow.GetWindow(inspectorWindowType);
            if (inspectorWindow != null)
            {
                inspectorWindow.ShowNotification(new GUIContent(message));
            }
        }

        public static string GetIndentString(int indentCount)
        {
            return string.Empty.PadLeft(indentCount * 4, ' ');
        }


        public const string UIWindowAssembly = "Assembly-CSharp";


        public static void GenBindingCodeReplaceFile(ObjectBinding binding)
        {
            if (binding == null)
            {
                EditorUtility.DisplayDialog("警告", $"当前ObjectBinding对象为空，生成失败", "ok");
                return;
            }
            string name = binding.EditorGetTargetName(false);

            //先找到对应的脚本
            if (!TryFindScript(name, out var scriptPath, out MonoScript dstScript))
            {
                //TryFindScript(name + "View", out scriptPath, out dstScript);
            }

            if (string.IsNullOrEmpty(scriptPath))
            {
                EditorUtility.DisplayDialog("警告", $"没有找到对应名字的代码脚本:{name}\n请拷贝代码自行替换", "ok");
                return;
            }

            try
            {
                string scriptContent = File.ReadAllText(scriptPath);

                if (EditorUtility.DisplayDialog("警告", $"是否替换[{scriptPath}]中ObjectBinding Generate的部分", "ok", "cancel"))
                {
                    Type baseClassType = null;
                    //先使用正则表达式找到UIB基类
                    string uibPattern = @"#region Template Generate,don't modify.*? partial class UIB_.+ : (\S*UIB_\S+).+?#endregion Template Generate,don't modify";
                    Match match = Regex.Match(scriptContent, uibPattern, RegexOptions.Singleline);
                    if (match.Success)
                    {
                        Group group = match.Groups[1];
                        string result = group.Value;
                        var assembly = Assembly.Load(UIWindowAssembly);
                        if (!result.Contains('.'))
                        {
                            return;
                        }
                        string parentClass = result.Substring(0, result.LastIndexOf('.'));
                        var parentClassType = assembly.GetType(parentClass);
                        string uibClassName = result.Substring(result.LastIndexOf('.') + 1);
                        baseClassType = parentClassType?.GetNestedType(uibClassName, BindingFlags.NonPublic) ?? null;
                    }

                    // 使用正则表达式找到指定的部分并替换
                    //string pattern = @"(?<=#region ObjectBinding Generate\s*).*?(?=#endregion ObjectBinding Generate)";
                    //string replacement = "\n" + binding.GetBindingCode(baseClassType, 3) + string.Empty.PadLeft(3 * 4);

                    string pattern = @$"[ \t]*#region {ObjectBinding.GenerateMark}\s*.*?#endregion {ObjectBinding.GenerateMark}[ \t]*";
                    string replacement = binding.GetBindingCode(baseClassType, 3);

                    string modifiedContent = Regex.Replace(scriptContent, pattern, replacement, RegexOptions.Singleline);

                    // 将修改后的内容写回脚本文件
                    File.WriteAllText(scriptPath, modifiedContent);

                    // 通知Unity刷新
                    AssetDatabase.Refresh();

                    Debug.Log("Replaced content in script: " + scriptPath);
                    // 高亮显示找到的脚本
                    EditorGUIUtility.PingObject(dstScript);
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("警告", $"读取修改代码失败:{name} {e.Message}\n请拷贝代码自行替换", "ok");
            }


        }

        private static bool TryFindScript(string name, out string scriptPath, out MonoScript dstScript)
        {
            //先找到对应的脚本
            string[] guids = AssetDatabase.FindAssets("t:Script " + name);
            scriptPath = string.Empty;
            dstScript = null;
            if (guids != null)
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    Type classType = script?.GetClass() ?? null;

                    if (classType != null && classType.Name == name)
                    {
                        dstScript = script;
                        scriptPath = path;
                        // 读取脚本内容
                        return true;
                    }
                }
            }
            return false;
        }
    }

}
