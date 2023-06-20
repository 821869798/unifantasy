using Sirenix.OdinInspector.Editor;
using UniFan;
using UnityEditor;
using UnityEngine;

namespace UniFanEditor
{
    public class VariableArrayDrawer : OdinValueDrawer<VariableArray>
    {

        protected override void Initialize()
        {
            base.Initialize();

            var entry = this.ValueEntry;
            var array = entry.SmartValue;
            foreach (var e in array.ArrayValue)
            {
                e.editorVaiableArray = array;
            }

        }
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // 获取属性的引用
            var entry = this.ValueEntry;
            var property = this.Property;
            var array = entry.SmartValue;
            foreach (var e in array.ArrayValue)
            {
                e.editorVaiableArray = array;
            }

            var prop = ValueEntry.Property.Children["_arrayValue"];
            prop.Draw(label);
            var rect = prop.LastDrawnValueRect;

            //拖拽的批量添加响应
            HandleDragAndDrop(array, rect, Event.current);

            this.ValueEntry.SmartValue = array;
        }

        private int dragDropControlID = -1;

        protected virtual void HandleDragAndDrop(VariableArray array, Rect dragArea, Event evt)
        {
            bool enable = false;
            switch (array.arrayType)
            {
                case VariableType.Object:
                case VariableType.GameObject:
                case VariableType.Component:
                    enable = true;
                    break;
            }
            if (!enable)
            {
                return;
            }

            dragDropControlID = GUIUtility.GetControlID(this.GetHashCode(), FocusType.Passive, dragArea);

            switch (evt.GetTypeForControl(dragDropControlID))
            {

                case EventType.DragUpdated:
                case EventType.DragPerform:

                    if (GUI.enabled && dragArea.Contains(evt.mousePosition))
                    {

                        UnityEngine.Object[] objectReferences = DragAndDrop.objectReferences;

                        bool acceptDrag = false;

                        foreach (var draggedObject in objectReferences)
                        {
                            if (array.arrayType == VariableType.Object)
                            {
                                //任意对象
                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                if (evt.type == EventType.DragPerform)
                                {
                                    acceptDrag = true;
                                    DragAndDrop.activeControlID = 0;

                                    AddArrayElement(array, draggedObject);
                                }
                                else
                                {

                                    DragAndDrop.activeControlID = dragDropControlID;
                                }
                            }
                            else
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
                                            DragAndDrop.activeControlID = 0;

                                            AddArrayElement(array, go);
                                        }
                                        else
                                        {

                                            DragAndDrop.activeControlID = dragDropControlID;
                                        }

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
                        HandleUtility.Repaint();
                    }

                    break;
            }
        }

        /// <summary>
        /// 拖拽添加数组对象
        /// </summary>
        /// <param name="array"></param>
        /// <param name="obj"></param>
        public void AddArrayElement(VariableArray array, UnityEngine.Object obj)
        {
            var element = new VariableElement();
            switch (array.editorArrayType)
            {
                case VariableType.Object:
                    element.editorObjectValue = obj;
                    break;
                case VariableType.GameObject:
                    element.editorObjectValue = obj as GameObject;
                    break;
                case VariableType.Component:
                    var go = obj as GameObject;
                    if (go != null)
                    {
                        element.editorObjectValue = go.GetComponent(array.EditorGetComponentType());
                    }
                    break;
            }


            var newArray = new VariableElement[array.editorArrayValue.Length + 1];
            newArray[newArray.Length - 1] = element;


            System.Array.Copy(array.editorArrayValue, newArray, array.editorArrayValue.Length);

            array.editorArrayValue = newArray;



        }
    }
}
