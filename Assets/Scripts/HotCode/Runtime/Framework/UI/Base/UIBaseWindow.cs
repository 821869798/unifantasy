using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}

