using System;
using System.Collections.Generic;


namespace UniFan
{
    public class ClassPool
    {
        private static Dictionary<Type, Stack<IReusableClass>> _reusablePool = new Dictionary<Type, Stack<IReusableClass>>();

        public static T Get<T>() where T : class, IReusableClass, new()
        {
            Type type = typeof(T);
            Stack<IReusableClass> classPool = null;
            if (_reusablePool.TryGetValue(type, out classPool))
            {
                if (classPool.Count > 0)
                {
                    return classPool.Pop() as T;
                }
            }
            return new T();
        }

        public static void Put<T>(T data) where T : class, IReusableClass, new()
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
            Stack<IReusableClass> classPool = null;
            if (!_reusablePool.TryGetValue(type, out classPool))
            {
                classPool = new Stack<IReusableClass>();
                _reusablePool[type] = classPool;
            }
            if (classPool.Count < data.MaxStore)
            {
                data.OnReset();
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
