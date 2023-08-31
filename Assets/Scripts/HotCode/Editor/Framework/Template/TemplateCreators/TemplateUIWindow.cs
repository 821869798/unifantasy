using UniFan;
using Scriban;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using HotCode.Framework;

namespace HotCode.FrameworkEditor
{
    public class TemplateUIWindow : TemplateBase
    {
        public override string templateFilePath => TemplateUtil.GetTemplatePath("UI/UIWindow.sbn");

        public override string outputSuffix => ".cs";

        public override ETemplateType TemplateType => ETemplateType.UIWindow;

        /// <summary>
        /// 预览的错误码
        /// </summary>
        protected enum PreviewErrorCode
        {
            None = 0,
            NoInputName,
            BaseClassNotFound,
            BaseClassNotHasUIB,                 //自定义基类没有UIB
            BaseClassNameEqual,                 //基类和当前类的名字一摸一样
            OnlyBaseClassPrefab,                //没有新prefab，但是有基类的prefab的情况？
            PrefabNotExist,
            ObjectBindingNotExist,
        }

        private PreviewErrorCode _errorType;

        /// <summary>
        /// 需要继承的业务上的UIWindow类所在的程序集的名字
        /// </summary>
        public const string UIWindowAssembly = "Assembly-CSharp";

        public const string OriginBaseClassName = "UIBaseWindow";

        public const string UIBPrefix = "UIB_";

        /// <summary>
        /// UI的名字
        /// </summary>
        public string UIWindowName { private set; get; }
        /// <summary>
        /// UI路径的名字，默认不需要
        /// </summary>
        public string ResParentName { private set; get; }

        /// <summary>
        /// 自定义父类的名字，需要带上命名空间
        /// </summary>
        public string CustomBaseName { private set; get; }
        public Type CustomBaseType { private set; get; }
        private Type _customeBaseUIB;

        /// <summary>
        /// 用于记录是否更改的
        /// </summary>
        private bool _isChanged;
        /// <summary>
        /// 用于缓存prefab的路径修改，然后判断是否有此prefab的ObjectBinding
        /// </summary>
        private string _cachePrefabPath;
        private bool _cacheHasPrefab;
        private bool _cachePrefabHasBinding;


        public TemplateUIWindow()
        {
            outPutFilePath = Path.Combine(Application.dataPath, "Scripts", "HotCode", "Runtime", "Tester").Replace('\\', '/');
        }


        public override void ResetOptions()
        {
            base.ResetOptions();
            _isChanged = false;
            _cachePrefabPath = string.Empty;
            _cacheHasPrefab = false;
            _cachePrefabHasBinding = false;
            _errorType = PreviewErrorCode.None;
        }

        protected override TemplateObjectBase GenerateTemplateObject()
        {
            var obj = base.GenerateTemplateObject();
            obj["cs_code_head"] = "using UniFan;\nusing HotCode.Framework;\nusing System;\nusing UnityEngine;\nusing UnityEngine.UI;";
            obj["code_namespace"] = "HotCode.Game";
            obj["parent_path"] = "string.Empty";
            obj["baseclass_fullname"] = OriginBaseClassName;
            obj["uib_prefix"] = UIBPrefix;
            obj["window_layer"] = EUILayer.Normal.ToString();
            obj["is_permanent"] = false;
            obj["is_custom_baseclass"] = false;

            return obj;

        }

        public override string RenderTemplate()
        {
            if (template == null)
            {
                return "template load is null!";
            }

            templateObject["class_name"] = UIWindowName;
            if (_isChanged)
            {
                _isChanged = false;
                bool hasCustomeBase = CustomBaseType != null && _customeBaseUIB != null;
                templateObject["is_custom_baseclass"] = hasCustomeBase;
                if (hasCustomeBase)
                {
                    //有自定义的基类
                    templateObject["baseclass_fullname"] = CustomBaseType.FullName;
                    templateObject["base_uibclass_fullname"] = _customeBaseUIB.FullName.Replace('+', '.');
                }
                else
                {
                    templateObject["baseclass_fullname"] = OriginBaseClassName;
                }


                templateObject["object_binding_code"] = GetObjectBindingCode();
            }


            return template.Render(templateContext);
        }

        /// <summary>
        /// 获取UI window上的ObjectBinding组件，并且生成绑定的代码
        /// </summary>
        /// <returns></returns>
        private string GetObjectBindingCode()
        {
            if (string.IsNullOrEmpty(UIWindowName))
            {
                return string.Empty;
            }
            string path = UIWindowName;
            if (!string.IsNullOrEmpty(ResParentName))
            {
                path = ResParentName + "/" + path;
            }
            path = "Assets/" + PathConstant.GetUIPrefabPath(path);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null)
            {
                return string.Empty;
            }
            var binding = go.GetComponent<ObjectBinding>();
            if (binding == null)
            {
                return string.Empty;
            }
            return binding.GetBindingCode(_customeBaseUIB);
        }

