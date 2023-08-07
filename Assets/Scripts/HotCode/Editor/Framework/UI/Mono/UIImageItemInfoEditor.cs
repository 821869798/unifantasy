using HotCode.Framework;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace HotCode.FrameworkEditor
{
    [CustomEditor(typeof(UIImageItemInfo))]
    public class UiImageItemInfoEditor : Editor
    {
        ReorderableList _List;
        SerializedProperty prop;

        UIImageItemInfo imageItemInfo;
        private void OnEnable()
        {
            imageItemInfo = target as UIImageItemInfo;
            prop = serializedObject.FindProperty("dataArray");
            _List = new ReorderableList(serializedObject, prop, true, true, true, true);
            _List.drawElementCallback = OnListElementGUI;
            _List.drawHeaderCallback = OnListHeaderGUI;
            _List.onChangedCallback = OnChangedCallback;
            _List.draggable = true;
            _List.elementHeight = 70;

            if (_List.count > 0)
                imageItemInfo.SetIndex(0);
        }
        private void OnChangedCallback(ReorderableList list)
        {
            if (list.count > 0)
                imageItemInfo.SetIndex(0);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        //绘制单个元素
        void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
        {
            var element = prop.GetArrayElementAtIndex(index);
            rect.height -= 4;
            rect.y += 2;
            EditorGUI.PropertyField(rect, element);
            EditorGUIUtility.labelWidth = 40;
            Rect indexRect = new Rect(rect)
            {
                x = rect.x + 270,
            };
            EditorGUI.LabelField(indexRect, "Index:", index.ToString());
        }
        //绘制头
        void OnListHeaderGUI(Rect rect)
        {
            Rect headerRect = new Rect(rect)
            {
                height = rect.height + 4
            };
            EditorGUI.LabelField(rect, $"当前Texture条目为:{prop.arraySize}");
        }
    }


    [CustomPropertyDrawer(typeof(ImageItem))]
    public class ImageItemDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //创建一个属性包装器
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                //设定高度
                position.height = EditorGUIUtility.singleLineHeight;
                //设定宽度
                EditorGUIUtility.labelWidth = 80;

                Rect IconRect = new Rect(position)
                {
                    width = 160,
                    height = 65
                };
                var IconProperty = property.FindPropertyRelative("_icon");
                string name = "";
                try
                {
                    name = IconProperty.objectReferenceValue.name;
                }
                catch (System.Exception)
                {
                    if (name == null || name.Length < 1)
                        name = "";
                }
                IconProperty.objectReferenceValue = EditorGUI.ObjectField(IconRect, name, IconProperty.objectReferenceValue, typeof(Sprite), false);
            }
        }
    }

}
