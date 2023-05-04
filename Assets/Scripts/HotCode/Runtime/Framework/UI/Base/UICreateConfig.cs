using System;
using System.Collections.Generic;

namespace HotCode.Framework
{
    /// <summary>
    /// 用来创建UI的配置
    /// </summary>
    public interface IUICreateConfig
    {
        /// <summary>
        /// UI prefab的资源名
        /// </summary>
        public string prefabName { get; }
        /// <summary>
        /// prefab的父级路径(默认不需要)
        /// </summary>
        public string parentPath { get; }
        /// <summary>
        /// UI所在的层级
        /// </summary>
        public EUILayer layer { get; }
        /// <summary>
        /// 该UI是否是常驻的，不会被UIManager普通的DeleteAll删除掉
        /// </summary>
        public bool permanent { get; }
    }

    public class UICreateConfig : IUICreateConfig
    {
        public string prefabName { get; set; }
        public string parentPath { get; set; }
        public EUILayer layer { get; set; }
        public bool permanent { get; set; }

    }
}
