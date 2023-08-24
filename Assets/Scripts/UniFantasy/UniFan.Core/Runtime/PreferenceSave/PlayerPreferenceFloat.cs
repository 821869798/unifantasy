
namespace UniFan
{
    public class PlayerPreferenceFloat : PreferenceBase<float>
    {
        public PlayerPreferenceFloat(string key, float defaultValue = 0) : base(key, defaultValue)
        {
        }

        protected override float ReadValue(float defaultValue)
        {
            return UnityEditor.EditorPrefs.GetFloat(key, defaultValue);
        }

        protected override void SaveValue(float value)
        {
            UnityEditor.EditorPrefs.SetFloat(key, value);
        }

        public static implicit operator float(PlayerPreferenceFloat pref)
        {
            return pref.Value;
        }
    }

}
