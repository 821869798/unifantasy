using UnityEngine;

namespace Main
{
    public static class AndroidInterface
    {

        private static AndroidJavaObject _gameActivity;
        public static AndroidJavaObject GameActivity
        {
            get
            {
                if (_gameActivity == null)
                {
                    AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    _gameActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                }
                return _gameActivity;
            }
        }


        private static AndroidJavaClass _androidUtil;
        public static AndroidJavaClass AndroidUtil
        {
            get
            {
                if (_androidUtil == null)
                {
                    _androidUtil = new AndroidJavaClass("com.unitygame.UnityAndroidUtil");
                }
                return _androidUtil;
            }
        }

        /// <summary>
        /// 获取状态栏高度
        /// AndroidJavaClass的调用泛型调用需要放到aot程序集里
        /// </summary>
        /// <returns></returns>
        public static int GetStatusHeight(int defaultValue = 0)
        {
            try
            {
                return AndroidUtil.CallStatic<int>("GetStatusHeight");
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool CallStatic_FuncBool(string method, bool defaultValue = false)
        {
            try
            {
                return AndroidUtil.CallStatic<bool>(method);
            }
            catch
            {
                return defaultValue;
            }
        }

    }

}
