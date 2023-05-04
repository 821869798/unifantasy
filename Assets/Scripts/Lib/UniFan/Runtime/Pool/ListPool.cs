using System;
using System.Collections.Generic;


namespace UniFan
{
    public static class ListPool<T>
    {
        private static readonly Stack<List<T>> pool = new Stack<List<T>>();
        public static int MaxPoolNum = 20;

        public static List<T> Get()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }

            List<T> list = new List<T>();
            return list;
        }

        public static void Put(List<T> list)
        {
            if (pool.Count >= MaxPoolNum)
                return;
            list.Clear();
            pool.Push(list);

        }

        public static void Clear()
        {
            pool.Clear();
        }

    }
}