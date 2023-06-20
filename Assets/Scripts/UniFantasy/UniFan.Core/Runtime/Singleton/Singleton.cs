
namespace UniFan
{
    public class Singleton<T> where T : Singleton<T>, new()
    {

        private static T m_instance;

        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new T();
                    m_instance.Initialize();
                }
                return m_instance;
            }
        }

        protected virtual void Initialize()
        {
        }
    }
}