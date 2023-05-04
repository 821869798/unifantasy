using HierarchyLabels;
using UniFan;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

namespace HotCode.FrameworkEditor
{
    public class ObjectBindingLabelRule : HierarchyLabelRule
    {

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.hierarchyChanged += RefreshSceneObjectBindings;
            Undo.postprocessModifications += OnPostprocessModifications;
        }

        private static Dictionary<Component, KeyValuePair<string, ObjectBinding>> _component2Binding;

        public static UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
        {
            bool objectBindingModify = false;
            foreach (var modification in modifications)
            {
                ObjectBinding myComponent = modification.currentValue.target as ObjectBinding;
                if (myComponent != null)
                {
                    objectBindingModify = true;
                }
            }
            //如果有修改，就刷新
            if (objectBindingModify)
            {
                EditorApplication.delayCall += () => RefreshSceneObjectBindings();
            }
            return modifications;
        }

        public static void RefreshSceneObjectBindings()
        {
            _component2Binding = new();
            ObjectBinding[] bindings;

            // 获取当前打开的预制件的舞台
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            // 如果当前打开的是一个预制件
            if (prefabStage != null)
            {
                // 获取预制件的根游戏对象
                GameObject prefabRoot = prefabStage.prefabContentsRoot;
                bindings = prefabRoot.GetComponentsInChildren<ObjectBinding>(true);
            }
            else
            {
                bindings = GameObject.FindObjectsOfType<ObjectBinding>(true);
            }

            foreach (var bind in bindings)
            {
                foreach (var variable in bind.Variables)
                {
                    if (variable.variableType == VariableType.Component && variable.editorObjectValue is Component com)
                    {
                        if (!_component2Binding.ContainsKey(com))
                        {
                            _component2Binding[com] = new(variable.name, bind);
                        }
                    }
                }
            }
        }

        public override bool GetLabel(Component component, out string label, out GUIStyle style)
        {
            style = StyleProvider.GetStyle(component);
            label = string.Empty;

            if (component is ObjectBinding objectBinding)
            {
                label = "[ObjectBindRoot]";
                style.normal.textColor = Color.white;
                return true;
            }
            else if (_component2Binding != null && _component2Binding.TryGetValue(component, out var kv))
            {
                label = $"[{component.GetType().Name}]{kv.Key}=>{kv.Value.name}";
                style.normal.textColor = Color.green;
                return true;
            }

            return false;
        }
    }
}
