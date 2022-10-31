using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace AudioStudio.Tools
{
    internal class AsCompareAnimationEventWindow : AsCompareWindow
    {
        internal static void ShowWindow()
        {
            var window = GetWindow<AsCompareAnimationEventWindow>();
            window.position = new Rect(500, 300, 700, 500);
            window.titleContent = new GUIContent("Compare Animation Events");
        }

        protected override void DisplayData(ComponentComparisonData data)
        {
            AnimationComparisonData animData = data as AnimationComparisonData;
            var xmlData = animData.ServerData == null ? animData.LocalData : animData.ServerData;
            EditorGUILayout.LabelField(
                new GUIContent(Path.GetFileName(animData.AssetPath),
                animData.AssetPath), GUILayout.Width(_objectWidth));

            string componentLabel = animData.ClipName;
            if (String.IsNullOrEmpty(componentLabel))
                componentLabel = "AnimationClip";
            EditorGUILayout.LabelField(componentLabel, GUILayout.Width(_componentWidth));

            if (GUILayout.Button(animData.ComponentStatus.ToString(), EditorStyles.label,
                GUILayout.Width(_statusWidth)))
                AsXmlInfo.Init(xmlData);
        }

        protected override void ProcessFilter()
        {
            foreach (var component in ModifiedComponents)
            {
                var animData = component as AnimationComparisonData;
                typesFilter.Add(animData.ClipName);
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
            AnimationComparisonData animData = data as AnimationComparisonData;
            return !ProcessFilter(animData.ClipName,
                    typesFilter, typesFilterSelector) ||
                    !ProcessFilter(data.AssetName,
                    objectFilter, objectFilterSelector) ||
                    !ProcessFilter(data.ComponentStatus.ToString(),
                    statusFilter, statusFilterSelector);
        }

        protected override void LocateComponent(ComponentComparisonData data)
        {
#if UNITY_2018_1_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
#else
                EditorApplication.ExecuteMenuItem("Window/Animation");
#endif

            EditorApplication.ExecuteMenuItem("Window/General/Inspector");
            focusedWindow.Repaint();
            if (data.AssetPath.Contains(".FBX"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.AssetPath);
                if (prefab) Selection.activeObject = prefab;
            }
            else
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(data.AssetPath);
                if (clip) Selection.activeObject = clip;
            }
                
        }
    }

}
