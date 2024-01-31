using System.Collections.Generic;

namespace MainEditor.HotUpdate
{
    public class PatchManifestGenerateConfig
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string filePath;

        /// <summary>
        /// 生成时需要添加的前缀路径
        /// </summary>
        public string addPrefixPath;

        /// <summary>
        /// 需要剔除的文件后缀(例如.meta)
        /// </summary>
        public HashSet<string> blackFileExt = new HashSet<string>();

        /// <summary>
        /// 需要剔除的文件名(完整的相对filePath的相对路径)
        /// </summary>
        public HashSet<string> blackFiles = new HashSet<string>();
    }
}
