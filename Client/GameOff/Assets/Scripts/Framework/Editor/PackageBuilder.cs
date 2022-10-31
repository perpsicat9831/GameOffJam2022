using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Framework
{
    //������ߵ�ʱ�䲶׽��
    [InitializeOnLoad]
    public class PackageBuilderEventCatcher
    {
        static PackageBuilderEventCatcher()
        {
            UnityEditorEventCatcher.OnBeforeBuildAppEvent -= OnBeforeBuildPlayerEvent;
            UnityEditorEventCatcher.OnBeforeBuildAppEvent += OnBeforeBuildPlayerEvent;
            UnityEditorEventCatcher.OnPostBuildPlayerEvent -= OnAfterBuildPlayerEvent;
            UnityEditorEventCatcher.OnPostBuildPlayerEvent += OnAfterBuildPlayerEvent;
        }
        private static void OnBeforeBuildPlayerEvent()
        {
            //��ab�����ƹ���
            var exportPath = Path.GetFullPath(AppConfig.AssetBundleBuildRelPath);
            var targetPath = ResourcesSymbolLinkHelper.AssetBundlesLinkPath;
            DeleteCopyBundle();
            Log.LogInfo("��Ŀ¼ " + exportPath + " �µ��ļ����Ƶ�Ŀ¼ " + targetPath);
            BuildTool.CopyFolder(exportPath, targetPath);
        }


        private static void OnAfterBuildPlayerEvent(BuildTarget buildTarget, string str)
        {
            DeleteCopyBundle();
        }

        [MenuItem("Framework/AutoBuilderTools/DeleteCopyBundle")]
        public static void DeleteCopyBundle()
        {
            var targetPath = ResourcesSymbolLinkHelper.AssetBundlesLinkPath;
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
                File.Delete(targetPath + ".meta");
                Log.LogInfo("��ɾ��Ŀ¼ " + targetPath);
                AssetDatabase.Refresh();
            }
            else
            {
                Log.LogInfo("δ�ҵ�Ŀ¼ " + targetPath + " ����Ҫɾ�����ļ�");
            }
        }
    }
    public class PackageBuilder
    {
        //using Unity.EditorCoroutines.Editor; //package:com.unity.editorcoroutines ,package�й�ѡ:Show preview packages

        private static string GetProjectName()
        {
            string[] s = Application.dataPath.Split('/');
            return s[s.Length - 2];
        }

        private static string[] GetScenePaths()
        {
            string[] scenes = new string[EditorBuildSettings.scenes.Length];

            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }

            return scenes;
        }

        /// <summary>
        /// �����и�unity������ ʾ����-BundleVersion=1.0.1 -AndroidKeyStoreName=KSFramework
        /// Unity.exe -batchmode -projectPath %codePath%\ -nographics -executeMethod BuildTest.PerformAndroidBuild -BundleVersion=1.0.1 -AndroidKeyStoreName=KSFramework -logFile %~dp0\build.log -quit
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="outputpath"></param>
        private static void ParseArgs(ref BuildOptions opt, ref string outputpath)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            if (!Directory.Exists(AppConfig.ProductRelPath))
            {
                Directory.CreateDirectory(AppConfig.ProductRelPath);
            }

            if (args.Length >= 2)
            {
                CommandArgs commandArg = CommandLine.Parse(args);
                //List<string> lparams = commandArg.Params;
                Dictionary<string, string> argPairs = commandArg.ArgPairs;

                foreach (KeyValuePair<string, string> item in argPairs)
                {
                    switch (item.Key)
                    {
                        case "BundleVersion":
                            PlayerSettings.bundleVersion = item.Value;
                            break;
                        case "AndroidVersionCode":
                            PlayerSettings.Android.bundleVersionCode = System.Int32.Parse(item.Value);
                            break;
                        case "AndroidKeyStoreName":
                            PlayerSettings.Android.keystoreName = item.Value;
                            break;
                        case "AndroidKeyStorePass":
                            PlayerSettings.Android.keystorePass = item.Value;
                            break;
                        case "AndroidkeyAliasName":
                            PlayerSettings.Android.keyaliasName = item.Value;
                            break;
                        case "AndroidKeyAliasPass":
                            PlayerSettings.Android.keyaliasPass = item.Value;
                            break;
                        case "BuildOptions":
                            {
                                opt = BuildOptions.None;
                                string[] opts = item.Value.Split('|');
                                foreach (string o in opts)
                                {
                                    opt = opt | (BuildOptions)System.Enum.Parse(typeof(BuildOptions), o);
                                }
                            }
                            break;
                        case "Outputpath":
                            outputpath = item.Value;
                            break;
                    }
                    UnityEngine.Debug.Log("parse arg -> " + item.Key + " : " + item.Value);
                }
            }
        }

        /// <summary>
        /// return full path or build
        /// </summary>
        /// <param name="outputpath"></param>
        /// <param name="tag"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        private static string PerformBuild(string outputpath, BuildTargetGroup buildTargetGroup, BuildTarget tag, BuildOptions opt)
        {
#if UNITY_2018_1_OR_NEWER
            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, tag);
