#if UNITY_EDITOR

namespace UniFan
{
    public class EditorPreferenceString : EditorPreferenceBase<string>
    {
        public EditorPreferenceString(string key,string defaultValue) : base(key, defaultValue)
        {
        }

        protected override string ReadValue(string defaultValue)
        {
            return UnityEditor.EditorPrefs.GetString(key, defaultValue);
        }

        protected override void SaveValue(string value)
        {
            UnityEditor.EditorPrefs.SetString(key, value);
        }

        public static implicit operator string(EditorPreferenceString pref)
        {
            return pref.Value;
        }
    }

}
#endif