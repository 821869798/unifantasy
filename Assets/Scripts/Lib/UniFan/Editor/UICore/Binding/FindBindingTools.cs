using UniFan;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniFanEditor
{
    public static class FindBindingTool
    {
        public static GameObject selectObj;
        public static HashSet<Component> comSets = new HashSet<Component>();
        public static bool IsFoundFlag = false;

        [MenuItem(("GameObject/Find Binding Parent"), false, 1000)]
        private static void FindBindingParent()
        {
            ClearData();
            selectObj = Selection.activeObject as GameObject;
            if (selectObj == null) return;

            //拿到所选物体自身的所有组件
            var compnents = selectObj.GetComponents<Component>();
            foreach (var comp in compnents)
            {
                comSets.Add(comp);
            }

            //拿到所有挂载LuaBinding的物体
            var parentBindings = selectObj.GetComponentsInParent<ObjectBinding>(true);

            BegainFindBinding(parentBindings);

            if (IsFoundFlag)
                return;

            //如果向上寻找找不到 那么就遍历全部
            //发现存在非父物体绑定的情况
            var rootBindings = selectObj.transform.root.GetComponentsInChildren<ObjectBinding>(true);
            BegainFindBinding(rootBindings);
            if (!IsFoundFlag)
                EditorUtility.DisplayDialog("Tip", "这个物体没有绑定在LuaBinding上", "Ojbk");
        }

        /// <summary>
        /// 查找主方法
        /// </summary>
        /// <param name="parentBinding"></param>
        private static void BegainFindBinding(ObjectBinding[] parentBinding)
        {
            for (int i = 0; i < parentBinding.Length; i++)
            {
                var bindingItem = parentBinding[i];
                //同名剔除队列
                if (bindingItem.name == selectObj.name) { continue; }
                //挨个循环查找Binding中是否有指定组件
                for (int t = 0; t < bindingItem.Variables.Count; t++)
                {
                    if (IsFoundFlag)
                        break;
                    var variable = bindingItem.Variables[t];
                    FindBindingWithVariable(bindingItem, variable);
                }
                if (IsFoundFlag)
                    break;
            }
        }
        /// <summary>
        /// 通过传入的Variable类型 判断该参数是否与被查找对象一致
        /// 如果是Array类型则套娃判断
        /// </summary>
        private static void FindBindingWithVariable(ObjectBinding bindingObj, Variable variable)
        {
            switch (variable.variableType)
            {
                case VariableType.GameObject:
                    {
                        var obj = variable.GetValue() as GameObject;
                        if (obj == selectObj)
                        {
                            Selection.activeObject = bindingObj.gameObject;
                            EditorGUIUtility.PingObject(Selection.activeObject);
                            IsFoundFlag = true;
                            return;
                        }
                    }
                    break;
                case VariableType.Component:
                    {
                        var obj = variable.GetValue() as Component;
                        if (obj != null && comSets.Contains(obj))
                        {
                            Selection.activeObject = bindingObj.gameObject;
                            EditorGUIUtility.PingObject(Selection.activeObject);
                            IsFoundFlag = true;
                            return;
                        }
                    }
                    break;
                case VariableType.Array:
                    {
                        var arrayList = variable.GetValue() as VariableArray;
                        foreach (var arrayitem in arrayList.ArrayValue)
                        {
                            FindBindingWithVariable(bindingObj, Convert2Variable(arrayList, arrayitem));
                        }
                    }
                    break;
            }
            //var variable = __item.GetBind(__item.variables[i].Name);
            ////拿不到则下一个
            //if (variable == null) continue;
            //var varName = variable.ToString().Replace(" ", "");
            //varName = Regex.Replace(varName, @"\([^\(]*\)", "");
            //if (varName == __select.name)
            //{
            //    Selection.activeObject = __item.gameObject;
            //    EditorGUIUtility.PingObject(Selection.activeObject);
            //    return;
            //}
        }
        /// <summary>
        /// Aarry中存的是VariableList 转换成 Variable
        /// </summary>
        private static Variable Convert2Variable(VariableArray arrayList, VariableElement list)
        {
            Variable variable = new Variable();
            variable.EditorSetVariableType(arrayList.arrayType);
            variable.SetValue(list.GetValue(arrayList.arrayType));
            return variable;
        }
        /// <summary>
        /// 静态参数需要还原
        /// </summary>
        private static void ClearData()
        {
            selectObj = null;
            comSets = new HashSet<Component>();
            IsFoundFlag = false;
        }
    }
}
