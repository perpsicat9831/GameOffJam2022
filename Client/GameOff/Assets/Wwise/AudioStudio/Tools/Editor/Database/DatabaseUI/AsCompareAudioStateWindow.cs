using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AudioStudio.Tools
{
    internal class AsCompareAudioStateWindow : AsCompareWindow
    {
        internal static void ShowWindow()
        {
            var window = GetWindow<AsCompareAudioStateWindow>();
            window.position = new Rect(500, 300, 700, 500);
            window.titleContent = new GUIContent("Compare AudioStates");
        }

        protected override void DisplayData(ComponentComparisonData data)
        {
            var layer = AsScriptingHelper.GetXmlAttribute(data.LocalData, "Layer");
            var state = AsScriptingHelper.GetXmlAttribute(data.LocalData, "AnimationState");
            if (GUILayout.Button(string.Format("{0}/{1}: {2} ({3})", layer, state, Path.GetFileNameWithoutExtension(data.AssetPath), data.ComponentStatus), GUI.skin.label))
                AsXmlInfo.Init(data.LocalData);
        }

        protected override void LocateComponent(ComponentComparisonData data)
        {
#if UNITY_2018_1_OR_NEWER
            EditorApplication.ExecuteMenuItem("Window/Animation/Animator");
#else
                EditorApplication.ExecuteMenuItem("Window/Animator");
#endif
            var animator = AssetDatabase.LoadAssetAtPath<AnimatorController>(data.AssetPath);
            if (!animator) return;
            foreach (var layer in animator.layers)
            {
                if (layer.name == AsScriptingHelper.GetXmlAttribute(data.LocalData, "Layer"))
                {
                    Selection.activeObject = layer.stateMachine;
                    var state = AsScriptingHelper.GetXmlAttribute(data.LocalData, "AnimationState");
                    if (state == "OnLayer") return;
                    foreach (var animatorState in layer.stateMachine.states)
                    {
                        if (animatorState.state.name == state)
                        {
                            Selection.activeObject = animatorState.state;
                            return;
                        }
                    }
                }
            }
        }

        protected override void RemoveComponentXML(ComponentComparisonData data)
        {
            AsAudioStateBackup.Instance.RemoveComponentXml(data);
        }
    }

}