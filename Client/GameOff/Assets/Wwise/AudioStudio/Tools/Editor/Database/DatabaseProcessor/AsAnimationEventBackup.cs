using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Xml.Linq;
using AudioStudio.Components;

namespace AudioStudio.Tools
{
    internal class AsAnimationEventBackup : AsSearchers
    {
        #region Fields          
        //-----------------------------------------------------------------------------------------
        protected override string DefaultXmlPath {
            get
            {
                return AsScriptingHelper.CombinePath(XmlDocDirectory, "AnimationEvents.xml");        
            }
        }

        private AsCompareAnimationEventWindow _compareWindow;
        //-----------------------------------------------------------------------------------------
        #endregion

        #region Init
        //-----------------------------------------------------------------------------------------
        private static AsAnimationEventBackup _instance;
        internal static AsAnimationEventBackup Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = CreateInstance<AsAnimationEventBackup>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            searchPrefab = false;
            searchScene = true;
            XRoot = new XElement("Root");
        }
        //-----------------------------------------------------------------------------------------
        #endregion

        #region Export
        //-----------------------------------------------------------------------------------------

        internal void Export(string inPath = "")
        {
            CleanUp();
            var completed = false;
            _importedXmlPath = EditorUtility.SaveFilePanel("Export to", 
                XmlDocDirectory, "AnimationEvents.xml", "xml");
            if (string.IsNullOrEmpty(_importedXmlPath)) return;		
            
            if (searchPrefab) 
                completed = FindFiles(ParseAnimation, 
                    "Exporting animation clips...", "*.anim", inPath);
            if (searchScene) 
                completed =  FindFiles(ParsePrefab, 
                    "Exporting FBX files...", "*.FBX", inPath);

            if (!completed) return;
            ShowExportDialog();
            EditorUtility.ClearProgressBar();
        }

        internal void ParseAnimation(string assetPath)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (clip == null) return;

            var xClip = ParseAnimationClip(clip, assetPath);

