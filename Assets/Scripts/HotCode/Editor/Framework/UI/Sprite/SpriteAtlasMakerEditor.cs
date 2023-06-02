using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace HotCode.FrameworkEditor
{
    public static class SpriteAtlasMakerEditor
    {
        public static readonly string SpriteAtlasPackSrcDir = Path.Combine(Application.dataPath, "Res", "03_AtlasClips");
        public static readonly string SpriteAtlasPackDstDir = Path.Combine(Application.dataPath, "Res", "04_SpriteAtlas");

        public const string SpriteAtlasExt = ".spriteatlasv2";

        [MenuItem("GameEditor/SpirteAtlas/MakerAllSpriteAtlas")]
        public static void MakerAllSpriteAtlas()
        {
            string[] packPaths = Directory.GetDirectories(SpriteAtlasPackSrcDir);
            if (EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2)
            {
                foreach (var path in packPaths)
                {
                    MakeSingleSpriteAtlasV2(path.Replace("\\", "/"), SpriteAtlasPackDstDir.Replace("\\", "/"));
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                foreach (var path in packPaths)
                {
                    MakeSingleSpriteAtlas(path.Replace("\\", "/"), SpriteAtlasPackDstDir.Replace("\\", "/"));
                }
                //打包
                AssetDatabase.Refresh();
                SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
                AssetDatabase.SaveAssets();
            }

        }

        public static void MakeSingleSpriteAtlasV2(string srcDir, string dstDir)
        {
            string atlasName = Path.GetFileName(srcDir);
            string dstResPath = Path.Combine(dstDir, atlasName + SpriteAtlasExt).Replace("\\", "/");
            string dstResDir = Path.GetDirectoryName(dstResPath);
            if (!Directory.Exists(dstResDir))
            {
                Directory.CreateDirectory(dstResDir);
            }
            string headPath = Application.dataPath.Replace("\\", "/").Replace("Assets", "");
            string unityDstResPath = dstResPath.Replace(headPath, "");
            SpriteAtlasAsset spriteAtlas = null;
            bool isExists = false;
            if (File.Exists(dstResPath))
            {
                spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlasAsset>(unityDstResPath);
                if (spriteAtlas != null)
                {
                    return;
                }
            }
            if (!isExists)
            {
                spriteAtlas = new SpriteAtlasAsset();
                spriteAtlas.name = atlasName;
            }

            List<UnityEngine.Object> objList = new List<UnityEngine.Object>();
            var SpriteDirAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(srcDir.Replace(headPath, ""));
            objList.Add(SpriteDirAsset);
            spriteAtlas.Add(objList.ToArray());
            //SpriteAtlas设置
            spriteAtlas.SetIncludeInBuild(true);
            SpriteAtlasPackingSettings spriteAtlasSettting = spriteAtlas.GetPackingSettings();
            spriteAtlasSettting.enableRotation = false;
            spriteAtlasSettting.enableTightPacking = false;
            spriteAtlasSettting.padding = 2;
            spriteAtlas.SetPackingSettings(spriteAtlasSettting);
            ////平台设置
            TextureImporterPlatformSettings importerSettings_PC = new TextureImporterPlatformSettings();
            importerSettings_PC.overridden = true;
            importerSettings_PC.name = "Standalone";
            importerSettings_PC.maxTextureSize = 2048;
            importerSettings_PC.compressionQuality = 100;
            importerSettings_PC.format = TextureImporterFormat.DXT5;

            TextureImporterPlatformSettings importerSettings_Andorid = new TextureImporterPlatformSettings();
            importerSettings_Andorid.overridden = true;
            importerSettings_Andorid.name = "Android";
            importerSettings_Andorid.maxTextureSize = 2048;
            importerSettings_Andorid.format = TextureImporterFormat.ASTC_6x6;

            TextureImporterPlatformSettings importerSettings_IOS = new TextureImporterPlatformSettings();
            importerSettings_IOS.overridden = true;
            importerSettings_IOS.name = "iPhone";
            importerSettings_IOS.maxTextureSize = 2048;
            importerSettings_IOS.format = TextureImporterFormat.ASTC_6x6;
            spriteAtlas.SetPlatformSettings(importerSettings_PC);
            spriteAtlas.SetPlatformSettings(importerSettings_Andorid);
            spriteAtlas.SetPlatformSettings(importerSettings_IOS);


            if (!isExists)
            {
                SpriteAtlasAsset.Save(spriteAtlas, unityDstResPath);
            }
        }

        public static void MakeSingleSpriteAtlas(string srcDir, string dstDir)
        {
            string atlasName = Path.GetFileName(srcDir);
            string dstResPath = Path.Combine(dstDir, atlasName + SpriteAtlasExt).Replace("\\", "/");
            string dstResDir = Path.GetDirectoryName(dstResPath);
            if (!Directory.Exists(dstResDir))
            {
                Directory.CreateDirectory(dstResDir);
            }
            string headPath = Application.dataPath.Replace("\\", "/").Replace("Assets", "");
            string unityDstResPath = dstResPath.Replace(headPath, "");
            SpriteAtlas spriteAtlas = null;
            bool isExists = false;
            if (File.Exists(dstResPath))
            {
                spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(unityDstResPath);
                if (spriteAtlas != null)
                {
                    return;
                }
            }
            if (!isExists)
            {
                SpriteAtlas sa = new SpriteAtlas();
                sa.name = atlasName;
                AssetDatabase.CreateAsset(sa, unityDstResPath);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
                spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(unityDstResPath);
            }
            List<UnityEngine.Object> objList = new List<UnityEngine.Object>();
            var SpriteDirAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(srcDir.Replace(headPath, ""));
            objList.Add(SpriteDirAsset);
            spriteAtlas.Add(objList.ToArray());
            //SpriteAtlas设置
            spriteAtlas.SetIncludeInBuild(true);
            SpriteAtlasPackingSettings spriteAtlasSettting = spriteAtlas.GetPackingSettings();
            spriteAtlasSettting.enableRotation = false;
            spriteAtlasSettting.enableTightPacking = false;
            spriteAtlasSettting.padding = 2;
            spriteAtlas.SetPackingSettings(spriteAtlasSettting);
            ////平台设置
            TextureImporterPlatformSettings importerSettings_PC = new TextureImporterPlatformSettings();
            importerSettings_PC.overridden = true;
            importerSettings_PC.name = "Standalone";
            importerSettings_PC.maxTextureSize = 2048;
            importerSettings_PC.compressionQuality = 100;
            importerSettings_PC.format = TextureImporterFormat.DXT5;

            TextureImporterPlatformSettings importerSettings_Andorid = new TextureImporterPlatformSettings();
            importerSettings_Andorid.overridden = true;
            importerSettings_Andorid.name = "Android";
            importerSettings_Andorid.maxTextureSize = 2048;
            importerSettings_Andorid.format = TextureImporterFormat.ASTC_6x6;

            TextureImporterPlatformSettings importerSettings_IOS = new TextureImporterPlatformSettings();
            importerSettings_IOS.overridden = true;
            importerSettings_IOS.name = "iPhone";
            importerSettings_IOS.maxTextureSize = 2048;
            importerSettings_IOS.format = TextureImporterFormat.ASTC_6x6;
            spriteAtlas.SetPlatformSettings(importerSettings_PC);
            spriteAtlas.SetPlatformSettings(importerSettings_Andorid);
            spriteAtlas.SetPlatformSettings(importerSettings_IOS);
            AssetDatabase.SaveAssets();
        }

    }
}
