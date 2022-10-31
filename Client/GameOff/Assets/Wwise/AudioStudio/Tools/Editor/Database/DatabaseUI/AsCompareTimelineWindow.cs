using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.Timeline;

namespace AudioStudio.Tools
{
    internal class AsCompareTimelineWindow : AsCompareWindow
    {
        protected override void DisplayData(ComponentComparisonData data)
        {
            var xmlData = data.ServerData == null ? data.LocalData : data.ServerData;
            var playableName = Path.GetFileNameWithoutExtension(data.AssetPath);
            var trackName = AsScriptingHelper.GetXmlAttribute(xmlData, "TrackName");
            var clipName = AsScriptingHelper.GetXmlAttribute(xmlData, "ClipName");

            EditorGUILayout.LabelField(new GUIContent(playableName, 
                data.AssetPath + "\n Track: " + trackName), 
                GUILayout.Width(_objectWidth));
            EditorGUILayout.LabelField(clipName, GUILayout.Width(_componentWidth));
            if (GUILayout.Button(data.ComponentStatus.ToString(), EditorStyles.label,
                GUILayout.Width(_statusWidth)))
                AsXmlInfo.Init(xmlData);
        }

        protected override void ProcessFilter()
        {
            foreach (var component in ModifiedComponents)
            {
                var timelineData = component as TimelineComparisonData;
                typesFilter.Add(timelineData.ClipName);
                objectFilter.Add(component.AssetName);
            }

            for (int i = 0; i < objectFilter.Count; i++)
            {
                objectFilterSelector.Add(false);
            }

            for (int i = 0; i < typesFilter.Count; i++)
            {
                typesFilterSelector.Add(false);
            }


            statusFilter.Add(ComponentStatus.AllRemoved.ToString());
            statusFilter.Add(ComponentStatus.ServerOnly.ToString());
            statusFilter.Add(ComponentStatus.LocalOnly.ToString());
            statusFilter.Add(ComponentStatus.UseServer.ToString());
            statusFilter.Add(ComponentStatus.UseLocal.ToString());
            statusFilter.Add(ComponentStatus.Different.ToString());
            statusFilter.Add(ComponentStatus.NoEvent.ToString());
            for (int i = 0; i < 7; i++)
            {
                statusFilterSelector.Add(false);
            }

        }

        protected override bool ProcessFilterGroup(ComponentComparisonData data)
        {
            TimelineComparisonData timelineData = data as TimelineComparisonData;
            return !ProcessFilter(timelineData.ClipName,
                    typesFilter, typesFilterSelector) ||
                    !ProcessFilter(data.AssetName,
                    objectFilter, objectFilterSelector) ||
                    !ProcessFilter(data.ComponentStatus.ToString(),
                    statusFilter, statusFilterSelector);
        }

        protected override void LocateComponent(ComponentComparisonData data)
        {
#if UNITY_2018_1_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
#else
                EditorApplication.ExecuteMenuItem("Window/Timeline");
#endif
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(data.AssetPath);
            if (timeline) Selection.activeObject = timeline;
        }
    }
}