#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UniFan;
using UnityEditor;
using UnityEngine;

namespace UniFan
{
    public class EditorBehaviourHelper : SingletonMono<EditorBehaviourHelper>
    {
        private System.Action onApplicationQuit;

        private readonly List<WeakReference> subscribers = new List<WeakReference>();

        public static bool isEditorPlaying => EditorApplication.isPlaying;

        private void Awake()
        {
            this.gameObject.name = "[EditorOnly]EditorBehaviourHelper";
            UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        public static void AddApplicationQuitEvent(Action action)
        {
            if (!isEditorPlaying)
            {
                return;
            }
            EditorBehaviourHelper.Instance.onApplicationQuit += action;
        }

        public static void RemoveApplicationQuitEvent(Action action)
        {
            if (instance == null)
            {
                // 防止Destory的时候又创建
                return;
            }
            instance.onApplicationQuit -= action;
        }

        public static void AddAppQuitDispose(IDisposable handler)
        {
            if (!isEditorPlaying)
            {
                return;
            }

            // 使用弱引用存储事件处理程序
            var instance = EditorBehaviourHelper.Instance;
            instance.subscribers.Add(new WeakReference(handler));
        }

        public static void RemoveAppQuitDispose(IDisposable handler)
        {
            if (instance == null)
            {
                // 防止Destory的时候又创建
                return;
            }

            var subscribers = instance.subscribers;
            // 删除已释放的引用
            for (int i = 0; i < subscribers.Count; i++)
            {
                var wr = subscribers[i];
                if (!wr.IsAlive || wr.Target == handler)
                {
                    subscribers.RemoveAt(i);
                    i--;
                }
            }

        }

        private void InvokeAppQuitDispose()
        {
            // 触发事件，删除已释放的引用
            foreach (var weakReference in subscribers.ToList())
            {
                if (weakReference.IsAlive)
                {
                    var action = (IDisposable)weakReference.Target;
                    action?.Dispose();
                }
                else
                {
                    subscribers.Remove(weakReference);
                }
            }
        }


        private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    {
                        EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
                        onApplicationQuit?.Invoke();
                        InvokeAppQuitDispose();
                    }
                    break;
            }
        }

        private void OnDestroy()
        {
            onApplicationQuit = null;
            subscribers.Clear();
        }
    }
}

#endif