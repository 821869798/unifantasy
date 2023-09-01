#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif
using System;
using System.Reflection;
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
        [ListDrawerSettings(NumberOfItemsPerPage = 30, HideAddButton = true, Expanded = true)]
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
            variableMap = new Dictionary<string, Variable>(variables.Count);
            foreach (var variable in variables)
            {
                variableMap.Add(variable.name, variable);
            }
        }


        public Variable GetVariableByName(string name)
        {
            if (variableMap.TryGetValue(name, out var variable))
            {
                return variable;
            }
            return null;
        }

#if UNITY_EDITOR

        /// <summary>
        /// 用于记录编辑器中是否修改了
        /// </summary>
        public bool editorChanged { get; set; }

        /// <summary>
        /// 通过基类获取空的模板
        /// </summary>
        /// <param name="customBaes"></param>
        /// <param name="indentCount"></param>
        /// <returns></returns>
        public static string GetBindingEmptyCodeOverride(int indentCount = 0)
        {
            string spaces = string.Empty.PadLeft(4 * indentCount);
            StringBuilder sb = new StringBuilder();
            sb.Append('\n');
            sb.Append(spaces);
            sb.Append("protected override void InitBinding(ObjectBinding __binding)\n");
            sb.Append(spaces);
            sb.Append("{\n");
            sb.Append($"{spaces}    base.InitBinding(__binding);\n");
            sb.Append(spaces);
            sb.Append("}\n");

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
                sb.Append(spaces);
                sb.Append($"    var {vname} = __binding.GetVariableByName(\"{Variables[i].name}\");\n");
                sb.Append(spaces);
                sb.Append($"    {GenerateValueAssignment(Variables[i], vname, indentCount)}\n");
            }
            sb.Append(spaces);
            sb.Append("}\n");

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

        public static string GenerateValueAssignment(Variable variable, string valueName, int indentCount = 0)
        {
            string valueAssignment = "";

            // 获取 VariableType 和 Name
            VariableType variableType = variable.variableType;
            string name = variable.name;

            // 根据 VariableType 生成读取数据的代码
            switch (variableType)
            {
                case VariableType.Object:
                    valueAssignment = $"this.{name} = {valueName}.GetValue<UnityEngine.Object>();";
                    break;
                case VariableType.GameObject:
                    valueAssignment = $"this.{name} = {valueName}.GetValue<GameObject>();";
                    break;
                case VariableType.Component:
                    var variableDeclaration = variable.editorObjectValue is Component component && component != null ? component.GetType().ToString() : string.Empty;
                    valueAssignment = $"this.{name} = {valueName}.GetValue<{variableDeclaration}>();";
                    break;
                case VariableType.Boolean:
                    valueAssignment = $"this.{name} = {valueName}.GetValue<bool>();";
                    break;
                case VariableType.Integer:
                    valueAssignment = $"this.{name} = {valueName}.GetValue<int>();";
                    break;
                case VariableType.Float:
                    valueAssignment = $"this.{name} = {valueName}.GetValue<float>();";
                    break;
                case VariableType.String:
                    valueAssignment = $"this.{name} = {valueName}.GetValue<string>();";
                    break;
                case VariableType.Color:
                    valueAssignment = $"this.{name} = {valueName}.GetValue<Color>();";
                    break;
                case VariableType.Vector2:
                    valueAssignment = $"this.{name} = {valueName}.GetValue<Vector2>();";
                    break;
                case VariableType.Vector3:
                    valueAssignment = $"this.{name} = {valueName}.GetValue<Vector3>();";
                    break;
                case VariableType.Vector4:
                    valueAssignment = $"this.{name} = {valueName}.GetValue<Vector4>();";
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
                    string spaces = string.Empty.PadLeft(4 * indentCount);
                    valueAssignment = $"this.{name} = new {defType}[{valueName}.variableArray?.ArrayValueCount ?? 0];";
                    valueAssignment += $"\n{spaces}    for (int __index = 0; __index < this.{name}.Length; __index++)";
                    valueAssignment += $"\n{spaces}    {{";
                    valueAssignment += $"\n{spaces}        this.{name}[__index] = {valueName}.variableArray.GetValue<{defType}>(__index);";
                    valueAssignment += $"\n{spaces}    }}";

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
#endif

    }
}

