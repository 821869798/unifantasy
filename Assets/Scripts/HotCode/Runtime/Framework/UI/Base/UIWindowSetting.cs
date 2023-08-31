using UniFan.Res;

namespace HotCode.Framework
{
    /// <summary>
    /// 一些需要用的数据
    /// </summary>
    public class UIWindowSetting
    {
        public EUILayer layer { get; internal set; }
        public int sortOrder { get; internal set; }
        public bool permanent { get; internal set; }
        public ResLoader resloader { get; internal set; }
    }
}
