using HotCode.Framework;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace HotCode.FrameworkEditor
{
    [CustomEditor(typeof(UIImageItemInfo))]
    public class UIImageItemInfoEditor : UnityEditor.Editor
    {
        ReorderableList _List;
        SerializedProperty _dataArray;
        SerializedProperty _isHasDiffColor;

        UIImageItemInfo imageItemInfo;
        private void OnEnable()
        {
            imageItemInfo = target as UIImageItemInfo;
            _dataArray = serializedObject.FindProperty("dataArray");
            _isHasDiffColor = serializedObject.FindProperty("isHasDiffColor");
            _List = new ReorderableList(serializedObject, _dataArray, true, true, true, true);
            _List.drawElementCallback = OnListElementGUI;
            _List.drawHeaderCallback = OnListHeaderGUI;
            _List.onChangedCallback = OnChangedCallback;
            _List.onAddCallback = OnAddCallBack;
            _List.draggable = true;
            _List.elementHeight = 70;
        }
        private void OnChangedCallback(ReorderableList list)
        {
            serializedObject.ApplyModifiedProperties();
            if (!Application.isPlaying && list.count > 0)
                imageItemInfo.SetIndex(0);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            // 绘制 _isHasDiffColor
            // 创建自定义的 GUIContent
            GUIContent dynamicLabel = new GUIContent("是否需要连带设置颜色");
            EditorGUILayout.PropertyField(_isHasDiffColor, dynamicLabel);
            _List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        //绘制单个元素
        void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
        {
            var element = _dataArray.GetArrayElementAtIndex(index);
            rect.height -= 4;
            rect.y += 2;
            EditorGUI.PropertyField(rect, element);
            EditorGUIUtility.labelWidth = 40;
            Rect indexRect = new Rect(rect)
            {
                x = rect.x + 240,
            };
            indexRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(indexRect, "Index:" + index.ToString());
        }
        //绘制头
        void OnListHeaderGUI(Rect rect)
        {
            Rect headerRect = new Rect(rect)
            {
                height = rect.height + 4
            };
            EditorGUI.LabelField(rect, $"当前Texture条目为:{_dataArray.arraySize}");
        }

        private void OnAddCallBack(ReorderableList list)
        {
            int index = _dataArray.arraySize;
            _dataArray.InsertArrayElementAtIndex(index);
            var property = _dataArray.GetArrayElementAtIndex(index);
            if (index == 0 && imageItemInfo.image.sprite != null)
            {
                property.FindPropertyRelative("_icon").objectReferenceValue = imageItemInfo.image.sprite;
                property.FindPropertyRelative("_spriteColor").colorValue = Color.white;
            }
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
                    width = 180,
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

                Rect ColorRect = new Rect(position)
                {
                    x = position.x + 200,
                    y = position.y + 40,
                    width = 60,
                };

                var ColorProperty = property.FindPropertyRelative("_spriteColor");
                ColorProperty.colorValue = EditorGUI.ColorField(ColorRect, ColorProperty.colorValue);
            }
        }
    }
}
