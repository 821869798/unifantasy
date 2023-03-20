using UnityEngine;


namespace UniFan
{
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;

        /**
           Returns the instance of this singleton.
        */
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    NewInstance();

                    if (instance == null)
                    {
                        GameObject Obj = new GameObject(typeof(T).ToString());
                        instance = Obj.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        private static void NewInstance()
        {
            if (!instance)
            {
                instance = (T)FindObjectOfType(typeof(T));
            }
        }
    }

}
