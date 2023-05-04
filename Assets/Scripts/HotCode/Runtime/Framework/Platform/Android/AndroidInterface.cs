using System;
using UnityEngine;

namespace HotCode.Framework
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
    }

}
