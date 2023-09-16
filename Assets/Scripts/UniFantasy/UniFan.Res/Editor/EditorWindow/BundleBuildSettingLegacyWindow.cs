using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UniFan.ResEditor
{
    public class BundleBuildSettingLegacyWindow : EditorWindow
    {
        internal static void Open()
        {
            var window = GetWindow<BundleBuildSettingLegacyWindow>("BundleRuleSettingWindow", true);
            window.Show();
        }

        private AssetBundleBuildConfig _config;
        private ReorderableList _list;
        private Vector2 _scrollPosition = Vector2.zero;
        //是否是共享资源的规则
        private bool _isSharedRule;
        private bool _changeRuleMode;

        private void InitConfig()
        {
            _config = AssetBundleBuildConfig.LoadOrCreateConfig();
            _isSharedRule = false;
            _changeRuleMode = false;
        }

        void InitFilterListDrawer()
        {
            var ruleList = _isSharedRule ? _config.sharedBuildRules : _config.buildRules;
            _list = new ReorderableList(ruleList, typeof(BuildRule));
            _list.drawElementCallback = OnListElementGUI;
            _list.drawHeaderCallback = OnListHeaderGUI;
            _list.draggable = true;
            _list.elementHeight = 130;
            _list.onAddCallback = (list) => AddRule();
        }

        private void OnGUI()
        {
            if (_config == null)
            {
                InitConfig();
            }
            if (_list == null || _changeRuleMode)
            {
                if (_changeRuleMode)
                {
                    _isSharedRule = !_isSharedRule;
                    _changeRuleMode = false;
                }
                InitFilterListDrawer();
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("StartBuild", EditorStyles.toolbarButton))
                {
                    AssetBundleMenuItem.StartBuildByMenu();
                }
                if (GUILayout.Button("StartBuildIncrement", EditorStyles.toolbarButton))
                {
                    AssetBundleMenuItem.StartBuildIncrementByMenu();
                }
                if (GUILayout.Button("Copy To StreamingAsset", EditorStyles.toolbarButton))
                {
                    AssetBundleMenuItem.CopyAssetBundlesToStreamingAssets();
                }
                if (GUILayout.Button("EnableAllRule(AutoSave)", EditorStyles.toolbarButton))
                {
                    SetAllRuleActive(true);
                    SaveConfig();
                }
                if (GUILayout.Button("DisableAllRule(AutoSave)", EditorStyles.toolbarButton))
                {
                    SetAllRuleActive(false);
                    SaveConfig();
                }
                const string ruleButtonInfo1 = "normal(click to shared)";
                const string ruleButtonInfo2 = "shared(click to normal)";
                if (GUILayout.Button(_isSharedRule ? ruleButtonInfo2 : ruleButtonInfo1, EditorStyles.toolbarButton))
                {
                    _changeRuleMode = true;
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("SaveConfig", EditorStyles.toolbarButton))
                {
                    SaveConfig();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            {
                GUILayout.Space(10);
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
                {
                    _list.DoLayoutList();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            if (GUI.changed)
                EditorUtility.SetDirty(_config);
        }

        void SetAllRuleActive(bool active)
        {
            var ruleList = _isSharedRule ? _config.sharedBuildRules : _config.buildRules;
            foreach (var rule in ruleList)
            {
                rule.active = active;
            }
        }

        void AddRule()
        {
            string path = SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                var filter = new BuildRule();
                filter.searchPath = path;
                var ruleList = _isSharedRule ? _config.sharedBuildRules : _config.buildRules;
                ruleList.Add(filter);
            }
        }

        void OnListHeaderGUI(Rect rect)
        {
            EditorGUI.LabelField(rect, "Asset Build Rules");
        }

        void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
        {
            const int cap = 5;
            const int heightCap = 25;

            var ruleList = _isSharedRule ? _config.sharedBuildRules : _config.buildRules;
            BuildRule rule = ruleList[index];
            rect.y++;

            Rect r = rect;
            r.width = 50;
            r.height = 20;
            GUI.Label(r, "Active:");
            r.x += r.width + cap;
            r.width = 20;
            rule.active = GUI.Toggle(r, rule.active, GUIContent.none);
            r.x += r.width + cap;
            r.width = 80;
            GUI.Label(r, "RuleBuilType:");
            r.x += r.width + cap;
            r.width = Mathf.Max(0, rect.width - r.x);

            rule.buildType = (RulePackerType)EditorGUI.EnumPopup(r, rule.buildType);

            if (rule.buildType == RulePackerType.AssetBundleName)
            {
                r.y += heightCap;
                r.x = 0;
                r.width = 200;
                GUI.Label(r, "Open Override AssetBundleName:");
                r.x += r.width + cap;
                r.width = 20;
                rule.isOverrideBundleName = GUI.Toggle(r, rule.isOverrideBundleName, GUIContent.none);
                r.x += r.width + cap;
                r.width = 100;
                GUI.Label(r, "Override Name:");
                r.x += r.width + cap;
                r.width = Mathf.Max(0, rect.width - r.x);
                rule.overrideBundleName = GUI.TextField(r, rule.overrideBundleName);
            }
            r.y += heightCap;
            r.x = 0;
            r.width = 120;
            GUI.Label(r, "AssetSearchPath:");

            r.x += r.width + cap;
            r.width = Mathf.Max(0, rect.width - 100 - 100 - cap - cap);
            GUI.enabled = false;
            rule.searchPath = GUI.TextField(r, rule.searchPath);
            GUI.enabled = true;

            r.x += r.width + cap;
            r.width = 100;
            if (GUI.Button(r, "SelectFolder"))
            {
                var path = SelectFolder();
                if (!string.IsNullOrEmpty(path))
                    rule.searchPath = path;
            }

            r.y += heightCap;
            r.x = 0;
            r.width = 120;
            GUI.Label(r, "AssetSearchPattern:");
            r.x += r.width + cap;
            r.width = 150;
            rule.searchPattern = GUI.TextField(r, rule.searchPattern);
            r.x += r.width + cap;
            r.width = 120;
            GUI.Label(r, "AssetSearchOption:");
            r.x += r.width + cap;
            r.width = Mathf.Max(0, rect.width - r.x);
            rule.searchOption = (SearchOption)EditorGUI.EnumPopup(r, rule.searchOption);

            r.y += heightCap;
            r.x = 0;
            r.width = 180;
            GUI.Label(r, "Force Include Dep(勿乱选):");
            r.x += r.width + cap;
            r.width = 20;
            rule.forceInclueDeps = GUI.Toggle(r, rule.forceInclueDeps, GUIContent.none);

            r.x += r.width + cap;
            r.width = 180;
            GUI.Label(r, "Manifest Info Type(勿乱选):");
            r.x += r.width;
            r.width = 150;
            rule.manifestWriteType = (ManifestWriteType)EditorGUI.EnumPopup(r, rule.manifestWriteType);

            r.x += r.width + cap;
            r.width = 160;
            GUI.Label(r, "特定语言下不被依赖(勿乱选):");
            r.x += r.width;
            r.width = 150;
            rule.depCulling = EditorGUI.MaskField(r, rule.depCulling, Consts.BuildCullingLangTypeNames);

            r.x += r.width + cap;
            r.width = 120;
            GUI.Label(r, "忽略该包的引用剔除:");
            r.x += r.width;
            r.width = 30;
            rule.ignoreDepCulling = EditorGUI.Toggle(r, rule.ignoreDepCulling);
        }

        string SelectFolder()
        {
            string dataPath = Application.dataPath.Replace('\\', '/');
            string selectedPath = EditorUtility.OpenFolderPanel("AssetSearchPath", dataPath, "").Replace('\\', '/');
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(dataPath))
                {
                    return "Assets/" + selectedPath.Substring(dataPath.Length + 1);
                }
                else
                {
                    ShowNotification(new GUIContent("不能在Assets目录之外!"));
                }
            }
            return null;
        }

        void SaveConfig()
        {
            if (_config)
            {
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
