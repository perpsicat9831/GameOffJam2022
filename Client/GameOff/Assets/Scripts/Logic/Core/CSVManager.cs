using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;
using UnityEditor;


namespace Framework
{
    public class CSVManager
    {
        public static cfg.Tables CSVData = null;

        public static void InitTable()
        {
            CSVData = new cfg.Tables(file => JSON.Parse(Resources.Load<TextAsset>(AppConfig.CsvDataPath + "/" + file).text));
            //CSVData = new cfg.Tables(file => JSON.Parse(File.ReadAllText(AppConfig.CsvDataPath + "/" + file + ".json")));
        }

    }
}