using System.IO;
using UnityEditor;
using UnityEngine;


namespace UniFanEditor
{
    public class ScreenCaptureEditor : EditorWindow
    {
        private static string directory = "Screenshots/Capture/";
        private static string latestScreenshotPath = "";
        private bool initDone = false;

        private GUIStyle BigText;

        void InitStyles()
        {
            initDone = true;
            BigText = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
        }

        private void OnGUI()
        {
            if (!initDone)
            {
                InitStyles();
            }

            GUILayout.Label("Screen Capture Tools", BigText);
            if (GUILayout.Button("单张截图"))
            {
                TakeScreenshot();
            }
            GUILayout.Label("当前分辨率： " + GetResolution());

            if (GUILayout.Button("打开文件夹"))
            {
                ShowFolder();
            }
            GUILayout.Label("保存路径: " + directory);
        }

        [MenuItem("Tools/Screenshots/打开窗口 &`", false, 0)]
        public static void ShowWindow()
        {
            GetWindow(typeof(ScreenCaptureEditor));
        }

        [MenuItem("Tools/Screenshots/存储路径 &2", false, 2)]
        private static void ShowFolder()
        {
            if (File.Exists(latestScreenshotPath))
            {
                EditorUtility.RevealInFinder(latestScreenshotPath);
                return;
            }
            Directory.CreateDirectory(directory);
            EditorUtility.RevealInFinder(directory);
        }

        [MenuItem("Tools/Screenshots/单张截图 &1", false, 1)]
        private static void TakeScreenshot()
        {
            Directory.CreateDirectory(directory);
            var currentTime = System.DateTime.Now;
            var filename = currentTime.ToString().Replace('/', '-').Replace(':', '_') + ".png";
            var path = directory + filename;
            ScreenCapture.CaptureScreenshot(path);
            latestScreenshotPath = path;
            Debug.Log($"截图路径: <b>{path}</b> 分辨率： <b>{GetResolution()}</b>");
        }

        private static string GetResolution()
        {
            Vector2 size = Handles.GetMainGameViewSize();
            Vector2Int sizeInt = new Vector2Int((int)size.x, (int)size.y);
            return $"{sizeInt.x}x{sizeInt.y}";
        }

    }

}

