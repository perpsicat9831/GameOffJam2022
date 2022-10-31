using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using UnityEditor.ProjectWindowCallback;
using System.Text.RegularExpressions;

namespace Framework
{

    public static class CreateExtend
    {
        readonly static string templatePath = "Assets/Scripts/Framework/Editor/CreateTemplate/Template/";

        #region 扩展
        [MenuItem("Assets/Create/Framework Template/NewUIBase C#", false, 79)]
        public static void CreatUIBaseScript()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
            GetSelectedPathOrFallback() + "/NewUIBase.cs",   //创建文件初始名
            null,
           templatePath + "NewUIBase.cs");   //模版路径
        }
        [MenuItem("Assets/Create/Framework Template/NewManager C#", false, 80)]
        public static void CreatManagerScript()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
            GetSelectedPathOrFallback() + "/NewManager.cs",   //创建文件初始名
            null,
           templatePath + "NewManager.cs");   //模版路径
        }

        [MenuItem("Assets/Create/Framework Template/NewPoolObject C#", false, 81)]
        public static void CreatPoolObjectScript()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
            GetSelectedPathOrFallback() + "/NewPoolObject.cs",   //创建文件初始名
            null,
           templatePath + "NewPoolObject.cs");   //模版路径
        }

        [MenuItem("Assets/Create/Framework Template/NewUI Prefab", false, 82)]
        public static void CreatUIPrefab()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
            GetSelectedPathOrFallback() + "/UI_New.prefab",   //创建文件初始名
            null,
           templatePath + "UI_New.prefab");   //模版路径
        }
        #endregion

        #region 核心代码
        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        class MyDoCreateScriptAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(o);
            }

            //获取MainGame文件夹下的路径名
            public static string getLuaPath(string fullPath)
            {
                string luaPath = "";
                string[] pathArr = fullPath.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < pathArr.Length; i++)
                {
                    if (pathArr[i] == "MainGame")
                    {
                        //处理创建文件路径
                        for (int j = i; j < pathArr.Length - 1; j++)
                        {
                            luaPath += pathArr[j] + ".";
                        }
                        break;
                    }
                }

                return luaPath;
            }


            internal static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
            {
                string fullPath = Path.GetFullPath(pathName);   //完整路径
                StreamReader streamReader = new StreamReader(resourceFile);
                string text = streamReader.ReadToEnd();
                streamReader.Close();
                string fileName = Path.GetFileNameWithoutExtension(pathName);  //文件名
                string luaPath = getLuaPath(fullPath);   //lua路径

                //正则替换
                text = Regex.Replace(text, "#NAME#", fileName);
                text = Regex.Replace(text, "#PATH#", luaPath);

                bool encoderShouldEmitUTF8Identifier = true;
                bool throwOnInvalidBytes = false;
                UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
                bool append = false;
                StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
                streamWriter.Write(text);
                streamWriter.Close();
                AssetDatabase.ImportAsset(pathName);
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
            }
        }
        #endregion
    }
}