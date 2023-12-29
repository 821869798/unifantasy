using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace AutoBuild
{
    public static class AutoBuildUtility
    {
        /// <summary>
        /// 获取打包的场景
        /// </summary>
        /// <returns></returns>
        public static string[] GetBuildScenes()
        {
            List<string> s = new List<string>();

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                if (EditorBuildSettings.scenes[i].enabled)
                {
                    s.Add(EditorBuildSettings.scenes[i].path);
                }
            }

            return s.ToArray();
        }

        /// <summary>
        /// 设置代码宏,[[请注意打包设置的时候对编辑器模式代码是当次无效，下次生效的]]
        /// </summary>
        /// <param name="macro"></param>
        /// <param name="active"></param>
        public static void SetScriptingDefineSymbolActive(string macro, bool active)
        {
            string[] symbolsArray = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> symbolsList = new List<string>(symbolsArray);
            bool contain = symbolsList.Contains(macro);
            bool changed = false;
            if (!contain && active)
            {
                symbolsList.Add(macro);
                changed = true;
            }
            else if (contain && !active)
            {
                symbolsList.RemoveAll((symbol) => symbol == macro);
                changed = true;
            }
            if (changed)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", symbolsList));
            }
        }

        public static bool ContainScriptingDefineSymbol(string macro)
        {
            string[] symbolsArray = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> symbolsList = new List<string>(symbolsArray);
            bool contain = symbolsList.Contains(macro);
            return contain;
        }

        public static void ProcessSvnCommand(string arg)
        {
            var p = new Process();
            p.StartInfo.FileName = "svn";
            p.StartInfo.Arguments = arg;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            p.OutputDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                    return;
                Debug.Log(e.Data);
            };
            p.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                    return;
                Debug.LogError(e.Data);
            };

            p.Start();
            //开始异步读取输出
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            //调用WaitForExit会等待Exited事件完成后再继续往下执行。
            p.WaitForExit();
            int code = p.ExitCode;
            p.Close();
            if (code != 0)
            {
                Debug.LogError("执行命令失败");
                EditorApplication.Exit(code);
            }
            else
            {
                Debug.Log("执行命令成功");
            }
        }
    }
}