        public override void OnGUI()
        {
            base.OnGUI();

            GUILayout.BeginVertical("GroupBox");
            GUILayout.Label("生成选项");

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("资源的路径（相对于Res/03_Prefabs/UI/，默认不需要）:", TemplateGUIStyles.MaxWidthNormalStyle);
                var tempName = GUILayout.TextField(ResParentName, TemplateGUIStyles.InputStyle);
                if (tempName != ResParentName)
                {
                    ResParentName = tempName;
                    _isChanged = true;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"自定义的基类(需要带上完整命名空间)空则继承{OriginBaseClassName}:", TemplateGUIStyles.MaxWidthNormalStyle);
                var newClass = GUILayout.TextField(CustomBaseName, TemplateGUIStyles.InputStyle);
                if (newClass != CustomBaseName)
                {
                    CustomBaseName = newClass;
                    var assembly = Assembly.Load(UIWindowAssembly);
                    CustomBaseType = assembly.GetType(CustomBaseName);
                    _customeBaseUIB = CustomBaseType?.GetNestedType(UIBPrefix + CustomBaseType.Name, BindingFlags.NonPublic) ?? null;
                    _isChanged = true;
                }
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("创建出的UIWindow名(不需要输入):", TemplateGUIStyles.MaxWidthNormalStyle);
                if (createFileName != UIWindowName)
                {
                    UIWindowName = createFileName;
                    _isChanged = true;
                }
                GUILayout.TextField(UIWindowName, TemplateGUIStyles.NoInputStyle);

            }
            GUILayout.EndHorizontal();



            string assetPath = UIWindowName;
            if (!string.IsNullOrEmpty(ResParentName))
            {
                assetPath = ResParentName + "/" + assetPath;
            }
            assetPath = PathConstant.GetUIPrefabPath(assetPath);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("对应的prefab路径:", TemplateGUIStyles.MaxWidthNormalStyle);

                GUILayout.TextField(assetPath, TemplateGUIStyles.NoInputStyle);
            }
            GUILayout.EndHorizontal();

            //检测prefab以及objectbinding
            assetPath = "Assets/" + assetPath;
            if (assetPath != _cachePrefabPath)
            {
                _cachePrefabPath = assetPath;
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(_cachePrefabPath);
                _cacheHasPrefab = go != null;
                if (go != null && go.GetComponent<ObjectBinding>() != null)
                {
                    _cachePrefabHasBinding = true;
                }
                else
                {
                    _cachePrefabHasBinding = false;
                }
            }

            //检测当前模板状态
            _errorType = RefreshPreviewErrorCode();
            switch (_errorType)
            {
                case PreviewErrorCode.None:
                    EditorGUILayout.HelpBox("正常", MessageType.Info);
                    break;
                case PreviewErrorCode.NoInputName:
                    EditorGUILayout.HelpBox("没有输入文件名", MessageType.Error);
                    break;
                case PreviewErrorCode.BaseClassNotFound:
                    EditorGUILayout.HelpBox("输入的基类不存在", MessageType.Error);
                    break;
                case PreviewErrorCode.BaseClassNotHasUIB:
                    EditorGUILayout.HelpBox("输入的基类不存在UIB内部类", MessageType.Error);
                    break;
                case PreviewErrorCode.BaseClassNameEqual:
                    EditorGUILayout.HelpBox("输入的基类和创建的类名一样", MessageType.Error);
                    break;
                case PreviewErrorCode.OnlyBaseClassPrefab:
                    EditorGUILayout.HelpBox("继承基类后无新Prefab", MessageType.Warning);
                    break;
                case PreviewErrorCode.PrefabNotExist:
                    EditorGUILayout.HelpBox("对应prefab资源不存在", MessageType.Error);
                    break;
                case PreviewErrorCode.ObjectBindingNotExist:
                    EditorGUILayout.HelpBox("Prefab上ObjectBinding不存在", MessageType.Warning);
                    break;

            }

            GUILayout.EndVertical();
        }

        private PreviewErrorCode RefreshPreviewErrorCode()
        {
            if (string.IsNullOrEmpty(createFileName))
            {
                return PreviewErrorCode.NoInputName;
            }
            //检测当前输入合法
            if (!string.IsNullOrEmpty(CustomBaseName) && CustomBaseType == null)
            {
                return PreviewErrorCode.BaseClassNotFound;
            }

            if (CustomBaseType != null && CustomBaseType.Name == createFileName)
            {
                return PreviewErrorCode.BaseClassNameEqual;
            }

            if (!_cacheHasPrefab)
            {
                if (CustomBaseType != null)
                {
                    return _customeBaseUIB != null ? PreviewErrorCode.OnlyBaseClassPrefab : PreviewErrorCode.BaseClassNotHasUIB;
                }
                else
                {
                    return PreviewErrorCode.PrefabNotExist;
                }
            }

            if (!_cachePrefabHasBinding)
            {
                return PreviewErrorCode.ObjectBindingNotExist;
            }

            return PreviewErrorCode.None;
        }
    }
}
