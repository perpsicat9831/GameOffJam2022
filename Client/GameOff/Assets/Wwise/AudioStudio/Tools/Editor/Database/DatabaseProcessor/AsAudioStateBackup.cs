using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AudioStudio.Components;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AudioStudio.Tools
{
    internal class AsAudioStateBackup : AsSearchers
    {
        #region Field
        //-----------------------------------------------------------------------------------------
        protected override string DefaultXmlPath
        {
            get { return AsScriptingHelper.CombinePath(XmlDocDirectory, "AudioStates.xml"); }
        }

        private AsComponentImportParser _asComponentImporter;
        private AsComponentExportParser _asComponentExporter;
        //-----------------------------------------------------------------------------------------
        #endregion

        #region init
        //-----------------------------------------------------------------------------------------
        private static AsAudioStateBackup _instance;
        internal static AsAudioStateBackup Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = CreateInstance<AsAudioStateBackup>();                    
                }
                return _instance;
            }
        }

        private void Awake()
        {
            _asComponentImporter = CreateInstance<AsComponentImportParser>();
            _asComponentExporter = CreateInstance<AsComponentExportParser>();
        }
        //-----------------------------------------------------------------------------------------
        #endregion

        #region Export
        //-----------------------------------------------------------------------------------------
        internal void Export(string inPath = "")
        {
            CleanUp();
            var fileName = EditorUtility.SaveFilePanel("Export to", XmlDocDirectory, "AudioStates.xml", ".xml");
            if (string.IsNullOrEmpty(fileName)) return;			
            FindFiles(ParseAnimator, "Exporting animation controllers...", "*.controller", inPath);			
            AsScriptingHelper.WriteXml(fileName, XRoot);
            EditorUtility.DisplayDialog("Process Finished!", "Found " + TotalCount + " components in " + EditedCount + " animator controllers!", "OK");		
        }

        internal void ParseAnimator(string assetPath)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            if (!controller) return;	
            var xAsset = new XElement("StateMachine");
            var found = false;
            foreach (var layer in controller.layers)
            {
                foreach (var behaviour in layer.stateMachine.behaviours)
                {
                    var audioState = behaviour as AudioState;
                    if (!audioState) continue;
                    found = true;
                    xAsset.Add(ParseComponent(audioState, layer.name, "OnLayer"));
                }			
                foreach (var state in layer.stateMachine.states)
                {
                    var animationState = state.state;
                    foreach (var behaviour in animationState.behaviours	)
                    {
                        var audioState = behaviour as AudioState;
                        if (!audioState) continue;
                        found = true;
                        xAsset.Add(ParseComponent(audioState, layer.name, animationState.name));
                    }					
                }				
            }
            if (found)
            {
                xAsset.SetAttributeValue("AssetPath", assetPath);
                XRoot.Add(xAsset);
                EditedCount++;
            }
        }
		
        private XElement ParseComponent(AudioState s, string layer, string stateName)
        {
            var xComponent = new XElement("Component");
            xComponent.SetAttributeValue("Layer", layer);
            xComponent.SetAttributeValue("AnimationState", stateName);
            _asComponentExporter.AudioStateExporter(s, xComponent);
            TotalCount++;
            return xComponent;
        }

        //-----------------------------------------------------------------------------------------
        #endregion
		
        #region Import
        //-----------------------------------------------------------------------------------------
        internal void Import()
        {
            CleanUp();
            var fileName = EditorUtility.OpenFilePanel("Import from", XmlDocDirectory, "xml");
            if (string.IsNullOrEmpty(fileName) || !ReadXmlData(fileName)) return;
            ImportAnimators(false);
            EditorUtility.DisplayProgressBar("Saving", "Overwriting assets...(might take a few minutes)", 1f);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Process Finished!", "Updated " + EditedCount + " animator controllers out of " + TotalCount, "OK");			
        }

        private void ImportAnimators(bool isCompare)
        {
            var xAnimators = XRoot.Elements().ToList();
            TotalCount = xAnimators.Count;
            try
            {
                for (var i = 0; i < TotalCount; i++)
                {
                    var assetPath = AsScriptingHelper.GetXmlAttribute(xAnimators[i], "AssetPath");
                    if (!assetPath.Contains(_defaultSearchingPath)) continue;
                    if (isCompare)
                        CompareAnimator(xAnimators[i]);
                    else
                        ImportAnimator(xAnimators[i]);
                    if (EditorUtility.DisplayCancelableProgressBar("Modifying controllers", assetPath, (i + 1f) / TotalCount)) break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            } 
        }

        internal bool ImportAnimator(XElement xAnimator)
        {
            var assetPath = AsScriptingHelper.GetXmlAttribute(xAnimator, "AssetPath");
            var modified = false;
            foreach (var xComponent in xAnimator.Elements())
            {
                var component = GetComponentFromXml(assetPath, xComponent, true);
                if (component && _asComponentImporter.AudioStateImporter(component, xComponent))
                {
                    EditorUtility.SetDirty(component);
                    modified = true;
                    EditedCount++;
                }
            }
            return modified;
        }
        //-----------------------------------------------------------------------------------------
        #endregion
        
        #region Locate
        //-----------------------------------------------------------------------------------------
        private XElement FindAssetNode(string assetPath, bool createIfMissing)
        {
            ReadXmlData();
            var xAsset = XRoot.Elements().FirstOrDefault(x => assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null && createIfMissing)
            {
                xAsset = new XElement("StateMachine");
                xAsset.SetAttributeValue("AssetPath", assetPath);
                XRoot.Add(xAsset);
            }
            return xAsset;
        }

        private static XElement FindComponentNode(XElement xAsset, string layer, string state)
        {
            return xAsset.Elements().FirstOrDefault(x => AsScriptingHelper.GetXmlAttribute(x, "Layer") == layer &&
                                                         AsScriptingHelper.GetXmlAttribute(x, "AnimationState") == state);
        }
        
        internal bool ComponentBackedUp(string assetPath, string layer, string state = "OnLayer")
        {
            var xAsset = FindAssetNode(assetPath, false);
            if (xAsset == null) return false;
            return FindComponentNode(xAsset, layer, state) != null;
        }
        
        internal static string GetLayerStateName(StateMachineBehaviour component, ref string stateName)
        {
            var path = AssetDatabase.GetAssetPath(component);
            var animator = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (!animator) return string.Empty;
            foreach (var layer in animator.layers)
            {
                if (layer.stateMachine.behaviours.Any(behaviour => behaviour == component))
                {					
                    return layer.name;
                }

                foreach (var state in layer.stateMachine.states)
                {			
                    var animationState = state.state;
                    if (animationState.behaviours.Any(behaviour => behaviour == component))
                    {
                        stateName = animationState.name;
                        return layer.name;
                    }					
                }
            }			
            return string.Empty;
        }

        private static AudioState GetComponentFromXml(string assetPath, XElement xComponent, bool addIfMissing)
        {
            var animator = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            if (!animator)
            {
                Debug.LogError("Backup Failed: Can't find Animator Controller at " + assetPath);
                return null;
            }
            var xAnimationState = AsScriptingHelper.GetXmlAttribute(xComponent, "AnimationState");
            var xLayer = AsScriptingHelper.GetXmlAttribute(xComponent, "Layer");
            foreach (var layer in animator.layers)
            {
                if (layer.name != xLayer) continue;
                if (xAnimationState == "OnLayer")
                {
                    foreach (var behaviour in layer.stateMachine.behaviours)
                    {
                        var audioState = behaviour as AudioState;
                        if (audioState) return audioState;							
                    }
                    if (addIfMissing) return layer.stateMachine.AddStateMachineBehaviour<AudioState>();
                }
                else
                {
                    foreach (var state in layer.stateMachine.states)
                    {
                        var s = state.state;
                        if (s.name != xAnimationState) continue;
                        foreach (var behaviour in s.behaviours)
                        {
                            var audioState = behaviour as AudioState;
                            if (audioState) return audioState;			
                        }
                        if (addIfMissing) return layer.stateMachine.AddStateMachineBehaviour<AudioState>();
                    }	
                }				
            }			
            return null;
        }
        //-----------------------------------------------------------------------------------------
        #endregion
        
        #region Compare
        //-----------------------------------------------------------------------------------------
        internal void Compare()
        {            
            var xmlPath = EditorUtility.OpenFilePanel("Compare with", XmlDocDirectory, "xml");
            if (string.IsNullOrEmpty(xmlPath) || !ReadXmlData(xmlPath)) return;
            ImportAnimators(true);                                                
            AsCompareAudioStateWindow.ShowWindow();          
            EditorUtility.ClearProgressBar();            
            CleanUp();
        }
        
        private void CompareAnimator(XElement xAnimator)
        {
            var assetPath = AsScriptingHelper.GetXmlAttribute(xAnimator, "AssetPath");
            foreach (var xComponent in xAnimator.Elements())
            {
                var data = new ComponentComparisonData
                {
                    AssetPath = assetPath,
                    LocalData = xComponent,
                    //XMLBackupStatus = ComponentXMLStatus.Unhandled
                };
                var component = GetComponentFromXml(assetPath, xComponent, false);
                //if (!component)
                //    AsCompareWindow.MissingComponents.Add(data);
                //else if (!component.IsValid())
                //    AsCompareWindow.EmptyComponents.Add(data);
                //else
                if (component && component.IsValid())
                {
                    var tempComponent = Instantiate(component);
                    if (_asComponentImporter.AudioStateImporter(tempComponent, xComponent))
                        AsCompareWindow.ModifiedComponents.Add(data);
                    DestroyImmediate(tempComponent, true);
                }
            }
        }

        

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Update
        //-----------------------------------------------------------------------------------------
        internal bool UpdateXmlFromComponent(string assetPath, AudioState component)
        {
            var state = "OnLayer";
            var layer = GetLayerStateName(component, ref state);
            var xAsset = FindAssetNode(assetPath, true);
            var xComponent = FindComponentNode(xAsset, layer, state);
            if (xComponent != null)
            {				
                var xTemp = ParseComponent(component, layer, state);
                if (XNode.DeepEquals(xTemp, xComponent)) return false;
                xComponent.ReplaceWith(xTemp);    
            }
            else
                xAsset.Add(ParseComponent(component, layer, state));
            AsScriptingHelper.WriteXml(DefaultXmlPath, XRoot);
            return true;
        }

        //-----------------------------------------------------------------------------------------
        #endregion
        
        #region Revert
        //-----------------------------------------------------------------------------------------
        internal bool RevertComponentToXml(string assetPath, AudioState component)
        {            
            var state = "OnLayer";
            var layer = GetLayerStateName(component, ref state);
            var xAsset = FindAssetNode(assetPath, true);
            var xComponent = FindComponentNode(xAsset, layer, state);
            return xComponent != null ? _asComponentImporter.AudioStateImporter(component, xComponent) : UpdateXmlFromComponent(assetPath, component);
        }

        //-----------------------------------------------------------------------------------------
        #endregion
        
        #region Remove
        //-----------------------------------------------------------------------------------------
        internal void RemoveUnsaved()
        {
            CleanUp();
            FindFiles(RemoveUnsavedInAnimator, "Removing Audio States", "*.playable");
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Process Finished!", "Removed " + EditedCount + " audio states in " + TotalCount + " animator controllers!", "OK");
        }
        
        internal void RemoveAll()
        {
            CleanUp();
            FindFiles(RemoveAllInAnimator, "Removing Audio States", "*.playable");
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Process Finished!", "Removed " + EditedCount + " audio states in " + TotalCount + " animator controllers!", "OK");
        }

        internal void RemoveUnsavedInAnimator(string assetPath)
        {                        
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            if (!controller) return;
            var toBeDeleted = new List<AudioState>();
            foreach (var layer in controller.layers)
            {
                toBeDeleted.AddRange(layer.stateMachine.behaviours.OfType<AudioState>().Where(a => !ComponentBackedUp(assetPath, layer.name)));
                foreach (var state in layer.stateMachine.states)
                {
                    toBeDeleted.AddRange(state.state.behaviours.OfType<AudioState>().Where(a => !ComponentBackedUp(assetPath, layer.name, state.state.name)));
                }
            }

            TotalCount++;
            if (toBeDeleted.Count == 0) return;
            foreach (var component in toBeDeleted)
            {
                RemoveComponentXml(assetPath, component);
                DestroyImmediate(component);
                EditedCount++;
            }
        }

        internal void RemoveAllInAnimator(string assetPath)
        {                        
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            if (!controller) return;
            var xAsset = FindAssetNode(assetPath, false);
            if (xAsset != null)
                xAsset.Remove();
            
            var toBeDeleted = new List<AudioState>();
            foreach (var layer in controller.layers)
            {
                toBeDeleted.AddRange(layer.stateMachine.behaviours.OfType<AudioState>());
                foreach (var state in layer.stateMachine.states)
                {
                    toBeDeleted.AddRange(state.state.behaviours.OfType<AudioState>());
                }
            }

            TotalCount++;
            if (toBeDeleted.Count == 0) return;
            foreach (var component in toBeDeleted)
            {
                DestroyImmediate(component);
                EditedCount++;
            }
        }

        // remove node from component inspector
        internal void RemoveComponentXml(string assetPath, AudioState component)
        {
            ReadXmlData();
            var state = "OnLayer";
            var layer = GetLayerStateName(component, ref state);
            var xAsset = FindAssetNode(assetPath, false);
            if (xAsset == null) return;
            var xComponent = FindComponentNode(xAsset, layer, state);
            if (xComponent == null) return;
            AsScriptingHelper.RemoveComponentXml(xComponent);
            AsScriptingHelper.WriteXml(DefaultXmlPath, XRoot);
        }
        
        // remove node from compare window
        internal void RemoveComponentXml(ComponentComparisonData data)
        {
            var xAsset = FindAssetNode(data.AssetPath, false);
            var xComponent = xAsset.Elements().FirstOrDefault(x => XNode.DeepEquals(x, data.LocalData));
            if (xComponent == null) return;
            AsScriptingHelper.RemoveComponentXml(xComponent);
            AsScriptingHelper.WriteXml(DefaultXmlPath, XRoot);
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        
    }	
}