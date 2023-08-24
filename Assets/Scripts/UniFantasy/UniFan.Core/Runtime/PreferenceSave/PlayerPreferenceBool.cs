
namespace UniFan
{
    public class PlayerPreferenceBool : PreferenceBase<bool>
    {
        public PlayerPreferenceBool(string key, bool defaultValue = false) : base(key, defaultValue)
        {
        }

        protected override bool ReadValue(bool defaultValue)
        {
            return UnityEngine.PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        protected override void SaveValue(bool value)
        {
            UnityEngine.PlayerPrefs.GetInt(key, value ? 1 : 0);
        }

        public static implicit operator bool(PlayerPreferenceBool pref)
        {
            return pref.Value;
        }
    }
}
