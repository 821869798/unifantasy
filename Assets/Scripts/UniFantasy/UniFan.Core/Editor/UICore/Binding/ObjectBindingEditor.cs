using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UniFan;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniFanEditor
{

    [CustomEditor(typeof(ObjectBinding))]
    public class ObjectBindingEditor : OdinEditor
    {
        SerializedProperty variables;

        private static readonly GUILayoutOption LayoutMinWidth = GUILayout.MinWidth(40);

        protected override void OnEnable()
        {
            base.OnEnable();

            variables = serializedObject.FindProperty("variables");
            if (target is ObjectBinding objectBinding)
            {
                objectBinding.editorChanged = false;
            }
        }

        public enum EdtitorCheckResult
        {
            Correct = 0,
            VariablesNull,
            NameNotValid,
            DuplicateName,
            DataValueNull,
        }

        public static readonly Regex RegexNameCheck = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");
        private static HashSet<string> _codeNames = new HashSet<string>();
        private static HashSet<string> _duplicateName = new HashSet<string>();
        /// <summary>
        /// 用来检测数据是否合法
        /// </summary>
        /// <returns></returns>
        public EdtitorCheckResult CheckVariableIsValid(ObjectBinding binding)
        {
            _codeNames.Clear();
            _duplicateName.Clear();
            if (binding.Variables == null)
            {
                return EdtitorCheckResult.VariablesNull;
            }
            foreach (var variable in binding.Variables)
            {
                bool isMatch = RegexNameCheck.IsMatch(variable.name);
                if (!isMatch)
                {
                    return EdtitorCheckResult.NameNotValid;
                }
                if (_codeNames.Contains(variable.name))
                {
                    _duplicateName.Add(variable.name);
                    return EdtitorCheckResult.DuplicateName;
                }
                _codeNames.Add(variable.name);

                if (!variable.EditorCheckVariableValid())
                {
                    return EdtitorCheckResult.DataValueNull;
                }
            }
            return EdtitorCheckResult.Correct;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            var objectBinding = target as ObjectBinding;
            if (objectBinding == null)
            {
                return;
            }

            var checkResult = CheckVariableIsValid(objectBinding);
            switch (checkResult)
            {
                case EdtitorCheckResult.VariablesNull:
                    EditorGUILayout.HelpBox("组件数据为空", MessageType.Error);
                    break;
                case EdtitorCheckResult.NameNotValid:
                    EditorGUILayout.HelpBox("存在不合法名字，请检查", MessageType.Error);
                    break;
                case EdtitorCheckResult.DuplicateName:
                    EditorGUILayout.HelpBox("存在重复名字，请检查", MessageType.Error);
                    break;
                case EdtitorCheckResult.DataValueNull:
                    EditorGUILayout.HelpBox("存在空的数据，请检查", MessageType.Error);
                    break;
                default:
                    if (objectBinding.editorChanged)
                    {
                        EditorGUILayout.HelpBox("已经修改，请重新生成绑定代码!", MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Normal", MessageType.Info);
                    }
                    break;
            }


            foreach (var variable in objectBinding.Variables)
            {
                variable.editorError = false;
                if (!variable.EditorCheckVariableValid())
                {
                    variable.editorError = true;
                }
                else if (_duplicateName.Contains(variable.name))
                {
                    variable.editorError = true;
                }
            }


            HandleDragAndDrop(Event.current);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(new GUIContent("+"), EditorStyles.miniButtonLeft, LayoutMinWidth))
                {
                    int index = variables.arraySize > 0 ? variables.arraySize : 0;
                    this.DrawContextMenu(variables, index);
                }
                if (GUILayout.Button(new GUIContent("-"), EditorStyles.miniButtonRight, LayoutMinWidth))
                {
                    int index = variables.arraySize > 0 ? variables.arraySize - 1 : -1;
                    RemoveVariable(variables, index);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("copy code", EditorStyles.miniButtonLeft, LayoutMinWidth))
                {
                    GUIUtility.systemCopyBuffer = (target as ObjectBinding).GetBindingCode();
                    EditorBindingUtil.ShowNotificationInInspector("拷贝成功请自行替换代码中的:\nregion ObjectBinding Generate");
                }
                if (GUILayout.Button("code2file", EditorStyles.miniButtonLeft, LayoutMinWidth))
                {
                    EditorBindingUtil.GenBindingCodeReplaceFile(target as ObjectBinding);
                }
            }
            EditorGUILayout.EndHorizontal();

            //绘制base
            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(target, "Modify Binding:" + target.name);
                serializedObject.ApplyModifiedProperties();
                objectBinding.editorChanged = true;
            }

        }

        protected virtual void DrawContextMenu(SerializedProperty variables, int index)
        {
            GenericMenu menu = new GenericMenu();
            foreach (VariableType variableType in System.Enum.GetValues(typeof(VariableType)))
            {
                var type = variableType;
                menu.AddItem(new GUIContent(variableType.ToString()), false, context =>
                {
                    AddVariable(variables, index, type);
                }, null);
            }
            menu.ShowAsContext();
        }

        protected virtual void RemoveVariable(SerializedProperty variables, int index)
        {
            if (index < 0)
                return;

            variables.serializedObject.Update();
            variables.DeleteArrayElementAtIndex(index);
            variables.serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
        }

        protected virtual void AddVariable(SerializedProperty variables, int index, VariableType type)
        {
            if (index < 0 || index > variables.arraySize)
                return;
            variables.serializedObject.Update();
            variables.InsertArrayElementAtIndex(index);
            SerializedProperty variableProperty = variables.GetArrayElementAtIndex(index);
            variableProperty.FindPropertyRelative("_variableType").enumValueIndex = (int)type;

            variableProperty.FindPropertyRelative("_name").stringValue = "";
            variableProperty.FindPropertyRelative("_objectValue").objectReferenceValue = null;
            variableProperty.FindPropertyRelative("_dataValue").stringValue = string.Empty;
            if (type == VariableType.Array)
            {
                variableProperty.FindPropertyRelative("_variableArray").managedReferenceValue = new VariableArray();
            }

            variables.serializedObject.ApplyModifiedProperties();
            GUI.FocusControl(null);
        }

        //以下是拖拽响应相关
        private static int dragAndDropHash = "ObjectBindingEditor".GetHashCode();
        private int dragDropControlID = -1;
        private bool isDraggingOver;
        private List<GameObject> dragObjects = new List<GameObject>();

        protected virtual void HandleDragAndDrop(Event evt)
        {
            GUIStyle helpBoxStyle = isDraggingOver ? EditorBindingUtil.DragHighlightStyle : EditorBindingUtil.DragNormalStyle;

            float dragAreaHeight = 40.0f;
            Rect dragArea = GUILayoutUtility.GetRect(GUIContent.none, helpBoxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(dragAreaHeight));
            GUI.Box(dragArea, "将物体拖拽到这里即可添加Binding(可多个)", helpBoxStyle);

            dragDropControlID = GUIUtility.GetControlID(dragAndDropHash, FocusType.Passive, dragArea);

            switch (evt.GetTypeForControl(dragDropControlID))
            {

                case EventType.DragUpdated:
                case EventType.DragPerform:

                    if (GUI.enabled && dragArea.Contains(evt.mousePosition))
                    {

                        Object[] objectReferences = DragAndDrop.objectReferences;

                        bool acceptDrag = false;

                        foreach (Object draggedObject in objectReferences)
                        {

                            // 检查拖放的对象是否为GameObject
                            GameObject go = draggedObject as GameObject;
                            if (go != null)
                            {
                                // 检查GameObject是否在场景中
                                if (go.scene != null && go.scene.IsValid())
                                {

                                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                    if (evt.type == EventType.DragPerform)
                                    {
                                        acceptDrag = true;
                                        isDraggingOver = false;
                                        DragAndDrop.activeControlID = 0;

                                        dragObjects.Add(go);
                                    }
                                    else
                                    {
                                        isDraggingOver = true;
                                        DragAndDrop.activeControlID = dragDropControlID;
                                    }

                                }
                            }
                        }

                        if (acceptDrag)
                        {

                            GUI.changed = true;
                            DragAndDrop.AcceptDrag();
                        }
                    }

                    break;

                case EventType.DragExited:

                    if (GUI.enabled)
                    {
                        isDraggingOver = false;
                        HandleUtility.Repaint();
                    }

                    break;
            }

            if (dragObjects.Count > 0)
            {
                DrawTypes(dragObjects, variables);
            }
        }

        private void DrawTypes(List<GameObject> dragObj, SerializedProperty variables)
        {
            if (dragObj == null || dragObj.Count == 0)
            {
                return;
            }
            UnityEditor.Editor repaint = ScriptableObject.CreateInstance<UnityEditor.Editor>();

            GameObject gameObject = dragObj[0];
            //拿到物体下的组件
            Component[] types = gameObject.GetComponents<Component>();

            //筛一下CanvasRenderer
            types = types.Where(val => val.GetType() != typeof(CanvasRenderer)).ToArray();

            //组件List包含了GameObject本身
            List<Object> objs = new List<Object> { gameObject };
            objs.AddRange(types);


            var nameRect = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true));
            GUI.Box(nameRect, $"[gameObject.name]所含组件:", EditorBindingUtil.DragTitleStyle);
            for (var i = 0; i < objs.Count; i++)
            {
                var systemType = objs[i].GetType();
                var r = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true));
                //鼠标移动到区域时变色
                EditorBindingUtil.DragObjEleStyle.normal.background = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath("fcfdfa07b9639df42ac836c057b50e00"), typeof(Texture2D)) as Texture2D;
                EditorBindingUtil.DragObjEleStyle.normal.textColor = r.Contains(Event.current.mousePosition) ? Color.green : Color.white;
                if (r.Contains(Event.current.mousePosition))
                {
                    repaint.Repaint();
                }
                //选中事件
                if (r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    int index = variables.arraySize > 0 ? variables.arraySize : 0;
                    if (index < 0 || index > variables.arraySize)
                        return;
                    variables.serializedObject.Update();
                    variables.InsertArrayElementAtIndex(index);
                    SerializedProperty variableProperty = variables.GetArrayElementAtIndex(index);
                    variableProperty.FindPropertyRelative("_variableType").enumValueIndex = systemType == typeof(GameObject) ? (int)VariableType.GameObject : (int)VariableType.Component;
                    variableProperty.FindPropertyRelative("_name").stringValue = EditorBindingUtil.NormalizeName(gameObject.name);
                    variableProperty.FindPropertyRelative("_objectValue").objectReferenceValue = objs[i];
                    variableProperty.FindPropertyRelative("_dataValue").stringValue = "";
                    variables.serializedObject.ApplyModifiedProperties();
                    GUI.FocusControl(null);
                    dragObj.Remove(gameObject);
                }
                GUI.Box(r, systemType.Name, EditorBindingUtil.DragObjEleStyle);
            }
            var skipR = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true));
            if (GUI.Button(skipR, "跳过", EditorBindingUtil.DragObjSkipStyle))
            {
                dragObj.Remove(gameObject);
            }

        }


    }
}
