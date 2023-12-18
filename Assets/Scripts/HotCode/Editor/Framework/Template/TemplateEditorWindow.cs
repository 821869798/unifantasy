using System;
using System.IO;
using UniFanEditor;
using UnityEditor;
using UnityEngine;

namespace HotCode.FrameworkEditor
{
    public class TemplateEditorWindow : EditorWindow
    {

        private static TemplateEditorWindow _instance;

        [MenuItem("GameEditor/TemplateWindow(文件创建模板)")]
        private static void ShowEditor()
        {
            OpenInternal();
        }

        private static TemplateEditorWindow OpenInternal()
        {
            _instance = GetWindow<TemplateEditorWindow>("Template");
            _instance.Show();
            return _instance;
        }

        [InitializeOnLoadMethod]
        static void RegisterTemplateFunction()
        {
            ObjectBindingEditor.CreateCSTemplateAction = (objectBinding, name) =>
            {
                var template = OpenByTemplateType<TemplateUIWindow>(ETemplateType.UIWindow);
                template.ResetOptions();
                template.LoadTemplateFile();
                template.InitTemplateUI(objectBinding, name);
            };
        }

        public static T OpenByTemplateType<T>(ETemplateType templateType) where T : TemplateBase
        {
            var window = OpenInternal();
            var targetType = typeof(T);
            foreach (var t in window._allTemplates)
            {
                if (t.TemplateType == templateType)
                {
                    window._curTemplateType = templateType;
                    return t as T;
                }
            }
            return default(T);
        }

        private TemplateBase[] _allTemplates;
        private ETemplateType _curTemplateType = ETemplateType.UIWindow;


        //当前的模板值
        public string currentTemplateValue;

        public Vector2 scrollPosition = Vector2.zero;


        protected virtual void Init()
        {
            _allTemplates = new TemplateBase[System.Enum.GetValues(typeof(ETemplateType)).Length];
            TemplateUtil.GetInterfaceClass<TemplateBase>(s => _allTemplates[(int)s.TemplateType] = s);

            var template = _allTemplates[(int)_curTemplateType];
            if (template != null)
            {
                //加载模板文件
                template.LoadTemplateFile();
            }
        }

        protected void OnEnable()
        {
            Init();
        }

        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            bool isChanged = false;
            EditorGUI.BeginChangeCheck();
            _curTemplateType = (ETemplateType)EditorGUILayout.EnumPopup("选择创建的文件类型", _curTemplateType);
            isChanged = isChanged || EditorGUI.EndChangeCheck();
            if (GUILayout.Button("重置并且刷新", GUILayout.MaxWidth(150)))
            {
                isChanged = true;
                foreach (var item in _allTemplates)
                {
                    item.ResetOptions();
                }
            }
            GUILayout.EndHorizontal();
            if (_allTemplates == null)
            {
                return;
            }
            var template = _allTemplates[(int)_curTemplateType];
            if (template == null)
            {
                return;
            }
            if (isChanged)
            {
                // 更改类型，重新加载模板文件
                template.LoadTemplateFile();
            }
            EditorGUI.BeginChangeCheck();
            template.OnGUI();
            isChanged = isChanged || EditorGUI.EndChangeCheck();
            if (isChanged || template.ForceRender())
            {
                currentTemplateValue = template.RenderTemplate();
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("拷贝内容到剪切板"))
            {
                GUIUtility.systemCopyBuffer = currentTemplateValue;
                this.ShowNotification(new GUIContent("拷贝成功"));
            }
            if (GUILayout.Button("保存到目标文件"))
            {
                if (string.IsNullOrEmpty(currentTemplateValue) || string.IsNullOrEmpty(template.createFileName))
                {
                    EditorUtility.DisplayDialog("错误提示", "文件名为空，或者生成模板内容为空!", "OK");
                }
                else
                {

                    try
                    {
                        if (!Directory.Exists(template.outPutFilePath))
                        {
                            Directory.CreateDirectory(template.outPutFilePath);
                        }

                        string finalOutputFilePath = Path.Combine(template.outPutFilePath,
                            template.createFileName + template.outputSuffix).Replace('\\', '/');
                        bool isWriteToFile = true;
                        if (File.Exists(finalOutputFilePath))
                        {
                            isWriteToFile = EditorUtility.DisplayDialog("提示", "目标文件已经存在，是否覆盖?", "覆盖", "取消");
                        }

                        if (isWriteToFile)
                        {
                            File.WriteAllText(finalOutputFilePath, currentTemplateValue);
                            AssetDatabase.Refresh();
                        }
                    }
                    catch (Exception e)
                    {
                        EditorUtility.DisplayDialog("错误提示", "保存失败！\n" + e.ToString(), "OK");
                    }
                }
            }
            GUILayout.EndHorizontal();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.TextArea(currentTemplateValue, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }
}
