#if UNITY_EDITOR

namespace UniFan
{
    public class EditorPreferenceInt : PreferenceBase<int>
    {
        public EditorPreferenceInt(string key, int defaultValue = 0) : base(key, defaultValue)
        {
        }

        protected override int ReadValue(int defaultValue)
        {
            return UnityEditor.EditorPrefs.GetInt(key, defaultValue);
        }

        protected override void SaveValue(int value)
        {
            UnityEditor.EditorPrefs.SetInt(key, value);
        }

        public static implicit operator int(EditorPreferenceInt pref)
        {
            return pref.Value;
        }
    }

}
#endif