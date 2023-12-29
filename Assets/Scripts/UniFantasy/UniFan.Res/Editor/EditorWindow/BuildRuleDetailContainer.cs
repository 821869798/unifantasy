using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UniFan.ResEditor
{
    internal class BuildRuleDetailContainer
    {
        EditorWindow _parentWindow;
        VisualElement _root;

        BuildRule _rule;
        bool _isShareRule;
        Action _refreshAction;

        Toggle _togActive;
        TextField _tfDescName;
        EnumField _efRulePackerType;
        VisualElement _overrideBundleNameContainer;
        Toggle _togOverrideBundleName;
        TextField _tfCustomBundleName;
        TextField _tfAssetSearchPath;
        Button _btnSelectFolder;
        EnumField _assetSearchOption;
        TextField _tfAssetSearchPattern;
        EnumField _efManifestInfoType;
        Toggle _togForceIncludeDep;
        MaskField _maskDepCulling;
        Toggle _togIgnoreDepCulling;
        VisualElement _assetRegexContainer;
        TextField _tfAssetSearchRegex;



        public BuildRuleDetailContainer(EditorWindow parentWindow, VisualElement root)
        {
            _parentWindow = parentWindow;
            _root = root;
            _root.visible = false;
            InitEditorItem(root);
        }

        public void RefreshRuleDetail(BuildRule rule, bool isSharedRule, Action simpleRefresh = null)
        {
            this._rule = rule;
            this._refreshAction = simpleRefresh;
            this._isShareRule = isSharedRule;

            if (rule == null)
            {
                _root.visible = false;
                return;
            }

            RefreshRuleDetailValues(this._rule);
        }

        private void InitEditorItem(VisualElement root)
        {

            _togActive = root.Q<Toggle>("TogActive");
            _togActive.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.active = evt.newValue;
                }
                _refreshAction?.Invoke();
            });

            _tfDescName = root.Q<TextField>("DescName");
            _tfDescName.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.buildDesc = evt.newValue;
                }
                _refreshAction?.Invoke();
            });

            _efRulePackerType = root.Q<EnumField>("RulePackerType");
            _efRulePackerType.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.buildType = (RulePackerType)evt.newValue;

                    if (_rule.buildType == RulePackerType.AssetBundleName)
                    {
                        _overrideBundleNameContainer.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        _overrideBundleNameContainer.style.display = DisplayStyle.None;
                    }
                }
            });

            _overrideBundleNameContainer = root.Q<VisualElement>("OverrideBundleNameContainer");

            _togOverrideBundleName = root.Q<Toggle>("TogOverrideBundleName");
            _togOverrideBundleName.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.isOverrideBundleName = evt.newValue;
                }
            });

            _tfCustomBundleName = root.Q<TextField>("CustomBundleName");
            _tfCustomBundleName.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.overrideBundleName = evt.newValue;
                }
            });

            _tfAssetSearchPath = root.Q<TextField>("AssetSearchPath");
            _tfAssetSearchPath.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.searchPath = evt.newValue;
                }
            });

            _btnSelectFolder = root.Q<Button>("BtnSelectFolder");
            _btnSelectFolder.clicked += OnBtnSelectFolder;

            _assetSearchOption = root.Q<EnumField>("AssetSearchOption");
            _assetSearchOption.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.searchOption = (SearchOption)evt.newValue;
                }
            });

            _tfAssetSearchPattern = root.Q<TextField>("AssetSearchPattern");
            _tfAssetSearchPattern.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.searchPattern = evt.newValue;
                }
            });

            _assetRegexContainer = root.Q<VisualElement>("AssetRegexContainer");
            _tfAssetSearchRegex = root.Q<TextField>("AssetSearchRegex");
            _tfAssetSearchRegex.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.searchRegex = evt.newValue;
                }
            });

            _efManifestInfoType = root.Q<EnumField>("ManifestInfoType");
            _efManifestInfoType.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.manifestWriteType = (ManifestWriteType)evt.newValue;
                }
            });

            _togForceIncludeDep = root.Q<Toggle>("ForceIncludeDep");
            _togForceIncludeDep.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.forceInclueDeps = evt.newValue;
                }
            });

            _maskDepCulling = root.Q<MaskField>("MaskDepCulling");
            _maskDepCulling.choices = ABBuildConsts.BuildCullingLangTypeNames.ToList();
            _maskDepCulling.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.depCulling = evt.newValue;
                }
            });

            _togIgnoreDepCulling = root.Q<Toggle>("IgnoreDepCulling");
            _togIgnoreDepCulling.RegisterValueChangedCallback(evt =>
            {
                if (this._rule != null)
                {
                    this._rule.ignoreDepCulling = evt.newValue;
                }
            });

        }

        private void RefreshRuleDetailValues(BuildRule rule)
        {
            if (rule.buildType == RulePackerType.AssetBundleName)
            {
                _overrideBundleNameContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                _overrideBundleNameContainer.style.display = DisplayStyle.None;
            }

            if (_isShareRule)
            {
                _assetSearchOption.style.display = DisplayStyle.None;
                _tfAssetSearchPattern.label = "ShareRes Regex Pattern";
                _tfAssetSearchPattern.tooltip = "正则匹配规则";
                _assetRegexContainer.style.display = DisplayStyle.None;
            }
            else
            {
                _assetSearchOption.style.display = DisplayStyle.Flex;
                _tfAssetSearchPattern.label = "AssetSearchPattern";
                _tfAssetSearchPattern.tooltip = "支持多个,使用|分割";
                _assetRegexContainer.style.display = DisplayStyle.Flex;
            }


            _togActive.value = rule.active;
            _tfDescName.value = rule.buildDesc;
            _efRulePackerType.value = rule.buildType;
            _tfAssetSearchPath.value = rule.searchPath;
            _assetSearchOption.value = rule.searchOption;
            _tfAssetSearchPattern.value = rule.searchPattern;
            _tfAssetSearchRegex.value = rule.searchRegex;
            _efManifestInfoType.value = rule.manifestWriteType;
            _togForceIncludeDep.value = rule.forceInclueDeps;
            _maskDepCulling.value = rule.depCulling;
            _togIgnoreDepCulling.value = rule.ignoreDepCulling;

            _togOverrideBundleName.value = rule.isOverrideBundleName;
            _tfCustomBundleName.value = rule.overrideBundleName;

            _root.visible = true;
        }

        void OnBtnSelectFolder()
        {
            if (_rule == null)
            {
                return;
            }
            string path = EditorHelper.SelectFolder(_parentWindow);
            if (!string.IsNullOrEmpty(path))
            {
                _rule.searchPath = path;
                _tfAssetSearchPath.value = _rule.searchPath;
            }

        }
    }
}
