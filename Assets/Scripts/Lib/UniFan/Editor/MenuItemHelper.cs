using System;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using System.IO;

public static class MenuItemHelper
{
    //存储用户数据存放的目录
    public const string EditorPersistentPath = "EditorPersistent/save_data/user_data";


    [MenuItem("GameEditor/SceneShortcut/不保存当前场景并进入Launcher %h", priority = 30)]
    static void OpenSceneLauncher()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Launcher.unity");
    }

    [MenuItem("GameEditor/SceneShortcut/不保存当前场景并进入特效预览场景 %j", priority = 31)]
    static void OpenSceneModTest()
    {
        EditorSceneManager.OpenScene("Assets/Test/ModTest/ModTest02.unity");
    }

    [MenuItem("GameEditor/Clear Editor ProgressBar(防止异常卡住)", priority = 1000)]
    public static void ClearEditorProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("GameEditor/清除本地缓存数据/Clear All PlayerPrefs(清除本地保存的数据)")]
    public static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("GameEditor/清除本地缓存数据/Clear All EditorPrefs(清除编辑器本地保存的数据)")]
    public static void ClearAllEditorPrefs()
    {
        EditorPrefs.DeleteAll();
    }

    [MenuItem("GameEditor/清除本地缓存数据/Clear All UserData(清除编辑器本地保存的用户数据)")]
    public static void ClearAllUserData()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("状态错误", "运行状态无法调用本方法,请退出后进行调用", "确认");
        }
        else
        {
            try
            {
                if (Directory.Exists(EditorPersistentPath))
                {
                    Directory.Delete(EditorPersistentPath, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

}
