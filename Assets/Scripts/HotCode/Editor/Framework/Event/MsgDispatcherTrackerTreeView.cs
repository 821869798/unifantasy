using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace HotCode.FrameworkEditor
{
    public class MsgDispatcherTrackerViewItem : TreeViewItem
    {
        static Regex removeHref = new Regex("<a href.+>(.+)</a>", RegexOptions.Compiled);

        public string MsgEventName { get; set; }
        public int MsgId { get; set; }
        public string MsgIdStr { get; set; }
        public Delegate EventDelegate { get; set; }
        public int ListenterCount { get; set; }
        public string ListenterCountStr { get; set; }

        private Delegate[] invokeList;

        private readonly static StringBuilder sb = new StringBuilder();

        public string GetDetailInfo()
        {
            if (invokeList == null)
            {
                return string.Empty;
            }
            sb.Clear();
            for (int i = 0; i < invokeList.Length; i++)
            {
                var invokeAction = invokeList[i];
                var method = invokeAction.Method;
                sb.Append($"[Target]<color=yellow>{invokeAction.Target}</color>\t\t[Method]<color=yellow>{method.Name}</color>\n");
            }
            return sb.ToString();
        }

        public MsgDispatcherTrackerViewItem(int id, Delegate d) : base(id)
        {
            var msgType = (MsgEventType)id;
            invokeList = d.GetInvocationList();
            MsgEventName = msgType.ToString();
            MsgId = id;
            MsgIdStr = id.ToString();
            ListenterCount = invokeList.Length;
            ListenterCountStr = ListenterCount.ToString();
        }
    }

    public class MsgDispatcherTrackerTreeView : TreeView
    {
        const string sortedColumnIndexStateKey = "UniTaskTrackerTreeView_sortedColumnIndex";

        public IReadOnlyList<TreeViewItem> CurrentBindingItems;

        public MsgDispatcherTrackerTreeView()
            : this(new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[]
            {
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("MsgTypeName"), width = 20},
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("MsgId"), width = 10},
                new MultiColumnHeaderState.Column() { headerContent = new GUIContent("ListenCount"), width = 10},
            })))
        {
        }

        MsgDispatcherTrackerTreeView(TreeViewState state, MultiColumnHeader header)
            : base(state, header)
        {
            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            header.sortingChanged += Header_sortingChanged;

            header.ResizeToFit();
            Reload();

            header.sortedColumnIndex = SessionState.GetInt(sortedColumnIndexStateKey, 1);
        }

        public void ReloadAndSort()
        {
            var currentSelected = this.state.selectedIDs;
            Reload();
            Header_sortingChanged(this.multiColumnHeader);
            this.state.selectedIDs = currentSelected;
        }

        private void Header_sortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SessionState.SetInt(sortedColumnIndexStateKey, multiColumnHeader.sortedColumnIndex);
            var index = multiColumnHeader.sortedColumnIndex;
            var ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);

            var items = rootItem.children.Cast<MsgDispatcherTrackerViewItem>();

            IOrderedEnumerable<MsgDispatcherTrackerViewItem> orderedEnumerable;
            switch (index)
            {
                case 0:
                    orderedEnumerable = ascending ? items.OrderBy(item => item.MsgEventName) : items.OrderByDescending(item => item.MsgEventName);
                    break;
                case 1:
                    orderedEnumerable = ascending ? items.OrderBy(item => item.MsgId) : items.OrderByDescending(item => item.MsgId);
                    break;
                case 2:
                    orderedEnumerable = ascending ? items.OrderBy(item => item.ListenterCount) : items.OrderByDescending(item => item.ListenterCount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }

            CurrentBindingItems = rootItem.children = orderedEnumerable.Cast<TreeViewItem>().ToList();
            BuildRows(rootItem);
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { depth = -1 };

            var children = new List<TreeViewItem>();

            foreach (var kv in MsgDispatcher.eventTable)
            {
                children.Add(new MsgDispatcherTrackerViewItem(kv.Key, kv.Value));
            }

            CurrentBindingItems = children;
            root.children = CurrentBindingItems as List<TreeViewItem>;
            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as MsgDispatcherTrackerViewItem;

            for (var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                var rect = args.GetCellRect(visibleColumnIndex);
                var columnIndex = args.GetColumn(visibleColumnIndex);

                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStyle.alignment = TextAnchor.MiddleLeft;
                switch (columnIndex)
                {
                    case 0:
                        EditorGUI.LabelField(rect, item.MsgEventName, labelStyle);
                        break;
                    case 1:
                        EditorGUI.LabelField(rect, item.MsgIdStr, labelStyle);
                        break;
                    case 2:
                        EditorGUI.LabelField(rect, item.ListenterCountStr, labelStyle);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                }
            }
        }
    }
}
