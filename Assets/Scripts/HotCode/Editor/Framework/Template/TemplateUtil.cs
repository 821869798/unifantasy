using Scriban;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HotCode.FrameworkEditor
{
    public static class TemplateUtil
    {
        public static readonly string TemplatePath = Path.Combine(Application.dataPath, "EditorConfigs", "Template").Replace("\\", "/");


        public static string GetTemplatePath(string filePath)
        {
            return Path.Combine(TemplatePath, filePath).Replace('\\', '/');
        }

        public static TemplateContext CreateDefaultTemplateContext()
        {
            return new TemplateContext()
            {
                LoopLimit = 0,
                NewLine = "\n",
            };
        }

        public static void GetInterfaceClass<T>(Action<T> registerAction) where T : class
        {
            Type interaceType = typeof(T);
            Assembly ass = Assembly.GetAssembly(interaceType);
            if (ass == null)
            {
                return;
            }

            Type[] types = ass.GetTypes()
                .Where(p => interaceType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .ToArray();

            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                T instance = (T)Activator.CreateInstance(type);
                registerAction?.Invoke(instance);
            }
        }
    }
}
