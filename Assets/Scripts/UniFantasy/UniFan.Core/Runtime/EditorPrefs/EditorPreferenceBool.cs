#if UNITY_EDITOR

namespace UniFan
{
    public class EditorPreferenceBool : EditorPreferenceBase<bool>
    {
        public EditorPreferenceBool(string key, bool defaultValue = false) : base(key, defaultValue)
        {
        }

        protected override bool ReadValue(bool defaultValue)
        {
            return UnityEditor.EditorPrefs.GetBool(key, defaultValue);
        }

        protected override void SaveValue(bool value)
        {
            UnityEditor.EditorPrefs.SetBool(key, value);
        }

        public static implicit operator bool(EditorPreferenceBool pref)
        {
            return pref.Value;
        }
    }
}

#endif