using Main;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HotCode.Framework
{
    //UI适配
    public static class UIAdaptation
    {
        public const float StandardRatio = 16.0f / 9.0f;
        public const float BackgroundRatio = 2f;


        public static float AdaptationCanvasScaler()
        {
            float curScreenRatio = (float)Screen.width / Screen.height;
            if (curScreenRatio > StandardRatio)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static Vector2 GenerateBackgroundCullSize(Vector2 referenceResolution)
        {
            var screenAspectRatio = (float)Screen.width / Screen.height;

            if (screenAspectRatio > BackgroundRatio)
            {
                var width = referenceResolution.x;
                return new Vector2(width, width / BackgroundRatio);
            }
            else
            {
                var height = referenceResolution.y;
                return new Vector2(height * BackgroundRatio, height);
            }
        }

        public static Vector2 GenerateMovieCullSize(Vector2 referenceResolution)
        {
            var screenAspectRatio = (float)Screen.width / Screen.height;

            if (screenAspectRatio > StandardRatio)
            {
                var width = referenceResolution.x;
                return new Vector2(width, width / StandardRatio);
            }
            else
            {
                var height = referenceResolution.y;
                return new Vector2(height * StandardRatio, height);
            }
        }

        public static Vector2 GenerateBackgroundStretchSize(Vector2 referenceResolution)
        {
            float curScreenRatio = (float)Screen.width / Screen.height;
            if (curScreenRatio > StandardRatio)
            {
                var height = referenceResolution.y;
                return new Vector2(height * curScreenRatio, height);
            }
            else
            {
                var width = referenceResolution.x;
                return new Vector2(width, width / curScreenRatio);
            }
        }

        public static bool HasNotch
        {
            get
            {
                bool hasNotch = Screen.width > Screen.safeArea.width;
#if UNITY_ANDROID
                hasNotch = hasNotch ? true : GetAndroidNotch();
#endif
                return hasNotch;
            }
        }

        public static float GetDefaultNotchValue()
        {
#if UNITY_ANDROID
            float notchHeight = GetAndroidStatusHeight();
            if (HasNotch)
            {
                return notchHeight / Screen.width * 100;
            }
#elif UNITY_IOS
        if (HasNotch)
        {
            return (float)(Screen.width - Screen.safeArea.width) / 2 / Screen.width * 100;
        }
#endif
            return 0;

        }

#if UNITY_ANDROID

        public static float GetAndroidStatusHeight()
        {
            return AndroidInterface.GetStatusHeight(0);
        }

        public static bool GetAndroidNotch()
        {
            var device = DeviceModel();
            if (!string.IsNullOrEmpty(device))
            {
                var method = string.Format("HasNotchIn{0}", device.Substring(0, 1).ToUpper() + device.Substring(1, device.Length - 1));
                return AndroidInterface.CallStatic_FuncBool(method, false);
            }
            return false;
        }


        static readonly string[] TargetDevice = new string[] { "huawei", "xiaomi", "oppo", "vivo" };

        static string DeviceModel()
        {
            var deviceName = SystemInfo.deviceModel.ToLower();
            foreach (var device in TargetDevice)
            {
                if (deviceName.Contains(device))
                    return device;
            }

            return null;
        }
#endif

    }

}
