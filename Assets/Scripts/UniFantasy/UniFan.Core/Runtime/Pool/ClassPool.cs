using System;
using System.Collections.Generic;


namespace UniFan
{
    public class ClassPool
    {
        private static Dictionary<Type, Stack<IReusableClass>> _reusablePool = new Dictionary<Type, Stack<IReusableClass>>();

#if UNITY_EDITOR
        private static Dictionary<Type, HashSet<IReusableClass>> _reusableCheckPool = new Dictionary<Type, HashSet<IReusableClass>>();
#endif

        public static T Get<T>() where T : class, IReusableClass, new()
        {
            Type type = typeof(T);
            Stack<IReusableClass> classPool = null;
            if (_reusablePool.TryGetValue(type, out classPool))
            {
                if (classPool.Count > 0)
                {
                    var classRef = classPool.Pop();
#if UNITY_EDITOR
                    if (!_reusableCheckPool[type].Remove(classRef))
                    {
                        UnityEngine.Debug.LogError("ClassPool Get Error: " + type.Name);
                    }
#endif
                    return classRef as T;
                }
            }
            return new T();
        }

        /// <summary>
        /// 使用泛型的typeof(T)，效率比GetType高，推荐使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public static void Put<T>(T data) where T : class, IReusableClass
        {
            if (data == null) return;
            Type type = typeof(T);
#if UNITY_EDITOR
            Type checkType = data.GetType();
            if (checkType != type)
            {
                UnityEngine.Debug.LogWarning($"ClassPool Put Type Error, Current Type:" + type.Name + " Real Type:" + checkType.Name);
            }
#endif
            PutInternal(type, data);
        }

        /// <summary>
        /// 使用GetType获取类型，只用于不确定类型的场景
        /// </summary>
        /// <param name="data"></param>
        public static void Put(IReusableClass data)
        {
            if (data == null) return;
            Type type = data.GetType();
            PutInternal(type, data);
        }

        private static void PutInternal(Type type, IReusableClass data)
        {
            Stack<IReusableClass> classPool = null;
            if (!_reusablePool.TryGetValue(type, out classPool))
            {
                classPool = new Stack<IReusableClass>();
                _reusablePool[type] = classPool;
#if UNITY_EDITOR
                _reusableCheckPool[type] = new HashSet<IReusableClass>();
#endif
            }
            if (classPool.Count < data.MaxStore)
            {
                data.OnReset();
#if UNITY_EDITOR
                if (!_reusableCheckPool[type].Add(data))
                {
                    UnityEngine.Debug.LogError("ClassPool Put Error: " + type.Name);
                    return;
                }
#endif
                classPool.Push(data);
            }
        }

        public void Reset()
        {
            _reusablePool.Clear();
        }

#if UNITY_EDITOR
        public static int GetPoolCacheCount<T>() where T : class, IReusableClass
        {
            Type type = typeof(T);
            if (_reusablePool.TryGetValue(type, out var classPool))
            {
                return classPool.Count;
            }
            return 0;
        }
#endif
    }
}
