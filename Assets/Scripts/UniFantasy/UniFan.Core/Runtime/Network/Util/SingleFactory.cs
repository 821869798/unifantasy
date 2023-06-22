using System;

namespace UniFan.Network
{

    public abstract class SingleFactory<T> where T : SingleFactory<T>, new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                    instance.Initialize();
                }
                return instance;
            }
        }

        protected virtual void Initialize()
        {

        }
    }
}


