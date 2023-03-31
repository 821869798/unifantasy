using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace UniFan
{

    public enum ResPathType
    {
        StreamingAssets,
        Persistent,
    }

    public class FilePathHelper : Singleton<FilePathHelper>
    {
        //各个平台的StreamAssets路径
        public string StreamingAssetsPath { private set; get; }

        //各个平台的StreamAssets的WWW加载方式的路径
        public string StreamingAssetsPathForWWW { private set; get; }

        //各个平台的PersistentDataPath路径,可读写
        public string PersistentDataPath { private set; get; }

        //各个平台的PersistentDataPath的WWW加载方式的路径,可读写
        public string PersistentDataPathForWWW { private set; get; }

        protected override void Initialize()
        {

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        StreamingAssetsPathForWWW = string.Format("file:///{0}/StreamingAssets/", Application.dataPath);
        StreamingAssetsPath = string.Format("{0}/StreamingAssets/", Application.dataPath);
        PersistentDataPathForWWW = string.Format("file:///{0}/StreamingAssets/", Application.dataPath);
        PersistentDataPath = string.Format("{0}/StreamingAssets/", Application.dataPath);
#elif UNITY_ANDROID
            StreamingAssetsPathForWWW = string.Format("jar:file://{0}!/assets/", Application.dataPath);
            StreamingAssetsPath = string.Format("{0}!/assets/", Application.dataPath);
#if UNITY_2021_1_OR_NEWER
            StreamingAssetsPath = string.Concat(Application.streamingAssetsPath, "/");
#else
            StreamingAssetsPath = string.Format("{0}!/assets/", Application.dataPath);
#endif
            PersistentDataPathForWWW = string.Format("file://{0}/", Application.persistentDataPath);
            PersistentDataPath = string.Concat(Application.persistentDataPath, "/");
#elif UNITY_IOS && !UNITY_EDITOR
        StreamingAssetsPathForWWW = string.Format("file://{0}/Raw/", Application.dataPath);
        StreamingAssetsPath = string.Format("{0}/Raw/", Application.dataPath);
        PersistentDataPathForWWW = string.Format("file://{0}/", Application.persistentDataPath);
        PersistentDataPath = string.Concat(Application.persistentDataPath,"/");
#else
            //编辑器模式下
            var assetPath = Path.GetDirectoryName(Application.dataPath);
            StreamingAssetsPathForWWW = string.Format("file://{0}/StreamingAssets/", Application.dataPath);
            StreamingAssetsPath = string.Format("{0}/StreamingAssets/", Application.dataPath);
            PersistentDataPathForWWW = string.Format("file://{0}/EditorPersistent/", assetPath);
            PersistentDataPath = string.Format("{0}/EditorPersistent/", assetPath);
#endif
        }

        public string GetBundlePath(string bundlePath, out ResPathType pathType)
        {
            var path = string.Concat(PersistentDataPath, PathConsts.AssetbundleLoadPath, bundlePath);
            if (File.Exists(path))
                pathType = ResPathType.Persistent;
            else
            {
                pathType = ResPathType.StreamingAssets;
                path = string.Concat(StreamingAssetsPath, PathConsts.AssetbundleLoadPath, bundlePath);
            }
            return path;
        }


        public string GetBundlePathForWWW(string bundlePath)
        {
            return string.Concat(StreamingAssetsPathForWWW, PathConsts.AssetbundleLoadPath, bundlePath);
        }

        public string GetStreamingPath(string path)
        {
            return string.Concat(StreamingAssetsPath, path);
        }

        public string GetStreamingPathForWWW(string path)
        {
            return string.Concat(StreamingAssetsPathForWWW, path);
        }

        public string GetPersistentDataPath(string path)
        {
            return string.Concat(PersistentDataPath, path);
        }

        public string GetPersistentDataPathForWWW(string path)
        {
            return string.Concat(PersistentDataPathForWWW, path);
        }

        public string GetAssetNameInBundle(string assetPath)
        {
            return Path.GetFileName(assetPath);
        }

        public const string EditorAssetPathHead = "Assets/";
        public string GetEditorAssetPath(string assetPath)
        {
            return string.Concat(EditorAssetPathHead, assetPath);
        }


        //获取各平台对应的资源目录名
        public static string GetResPlatformName()
        {
#if UNITY_STANDALONE_WIN
            return "Windows";
#elif UNITY_STANDALONE_OSX
        return "OSX";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
        return "iOS";
#endif
        }
    }

}
