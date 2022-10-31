using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Framework
{

    public class BuildTool
    {
        private const string UseABMenuPath = "Framework/AssetBundle/UseAB";

        [MenuItem(UseABMenuPath,priority = 0)]
        public static void SetAssetBundleUseMode()
        {
            bool useAB = Menu.GetChecked(UseABMenuPath);
            Menu.SetChecked(UseABMenuPath, !useAB);
            EditorPrefs.SetBool(AppConfig.UseABPrefsKey, !useAB);

        }
        [MenuItem(UseABMenuPath, true)]
        public static bool SetAssetBundleUseModeCheck()
        {
            Menu.SetChecked(UseABMenuPath, EditorPrefs.GetBool(AppConfig.UseABPrefsKey, true));
            return true;
        }
        [MenuItem("Framework/AssetBundle/ReBuild All")]
        public static void ReBuildAllAssetBundles()
        {
            var outputPath = GetExportPath();
            Directory.Delete(outputPath, true);

            Debug.Log("Delete folder: " + outputPath);

            BuildAllAssetBundles();
        }
        [MenuItem("Framework/AssetBundle/Build All %&b")]
        public static void BuildAllAssetBundles()
        {
            if (EditorApplication.isPlaying)
            {
                Log.Error("Cannot build in playing mode! Please stop!");
                return;
            }
            MakeAssetBundleNames();
            var outputPath = GetExportPath();
            //KProfiler.BeginWatch("BuildAB");
            Log.LogInfo("AsseBundle start build to: {0}", outputPath);
            //ѹ���㷨��������Lzma��Ҫ��LZ4 . Lzma��ȫ����buffer Lz4һ��һ��block��ȡ��ֻ��ȡ4�ֽ�
            var opt = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression;//BuildAssetBundleOptions.AppendHashToAssetBundleName;
            BuildPipeline.BuildAssetBundles(outputPath, opt, EditorUserBuildSettings.activeBuildTarget);
            //KProfiler.EndWatch("BuildAB", "AsseBundle build Finish");
        }

        /// <summary>
        /// Unity 5��AssetBundleϵͳ����ҪΪ�����AssetBundle��������
        /// 
        /// ֱ�ӽ�KEngine���õ�BundleResourcesĿ¼�����Զ��������ƣ���Ϊ���Ŀ¼����������������
        /// </summary>
        [MenuItem("Framework/AssetBundle/Make Names from [BundleResources]")]
        public static void MakeAssetBundleNames()
        {
            var dir = ResourcesBuildDir;
            int dirLength = dir.Length;
            // set BundleResources's all bundle name
            var files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
            foreach (var filepath in files)
            {
                if (filepath.EndsWith(".meta")) continue;

                var importer = AssetImporter.GetAtPath(filepath);
                if (importer == null)
                {
                    Log.Error("Not found: {0}", filepath);
                    continue;
                }
                var bundleName = filepath.Substring(dirLength, filepath.Length - dirLength);
                var file = new FileInfo(filepath);
                bundleName = bundleName.Replace(file.Extension, "");//ȥ����׺��ԭ��abBrowser���޷�ʶ��abName���ж��.
                importer.assetBundleName = bundleName + AppConfig.AssetBundleExt;
            }
            Log.LogInfo("Make all asset name successs!");
        }
        static string ResourcesBuildDir
        {
            get
            {
                var dir = "Assets/" + AppDef.ResourcesBuildDir + "/";
                return dir;
            }
        }
        /// <summary>
        /// Extra Flag ->   ex:  Android/  AndroidSD/  AndroidHD/
        /// </summary>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static string GetExportPath(KResourceQuality quality = KResourceQuality.Sd)
        {
            string basePath = Path.GetFullPath(AppConfig.AssetBundleBuildRelPath);
            if (File.Exists(basePath))
            {
                ShowDialog("·�����ô���: " + basePath);
                throw new System.Exception("·�����ô���");
            }
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            string path = null;
            var platformName = ResourceModule.GetBuildPlatformName();
            if (quality != KResourceQuality.Sd) // SD no need add
                platformName += quality.ToString().ToUpper();

            path = basePath + "/" + platformName + "/";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        public static bool ShowDialog(string msg, string title = "��ʾ", string button = "ȷ��")
        {
            return EditorUtility.DisplayDialog(title, msg, button);
        }

        public static void CopyFolder(string sPath, string dPath)
        {
            if (!Directory.Exists(dPath))
            {
                Directory.CreateDirectory(dPath);
            }

            DirectoryInfo sDir = new DirectoryInfo(sPath);
            FileInfo[] fileArray = sDir.GetFiles();
            foreach (FileInfo file in fileArray)
            {
                if (file.Extension != ".meta")
                    file.CopyTo(dPath + "/" + file.Name, true);
            }

            DirectoryInfo[] subDirArray = sDir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirArray)
            {
                CopyFolder(subDir.FullName, dPath + "/" + subDir.Name);
            }
        }
    }
}
