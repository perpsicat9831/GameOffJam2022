using System;
using System.Collections.Generic;
using System.IO;
using AK.Wwise;
using AudioStudio.Components;
using AudioStudio.Editor;
using UnityEditor;
using UnityEngine;

namespace AudioStudio.Tools
{
    public class PrefabComponentsSynchronizer : EditorWindow
    {
        private string _prefabFolder = "";
        private GameObject _samplePrefab;
        private string _sampleCharacterName = "";
        private bool _saveToXml = true;

        public void OnGUI()
        {
            AsGuiDrawer.DisplaySearchPath(ref _prefabFolder);
            _samplePrefab = (GameObject) EditorGUILayout.ObjectField("Sample Prefab", _samplePrefab, typeof(GameObject), false);
            _sampleCharacterName = EditorGUILayout.TextField("Character Name", _sampleCharacterName);
            _saveToXml = EditorGUILayout.Toggle("Save to Xml", _saveToXml);
            if (GUILayout.Button("Run"))
                Run();
        }

        private void Run()
        {
            if (!_samplePrefab)
            {
                EditorUtility.DisplayDialog("Error", "Please select a sample prefab!", "OK");
                return;
            }

            var sampleLoadBank = _samplePrefab.GetComponent<LoadBank>();
            var sampleEffectSound = _samplePrefab.GetComponent<EffectSound>();

            if (!sampleLoadBank && !sampleEffectSound)
            {
                EditorUtility.DisplayDialog("Error", "Please use a prefab with LoadBank or EffectSound!", "OK");
                return;
            }

            var prefabNamePrefix = _samplePrefab.name.Substring(0, _samplePrefab.name.IndexOf(_sampleCharacterName, StringComparison.Ordinal));
            var prefabNameSuffix = _samplePrefab.name.Substring( _samplePrefab.name.IndexOf(_sampleCharacterName, StringComparison.Ordinal) + _sampleCharacterName.Length);
            var prefabPaths = Directory.GetFiles(_prefabFolder, "*.prefab", SearchOption.AllDirectories);
            foreach (var prefabPath in prefabPaths)
            {
                var prefabName = Path.GetFileNameWithoutExtension(prefabPath);
                if (prefabName == _samplePrefab.name) continue;
                var prefabCharacterName = prefabName.Replace(prefabNamePrefix, "").Replace(prefabNameSuffix, "");
                if (!prefabName.StartsWith(prefabNamePrefix) || !prefabName.EndsWith(prefabNameSuffix)) continue;
                var prefabPathShort = AsScriptingHelper.ShortPath(prefabPath);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPathShort);
                if (!prefab) continue;
                
                if (sampleLoadBank && sampleLoadBank.Banks.Length > 0)
                {
                    var sampleBank = sampleLoadBank.Banks[0];
                    var prefabLoadBank = AsUnityHelper.GetOrAddComponent<LoadBank>(prefab);
                    var bankName = sampleLoadBank.Banks[0].Name.Replace(_sampleCharacterName, prefabCharacterName);
                    var bank = AkWwiseProjectInfo.GetData().FindWwiseObject<BankExt>(bankName);
                    if (bank != null)
                    {
                        var newBank = new BankExt { UnloadOnDisable = sampleBank.UnloadOnDisable, UseCounter = sampleBank.UseCounter};
                        var newFinishEvents = new List<AudioEvent>();
                        foreach (var finishEvent in sampleBank.LoadFinishEvents)
                        {
                            var newEventName = finishEvent.Name.Replace(_sampleCharacterName, prefabCharacterName);
                            var evt = AkWwiseProjectInfo.GetData().FindWwiseObject<AudioEvent>(newEventName);
                            if (evt != null)
                            {
                                var newEvent = new AudioEvent();
                                newEvent.SetupReference(evt.Name, evt.Guid);
                                newFinishEvents.Add(newEvent);
                            }
                        }
                        newBank.SetupReference(bank.Name, bank.Guid);
                        newBank.LoadFinishEvents = newFinishEvents.ToArray();
                        prefabLoadBank.Banks = new[] {newBank};
                    }
                    if (_saveToXml)
                        AsComponentBackup.Instance.SaveComponentDataToXML(prefabPathShort, prefabLoadBank);
                }
                
                if (sampleEffectSound && sampleEffectSound.EnableEvents.Length > 0)
                {
                    var sampleEvent = sampleEffectSound.EnableEvents[0];
                    var prefabEffectSound = AsUnityHelper.GetOrAddComponent<EffectSound>(prefab);
                    var eventName = sampleEvent.Name.Replace(_sampleCharacterName, prefabCharacterName);
                    var evt = AkWwiseProjectInfo.GetData().FindWwiseObject<AudioEvent>(eventName);
                    if (evt != null)
                    {
                        var newEvent = new AudioEventExt
                        {
                            FadeOutTime = sampleEvent.FadeOutTime,
                            StopOnDisable = sampleEvent.StopOnDisable
                        };
                        newEvent.SetupReference(evt.Name, evt.Guid);
                        prefabEffectSound.EnableEvents = new[] {newEvent};
                    }
                    if (_saveToXml)
                        AsComponentBackup.Instance.SaveComponentDataToXML(prefabPathShort, prefabEffectSound);
                }
                AsComponentBackup.SaveComponentAsset(prefab, prefabPathShort);
            }
        }

    }
}