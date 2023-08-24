#if UNITY_EDITOR

namespace UniFan
{
    public abstract class EditorPreferenceBase<T> where T : System.IEquatable<T>
    {
        private bool inited = false;
        private T value;
        private T defaultValue { get; }
        public string key { private set; get; }

        public EditorPreferenceBase(string key, T defaultValue)
        {
            this.key = key;
            this.defaultValue = defaultValue;
            this.value = defaultValue;
        }

        protected abstract T ReadValue(T defaultValue);

        protected abstract void SaveValue(T value);


        public T Value
        {
            get
            {
                if (!inited)
                {
                    value = ReadValue(this.defaultValue);
                    inited = true;
                }
                return value;
            }
            set
            {
                T newValue = value;
                if (!newValue.Equals(this.value))
                {
                    this.value = newValue;
                    SaveValue(value);
                }
            }
        }

        public override string ToString()
        {
            return value.ToString();
        }


    }
}

#endif