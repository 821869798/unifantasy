

namespace UniFan
{
    public class PlayerPreferenceString : PreferenceBase<string>
    {
        public PlayerPreferenceString(string key, string defaultValue) : base(key, defaultValue)
        {
        }

        protected override string ReadValue(string defaultValue)
        {
            return UnityEngine.PlayerPrefs.GetString(key, defaultValue);
        }

        protected override void SaveValue(string value)
        {
            UnityEngine.PlayerPrefs.SetString(key, value);
        }

        public static implicit operator string(PlayerPreferenceString pref)
        {
            return pref.Value;
        }
    }

}