#else
			EditorUserBuildSettings.SwitchActiveBuildTarget(tag);
#endif
            if (AppConfig.IsDownloadRes && !File.Exists(AppConfig.VersionTextPath))
            {
                Log.LogInfo("���ʧ�ܣ������ظ��µİ�����Ҫ������vresion.txt");
                return null;
            }


            //OnBeforeBuildPlayerEvent Unity����Ĵ��ǰ�¼�
            ParseArgs(ref opt, ref outputpath);
            string fullPath = System.IO.Path.Combine(AppConfig.ProductRelPath, outputpath);
            string fullDir = System.IO.Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(fullDir))
                Directory.CreateDirectory(fullDir);

            Log.LogInfo("Start Build Client {0} to: {1}", tag, Path.GetFullPath(fullPath));
#if xLua
         	// NOTE xlua�ڱ༭������ģʽ�����ɴ��룬��Ϊ.NET Standard 2.0��֧��emit���ᵼ��ĳЩCSharpCallLuaע��ʧ�ܣ�ApiҪ�ĳ�.Net4.X���ڴ��ʱ�������Ҫ���޸Ļ�
         	// ��Ҫ��clear����gen������ͬһ��class�޸ĺ󣬸���gen�ᱨ��
            XLua.DelegateBridge.Gen_Flag = true;
            CSObjectWrapEditor.Generator.ClearAll();
            CSObjectWrapEditor.Generator.GenAll();
#elif ILRuntime
            ILRuntimeEditor.GenerateCLRBindingByAnalysis();
            ILRuntimeEditor.GenerateCrossbindAdapter();
#endif
            var buildResult = BuildPipeline.BuildPlayer(GetScenePaths(), fullPath, tag, opt);
#if xLua
            if(buildResult.summary.result == BuildResult.Succeeded) CSObjectWrapEditor.Generator.ClearAll();
