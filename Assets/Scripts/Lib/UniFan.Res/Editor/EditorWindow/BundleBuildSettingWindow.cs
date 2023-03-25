#if UNITY_2019_4_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PlasticPipe.PlasticProtocol.Messages;

namespace UniFan.Res.Editor
{
    public class BundleBuildSettingWindow : EditorWindow
    {

        private AssetBundleBuildConfig _config;


        private ListView _rulePreviewList;
        private BuildRuleDetailContainer _ruleDetailContainer;
        private Label _ruleTypeInfo;

        //上次选择的规则索引
        private int _lastSelectRuleIndex;
        //是否是公用资源包规则
        private bool _isShareRuleMode = false;

        public List<BuildRule> GetCurModeRules()
        {
            if (_isShareRuleMode)
            {
                return _config.sharedBuildRules;
            }
            else
            {
                return _config.buildRules;
            }
        }

        public void CreateGUI()
        {

            try
            {
                _config = AssetBundleBuildConfig.LoadOrCreateConfig();

                // Each editor window contains a root VisualElement object
                VisualElement root = rootVisualElement;

                var visualAsset = EditorHelper.LoadWindowUXML<BundleBuildSettingWindow>();
                if (visualAsset == null)
                {
                    return;
                }
                visualAsset.CloneTree(root);

                //初始化按钮
                InitToolButtons();

                _ruleTypeInfo = root.Q<Label>("RuleTypeInfo");

                var togShared = root.Q<Toggle>("TogShared");
                togShared.value = _isShareRuleMode;
                togShared.RegisterValueChangedCallback(evt =>
                {
                    //刷新显示模式
                    _isShareRuleMode = evt.newValue;
                    _lastSelectRuleIndex = 0;
                    RefreshWindow();
                });


                _rulePreviewList = root.Q<ListView>("RulePreviewList");
                _rulePreviewList.makeItem = MakeRulePreviewListItem;
                _rulePreviewList.bindItem = BindRulePreviewListItem;
                _rulePreviewList.itemsAdded += AddRulePreviewListItem;
                _rulePreviewList.itemsRemoved += RemoveRulePreviewListItem;
                _rulePreviewList.onSelectionChange += OnRulePreviewSelectionChange;

                var detailContainer = root.Q<VisualElement>("BuildRuleContainer");
                _ruleDetailContainer = new BuildRuleDetailContainer(this, detailContainer);


                //刷新窗体
                RefreshWindow();

            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }

        void InitToolButtons()
        {
            VisualElement root = rootVisualElement;

            var btnBuildForce = root.Q<Button>("BtnBuildForce");
            btnBuildForce.clicked += () =>
            {
                AssetBundleMenuItem.StartBuildByMenu();
            };

            var btnBuildIncre = root.Q<Button>("BtnBuildIncre");
            btnBuildIncre.clicked += () =>
            {
                AssetBundleMenuItem.StartBuildIncrementByMenu();
            };

            var btnCopyAssets = root.Q<Button>("BtnCopyAssets");
            btnCopyAssets.clicked += () =>
            {
                AssetBundleMenuItem.CopyAssetBundlesToStreamingAssets();
            };

            var btnSave = root.Q<Button>("BtnSave");
            btnSave.clicked += SaveConfig;

            var btnDisableAllRule = root.Q<Button>("BtnDisableAll");
            btnDisableAllRule.clicked += DisableAllRule;

            var btnEnableAllRule = root.Q<Button>("BtnEnableAll");
            btnEnableAllRule.clicked += EnableAllRule;
        }

        void RefreshWindow()
        {
            if (_isShareRuleMode)
            {
                _ruleTypeInfo.text = "ResShareRuleList";
                _ruleTypeInfo.style.backgroundColor = new StyleColor(new Color(215f / 255, 168f / 255, 96f / 255));
            }
            else
            {
                _ruleTypeInfo.text = "ResBuildRuleList";
                _ruleTypeInfo.style.backgroundColor = new StyleColor(new Color(89f / 255, 89f / 255, 89f / 255));
            }

            FillRulePreviewList();
        }


        #region RulePreviewList

        void FillRulePreviewList()
        {
            _rulePreviewList.Clear();
            _rulePreviewList.ClearSelection();
            _rulePreviewList.itemsSource = GetCurModeRules();
            _rulePreviewList.Rebuild();

            if (_lastSelectRuleIndex >= 0 && _lastSelectRuleIndex < _rulePreviewList.itemsSource.Count)
            {
                _rulePreviewList.selectedIndex = _lastSelectRuleIndex;
            }
        }

        VisualElement MakeRulePreviewListItem()
        {
            VisualElement element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;

            var toggle = new Toggle();
            toggle.name = "toggleActive";
            toggle.style.unityTextAlign = TextAnchor.MiddleLeft;
            toggle.style.flexGrow = 0f;
            toggle.style.height = 24f;
            toggle.style.width = 30f;
            toggle.text = "";
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (element.userData is BuildRule rule)
                {
                    rule.active = evt.newValue;
                }
            });
            element.Add(toggle);

