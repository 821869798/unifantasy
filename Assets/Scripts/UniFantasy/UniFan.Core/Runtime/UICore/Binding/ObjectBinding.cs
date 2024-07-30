#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System.Reflection;
#endif
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UniFan
{
#if UNITY_EDITOR
    [DisallowMultipleComponent]
    [HideMonoScript]
#endif
    public class ObjectBinding : MonoBehaviour
    {
#if UNITY_EDITOR
        /// <summary>
        /// 编辑器用，用于直接生成代码到文件中(如果为空，就使用当前GameObject name作为类型生成),否则用当前类名来寻找cs文件
        /// </summary>
        [HideInInspector]
        public string editorCustomClass;
#endif

#if UNITY_EDITOR
        [ListDrawerSettings(NumberOfItemsPerPage = 30, HideAddButton = true)]
#endif
        [SerializeField]
        private List<Variable> variables;

        public List<Variable> Variables
        {
            get
            {
#if UNITY_EDITOR
                if (variables == null)
                {
                    variables = new List<Variable>();
                }
#endif
                return variables;
            }
        }

        private Dictionary<string, Variable> variableMap;

        private void Awake()
        {
            if (variableMap == null)
            {
                InitVariableMap();
            }
        }

        private void InitVariableMap()
        {
            variableMap = new Dictionary<string, Variable>(variables.Count);
            foreach (var variable in variables)
            {
                variableMap.Add(variable.name, variable);
            }
        }


        public Variable GetVariableByName(string name)
        {
            if (variableMap == null)
            {
                InitVariableMap();
            }
            if (variableMap.TryGetValue(name, out var variable))
            {
                return variable;
            }
            return null;
        }

        public bool TryGetVariableValue<T>(string name, out T value)
        {
            if (variableMap == null)
            {
                InitVariableMap();
            }
            if (variableMap.TryGetValue(name, out var variable))
            {
                value = variable.GetValue<T>();
                return true;
            }
            value = default(T);
            Debug.LogError($"[{nameof(ObjectBinding)}|{this.name}] can't find variable: {name}");
            return false;
        }
#if UNITY_EDITOR

        /// <summary>
        /// 用于记录编辑器中是否修改了
        /// </summary>
        public bool editorChanged { get; set; }

        public const string GenerateMark = "ObjectBinding Generate";

        /// <summary>
        /// 通过基类获取空的模板
        /// </summary>
        /// <param name="customBaes"></param>
        /// <param name="indentCount"></param>
        /// <returns></returns>
        public static string GetBindingEmptyCode(Type customBaes = null, int indentCount = 0)
        {
            string spaces = string.Empty.PadLeft(4 * indentCount);
            StringBuilder sb = new StringBuilder();
            sb.Append($"{spaces}#region {GenerateMark}\n");
            sb.Append(spaces);
            if (customBaes != null)
            {
                sb.Append("protected override void InitBinding(ObjectBinding __binding){ base.InitBinding(__binding); }\n");
            }
            else
            {
                sb.Append("protected virtual void InitBinding(ObjectBinding __binding){}\n");
            }
            sb.Append($"{spaces}#endregion {GenerateMark}\n");

            return sb.ToString();
        }

        /// <summary>
        /// 生成绑定的c#代码
        /// </summary>
        /// <param name="indentCount">tab的个数</param>
        /// <param name="indentCount">自定义的基类</param>
        /// <returns></returns>
        public string GetBindingCode(Type customBaes = null, int indentCount = 0)
        {
            string spaces = string.Empty.PadLeft(4 * indentCount);

            //处理自定义基类
            HashSet<string> baseHasNames;
            if (customBaes != null)
            {
                baseHasNames = EditorGetTypeAllPublicGetProperty(customBaes);
            }
            else
            {
                baseHasNames = new HashSet<string>();
            }

            StringBuilder sb = new StringBuilder();
            sb.Append($"{spaces}#region {GenerateMark}\n");

            foreach (var variable in Variables)
            {
                if (baseHasNames.Contains(variable.name))
                {
                    continue;
                }
                sb.Append(spaces);
                sb.Append(GenerateVariableDeclaration(variable));
                sb.Append('\n');
            }

            sb.Append($"{spaces}protected {(customBaes != null ? "override" : "virtual")} void InitBinding(ObjectBinding __binding)\n");
            sb.Append(spaces);
            sb.Append("{\n");
            if (customBaes != null)
            {
                sb.Append($"{spaces}    base.InitBinding(__binding);\n");
            }
            for (int i = 0; i < Variables.Count; i++)
            {
                if (baseHasNames.Contains(Variables[i].name))
                {
                    continue;
                }
                var vname = "__tbv" + i;
                sb.Append($"{GenerateValueAssignment("__binding", Variables[i], vname, indentCount + 1)}\n");
            }
            sb.Append(spaces);
            sb.Append("}\n");

            sb.Append($"{spaces}#endregion {GenerateMark}");

            return sb.ToString();
        }


        public static string GenerateVariableDeclaration(Variable variable)
        {
            // 获取 VariableType 和 Name
            VariableType variableType = variable.variableType;
            string name = variable.name;
            string def = GetVariableTypeDeclaration(variable, variableType, variable.editorObjectValue);

            return $"public {def} {name} {{ protected set; get; }}";
        }


        private static string GetVariableTypeDeclaration(Variable variable, VariableType variableType, UnityEngine.Object value)
        {
            // 根据 VariableType 生成声明对象的代码
            string variableDeclaration = string.Empty;
            switch (variableType)
            {
                case VariableType.Object:
                    variableDeclaration = "UnityEngine.Object";
                    break;
                case VariableType.GameObject:
                    variableDeclaration = "GameObject";
                    break;
                case VariableType.Component:
                    variableDeclaration = value is Component component && component != null ? component.GetType().ToString() : string.Empty;
                    break;
                case VariableType.Boolean:
                    variableDeclaration = "bool";
                    break;
                case VariableType.Integer:
                    variableDeclaration = "int";
                    break;
                case VariableType.Float:
                    variableDeclaration = "float";
                    break;
                case VariableType.String:
                    variableDeclaration = "string";
                    break;
                case VariableType.Color:
                    variableDeclaration = "Color";
                    break;
                case VariableType.Vector2:
                    variableDeclaration = "Vector2";
                    break;
                case VariableType.Vector3:
                    variableDeclaration = "Vector3";
                    break;
                case VariableType.Vector4:
                    variableDeclaration = "Vector4";
                    break;
                case VariableType.Array:
                    string defType;
                    if (variable.editorVariableArray.editorArrayType == VariableType.Component)
                    {
                        defType = variable.editorVariableArray.EditorGetComponentType().ToString();
                    }
                    else
                    {
                        defType = GetVariableTypeDeclaration(variable, variable.editorVariableArray.editorArrayType, null);
                    }
                    variableDeclaration = $"{defType}[]";
                    break;
                default:
                    throw new NotSupportedException("Variable type not supported.");
            }
            return variableDeclaration;
        }

        public static string GenerateValueAssignment(string bindingName, Variable variable, string valueName, int indentCount = 0)
        {
            string valueAssignment = "";

            // 获取 VariableType 和 Name
            VariableType variableType = variable.variableType;
            string name = variable.name;

            // 前面的空格
            string spaces = string.Empty.PadLeft(4 * indentCount);

            // 根据 VariableType 生成读取数据的代码
            switch (variableType)
            {
                case VariableType.Object:
                    valueAssignment = $"{spaces}{bindingName}.TryGetVariableValue<UnityEngine.Object>(\"{variable.name}\", out var {valueName});\n{spaces}this.{name} = {valueName};";
                    break;
                case VariableType.GameObject:
                    valueAssignment = $"{spaces}{bindingName}.TryGetVariableValue<UnityEngine.GameObject>(\"{variable.name}\", out var {valueName});\n{spaces}this.{name} = {valueName};";
                    break;
                case VariableType.Component:
                    var variableDeclaration = variable.editorObjectValue is Component component && component != null ? component.GetType().ToString() : string.Empty;
                    valueAssignment = $"{spaces}{bindingName}.TryGetVariableValue<{variableDeclaration}>(\"{variable.name}\", out var {valueName});\n{spaces}this.{name} = {valueName};";
                    break;
                case VariableType.Boolean:
                    break;
                case VariableType.Integer:
                    valueAssignment = $"{spaces}{bindingName}.TryGetVariableValue<int>(\"{variable.name}\", out var {valueName});\n{spaces}this.{name} = {valueName};";
                    break;
                case VariableType.Float:
                    valueAssignment = $"{spaces}{bindingName}.TryGetVariableValue<float>(\"{variable.name}\", out var {valueName});\n{spaces}this.{name} = {valueName};";
                    break;
                case VariableType.String:
                    valueAssignment = $"{spaces}{bindingName}.TryGetVariableValue<string>(\"{variable.name}\", out var {valueName});\n{spaces}this.{name} = {valueName};";
                    break;
                case VariableType.Color:
                    valueAssignment = $"{spaces}{bindingName}.TryGetVariableValue<UnityEngine.Color>(\"{variable.name}\", out var {valueName});\n{spaces}this.{name} = {valueName};";
                    break;
                case VariableType.Vector2:
                    valueAssignment = $"{spaces}{bindingName}.TryGetVariableValue<UnityEngine.Vector2>(\"{variable.name}\", out var {valueName});\n{spaces}this.{name} = {valueName};";
                    break;
                case VariableType.Vector3:
                    valueAssignment = $"{spaces}{bindingName}.TryGetVariableValue<UnityEngine.Vector3>(\"{variable.name}\", out var {valueName});\n{spaces}this.{name} = {valueName};";
                    break;
                case VariableType.Vector4:
                    valueAssignment = $"{spaces}{bindingName}.TryGetVariableValue<UnityEngine.Vector4>(\"{variable.name}\", out var {valueName});\n{spaces}this.{name} = {valueName};";
                    break;
                case VariableType.Array:
                    string defType;
                    if (variable.editorVariableArray.editorArrayType == VariableType.Component)
                    {
                        defType = variable.editorVariableArray.EditorGetComponentType().ToString();
                    }
                    else
                    {
                        defType = GetVariableTypeDeclaration(variable, variable.editorVariableArray.editorArrayType, null);
                    }

                    valueAssignment = $"{spaces}var {valueName} = {bindingName}.GetVariableByName(\"{variable.name}\");\n";
                    valueAssignment += $"{spaces}this.{name} = new {defType}[{valueName}?.arrayValueCount ?? 0];";
                    valueAssignment += $"\n{spaces}for (int __index = 0; __index < this.{name}.Length; __index++)";
                    valueAssignment += $"\n{spaces}{{";
                    valueAssignment += $"\n{spaces}    this.{name}[__index] = {valueName}.variableArray.GetValue<{defType}>(__index);";
                    valueAssignment += $"\n{spaces}}}";

                    break;
                default:
                    throw new NotSupportedException("Variable type not supported.");
            }
            return valueAssignment;
        }

        /// <summary>
        /// 筛选一个类中所有public get的属性，包括它的继承基类
        /// </summary>
        /// <param name="type"></param>
        private static HashSet<string> EditorGetTypeAllPublicGetProperty(Type type)
        {
            // 获取当前类及其所有基类中所有的公共属性
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            HashSet<string> propertyNames = new HashSet<string>();
            foreach (var p in properties)
            {
                if (p.CanRead && p.GetMethod.IsPublic)
                {
                    propertyNames.Add(p.Name);
                }

            }
            return propertyNames;
        }

        public string EditorGetTargetName(bool view = false)
        {
            string name;
            if (!string.IsNullOrEmpty(this.editorCustomClass))
            {
                name = this.editorCustomClass;
            }
            else
            {
                name = this.gameObject.name;
                if (view)
                {
                    name += "View";
                }
            }
            return name;
        }

#endif

    }
}

