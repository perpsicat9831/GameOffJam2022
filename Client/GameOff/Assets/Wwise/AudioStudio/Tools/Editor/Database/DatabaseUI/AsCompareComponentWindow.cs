using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEditor.Experimental.SceneManagement;

namespace AudioStudio.Tools
{
    internal class AsCompareComponentWindow : AsCompareWindow
    {
        public delegate void RemoveXMLComponent(ComponentComparisonData data);
        public RemoveXMLComponent removeXMLComponentEvent;

        protected override void DisplayData(ComponentComparisonData data)
        {
            var xmlData = data.ServerData == null ? data.LocalData : data.ServerData;
            var path = AsScriptingHelper.GetXmlAttribute(xmlData, "GameObject");
            if (data.AssetPath.Contains(".unity"))
                path = data.AssetName + "/" + path;
            EditorGUILayout.LabelField( 
                new GUIContent(Path.GetFileName(data.AssetPath),
                path), GUILayout.Width(_objectWidth));
            
            var type = AsScriptingHelper.GetXmlAttribute(xmlData, "Type");
            EditorGUILayout.LabelField(type, GUILayout.Width(_componentWidth));

            if (GUILayout.Button(data.ComponentStatus.ToString(), EditorStyles.label,
                GUILayout.Width(_statusWidth)))
                AsXmlInfo.Init(xmlData);

        }

        /// <summary>
        /// auto select the component asset
        /// </summary>
        /// <param name="data"></param>
        protected override void LocateComponent(ComponentComparisonData data)
        {
            var xmlData = data.ServerData == null ? data.LocalData : data.ServerData;
            var gameObjectPath = AsScriptingHelper.
                GetXmlAttribute(xmlData, "GameObject");
            if (data.AssetPath.EndsWith(".prefab"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.AssetPath);
                AssetDatabase.OpenAsset(prefab);
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                FindGameObjectAndSelect(prefabStage.prefabContentsRoot, gameObjectPath);
            }
            else
            {
                EditorSceneManager.OpenScene(data.AssetPath);
                var scene = SceneManager.GetActiveScene();
                GameObject[] rootGameObjects = scene.GetRootGameObjects();
                foreach (var rootGameObject in rootGameObjects)
                {
                    FindGameObjectAndSelect(rootGameObject, gameObjectPath);
                }
            }
        }

        private void FindGameObjectAndSelect(GameObject gameObject, string gameObjectPath)
        {
            var child = GetGameObject(gameObject, gameObjectPath);
            if (child)
                Selection.activeObject = child;
        }

    }

}
