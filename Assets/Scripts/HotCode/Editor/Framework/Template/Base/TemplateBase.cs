using UnityEngine;
using UnityEditor;
using System.IO;
using UniFan;
using System;
using Scriban;

namespace HotCode.FrameworkEditor
{
    public abstract class TemplateBase
    {
        public abstract ETemplateType TemplateType { get; }

        public abstract string templateFilePath { get; }

        /// <summary>
        /// 模板对象
        /// </summary>
        public virtual Template template { get; protected set; }

        public virtual TemplateObjectBase templateObject { get; protected set; }

        protected TemplateContext templateContext { get; private set; }

        //需要创建的文件名
        public string createFileName { protected set; get; }
        //输出的目标路径
        public string outPutFilePath { protected set; get; }

        //是否可以修改输出的目录
        public virtual bool isAbleModifyOutputPaht
        {
            get { return true; }
        }

        /// <summary>
        /// 输出的后缀名
        /// </summary>
        public abstract string outputSuffix { get; }

        public TemplateBase()
        {
            outPutFilePath = Application.dataPath.Replace('\\', '/');
            ResetOptions();
        }

        /// <summary>
        /// 只会在初始化创建
        /// </summary>
        /// <returns></returns>
        protected virtual TemplateObjectBase GenerateTemplateObject()
        {
            TemplateObjectBase env = new TemplateObjectBase();
            return env;
        }


        public virtual void LoadTemplateFile()
        {
            if (template != null)
            {
                return;
            }
            if (!File.Exists(templateFilePath))
            {
                Debug.LogError($"Load template faild:{templateFilePath}");
                return;
            }
            string content = string.Empty;
            try
            {
                content = File.ReadAllText(templateFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Load template faild:{templateFilePath},{e.Message}");
                return;
            }

            var t = Template.Parse(content);
            if (t.HasErrors)
            {
                return;
            }
            template = t;
            var obj = GenerateTemplateObject();
            var ctx = TemplateUtil.CreateDefaultTemplateContext();
            ctx.PushGlobal(obj);
            templateContext = ctx;
            templateObject = obj;
        }



        //重置当前为默认选项
        public virtual void ResetOptions()
        {
            template = null;
            templateObject = null;
            templateContext = null;
        }

        /// <summary>
        /// 渲染模板,跟随GUI变动实时更新
        /// </summary>
        /// <returns></returns>
        public abstract string RenderTemplate();

        public virtual bool ForceRender()
        {
            return false;
        }

        public virtual void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("模板文件名", TemplateGUIStyles.MaxWidthNormalStyle);
            GUILayout.TextField(templateFilePath, TemplateGUIStyles.NoInputStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("需要创建的文件名", TemplateGUIStyles.MaxWidthNormalStyle);
            createFileName = GUILayout.TextField(createFileName, TemplateGUIStyles.InputStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("输出的目标目录", TemplateGUIStyles.MaxWidthNormalStyle);
            outPutFilePath = GUILayout.TextField(outPutFilePath, TemplateGUIStyles.InputStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("输出的最终文件路径", TemplateGUIStyles.MaxWidthNormalStyle);
            GUILayout.TextField(Path.Combine(outPutFilePath, createFileName + outputSuffix).Replace('\\', '/'), TemplateGUIStyles.NoInputStyle);
            GUILayout.EndHorizontal();
        }
    }
}
