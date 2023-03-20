using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniFan
{
    public enum eLanguageType
    {
        ZH_CN = 0,  //简体中文
        ZH_TW,  //繁体中文
        EN_US,  //英文
        JA_JP,  //日文
        KO_KR,  //韩文
    }
    public enum eUIAssetComType
    {
        None,
        ExText,
    }


    /// <summary>
    /// 本地化装饰屏蔽方式
    /// </summary>
    public enum LangDecorBlockType
    {
        None = 0,               //无屏蔽
        ObjectActive = 1,       //GameObject显影方式
        Scale = 2,              //缩放方式
    }

    public static class LanguageGlobal
    {
        public static readonly string ZHCNLangKey = eLanguageType.ZH_CN.ToString();

        public static readonly int LanguageCount = System.Enum.GetValues(typeof(eLanguageType)).Length;

        //当前的语言
        public static eLanguageType language { get; private set; } = eLanguageType.ZH_CN;

        //语音的语言
        public static eLanguageType voiceLanguage { get; private set; } = eLanguageType.JA_JP;

        private static string LangStr { get; set; }

        private static string LangStrLower { get; set; }

        private static string VoiceLangStr { get; set; }

        private static string VoiceLangStrLower { get; set; }


#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void Init()
        {
            Debug.Log("Init->Lanauge: " + LanguageEditorMode);
            Debug.Log("Init->VoiceLanguage: " + LanguageVoiceEditorMode);
        }


        private static int _languageEditorMode = -1;
        const string kLanguageEditorMode = "LanguageEditorMode";

        private static int _languageVoiceEditorMode = -1;
        const string kLanguageVoiceEditorMode = "LanguageVoiceEditorMode";

        public static eLanguageType LanguageEditorMode
        {
            get
            {
                if (_languageEditorMode == -1)
                {
                    var s = EditorUserSettings.GetConfigValue(kLanguageEditorMode);
                    int.TryParse(s, out var langint);
                    _languageEditorMode = langint;
                }

                return (eLanguageType)_languageEditorMode;
            }
            set
            {
                int newValue = (int)value;
                if (newValue != _languageEditorMode)
                {
                    _languageEditorMode = newValue;
                    EditorUserSettings.SetConfigValue(kLanguageEditorMode, newValue.ToString());
                }
            }
        }

        public static eLanguageType LanguageVoiceEditorMode
        {
            get
            {
                if (_languageVoiceEditorMode == -1)
                {
                    var s = EditorUserSettings.GetConfigValue(kLanguageVoiceEditorMode);
                    if (int.TryParse(s, out var langint))
                    {
                        _languageVoiceEditorMode = langint;
                    }
                    else
                    {
                        _languageVoiceEditorMode = (int)eLanguageType.JA_JP;
                    }
                }
                return (eLanguageType)_languageVoiceEditorMode;
            }
            set
            {
                int newValue = (int)value;
                if (newValue != _languageVoiceEditorMode)
                {
                    _languageVoiceEditorMode = newValue;
                    EditorUserSettings.SetConfigValue(kLanguageVoiceEditorMode, newValue.ToString());
                }
            }
        }

#endif

        public static void InitLanguage()
        {
#if UNITY_EDITOR
            ChangeRunTimeLanguage(LanguageEditorMode, LanguageVoiceEditorMode);
#else
        //非编辑器
        //var lang = GetDefaultLanguage(ChannelConfig.ChannelId);
        //ChangeRunTimeLanguage(lang);
#endif
        }

        public static eLanguageType GetDefaultLanguage(int channelId)
        {
            if (channelId < 100)
            {
                return eLanguageType.ZH_CN;
            }
            if (channelId >= 100 && channelId < 200)
            {
                return eLanguageType.EN_US;
            }
            else if (channelId >= 200 && channelId < 300)
            {
                return eLanguageType.JA_JP;
            }
            else if (channelId >= 300 && channelId < 400)
            {
                return eLanguageType.KO_KR;
            }
            else if (channelId >= 400 && channelId < 500)
            {
                return eLanguageType.ZH_TW;
            }
            else
            {
                Debug.LogError("chanelId is not support,id:" + channelId);
                return eLanguageType.ZH_CN;
            }
        }


        public static void ChangeRunTimeLanguage(eLanguageType langType, eLanguageType voiceType = eLanguageType.JA_JP)
        {
            language = langType;
            voiceLanguage = voiceType;
            InitLanguageStr();
        }


        private static void InitLanguageStr()
        {
            LangStr = GetLangStrByType(language);
            LangStrLower = GetLangStrByType(language, true);
            VoiceLangStr = GetLangStrByType(voiceLanguage);
            VoiceLangStrLower = GetLangStrByType(voiceLanguage, true);
        }

        /// <summary>
        /// 加载当前语言所需
        /// </summary>
        /// <returns></returns>
        //public static IEnumerator LoadLanguage()
        //{
        //    //加载Asset文件
        //    yield return LanguageHelper.Instance.LoadLangAsset(language);
        //    //加载语言包文件
        //    LanguageHelper.Instance.LoadLanguageBytes(language);
        //}

        public static int GetLanguageInt()
        {
            return (int)language;
        }

        public static string GetLanguageStr(bool toLower = false)
        {
            if (toLower)
            {
                return LangStrLower;
            }
            return LangStr;
        }

        public static string GetVoiceLanguageStr(bool toLower = false)
        {
            if (toLower)
            {
                return VoiceLangStrLower;
            }
            return VoiceLangStr;
        }

        public static string GetLangStrByType(eLanguageType type, bool toLower = false)
        {
            string temp;
            switch (type)
            {
                case eLanguageType.ZH_TW:
                    temp = "ZH_TW";
                    break;
                case eLanguageType.EN_US:
                    temp = "EN_US";
                    break;
                case eLanguageType.JA_JP:
                    temp = "JA_JP";
                    break;
                case eLanguageType.KO_KR:
                    temp = "KO_KR";
                    break;
                default:
                    temp = "ZH_CN";
                    break;
            }
            if (toLower)
            {
                temp = temp.ToLower();
            }
            return temp;
        }

        #region 自动适配本机设置的系统语言，暂时不用

        /// <summary>切换环境语言 </summary>
        private static void SwitchGameLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Chinese:
                    language = GetSystemLanguage();
                    break;
                case SystemLanguage.ChineseSimplified:
                    language = eLanguageType.ZH_CN;
                    break;
                case SystemLanguage.ChineseTraditional:
                    language = eLanguageType.ZH_TW;
                    break;
                case SystemLanguage.Japanese:
                    language = eLanguageType.JA_JP;
                    break;
                case SystemLanguage.Korean:
                    language = eLanguageType.KO_KR;
                    break;
                default:
                    language = eLanguageType.EN_US;
                    break;
            }
        }

        //#if UNITY_IPHONE && !UNITY_EDITOR
        //    [DllImport("__Internal",CallingConvention = CallingConvention.Cdecl)]
        //    private static extern string CurIOSLang();
        //#else
        // ios手机的当前语言 "en"、“zh"、“zh-Hans"、"zh-Hant"
        private static string CurIOSLang()
        {
            return "zh-Hans";
        }
        //#endif
        private static eLanguageType GetSystemLanguage()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                string name = CurIOSLang();
                if (!name.StartsWith("zh-Hans"))
                {
                    return eLanguageType.ZH_TW;
                }
            }
            return eLanguageType.ZH_CN;
        }

        #endregion

    }

}