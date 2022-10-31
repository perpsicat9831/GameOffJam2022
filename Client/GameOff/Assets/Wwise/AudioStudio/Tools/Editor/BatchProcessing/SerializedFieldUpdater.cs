using System;
using System.IO;
using AudioStudio.Editor;
using UnityEditor;
using UnityEngine;

namespace AudioStudio.Tools
{
    /// <summary>
    /// Update a serialized field by text from prefabs and scenes.
    /// </summary>
    internal class SerializedFieldUpdater : AsSearchers
    {		
        private MonoScript _script;
        private Type _type;
        private string _oldString;
        private string _newString;
		
        private void Upgrade(string filePath)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);						
            if (!go.GetComponentInChildren(_type)) return;
            var path = Path.Combine(Application.dataPath, filePath.Substring(7));					
            var text = File.ReadAllText(path);			
            var newText = text.Replace(_oldString, _newString);			
            if (text != newText)
            {				
                AsScriptingHelper.CheckoutLockedFile(path);
                File.WriteAllText(path, newText);
                EditorUtility.SetDirty(go);
            }						
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Script Class To Upgrade");
            _script = EditorGUILayout.ObjectField(_script, typeof(MonoScript), false) as MonoScript;
            GUILayout.EndHorizontal();
						 			
            _oldString = GUILayout.TextField("Old Field Name", _oldString);
            _newString = GUILayout.TextField("New Field Name", _newString);
			
            GUILayout.BeginHorizontal();
            searchPrefab = GUILayout.Toggle(searchPrefab, "Search in prefabs");
            searchScene = GUILayout.Toggle(searchScene, "Search in scenes");
            GUILayout.EndHorizontal();
			
            AsGuiDrawer.DisplaySearchPath(ref _defaultSearchingPath);
            if (GUILayout.Button("Replace!")) Replace();
        }

        private void Replace()
        {
            if (_script == null)
            {
                EditorUtility.DisplayDialog("Error", "Please Select a Script!", "OK");
            }
            TotalCount = 0;
            _type = _script.GetClass();
            if (searchPrefab) FindFiles(Upgrade, "Upgrading Prefabs", "*.prefab");
            if (searchScene) FindFiles(Upgrade, "Upgrading Scenes", "*.unity");
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Finished", "Upgraded " + TotalCount + " Assets!", "OK");
        }				
    }
}