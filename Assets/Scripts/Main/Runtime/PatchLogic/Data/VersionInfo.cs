using System;

namespace Main.HotUpdate
{
    [Serializable]
    public class GameVersionInfo
    {
        /// <summary>
        /// 整包版本
        /// </summary>
        public string appVersion;
        /// <summary>
        /// 资源版本
        /// </summary>
        public string resVersion;

        [System.NonSerialized]
        public Version appVersionObject;

        [System.NonSerialized]
        public Version resVersionObject;

        // todo，可以添加一个url参数来替换热更地址，测试调试用

        public void InitRead()
        {
            appVersionObject = new Version(appVersion);
            resVersionObject = new Version(resVersion);
        }

        public void InitWrite()
        {
            appVersion = appVersionObject.ToString();
            resVersion = resVersionObject.ToString();
        }

    }
}
