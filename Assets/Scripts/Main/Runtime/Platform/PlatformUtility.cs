/// 请不要引用任何命名空间!!! 因为有宏，怕有误删，直接使用这样的格式调用 namesapce1.namesapce2.class.func
/// 
namespace Main
{
    public class PlatformUtility
    {

        //获取剩余空间大小，比特
        public static long GetFreeDiskSpace()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                return AndroidInterface.AndroidUtil.CallStatic<long>("GetAvailableSize");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
#endif

#if UNITY_IOS
        return _IOS_GetFreeDiskSpace();
#endif

            return 1024;
        }

#if UNITY_IOS
        [System.Runtime.InteropServices.DllImport("__Internal")]
        static extern long _IOS_GetFreeDiskSpace();
#endif

    }
}
