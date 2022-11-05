using UnityEngine;
using System.IO;
using UnityEditor;
using System.Text;
using System;

namespace Framework
{
    public class ToUTF8
    {
        [MenuItem("Framework/ToUTF8", false)]
        public static void Start()
        {
            //var dir = Directory.GetCurrentDirectory();
            var dir = "Assets/Scripts";

            foreach (var f in new DirectoryInfo(dir).GetFiles("*.cs", SearchOption.AllDirectories))
            {
                if (!DetectFileEncoding(f.FullName, "utf-8") && DetectFileEncoding(f.FullName, "gb2312"))
                {
                    var s = File.ReadAllText(f.FullName, Encoding.GetEncoding("GB2312"));
                    Debug.Log("ToUTF8 " + f.FullName);
                    try
                    {
                        File.WriteAllText(f.FullName, s, new UTF8Encoding(false));
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            AssetDatabase.Refresh();
        }

        public static bool DetectFileEncoding(string file, string name)
        {
            var encodingVerifier = Encoding.GetEncoding(name, new EncoderExceptionFallback(), new DecoderExceptionFallback());
            using (var reader = new StreamReader(file, encodingVerifier, true, 1024))
            {
                try
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                    }
                    return reader.CurrentEncoding.BodyName == name;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
