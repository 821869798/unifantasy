using UniFan.Res;

namespace HotCode.Framework
{
    public abstract class UIBaseWindow : UIBaseNode
    {
        /// <summary>
        /// 用来创建UI的设置
        /// </summary>
        public abstract IUICreateConfig createConfig { get; }

        /// <summary>
        /// 实际生效的UI设置，一般是一样的，除非创建UI的时候用自定义的
        /// </summary>
        public UIWindowSetting winSetting { get; internal set; } = new UIWindowSetting();

        /// <summary>
        /// 获取这个Window的Resloader，适用于跟随window的生命周期一起销毁的资源
        /// 如果是更换卸载的这种资源，请别用这个接口
        /// </summary>
        public ResLoader GetWindowResloader()
        {
            return winSetting.resloader;
        }
    }
}

