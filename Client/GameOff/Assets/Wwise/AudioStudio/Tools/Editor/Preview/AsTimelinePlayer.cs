using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

namespace AudioStudio.Tools
{
    public static class AsTimelinePlayer
    {
        public static void PreviewTimeline(UnityEngine.Object selectedObject)
        {
            var timelinePrefab = selectedObject as GameObject;
            if (!timelinePrefab) return;
            var configPath = AsScriptingHelper.CombinePath(WwisePathSettings.EditorConfigPathFull, "TimelineSceneTable.txt");
            try
            {
                var data = File.ReadAllLines(configPath);
                for (var i = 1; i < data.Length; i++)
                {
                    var lineSplit = data[i].Split('\t');
                    if (lineSplit.Length < 2) continue;
                    var prefabPattern = lineSplit[1];
                    var match = Regex.Match(timelinePrefab.name, prefabPattern);
                    if (match.Success)
                    {
                        var scenePath = AsScriptingHelper.CombinePath(data[0], lineSplit[0] + ".unity");
                        try
                        {
                            var timelineRoot = PrefabUtility.InstantiatePrefab(timelinePrefab, EditorSceneManager.OpenScene(scenePath)) as GameObject;
                            if (timelineRoot)
                                Selection.activeObject = timelineRoot.GetComponentInChildren<PlayableDirector>().playableAsset;
                        }
                        catch (ArgumentException)
                        {
                            if (EditorUtility.DisplayDialog("Error", "Can't find scene " + scenePath, "Edit Config", "OK"))
                                Process.Start(configPath);
                            return;
                        }

                        var eventSystem = new GameObject("EventSystem");
                        eventSystem.AddComponent<EventSystem>();
                        eventSystem.AddComponent<StandaloneInputModule>();
                        EditorApplication.isPlaying = true;
                        AudioInitSettings.Instance.Initialize(true);
                        return;
                    }
                }
                if (EditorUtility.DisplayDialog("Error", "Can't find matching scene with Timeline!", "Edit Config", "OK"))
                    Process.Start(configPath);
            }
#pragma warning disable 168
            catch (FileNotFoundException e)
#pragma warning restore 168
            {
                if (EditorUtility.DisplayDialog("Error", "Can't find config file at " + configPath, "Create New", "OK"))
                {
                    File.WriteAllText(configPath, "Assets/Scenes");
                    Process.Start(configPath);
                }
            }
        }
    }
}