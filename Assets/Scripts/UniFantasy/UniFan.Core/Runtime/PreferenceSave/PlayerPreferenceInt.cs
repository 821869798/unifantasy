
namespace UniFan
{
    public class PlayerPreferenceInt : PreferenceBase<int>
    {
        public PlayerPreferenceInt(string key, int defaultValue = 0) : base(key, defaultValue)
        {
        }

        protected override int ReadValue(int defaultValue)
        {
            return UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
        }

        protected override void SaveValue(int value)
        {
            UnityEngine.PlayerPrefs.SetInt(key, value);
        }

        public static implicit operator int(PlayerPreferenceInt pref)
        {
            return pref.Value;
        }
    }

}