#endif
            Log.LogInfo("Build App result:{0} ,errors:{1}", buildResult.summary.result, buildResult.summary.totalErrors);
            return fullPath;
        }

        //public static int GetProgramVersion()
        //{
        //    var oldVersion = 0;
        //    if (File.Exists(GetProgramVersionFullPath()))
        //        oldVersion = File.ReadAllText(GetProgramVersionFullPath()).ToInt32();

        //    return oldVersion;
        //}

        //public static string GetProgramVersionFullPath()
        //{
        //    string programVersionFile = string.Format("{0}/Resources/ProgramVersion.txt", Application.dataPath);
        //    return programVersionFile;
        //}

        [MenuItem("Framework/AutoBuilder/WindowsX86 Dev")]
        public static void PerformWinBuild()
        {
            PerformBuild("Apps/Windows_Dev/" + AppConfig.ProjectName + "_Dev.exe", BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows,
                BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler);
        }

        [MenuItem("Framework/AutoBuilder/WindowsX86")]
        public static void PerformWinReleaseBuild()
        {
            PerformBuild("Apps/Windows/" + AppConfig.ProjectName + ".exe", BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, BuildOptions.None);
        }

        [MenuItem("Framework/AutoBuilder/iOS")]
        public static void PerformiOSBuild()
        {
            PerformiOSBuild(AppConfig.ProjectName, false);
        }

        public static string PerformiOSBuild(string ipaName, bool isDevelopment = true)
        {
            //��������xcode project
            BuildOptions opt = isDevelopment
                ? (BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler | BuildOptions.AcceptExternalModificationsToPlayer)
                : BuildOptions.AcceptExternalModificationsToPlayer;
#if UNITY_5 || UNITY_2017_1_OR_NEWER
            return PerformBuild("Apps/IOSProjects/" + ipaName, BuildTargetGroup.iOS, BuildTarget.iOS, opt);
#else
            return PerformBuild("Apps/IOSProjects/" + ipaName, BuildTarget.iOS, opt);
#endif
        }

        [MenuItem("Framework/AutoBuilder/Android")]
        public static void PerformAndroidBuild()
        {
            PerformAndroidBuild(AppConfig.ProjectName, false);
        }

        public static string PerformAndroidBuild(string apkName, bool isDevelopment = true)
        {
            BuildOptions opt = isDevelopment
                ? (BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler)
                : BuildOptions.None;
            var path = string.Format("Apps/{0}/{1}.apk", "Android", apkName);
            return PerformBuild(path, BuildTargetGroup.Android, BuildTarget.Android, opt);
        }

        [MenuItem("Framework/AutoBuilderTools/Open Apps Folder")]
        public static void OpenAppPackagePath()
        {
            var path = System.IO.Path.Combine(AppConfig.ProductRelPath, "Apps");
            System.Diagnostics.Process.Start(path);
        }

        [MenuItem("Framework/UserData/Clear PC PersistentDataPath", false, 99)]
        public static void ClearPersistentDataPath()
        {
            foreach (string dir in Directory.GetDirectories(ResourceModule.AppDataPath))
            {
                Directory.Delete(dir, true);
            }
            foreach (string file in Directory.GetFiles(ResourceModule.AppDataPath))
            {
                File.Delete(file);
            }
        }

        [MenuItem("Framework/UserData/Open PC PersistentDataPath Folder", false, 98)]
        public static void OpenPersistentDataPath()
        {
            System.Diagnostics.Process.Start(ResourceModule.AppDataPath);
        }

        [MenuItem("Framework/UserData/Clear Prefs")]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            BuildTool.ShowDialog("Prefs Cleared!");
        }

    }

    public class ResourcesSymbolLinkHelper
    {
        public static string StreamingPath = "Assets/StreamingAssets/";
        public static string AssetBundlesLinkPath = StreamingPath + AppConfig.StreamingBundlesFolderName;
        public static string LuaLinkPath = StreamingPath + AppConfig.LuaPath + "/";
        public static string SettingLinkPath = StreamingPath + AppConfig.SettingResourcesPath + "/";
        //WeakReference ins
        //public static object ins;

        public static string GetABLinkPath()
        {
            if (!Directory.Exists(AssetBundlesLinkPath))
            {
                Directory.CreateDirectory(AssetBundlesLinkPath);
                Log.LogInfo("Create StreamingAssets Bundles Director {0}", AssetBundlesLinkPath);
            }
            return AssetBundlesLinkPath + "/" + ResourceModule.GetBuildPlatformName() + "/";
        }

        public static string GetResourceExportPath()
        {
            var resourcePath = BuildTool.GetExportPath(ResourceModule.Quality);
            return resourcePath;
        }

        /// <summary>
        /// Assets/xx -> E:\Code\KSFramework\xxx
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string AssetPathToFullPath(string assetPath)
        {
            assetPath = assetPath.Replace("\\", "/");
            return Path.GetFullPath(Application.dataPath + "/" + assetPath.Remove(0, "Assets/".Length));
        }
    }
}
