using HotCode.Framework;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace HotCode.FrameworkEditor
{
    [CustomEditor(typeof(UITMPItemInfo))]
    public class UITMPItemInfoEditor : UnityEditor.Editor
    {
        private UITMPItemInfo textItemInfo;
        //
        private ReorderableList _List;
        //
        private SerializedProperty _dataArray;

        private void OnEnable()
        {
            textItemInfo = target as UITMPItemInfo;
            _dataArray = serializedObject.FindProperty("dataArray");
            InitReorderableList(_dataArray);
            CheckExistExText();
        }
        //初始化列表
        private void InitReorderableList(SerializedProperty dataArray)
        {
            _List = new ReorderableList(serializedObject, dataArray, true, true, true, true);
            _List.drawHeaderCallback = OnListHeaderGUI;
            _List.drawElementCallback = OnListElementGUI;
            _List.onChangedCallback = OnChangedCallback;
            _List.onReorderCallbackWithDetails = OnReorderCallbackWithDetails;
            _List.onAddCallback = OnAddCallback;
            //_List.onRemoveCallback = OnRemoveCallback;
            _List.draggable = true;
            _List.elementHeight = 40;
        }
        //绘制表头
        private void OnListHeaderGUI(Rect rect)
        {
            Rect headerRect = new Rect(rect)
            {
                height = rect.height + 4
            };
            EditorGUI.LabelField(rect, $"当前LanguageData条目为:{_dataArray.arraySize}");
        }
        //绘制单个元素
        private void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
        {
            var property = _dataArray.GetArrayElementAtIndex(index);
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += 2;

            EditorGUIUtility.labelWidth = 40;
            Rect indexRect = new Rect(rect)
            {
                x = rect.width - 30,
            };
            EditorGUI.LabelField(indexRect, "Index:", index.ToString());

            indexRect.x = rect.width - 100;
            indexRect.width = 60;
            if (GUI.Button(indexRect, "预览文本"))
            {
                textItemInfo.SetIndex(index);
            }

            EditorGUI.PropertyField(rect, property);
        }
        //当列表变化
        private void OnReorderCallbackWithDetails(ReorderableList list, int oldIndex, int newIndex)
        {
            if (textItemInfo.isHasImage && textItemInfo.imageItemInfo.dataArray != null)
            {
                var Array = textItemInfo.imageItemInfo.dataArray;
                var oldItem = Array[oldIndex];
                var NewItem = Array[newIndex];
                Array[newIndex] = oldItem;
                Array[oldIndex] = NewItem;
            }
        }
        //当列表变动
        private void OnChangedCallback(ReorderableList list)
        {
            //if (list.count > 0)
            //    textItemInfo.SetIndex(0);
            //else
            //    textItemInfo.ClearText();
            serializedObject.ApplyModifiedProperties();
            if (!Application.isPlaying && list.count > 0)
                textItemInfo.SetIndex(0);
        }
        //当增加
        private void OnAddCallback(ReorderableList list)
        {
            int index = _dataArray.arraySize;
            _dataArray.InsertArrayElementAtIndex(index);
            var property = _dataArray.GetArrayElementAtIndex(index);
            property.FindPropertyRelative("_strColor").colorValue = textItemInfo.text.color;
            property.FindPropertyRelative("_tid").longValue = 0;
            property.FindPropertyRelative("_strContent").stringValue = string.Empty;
            if (index == 0)
            {
                property.FindPropertyRelative("_strContent").stringValue = textItemInfo.text.text;
            }
        }
        //当删除
        private void OnRemoveCallback(ReorderableList list)
        {

        }
        //绘制列表以外的部分
        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical("HelpBox");
            {
                textItemInfo.isHasImage = EditorGUILayout.ToggleLeft("和UiImageItemInfo这个脚本有关联?:", textItemInfo.isHasImage);
                if (textItemInfo.isHasImage)
                {
                    var guiStyle = new GUIStyle();
                    guiStyle.normal.textColor = Color.black;
                    guiStyle.fontSize = 12;
                    guiStyle.alignment = TextAnchor.MiddleLeft;

                    GUILayout.Box("请确保两个脚本的<b><Color=#980000FF>数量一致</Color></b>,以及<b><Color=#980000FF>顺序一致</Color></b>", guiStyle);
                    textItemInfo.imageItemInfo = EditorGUILayout.ObjectField("脚本[拖GameObj进来]:", textItemInfo.imageItemInfo, typeof(UIImageItemInfo), true) as UIImageItemInfo;
                }

                textItemInfo.isHasDiffColor = EditorGUILayout.ToggleLeft("是否需要连带设置颜色", textItemInfo.isHasDiffColor);
            }
            GUILayout.EndVertical();

            serializedObject.Update();
            _List.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        //存在TextItem的时候ExText不应该存在Id 
        private void CheckExistExText()
        {
            // TODO 重新适配本地化
            //var exText = textItemInfo.gameObject.GetComponent<TextMeshProUGUI>();
            //if (exText != null)
            //{
            //    exText.tid = 0;
            //    //对象的修改不会被记录 需要手动记录下
            //    EditorUtility.SetDirty(exText);
            //}

        }
    }

}