            var label = new Label();
            label.name = "labelRule";
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.flexGrow = 1f;
            label.style.height = 24f;
            label.style.paddingBottom = 2f;
            label.style.paddingTop = 2f;
            label.enableRichText = true;
            element.Add(label);

            return element;
        }


        void AddRulePreviewListItem(IEnumerable<int> collections)
        {
            var buildRules = GetCurModeRules();
            var last = buildRules.Count - 1;
            var filter = new BuildRule();
            buildRules[last] = filter;

            string path = EditorHelper.SelectFolder(this);
            if (!string.IsNullOrEmpty(path))
            {
                filter.searchPath = path;
            }

            _rulePreviewList.selectedIndex = last;
        }

        void RemoveRulePreviewListItem(IEnumerable<int> collections)
        {

        }

        void BindRulePreviewListItem(VisualElement element, int index)
        {
            var buildRules = GetCurModeRules();

            BuildRule rule = buildRules[index];

            //绑定数据
            element.userData = rule;

            var toggle = element.Q<Toggle>("toggleActive");
            toggle.value = rule.active;

            var textField1 = element.Q<Label>("labelRule");

            string buildDesc = rule.buildDesc;
            if (string.IsNullOrEmpty(buildDesc))
            {
                buildDesc = "<color=#ff0000>未定义描述</color>";
            }
            string searchPath = rule.searchPath;
            if (string.IsNullOrEmpty(rule.searchPath))
            {
                searchPath = "<color=#ff0000>未定义资源路径</color>";
            }

            textField1.text = $"[{buildDesc}]{searchPath}";
        }


        void OnRulePreviewSelectionChange(IEnumerable<object> objs)
        {
            var buildRules = GetCurModeRules();

            var selectRule = _rulePreviewList.selectedItem as BuildRule;
            if (selectRule == null)
            {
                if (buildRules.Count > 0)
                {
                    _rulePreviewList.selectedIndex = buildRules.Count - 1;
                    return;
                }

                _lastSelectRuleIndex = -1;
                _ruleDetailContainer.RefreshRuleDetail(null,_isShareRuleMode);
                return;
            }

            _lastSelectRuleIndex = _rulePreviewList.selectedIndex;

            _ruleDetailContainer.RefreshRuleDetail(selectRule, _isShareRuleMode, () =>
            {
                _rulePreviewList.RefreshItem(_lastSelectRuleIndex);
            });
        }
        #endregion

        private void DisableAllRule()
        {
            var buildRules = GetCurModeRules();
            foreach (var rule in buildRules)
            {
                rule.active = false;
            }
            SaveConfig();
            RefreshWindow();
        }

        private void EnableAllRule()
        {
            var buildRules = GetCurModeRules();
            foreach (var rule in buildRules)
            {
                rule.active = true;
            }
            SaveConfig();
            RefreshWindow();
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



#endif