            if (!xClip.HasElements) return;
            XRoot.Add(xClip);
            TotalCount++;
        }

        /// <summary>
        /// Export individual animation clip assets
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private XElement ParseAnimationClip(AnimationClip clip, string assetPath)
        {
            var xClip = new XElement("Animation");
            xClip.SetAttributeValue("AssetPath", assetPath);

            var events = clip.events;
            foreach (var evt in events)
            {
                // only export AudioStudio events
                if (IsAudioStudioAnimationEvent(evt))
                {
                    var xEvent = ParseEvent(evt);
                    xClip.Add(xEvent);
                    SaveSingleModifiedComponent(assetPath, "", null, 
                        xClip, ComponentStatus.LocalOnly);
                }
            }
            return xClip;
        }

        internal void ParsePrefab(string assetPath)
        {
            var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (modelImporter == null) return;		
			
            var xModel = new XElement("Model");
            foreach (var clip in modelImporter.clipAnimations)
            {
                var xClip = ParseModelClip(clip, assetPath);
                if (!xClip.HasElements) continue;
                xModel.Add(xClip);
                TotalCount++;
            }
            if (!xModel.HasElements) return;
            xModel.SetAttributeValue("AssetPath", assetPath);
            XRoot.Add(xModel);
        }
		
        private XElement ParseModelClip(ModelImporterClipAnimation clip, string assetPath)
        {
            var xClip = new XElement("Clip");
            xClip.SetAttributeValue("Name", clip.name);

            var events = clip.events;
            foreach (var evt in events)
            {
                if (IsAudioStudioAnimationEvent(evt))
                {
                    var xEvent = ParseEvent(evt);
                    xClip.Add(xEvent);
                    SaveSingleModifiedComponent(assetPath, "", null,
                        xClip, ComponentStatus.LocalOnly);
                }
                xClip.Add(ParseEvent(evt));
            }	
            return xClip;
        }
		
        private XElement ParseEvent(AnimationEvent evt)
        {
            EditedCount++;
            var xAnimationEvent = new XElement("AnimationEvent");
            xAnimationEvent.SetAttributeValue("Function", evt.functionName);
            xAnimationEvent.SetAttributeValue("AudioEvent", evt.stringParameter);
            xAnimationEvent.SetAttributeValue("Time", evt.time);
            return xAnimationEvent;
        }

        private void ShowExportDialog()
        {
            int dialog = EditorUtility.DisplayDialogComplex(
                "Process Finished!",
                string.Format("Found {0} animation events in {1} clips!",
                EditedCount, 
                TotalCount), 
                "Save", "Cancel", "More Details");

            switch (dialog)
            {
                // Save
                case 0:
                    AsCompareWindow.ClearBuffer();
                    AsScriptingHelper.WriteXml(_importedXmlPath, XRoot);
                    break;
                // Cancel
                case 1:
                    AsCompareWindow.ClearBuffer();
                    break;
                // More Details
                case 2:
                    CreateWindow();
                    break;
                default:
                    Debug.LogError("Unrecognized option.");
                    break;
            }
        }
        //-----------------------------------------------------------------------------------------
        #endregion

        #region Import
        //-----------------------------------------------------------------------------------------
        internal void Import(string inPath = "")
        {
            CleanUp();
            _importedXmlPath = EditorUtility.OpenFilePanel("Import from", XmlDocDirectory, "xml");
            if (string.IsNullOrEmpty(_importedXmlPath) || !ReadXmlData(_importedXmlPath))
            {
                Debug.Log("Can't match the path");
                return;
            }
            
            if (searchPrefab) ImportClips(inPath);
            if (searchScene) ImportModels(inPath);
            
            EditorUtility.DisplayProgressBar("Saving", 
                "Overwriting assets...(might take a few minutes)", 1f);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Process Finished!", 
                string.Format("Updated {0} animation clips out of {1}", 
                EditedCount, TotalCount), "OK");			
        }

        private void ImportClips(string inPath)
        {
            var xClips = XRoot.Elements("Animation").ToList();
            try
            {
                for (var i = 0; i < xClips.Count; i++)
                {
                    var assetPath = AsScriptingHelper.GetXmlAttribute(xClips[i], "AssetPath");
                    if (!assetPath.Contains(inPath)) continue;
                    ImportClip(xClips[i]);
                    if (EditorUtility.DisplayCancelableProgressBar("Processing clips", 
                        assetPath , (i + 1f) / TotalCount)) 
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            }
        }		
        
        // import individual animation clip assets
        internal bool ImportClip(XElement xClip)
        {
            var assetPath = AsScriptingHelper.GetXmlAttribute(xClip, "AssetPath");
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (!clip) return false;
            
            var modified = false;
            // find non audio events to initialize
            var events = clip.events.Where(evt => !IsAudioStudioAnimationEvent(evt)).ToList();                        	                                    
            ImportEvents(events, xClip.Elements());
            // event count is different, data must have changed
            if (events.Count != clip.events.Length)
            {
                AnimationUtility.SetAnimationEvents(clip, events.ToArray());
                modified = true;
            }
            // sort events by time for comparison
            events = events.OrderBy(e => e.time).ToList();
            if (events.Where((t, i) => !CompareAnimationEvent(clip.events[i], t)).Any())
            {
                AnimationUtility.SetAnimationEvents(clip, events.ToArray());
                modified = true;
            }
            return modified;
        }
		
        private void ImportModels(string inPath)
        {
            var xModels = XRoot.Elements("Model").ToList();
            try
            {
                for (var i = 0; i < xModels.Count; i++)
                {
                    var assetPath = AsScriptingHelper.GetXmlAttribute(xModels[i], "AssetPath");
                    if (!assetPath.Contains(inPath)) continue;
                    ImportModel(xModels[i]);
                    if (EditorUtility.DisplayCancelableProgressBar("Processing models", 
                        assetPath, (i + 1f) / TotalCount)) 
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            }
        }

        internal bool ImportModel(XElement xModel)
        {			
            var assetPath = AsScriptingHelper.GetXmlAttribute(xModel, "AssetPath");
            var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (modelImporter == null) return false;
			
            var modified = false;
            // model clips can only be replaced as a whole, 
            // so creating a new list and populate with clips
            var newClips = new List<ModelImporterClipAnimation>(); 
            foreach (var clip in modelImporter.clipAnimations)
            {
                var clipName = clip.name;
                // find corresponding node by clip name
                var xClip = xModel.Elements().FirstOrDefault(x => 
                clipName == AsScriptingHelper.GetXmlAttribute(x, "Name"));
                if (xClip != null && ImportModelClip(clip, xClip.Elements("AnimationEvent"))) 
                    modified = true;					
                newClips.Add(clip);
            }

            if (modified)
            {
                modelImporter.clipAnimations = newClips.ToArray();
                EditorUtility.SetDirty(modelImporter);
                AssetDatabase.ImportAsset(assetPath);
            }
            return modified;
        }

        // import clips embedded in models
        private bool ImportModelClip(ModelImporterClipAnimation clip, IEnumerable<XElement> xEvents)
        {
            var events = clip.events.Where(evt => !IsAudioStudioAnimationEvent(evt)).ToList();
            ImportEvents(events, xEvents);
            if (events.Count != clip.events.Length)
            {
                EditedCount++;
                clip.events = events.ToArray();
                return true;
            }		
            events = events.OrderBy(e => e.time).ToList();
            if (events.Where((t, i) => !CompareAnimationEvent(clip.events[i], t)).Any())
            {
                EditedCount++;
                clip.events = events.ToArray();
                return true;
            }
            return false;
        }

        private void ImportEvents(ICollection<AnimationEvent> events, IEnumerable<XElement> xEvents)
        {
            foreach (var e in xEvents)
            {
                TotalCount++;
                var animEvent = new AnimationEvent
                {
                    time = AsScriptingHelper.StringToFloat(AsScriptingHelper.GetXmlAttribute(e, "Time")),
                    stringParameter = AsScriptingHelper.GetXmlAttribute(e, "AudioEvent"),
                    functionName = AsScriptingHelper.GetXmlAttribute(e, "Function")
                };
                events.Add(animEvent);
            }
        }

        //-----------------------------------------------------------------------------------------
        #endregion
		
        #region Compare
        //-----------------------------------------------------------------------------------------
        internal void Compare(string inPath = "")
        {            
            CleanUp();
            _importedXmlPath = EditorUtility.OpenFilePanel("Compare with", XmlDocDirectory, "xml");
            if (string.IsNullOrEmpty(_importedXmlPath) || !ReadXmlData(_importedXmlPath))
            {
                Debug.Log("Can't match the path");
                return;
            }

            if (searchPrefab) CompareClips(inPath);            
            if (searchScene) CompareModels(inPath);

            EditorUtility.DisplayProgressBar("Saving",
                "Overwriting assets...(might take a few minutes)", 1f);
            EditorUtility.ClearProgressBar();
            CreateWindow();
            
        }

        private void CompareClips(string inPath)
        {
            try
            {
                FindFiles(FindUnsavedAnimationCips,
                    "Find unsaved animation clips...", "*.anim", inPath);
                var xClips = XRoot.Elements("Animation").ToList();
                for (var i = 0; i < xClips.Count; i++)
                {
                    var assetPath = AsScriptingHelper.GetXmlAttribute(xClips[i], "AssetPath");
                    if (!assetPath.Contains(inPath)) continue;
                    CompareAnimationClip(xClips[i]);
                    if (EditorUtility.DisplayCancelableProgressBar("Processing clips",
                        assetPath, (i + 1f) / TotalCount))
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            }
        }

        private void FindUnsavedAnimationCips(string assetPath)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (clip == null) return;

            var xClips = XRoot.Elements("Animation").ToList();
            var events = clip.events;

            var matchClip = xClips.FirstOrDefault(x => clip.name ==
            AsScriptingHelper.GetXmlAttribute(x, "Name"));
            if (matchClip != null) return;
            BackupUnsavedEvents(assetPath, "", clip.events);
            
        }

        /// <summary>
        /// Wrapper for getting events from clip 
        /// </summary>
        /// <param name="xClip"></param>
        private void CompareAnimationClip(XElement xClip)
        {
            var assetPath = AsScriptingHelper.GetXmlAttribute(xClip, "AssetPath");
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (!clip)
            {
                Debug.LogWarning("Can't find the clip");
                return;
            }
            CompareClip(assetPath, "", xClip, clip.events);
        }

        private void CompareModels(string inPath)
        {
            try
            {
                FindFiles(FindUnsavedFBXPrefabs,
                    "Find unsaved models...", "*.FBX", inPath);
                var xModels = XRoot.Elements("Model").ToList();
                for (var i = 0; i < xModels.Count; i++)
                {
                    var assetPath = AsScriptingHelper.
                        GetXmlAttribute(xModels[i], "AssetPath");
                    if (!assetPath.Contains(inPath)) continue;
                    CompareModel(xModels[i]);
                    if (EditorUtility.DisplayCancelableProgressBar("Processing models",
                        assetPath, (i + 1f) / TotalCount))
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            }
        }

        private void FindUnsavedFBXPrefabs(string assetPath)
        {
            var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (modelImporter == null) return;

            var xModels = XRoot.Elements("Model").ToList();
            var matchModel = xModels.FirstOrDefault(x => assetPath ==
            AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            var clips = modelImporter.clipAnimations;
            if (clips.Length == 0) return;

            if (matchModel == null)
            {
                foreach (var clip in clips)
                {
                    BackupUnsavedEvents(assetPath, clip.name, clip.events);
                }
                return;
            }
            
            var xClips = matchModel.Elements();
            foreach (var clip in clips)
            {
                var matchClip = xClips.First(x => clip.name ==
                AsScriptingHelper.GetXmlAttribute(x, "Name"));
                if (matchClip != null) continue; ;
                BackupUnsavedEvents(assetPath, clip.name, clip.events);
            }

        }

        private void BackupUnsavedEvents(string assetPath, 
            string clipName, AnimationEvent[] clipEvents)
        {
            foreach (var evt in clipEvents)
            {
                if (!IsAudioStudioAnimationEvent(evt)) continue;
                var localEventXML = ParseEvent(evt);
                SaveSingleModifiedComponent(assetPath, clipName, null,
                    localEventXML, ComponentStatus.LocalOnly);
            }
        }

        private void CompareModel(XElement xModel)
        {			
            var assetPath = AsScriptingHelper.GetXmlAttribute(xModel, "AssetPath");
            var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (modelImporter == null) return;
			
            foreach (var clip in modelImporter.clipAnimations)
            {
                var clipName = clip.name;
                // find corresponding node by clip name
                var xClip = xModel.Elements().FirstOrDefault(x => 
                clipName == AsScriptingHelper.GetXmlAttribute(x, "Name"));
                // if local clip not found
                if (xClip == null)
                {
                    var events = clip.events;
                    if (events.Length == 0) continue;
                    foreach (var evt in events)
                    {
                        if (!IsAudioStudioAnimationEvent(evt)) continue;
                        var localEventXML = ParseEvent(evt);
                        SaveSingleModifiedComponent(assetPath, clipName, null, 
                            localEventXML, ComponentStatus.LocalOnly);
                    }
                    TotalCount++;
                    continue;
                }
                CompareClip(assetPath, clipName, xClip, clip.events);
            }
        }

        /// <summary>
        /// Actual place where comparison is done
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="xClip"></param>
        /// <param name="localEvents"></param>
        private void CompareClip(string assetPath, string clipName, 
            XElement xClip, AnimationEvent[] localEvents)
        {
            var xEvents = xClip.Elements("AnimationEvent");
            var importEvents = localEvents.Where(evt => 
            !IsAudioStudioAnimationEvent(evt)).ToList();
            // If no events are found
            if (importEvents.Count == localEvents.Length || localEvents.Length == 0)
            {
                foreach (var xEvent in xEvents)
                {
                    SaveSingleModifiedComponent(assetPath, clipName, xEvent,
                        null, ComponentStatus.ServerOnly);
                }
                return;
            }
            ImportEvents(importEvents, xEvents);
            // events = events.OrderBy(e => e.time).ToList();
            // search server only event
            for (int index = 0; index < importEvents.Count; index++)
            {
                if (!IsAudioStudioAnimationEvent(importEvents[index]))
                    continue;
                var localMatchEvents = localEvents.ToList().Where(x =>
                CompareAnimationEvent(x, importEvents[index]));
                if (localMatchEvents.Count() > 0)
                    continue;
                SaveSingleModifiedComponent(assetPath, clipName, xEvents.ElementAt(index),
                        null, ComponentStatus.ServerOnly);
            }
            // search local only event
            for (int index = 0; index < localEvents.Count(); index++)
            {
                if (!IsAudioStudioAnimationEvent(localEvents[index]))
                    continue;
                var serverMatchEvents = importEvents.Where(x =>
                CompareAnimationEvent(x, localEvents[index]));
                if (serverMatchEvents.Count() > 0)
                    continue;
                SaveSingleModifiedComponent(assetPath, clipName, null,
                    ParseEvent(localEvents[index]), ComponentStatus.LocalOnly);
            }
        }

        /// <summary>
        /// Check if events are equal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool CompareAnimationEvent(AnimationEvent a, AnimationEvent b)
        {			
            return Math.Abs(a.time - b.time) < 0.01f && 
                a.stringParameter == b.stringParameter && 
                a.functionName == b.functionName;
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Revert
        //-----------------------------------------------------------------------------------------
        internal void SaveXmlDataToComponent(ComponentComparisonData data)
        {
            AnimationComparisonData animData = data as AnimationComparisonData;
            //var xAsset = FindAssetNode(animData.AssetPath, true);
            if (string.IsNullOrEmpty(animData.ClipName))
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animData.AssetPath);
                if (!clip) return;

                var newEventList = AddEventXmlToLocal(data.ServerData, clip.events);
                AnimationUtility.SetAnimationEvents(clip, newEventList.ToArray());
            }
            else
            {
                var modelImporter = AssetImporter.GetAtPath(animData.AssetPath) as ModelImporter;
                if (modelImporter == null) return;

                var clips = modelImporter.clipAnimations;
                if (clips.Length == 0) return;

                var clip = clips.FirstOrDefault(c => c.name == animData.ClipName);
                if (clip == null) return;

                var newEventList = AddEventXmlToLocal(data.ServerData, clip.events);
                clip.events = newEventList.ToArray();

                var newClips = new List<ModelImporterClipAnimation>();
                newClips.Add(clip);
                newClips.AddRange(modelImporter.clipAnimations.Where(c => c.name != clip.name));
                modelImporter.clipAnimations = newClips.ToArray();
                AssetDatabase.ImportAsset(animData.AssetPath);
            }
        }

        private void AddLocalComponent(ComponentComparisonData data)
        {
            AnimationComparisonData animData = data as AnimationComparisonData;
            //var xAsset = FindAssetNode(animData.AssetPath, true);
            if (string.IsNullOrEmpty(animData.ClipName))
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animData.AssetPath);
                if (!clip) return;

                var newEventList = AddEventXmlToLocal(data.LocalData, clip.events);
                AnimationUtility.SetAnimationEvents(clip, newEventList.ToArray());
            }
            else
            {
                var modelImporter = AssetImporter.GetAtPath(animData.AssetPath) as ModelImporter;
                if (modelImporter == null) return;

                var clips = modelImporter.clipAnimations;
                if (clips.Length == 0) return;

                var clip = clips.FirstOrDefault(c => c.name == animData.ClipName);
                if (clip == null) return;

                var newEventList = AddEventXmlToLocal(data.LocalData, clip.events);
                clip.events = newEventList.ToArray();

                var newClips = new List<ModelImporterClipAnimation>();
                newClips.Add(clip);
                newClips.AddRange(modelImporter.clipAnimations.Where(c => c.name != clip.name));
                modelImporter.clipAnimations = newClips.ToArray();
                AssetDatabase.ImportAsset(data.AssetPath);
            }
        }

        private List<AnimationEvent> AddEventXmlToLocal(XElement eventData, 
            AnimationEvent[] clipEvents)
        {
            var animEvent = new AnimationEvent
            {
                time = AsScriptingHelper.StringToFloat(AsScriptingHelper.
                                GetXmlAttribute(eventData, "Time")),
                stringParameter = AsScriptingHelper.
                                GetXmlAttribute(eventData, "AudioEvent"),
                functionName = AsScriptingHelper.
                                GetXmlAttribute(eventData, "Function")
            };
            var newEventList = clipEvents.ToList();
            var sameEvent = newEventList.FirstOrDefault(evt => 
            evt.functionName == animEvent.functionName &&
            evt.stringParameter == animEvent.stringParameter &&
            Mathf.Abs(evt.time - animEvent.time) < 0.01);
            if (sameEvent == null) newEventList.Add(animEvent);
            return newEventList;
            
        }

        private void SaveComponentDataToXML(ComponentComparisonData data)
        {
            var xAsset = XRoot.Elements().FirstOrDefault(element => 
            AsScriptingHelper.GetXmlAttribute(element, "AssetPath") == data.AssetPath);

            if (data.AssetPath.EndsWith(".anim"))
            {
                if (xAsset == null)
                {
                    xAsset = new XElement("Animation");
                    xAsset.SetAttributeValue("AssetPath", data.AssetPath);
                    XRoot.Add(xAsset);
                }
                var sameEvent = xAsset.Elements().FirstOrDefault(xEvent =>
                XNode.DeepEquals(xEvent, data.LocalData));
                if (sameEvent == null) xAsset.Add(data.LocalData);
            }
            else if (data.AssetPath.EndsWith(".FBX"))
            {
                var animData = data as AnimationComparisonData;
                if (xAsset == null)
                {
                    xAsset = new XElement("Model");
                    xAsset.SetAttributeValue("AssetPath", animData.AssetPath);
                    XRoot.Add(xAsset);
                }
                var xClip = xAsset.Elements().FirstOrDefault(clip =>
                AsScriptingHelper.GetXmlAttribute(clip, "Name") == animData.ClipName);
                if (xClip == null)
                {
                    xClip = new XElement("Clip");
                    xClip.SetAttributeValue("Name", animData.ClipName);
                    xAsset.Add(xClip);
                }
                var sameEvent = xClip.Elements().FirstOrDefault(xEvent =>
                XNode.DeepEquals(xEvent, data.LocalData));
                if (sameEvent == null) xClip.Add(data.LocalData);
            }
        }

        private void AddServerXml(ComponentComparisonData data)
        {
            var xAsset = XRoot.Elements().FirstOrDefault(element =>
            AsScriptingHelper.GetXmlAttribute(element, "AssetPath") == data.AssetPath);

            if (data.AssetPath.EndsWith(".anim"))
            {
                if (xAsset == null)
                {
                    xAsset = new XElement("Animation");
                    xAsset.SetAttributeValue("AssetPath", data.AssetPath);
                    XRoot.Add(xAsset);
                }
                var sameEvent = xAsset.Elements().FirstOrDefault(xEvent =>
                XNode.DeepEquals(xEvent, data.ServerData));
                if (sameEvent == null) xAsset.Add(data.ServerData);
            }
            else if (data.AssetPath.EndsWith(".FBX"))
            {
                var animData = data as AnimationComparisonData;
                if (xAsset == null)
                {
                    xAsset = new XElement("Model");
                    xAsset.SetAttributeValue("AssetPath", animData.AssetPath);
                    XRoot.Add(xAsset);
                }
                var xClip = xAsset.Elements().FirstOrDefault(clip =>
                AsScriptingHelper.GetXmlAttribute(clip, "Name") == animData.ClipName);
                if (xClip == null)
                {
                    xClip = new XElement("Clip");
                    xClip.SetAttributeValue("Name", animData.ClipName);
                    xAsset.Add(xClip);
                }
                var sameEvent = xClip.Elements().FirstOrDefault(xEvent =>
                XNode.DeepEquals(xEvent, data.ServerData));
                if (sameEvent == null) xClip.Add(data.ServerData);
            }
        }

        
        //-----------------------------------------------------------------------------------------
        #endregion

        #region Remove
        //-----------------------------------------------------------------------------------------
        internal void RemoveAll()
        {
            ReadXmlData();
            if (searchPrefab) FindFiles(RemoveClip, "Removing animation clips...", "*.anim");
            if (searchScene) FindFiles(RemoveModel, "Removing FBX files...", "*.fbx");
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Process Finished!", "Removed " + EditedCount + 
                " animation events in " + TotalCount + " clips and models!", "OK");
        }

        internal void RemoveClip(string assetPath)
        {                        
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (!clip || clip.events.Length == 0) return;
            var toBeRemained = clip.events.Where(animationEvent => 
            !IsAudioStudioAnimationEvent(animationEvent)).ToArray();
            TotalCount++;
            if (toBeRemained.Length == clip.events.Length) return;
            clip.events = toBeRemained;
            EditedCount++;
            EditorUtility.SetDirty(clip);
        }

        internal void RemoveModel(string assetPath)
        {                        
            var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (modelImporter == null) return;
			
            var modified = false;
            var newClips = new List<ModelImporterClipAnimation>();
            TotalCount++;
            foreach (var clip in modelImporter.clipAnimations)
            {
                if (clip.events.Length == 0) continue;
                var toBeRemained = clip.events.Where(animationEvent => 
                !IsAudioStudioAnimationEvent(animationEvent)).ToArray();
                if (toBeRemained.Length == clip.events.Length) continue;
                clip.events = toBeRemained;
                modified = true;
                newClips.Add(clip);
            }

            if (modified)
            {
                modelImporter.clipAnimations = newClips.ToArray();
                AssetDatabase.ImportAsset(assetPath);
                EditedCount++;
            }
        }

        internal void RemoveLocalAnimationEvent(ComponentComparisonData data)
        {
            AnimationComparisonData animData = data as AnimationComparisonData;
            var xmlData = animData.LocalData != null ? animData.LocalData : animData.ServerData;
            if (string.IsNullOrEmpty(animData.ClipName))
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animData.AssetPath);
                if (!clip || clip.events.Length == 0) return;

                var newAnimEvents = RemoveSingleClipEvent(xmlData, clip.events);
                AnimationUtility.SetAnimationEvents(clip, newAnimEvents);
                EditorUtility.SetDirty(clip);
            }
            else
            {
                var modelImporter = AssetImporter.GetAtPath(animData.AssetPath) as ModelImporter;
                if (modelImporter == null) return;

                var clips = modelImporter.clipAnimations;
                if (clips.Length == 0) return;

                var clip = clips.FirstOrDefault(c => c.name == animData.ClipName);
                if (clip == null || clip.events.Length == 0) return;
                clip.events = RemoveSingleClipEvent(xmlData, clip.events);
                var newClips = new List<ModelImporterClipAnimation>();
                newClips.Add(clip);
                newClips.AddRange(modelImporter.clipAnimations.Where(c => c.name != clip.name));
                modelImporter.clipAnimations = newClips.ToArray();
                AssetDatabase.ImportAsset(data.AssetPath);
            }
        }

        private AnimationEvent[] RemoveSingleClipEvent(XElement animEventData, 
            AnimationEvent[] clipEvents)
        {
            var remainedClipEvents = clipEvents.Where(evt =>
                            evt.functionName != AsScriptingHelper.
                            GetXmlAttribute(animEventData, "Function") ||
                            evt.stringParameter != AsScriptingHelper.
                            GetXmlAttribute(animEventData, "AudioEvent") ||
                            Mathf.Abs(evt.time - float.Parse(AsScriptingHelper.
                            GetXmlAttribute(animEventData, "Time"))) >= 0.01);

            clipEvents = remainedClipEvents.ToArray();
            return clipEvents;
        }

        internal void RemoveServerAnimationEventXml(ComponentComparisonData data)
        {
            AnimationComparisonData animData = data as AnimationComparisonData;
            var xmlData = animData.LocalData != null ? animData.LocalData : animData.ServerData;
            var xAsset = XRoot.Elements().FirstOrDefault(x =>
            animData.AssetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null) return;

            if (string.IsNullOrEmpty(animData.ClipName))
            {
                var xEvent = xAsset.Elements().
                    FirstOrDefault(x => XNode.DeepEquals(x, xmlData));
                if (xEvent != null)
                    AsScriptingHelper.RemoveComponentXml(xEvent);
            }
            else
            {
                var xClip = xAsset.Elements().FirstOrDefault(x =>
                animData.ClipName == AsScriptingHelper.GetXmlAttribute(x, "Name"));
                if (xClip == null) return;

                var xEvent = xClip.Elements().
                    FirstOrDefault(x => XNode.DeepEquals(x, xmlData));
                if (xEvent != null)
                    AsScriptingHelper.RemoveComponentXml(xEvent);
            }
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        private static bool IsAudioStudioAnimationEvent(AnimationEvent animationEvent)
        {
            return typeof(AnimationSound).GetMethods().Where(method => method.IsPublic).
                Any(method => animationEvent.functionName == method.Name);
        }

        private static void SaveSingleModifiedComponent(string assetPath, string clipName,
            XElement inServerData, XElement inLocalData, ComponentStatus inComponentStatus)
        {
            var data = new AnimationComparisonData
            {
                AssetPath = assetPath,
                ClipName = clipName,
                ServerData = inServerData,
                LocalData = inLocalData,
                ComponentStatus = inComponentStatus
            };
            AsCompareWindow.ModifiedComponents.Add(data);
        }

        private void CreateWindow()
        {
            _compareWindow = GetWindow<AsCompareAnimationEventWindow>();
            _compareWindow.position = new Rect(500, 300, 800, 600);
            _compareWindow.titleContent = new GUIContent("Compare Components");
            _compareWindow.saveSingleDataEvent = ProcessCompareData;
        }

        private void ProcessCompareData(ComponentComparisonData data)
        {
            if (!data.selectionToggle) return;

            switch (data.ComponentStatus)
            {
                case ComponentStatus.AllRemoved:
                    RemoveLocalAnimationEvent(data);
                    RemoveServerAnimationEventXml(data);
                    break;
                case ComponentStatus.UseServer:
                    SaveXmlDataToComponent(data);
                    AddServerXml(data);
                    break;
                case ComponentStatus.UseLocal:
                    SaveComponentDataToXML(data);
                    AddLocalComponent(data);
                    break;
            }

            AsScriptingHelper.WriteXml(_importedXmlPath, XRoot);
        }
    }  	
}