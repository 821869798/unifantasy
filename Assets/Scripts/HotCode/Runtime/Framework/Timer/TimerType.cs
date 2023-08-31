
namespace HotCode.Framework
{
    public enum TimerType
    {
        /// <summary>
        /// 按帧刷新
        /// </summary>
        Frame,
        /// <summary>
        /// 按带缩放时间刷新
        /// </summary>
        Time,
        /// <summary>
        /// 按真实时间刷新
        /// </summary>
        UnscaledTime
    }
}
