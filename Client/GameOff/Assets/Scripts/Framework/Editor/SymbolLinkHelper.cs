using System;
using System.IO;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Ӳ����Ŀ¼���ߡ�����֧��win+mac, ��Ҫwin 7���ϲ���mklink����
    /// </summary>
    public class SymbolLinkHelper
    {
        /// <summary>
        /// ɾ��Ӳ����Ŀ¼
        /// </summary>
        /// <param name="linkPath"></param>
        public static void DeleteLink(string linkPath)
        {
            var os = Environment.OSVersion;
            if (os.ToString().Contains("Windows"))
            {
                EditorUtils.ExecuteCommand(String.Format("rmdir \"{0}\"", linkPath));
            }
            else if (os.ToString().Contains("Unix"))
            {
                EditorUtils.ExecuteCommand(String.Format("rm -Rf \"{0}\"", linkPath));
            }
            else
            {
                Debug.LogError(String.Format("[SymbolLinkFolder]Error on OS: {0}", os.ToString()));
            }
        }

        public static void SymbolLinkFolder(string srcFolderPath, string targetPath)
        {
            var os = Environment.OSVersion;
            if (os.ToString().Contains("Windows"))
            {
                EditorUtils.ExecuteCommand(String.Format("mklink /J \"{0}\" \"{1}\"", targetPath, srcFolderPath));
            }
            else if (os.ToString().Contains("Unix"))
            {
                var fullPath = Path.GetFullPath(targetPath);
                if (fullPath.EndsWith("/"))
                {
                    fullPath = fullPath.Substring(0, fullPath.Length - 1);
                    fullPath = Path.GetDirectoryName(fullPath);
                }
                EditorUtils.ExecuteCommand(String.Format("ln -s {0} {1}", Path.GetFullPath(srcFolderPath), fullPath));
            }
            else
            {
                Debug.LogError(String.Format("[SymbolLinkFolder]Error on OS: {0}", os.ToString()));
            }
        }

        /// <summary>
        /// ɾ��ָ��Ŀ¼����Ӳ����
        /// </summary>
        /// <param name="assetBundlesLinkPath"></param>
        public static void DeleteAllLinks(string assetBundlesLinkPath)
        {
            if (Directory.Exists(assetBundlesLinkPath))
            {
                var dirs = Directory.GetDirectories(assetBundlesLinkPath);
                foreach (var dirPath in dirs)
                {
                    DeleteLink(dirPath);
                }
                DeleteLink(assetBundlesLinkPath);
            }

        }
    }
}