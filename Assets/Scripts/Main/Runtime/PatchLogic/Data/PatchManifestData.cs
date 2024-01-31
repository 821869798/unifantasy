using System;
using System.Collections.Generic;
using System.IO;

namespace Main.HotUpdate
{
    [Serializable]
    public class PatchManifestData
    {
        /// <summary>
        /// 文件版本，通过这个可以判断结构发生变化，然后升级处理
        /// </summary>
        public int version;

        /// <summary>
        /// 资源热更版本
        /// </summary>
        public string patchVersion;

        /// <summary>
        /// 所有的文件列表
        /// </summary>
        public List<PatchFileInfo> fileInfoList = new List<PatchFileInfo>();

        public void Write2Binary(BinaryWriter bw)
        {
            bw.Write('P');
            bw.Write('M');
            bw.Write('D');
            bw.Write('\0');
            bw.Write(version);
            bw.Write(patchVersion);
            bw.Write(fileInfoList.Count);
            for (int i = 0; i < fileInfoList.Count; i++)
            {
                bw.Write(fileInfoList[i].filePath);
                bw.Write(fileInfoList[i].fileSize);
                bw.Write(fileInfoList[i].fileMd5);
            }
        }

        public void Read4Binary(BinaryReader br)
        {

            char[] fileHeadChars = br.ReadChars(4);
            if (fileHeadChars[0] != 'P' || fileHeadChars[1] != 'M' || fileHeadChars[2] != 'D' || fileHeadChars[3] != '\0')
                return;
            version = br.ReadInt32();
            patchVersion = br.ReadString();
            int bundleCount = br.ReadInt32();
            fileInfoList.Capacity = bundleCount;

            for (int i = 0; i < bundleCount; i++)
            {
                var fileInfo = new PatchFileInfo();
                fileInfo.filePath = br.ReadString();
                fileInfo.fileSize = br.ReadInt64();
                fileInfo.fileMd5 = br.ReadString();
                fileInfoList.Add(fileInfo);
            }
        }

    }

    [Serializable]
    public class PatchFileInfo
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string filePath;

        /// <summary>
        /// 文件大小
        /// </summary>
        public long fileSize;

        /// <summary>
        /// md5值
        /// </summary>
        public string fileMd5;
    }
}
