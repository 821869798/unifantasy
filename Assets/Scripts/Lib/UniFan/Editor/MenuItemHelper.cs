using System;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using System.IO;

public static class MenuItemHelper
{
    //�洢�û����ݴ�ŵ�Ŀ¼
    public const string EditorPersistentPath = "EditorPersistent/save_data/user_data";


    [MenuItem("GameEditor/SceneShortcut/�����浱ǰ����������Launcher %h", priority = 30)]
    static void OpenSceneLauncher()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Launcher.unity");
    }

    [MenuItem("GameEditor/SceneShortcut/�����浱ǰ������������ЧԤ������ %j", priority = 31)]
    static void OpenSceneModTest()
    {
        EditorSceneManager.OpenScene("Assets/Test/ModTest/ModTest02.unity");
    }

    [MenuItem("GameEditor/Clear Editor ProgressBar(��ֹ�쳣��ס)", priority = 1000)]
    public static void ClearEditorProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("GameEditor/������ػ�������/Clear All PlayerPrefs(������ر��������)")]
    public static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("GameEditor/������ػ�������/Clear All EditorPrefs(����༭�����ر��������)")]
    public static void ClearAllEditorPrefs()
    {
        EditorPrefs.DeleteAll();
    }

    [MenuItem("GameEditor/������ػ�������/Clear All UserData(����༭�����ر�����û�����)")]
    public static void ClearAllUserData()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("״̬����", "����״̬�޷����ñ�����,���˳�����е���", "ȷ��");
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
