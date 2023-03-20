
namespace UniFan
{
    public abstract class ManagerSingleton<T> : ManagerBase where T : ManagerSingleton<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                    _instance.InitManager();
                }
                return _instance;
            }
        }

        protected virtual void InitManager()
        {

        }
    }

}

