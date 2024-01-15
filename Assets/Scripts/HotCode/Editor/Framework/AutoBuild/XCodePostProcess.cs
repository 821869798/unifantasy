using System;
using UnityEngine;
using System.IO;
using UnityEditor.Build.Reporting;
using UnityEditor;
#if UNITY_EDITOR && UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif


namespace AutoBuild
{
    public static class XCodePostProcess
    {
        public static void PostProcessBuild(BuildReport report)
        {
#if UNITY_IOS
            if (report.summary.platform != BuildTarget.iOS)
            {
                Debug.LogError("Current BuildTarget not iOS");
                return;
            }

            // 获取Xcode项目路径
            string projPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
            // 初始化PBXProject
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            // 获取目标GUID
            var targetGuid = proj.GetUnityMainTargetGuid();
            var targetGuidUnityFramework = proj.GetUnityFrameworkTargetGuid();

            // 禁用Bitcode
            proj.SetBuildProperty(targetGuidUnityFramework, "ENABLE_BITCODE", "NO");
            proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");

            // 应用修改后的设置
            proj.WriteToFile(projPath);

            // 编辑plist 文件
            EditorPlist(report.summary.outputPath);

#endif
        }

        private static void EditorPlist(string pathToBuiltProject)
        {
#if UNITY_IOS

            // Get plist
            string plistPath = pathToBuiltProject + "/Info.plist";
            UnityEditor.iOS.Xcode.PlistDocument plist = new UnityEditor.iOS.Xcode.PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            // Get root
            UnityEditor.iOS.Xcode.PlistElementDict rootDict = plist.root;

            // if (IsInland)
            // {
            //     //语言
            //     rootDict.SetString("CFBundleDevelopmentRegion", "zh-Hans");
            //
            //     //权限
            //     rootDict.SetString("NSUserTrackingUsageDescription", "为给您更好的游戏体验和服务，需要您允许我们获取设备信息");
            //     rootDict.SetString("NSPhotoLibraryUsageDescription", "此 App 需要您的同意才能访问相册");
            //     rootDict.SetString("NSPhotoLibraryAddUsageDescription", "此 App 需要您的同意才能访问相册");
            //     rootDict.SetString("NSLocationWhenInUseUsageDescription", "此 App 需要您的同意才能使用定位");
            //     rootDict.SetString("NSAppleMusicUsageDescription", "此 App 需要您的同意才能读取媒体资料库");
            // }
            // else if(IsOversea)
            // {
            //     //解决日美的facebook请求登陆时显示的名字是project3的问题
            //     rootDict.SetString("CFBundleName",  PlayerSettings.productName);
            // }

            //出口合规证明 false
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            //屏蔽iOS深色模式
            rootDict.SetString("UIUserInterfaceStyle", "Light");

            if (AutoBuildUtility.ContainScriptingDefineSymbol("GameDev"))
            {
                //开发者模式
                rootDict.SetBoolean("UIFileSharingEnabled", true);
            }


            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());

#endif
        }

        public static void Log(string message)
        {
            UnityEngine.Debug.Log("PostProcess: " + message);
        }
    }
}