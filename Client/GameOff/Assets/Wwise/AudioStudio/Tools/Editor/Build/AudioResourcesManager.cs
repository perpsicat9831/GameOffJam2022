using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AudioStudio.Tools
{
    public class AudioResourcesManager : EditorWindow
    {
        static string md5FileName = Path.Combine(WwisePathSettings.Instance.EditorBankLoadPath, "AudioResources" + WwisePathSettings.GetPlatformName() + ".json");
        static Dictionary<string, string> md5dic = new Dictionary<string, string>();
        [UnityEditor.MenuItem("AudioStudio/Build/GenerateFileMD5")]
        public static void GenerateFileMD5()
        {
            GenerateMD5Dic();
            SerializeMD5Dic();
        }
        public static void GenerateMD5Dic()
        {
            string resPath = Path.Combine(WwisePathSettings.Instance.EditorBankLoadPath, WwisePathSettings.GetPlatformName());
            if (Directory.Exists(resPath))
            {
                md5dic.Clear();
                string[] files = Directory.GetFiles(resPath, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    if (file.EndsWith(".bnk") || file.EndsWith(".wem"))
                    {
                        string asname = file.Replace(Path.Combine(WwisePathSettings.Instance.EditorBankLoadPath, WwisePathSettings.GetPlatformName()), "");
                        asname = asname.Replace("\\", "/");
                        string md5value = GetMD5(file);
                        md5dic.Add(asname, md5value);
                    }
                }
            }
        }

        private static void SerializeMD5Dic()
        {
            string jsonStr = JsonConvert.SerializeObject(md5dic);
            using(StreamWriter fs = new StreamWriter(md5FileName))
            {
                fs.Write(jsonStr);
            }
        }

        private static void DeserializeMD5Dic()
        {
            md5dic.Clear();
            if (!File.Exists(md5FileName))
                return;

            string jsonStr;
            using (StreamReader fs = new StreamReader(md5FileName))
            {
                jsonStr = fs.ReadToEnd();
            }

            md5dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
        }
        public static string GetMD5(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        [UnityEditor.MenuItem("AudioStudio/Build/CopyPatchFile")]
        public static void CopyPatchFile()
        {
            string DestPath = "";
            CopyPatchFile(DestPath);
        }

        public static void CopyPatchFile(string DestPath)
        {
            DeserializeMD5Dic();

            if (md5dic.Count > 0)
            {
                string dest = string.IsNullOrEmpty(DestPath) ? Path.Combine(WwisePathSettings.Instance.EditorBankLoadPath, "AudioPatch") : DestPath;
                if (Directory.Exists(dest))
                    Directory.Delete(dest, true);
                Directory.CreateDirectory(dest);
                string resPath = Path.Combine(WwisePathSettings.Instance.EditorBankLoadPath, WwisePathSettings.GetPlatformName());
                if (Directory.Exists(resPath))
                {
                    string[] files = Directory.GetFiles(resPath, "*.*", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        if (file.EndsWith(".bnk") || file.EndsWith(".wem"))
                        {
                            string asname = file.Replace(Path.Combine(WwisePathSettings.Instance.EditorBankLoadPath, WwisePathSettings.GetPlatformName()), "");
                            asname = asname.Replace("\\", "/");
                            string md5value = GetMD5(file);
                            if (md5dic.TryGetValue(asname, out string md5))
                            {
                                if (!md5value.Equals(md5))
                                {
                                    CopyFileTo(file, dest, asname);
                                    md5dic[asname] = md5value;
                                }
                            }
                            else
                            {
                                CopyFileTo(file, dest, asname);
                                md5dic.Add(asname, md5value);
                            }
                        }
                    }
                }

                SerializeMD5Dic();
            }
        }

        private static void CopyFileTo(string originPath, string DestPath, string fileName)
        {
            Debug.Log("Copy " + originPath + " to " + DestPath);

            string destFileName = DestPath + fileName;
            string dir = Path.GetDirectoryName(destFileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.Copy(originPath, destFileName, true);
        }
    }
}