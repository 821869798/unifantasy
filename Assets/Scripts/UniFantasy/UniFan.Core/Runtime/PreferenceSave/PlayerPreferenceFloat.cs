
namespace UniFan
{
    public class PlayerPreferenceFloat : PreferenceBase<float>
    {
        public PlayerPreferenceFloat(string key, float defaultValue = 0) : base(key, defaultValue)
        {
        }

        protected override float ReadValue(float defaultValue)
        {
            return UnityEngine.PlayerPrefs.GetFloat(key, defaultValue);
        }

        protected override void SaveValue(float value)
        {
            UnityEngine.PlayerPrefs.SetFloat(key, value);
        }

        public static implicit operator float(PlayerPreferenceFloat pref)
        {
            return pref.Value;
        }
    }

}
