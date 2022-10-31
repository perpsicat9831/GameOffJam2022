using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Xml.Linq;
using AudioStudio.Timeline;
using UnityEngine.Timeline;

namespace AudioStudio.Tools
{
    internal class AsTimelineAudioBackup : AsSearchers
    {
        #region Field
        //-----------------------------------------------------------------------------------------

        protected override string DefaultXmlPath
        {
            get { return AsScriptingHelper.CombinePath(XmlDocDirectory, "WwiseTimelineClip.xml"); }
        }

        private AsComponentImportParser _asComponentImporter;
        private AsComponentExportParser _asComponentExporter;

        private AsCompareTimelineWindow _compareWindow;
        //-----------------------------------------------------------------------------------------
        #endregion

        #region init
        //-----------------------------------------------------------------------------------------

        private static AsTimelineAudioBackup _instance;
        internal static AsTimelineAudioBackup Instance
        {
            get
            {
                if (!_instance)
                    _instance = CreateInstance<AsTimelineAudioBackup>();
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
            var completed = false;
            _importedXmlPath = EditorUtility.SaveFilePanel("Export to", 
                XmlDocDirectory, "TimelineClip.xml", ".xml");
            if (string.IsNullOrEmpty(_importedXmlPath)) return;
            
            completed = FindFiles(ParseTimeline, "Exporting Timeline Assets", "*.playable", inPath);

            if (!completed) return;
            ShowExportDialog();
        }

        internal void ParseTimeline(string assetPath)
        {                        
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(assetPath);
            if (!timeline) return;
            var xTimeline = new XElement("Timeline");
            foreach (var track in timeline.GetOutputTracks())
            {     
                if (!(track is WwiseTimelineTrack)) continue;
                foreach (var clip in track.GetClips())
                {
                    var asset = clip.asset as WwiseTimelineClip;
                    if (asset == null) continue;
                    xTimeline.Add(ParseComponent(asset, track.name, clip));
                }
            }

            if (xTimeline.HasElements)
            {
                xTimeline.SetAttributeValue("AssetPath", assetPath);
                XRoot.Add(xTimeline);
            }
            EditedCount++;
        }

        private XElement ParseComponent(WwiseTimelineClip asset, string trackName, TimelineClip clip)
        {
            var xComponent = new XElement("Component");
            xComponent.SetAttributeValue("TrackName", trackName);
            xComponent.SetAttributeValue("ClipName", clip.displayName);
            _asComponentExporter.WwiseTimelineClipExporter(asset, clip, xComponent);
            TotalCount++;
            return xComponent;
        }

        private void ShowExportDialog()
        {
            int dialog = EditorUtility.DisplayDialogComplex(
                "Process Finished!",
                string.Format("Found {0} components in {1} timeline assets!",
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
            _importedXmlPath = EditorUtility.OpenFilePanel("Import from", XmlDocDirectory, "xml");
            if (string.IsNullOrEmpty(_importedXmlPath) || !ReadXmlData(_importedXmlPath)) return;
            ImportTimelines(inPath);
            EditorUtility.DisplayProgressBar("Saving", 
                "Overwriting assets...(might take a few minutes)", 1f);
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Process Finished!", 
                "Updated " + EditedCount + " timeline assets out of " + TotalCount, "OK");            
        }

        internal void LoadXmlData()
        {
            //if (XRoot == null)
            {
                XRoot = XDocument.Load(DefaultXmlPath).Root;
            }
        }

        internal void ImportTimelines(string inPath = "")
        {
            LoadXmlData();
            var xAssets = XRoot.Elements().ToList();
            var totalNum = xAssets.Count;
            try
            {
                for (var i = 0; i < totalNum; i++)
                {
                    var assetPath = AsScriptingHelper.GetXmlAttribute(xAssets[i], "AssetPath");
                    if (!assetPath.Contains(inPath)) continue;
                    if (ImportTimeline(xAssets[i]))
                        EditedCount++;
                    if (EditorUtility.DisplayCancelableProgressBar("Modifying timeline assets", 
                        assetPath, (i + 1f) / totalNum)) 
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            }
            EditorUtility.ClearProgressBar();
        }

        internal bool ImportTimeline(XElement xTimeline)
        {
            var assetPath = AsScriptingHelper.GetXmlAttribute(xTimeline, "AssetPath");
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(assetPath);
            if (!timeline)
            {
                Debug.LogError("Backup Failed: Can't find Timeline asset at " + assetPath);
                return false;
            }
            var modified = false;
            foreach (var xComponent in xTimeline.Elements())
            {
                var clip = GetClipFromXml(timeline, xComponent, true);
                if (clip == null) continue;
                var asset = (WwiseTimelineClip) clip.asset;
                if (_asComponentImporter.WwiseTimelineClipImporter(asset, clip, xComponent))
                {
                    EditorUtility.SetDirty(asset);
                    modified = true;
                }
            }
            return modified;
        }

        //-----------------------------------------------------------------------------------------
        #endregion
        
        #region Compare
        //-----------------------------------------------------------------------------------------
        internal void Compare(string inPath = "")
        {
            _importedXmlPath = EditorUtility.OpenFilePanel("Compare with", XmlDocDirectory, "xml");
            if (string.IsNullOrEmpty(_importedXmlPath) || !ReadXmlData(_importedXmlPath)) return;
            FindFiles(FindUnsavedTimelineClips, "Find Unsaved Timeline Clip", "*.playable", inPath);
            CompareTimelines(inPath);
            CreateWindow();
            EditorUtility.ClearProgressBar();
        }

        private void CompareTimelines(string inPath = "")
        {
            var xAssets = XRoot.Elements().ToList();
            var totalNum = xAssets.Count;
            try
            {
                for (var i = 0; i < totalNum; i++)
                {
                    var assetPath = AsScriptingHelper.GetXmlAttribute(xAssets[i], "AssetPath");
                    if (EditorUtility.DisplayCancelableProgressBar("Modifying timeline assets",
                        assetPath, (i + 1f) / totalNum))
                        break;
                    if (!assetPath.Contains(inPath)) continue;
                    //if (CompareTimeline(xAssets[i]))
                    //    EditedCount++;
                    CompareTimeline(xAssets[i]);
                }
                EditorUtility.ClearProgressBar();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            }
        }

        private void FindUnsavedTimelineClips(string assetPath)
        {
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(assetPath);
            if (!timeline) return;

            var xAssets = XRoot.Elements().ToList();
            var xTimeline = xAssets.FirstOrDefault(t => 
            AsScriptingHelper.GetXmlAttribute(t, "AssetPath") == assetPath);
            if (xTimeline == null)
            {
                CacheAllTimelineComponents(timeline, assetPath);
                return;
            }

            var tracks = timeline.GetOutputTracks().Where(t => 
                t is WwiseTimelineTrack);

            foreach (var track in tracks)
            {
                var clips = track.GetClips();
                if (clips == null) continue;
                var xClips = xTimeline.Elements().Where(c =>
                AsScriptingHelper.GetXmlAttribute(c, "TrackName") == track.name);
                if (xClips == null)
                {
                    foreach (var clip in clips)
                    {
                        var asset = clip.asset as WwiseTimelineClip;
                        if (asset == null) continue;
                        SaveSingleModifiedComponent(assetPath, clip.displayName, null,
                            ParseComponent(asset, track.name, clip),
                            ComponentStatus.LocalOnly);
                    }
                    continue;
                }

                foreach (var clip in clips)
                {
                    var xClip = xClips.FirstOrDefault(c =>
                    Math.Abs(clip.start - AsScriptingHelper.StringToFloat(AsScriptingHelper.
                        GetXmlAttribute(c.Element("Settings"), "StartTime"))) <= 0.01f);
                    if (xClip != null) continue;
                    var asset = clip.asset as WwiseTimelineClip;
                    if (asset == null) continue;
                    SaveSingleModifiedComponent(assetPath, clip.displayName, null,
                        ParseComponent(asset, track.name, clip),
                        ComponentStatus.LocalOnly);
                }
            }
        }

        private void CacheAllTimelineComponents(TimelineAsset timeline, string assetPath)
        {
            foreach (var track in timeline.GetOutputTracks())
            {
                if (!(track is WwiseTimelineTrack)) continue;
                foreach (var clip in track.GetClips())
                {
                    var asset = clip.asset as WwiseTimelineClip;
                    if (asset == null) continue;
                    SaveSingleModifiedComponent(assetPath, clip.displayName, null,
                        ParseComponent(asset, track.name, clip), 
                        ComponentStatus.LocalOnly);
                }
            }
            EditedCount++;
        }

        private void CompareTimeline(XElement xTimeline)
        {
            var assetPath = AsScriptingHelper.GetXmlAttribute(xTimeline, "AssetPath");
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(assetPath);
            if (!timeline)
            {
                Debug.LogError("Backup Failed: Can't find Timeline asset at " + assetPath);
                return;
            }
            foreach (var xComponent in xTimeline.Elements())
            {
                var trackName = AsScriptingHelper.GetXmlAttribute(xComponent, "TrackName");
                var track = timeline.GetOutputTracks().FirstOrDefault(t =>
                    t is WwiseTimelineTrack && t.name == trackName);
                if (!track)
                {
                    SaveSingleModifiedComponent(assetPath,
                        AsScriptingHelper.GetXmlAttribute(xComponent, "ClipName"), 
                        xComponent, null, ComponentStatus.ServerOnly);
                    continue;
                }

                var startTime = AsScriptingHelper.StringToFloat(AsScriptingHelper.
                GetXmlAttribute(xComponent.Element("Settings"), "StartTime"));
                var clip = track.GetClips().FirstOrDefault(c => Math.Abs(c.start - startTime) <= 0.01f);
                if (clip == null)
                {
                    SaveSingleModifiedComponent(assetPath,
                        AsScriptingHelper.GetXmlAttribute(xComponent, "ClipName"),
                        xComponent, null, ComponentStatus.ServerOnly);
                    continue;
                }

                var component = (WwiseTimelineClip) clip.asset;
                if (!component.IsValid())
                {
                    SaveSingleModifiedComponent(assetPath,
                        AsScriptingHelper.GetXmlAttribute(xComponent, "ClipName"),
                        xComponent, ParseComponent(component, trackName, clip), ComponentStatus.NoEvent);
                    continue;
                }

                var tempComponent = Instantiate(component);
                if (_asComponentImporter.WwiseTimelineClipImporter(tempComponent, clip, xComponent))
                    SaveSingleModifiedComponent(assetPath,
                        AsScriptingHelper.GetXmlAttribute(xComponent, "ClipName"),
                        xComponent, ParseComponent(component, trackName, clip), ComponentStatus.Different);
                DestroyImmediate(tempComponent, true);
            }
        }

        //-----------------------------------------------------------------------------------------
        #endregion
        
        #region Locate
        //-----------------------------------------------------------------------------------------
        
        internal static TimelineClip GetClipFromComponent(string assetPath, WwiseTimelineClip component)
        {
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(assetPath);
            if (!timeline) return null;
            var AudioTracks = timeline.GetOutputTracks().Where(track => track is WwiseTimelineTrack);
            return AudioTracks.SelectMany(track => track.GetClips()).FirstOrDefault(clip => clip.asset == component);
        }

        private TimelineClip GetClipFromXml(TimelineAsset timeline, XElement xComponent, bool addIfMissing)
        {
            var trackName = AsScriptingHelper.GetXmlAttribute(xComponent, "TrackName");
            var track = timeline.GetOutputTracks().FirstOrDefault(t => 
            t is WwiseTimelineTrack && t.name == trackName);
            if (!track && addIfMissing)
                return timeline.CreateTrack<WwiseTimelineTrack>(null, trackName).CreateClip<WwiseTimelineClip>();
            
            var startTime = AsScriptingHelper.StringToFloat(AsScriptingHelper.
                GetXmlAttribute(xComponent.Element("Settings"), "StartTime"));
            var clip = track.GetClips().FirstOrDefault(c => Math.Abs(c.start - startTime) <= 0.01f);
            if (clip == null)
            {
                var clipName = AsScriptingHelper.GetXmlAttribute(xComponent, "ClipName");
                clip = track.GetClips().FirstOrDefault(c => c.displayName == clipName);
            }
            return clip ?? (addIfMissing ? track.CreateClip<WwiseTimelineClip>() : null);
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Revert
        //-----------------------------------------------------------------------------------------

        private void SaveXmlDataToComponent(ComponentComparisonData data)
        {
            if (data.ServerData == null) return;

            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(data.AssetPath);
            if (!timeline) return;

            var track = timeline.GetOutputTracks().FirstOrDefault(t =>
            t.name == AsScriptingHelper.GetXmlAttribute(data.ServerData, "TrackName"));
            if (track == null) track = timeline.CreateTrack<WwiseTimelineTrack>
                    (null, AsScriptingHelper.GetXmlAttribute(data.ServerData, "TrackName"));

            
            var startTime = AsScriptingHelper.StringToFloat(AsScriptingHelper.
                GetXmlAttribute(data.ServerData.Element("Settings"), "StartTime"));
            var clip = track.GetClips().FirstOrDefault(c => Math.Abs(c.start - startTime) <= 0.01f);
            if (clip == null) clip = track.CreateClip<WwiseTimelineClip>();
            var component = (WwiseTimelineClip)clip.asset;
            _asComponentImporter.WwiseTimelineClipImporter(component, clip, data.ServerData);
        }

        private void AddLocalComponent(ComponentComparisonData data)
        {
            if (data.LocalData == null) return;

            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(data.AssetPath);
            if (!timeline) return;

            var track = timeline.GetOutputTracks().FirstOrDefault(t =>
            t.name == AsScriptingHelper.GetXmlAttribute(data.LocalData, "TrackName"));
            if (track == null) track = timeline.CreateTrack<WwiseTimelineTrack>
                    (null, AsScriptingHelper.GetXmlAttribute(data.LocalData, "TrackName"));

            var timelineData = (TimelineComparisonData)data;
            var startTime = AsScriptingHelper.StringToFloat(AsScriptingHelper.
                GetXmlAttribute(data.LocalData.Element("Settings"), "StartTime"));
            var clip = track.GetClips().FirstOrDefault(c => Math.Abs(c.start - startTime) <= 0.01f);
            if (clip == null) clip = track.CreateClip<WwiseTimelineClip>();
            var component = (WwiseTimelineClip)clip.asset;
            _asComponentImporter.WwiseTimelineClipImporter(component, clip, data.LocalData);
        }

        private void AddServerXml(ComponentComparisonData data)
        {
            if (data.ServerData == null) return;
            var xAsset = XRoot.Elements().FirstOrDefault(x =>
            data.AssetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null)
            {
                xAsset = new XElement("Timeline");
                xAsset.SetAttributeValue("AssetPath", data.AssetPath);
                XRoot.Add(xAsset);
            }
            var xComponent = xAsset.Elements().FirstOrDefault(c =>
            XNode.DeepEquals(c.Element("Settings"), data.ServerData.Element("Settings")) &&
            c.Attribute("TrackName").Value == data.ServerData.Attribute("TrackName").Value &&
            c.Attribute("ClipName").Value == data.ServerData.Attribute("ClipName").Value);
            if (xComponent == null) xAsset.Add(data.ServerData);
            else xComponent.ReplaceWith(data.ServerData);
        }
        private void SaveComponentDataToXML(ComponentComparisonData data)
        {
            if (data.LocalData == null) return;
            var xAsset = XRoot.Elements().FirstOrDefault(x =>
            data.AssetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null)
            {
                xAsset = new XElement("Timeline");
                xAsset.SetAttributeValue("AssetPath", data.AssetPath);
                XRoot.Add(xAsset);
            }
            var xComponent = xAsset.Elements().FirstOrDefault(c =>
            XNode.DeepEquals(c.Element("Settings"), data.LocalData.Element("Settings")) &&
            c.Attribute("TrackName").Value == data.LocalData.Attribute("TrackName").Value &&
            c.Attribute("ClipName").Value == data.LocalData.Attribute("ClipName").Value);
            if (xComponent == null) xAsset.Add(data.LocalData);
            else xComponent.ReplaceWith(data.LocalData);
        }
        //-----------------------------------------------------------------------------------------
        #endregion

        #region Remove
        //-----------------------------------------------------------------------------------------
        internal void RemoveUnsavedInTimeline(string assetPath)
        {
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(assetPath);
            if (!timeline) return;

            var audioTracks = timeline.GetOutputTracks().Where(track => track is WwiseTimelineTrack);
            foreach (var track in audioTracks)
            {
                foreach (var clip in track.GetClips())
                {
                    if (!ComponentBackedUp(assetPath, clip))
                        DestroyImmediate(clip.asset);
                }
                EditedCount++;
                EditorUtility.SetDirty(timeline);
            }
            TotalCount++;
        }

        internal void RemoveAll()
        {
            CleanUp();
            FindFiles(RemoveAllInTimeline, "Removing Timeline Assets", "*.playable");
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Process Finished!", "Removed " + EditedCount + " audio tracks in " + TotalCount + " timeline assets!", "OK");
        }

        internal void RemoveAllInTimeline(string assetPath)
        {
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(assetPath);
            if (!timeline) return;
            var xAsset = FindAssetNode(assetPath, false);
            if (xAsset != null)
                xAsset.Remove();
            
            var toBeDeleted = timeline.GetOutputTracks().Where(track => track is WwiseTimelineTrack);
            foreach (var track in toBeDeleted)
            {
                timeline.DeleteTrack(track);
                EditedCount++;
                EditorUtility.SetDirty(timeline);
            }
            TotalCount++;
        }

        internal void RemoveLocalTimelineClip(ComponentComparisonData data)
        {
            var xmlData = data.LocalData != null ? data.LocalData : data.ServerData;
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(data.AssetPath);
            if (!timeline) return;

            var audioTrack = timeline.GetOutputTracks().FirstOrDefault(track => 
            track is WwiseTimelineTrack && 
            track.name == AsScriptingHelper.GetXmlAttribute(xmlData, "TrackName"));
            if (audioTrack == null) return;

            
            var clip = audioTrack.GetClips().FirstOrDefault(c => 
            Math.Abs(c.start - AsScriptingHelper.StringToFloat(AsScriptingHelper.
            GetXmlAttribute(xmlData.Element("Settings"), "StartTime"))) <= 0.01f);
            if (clip == null) return;
            

            //DestroyImmediate(clip.asset, true);
            timeline.DeleteClip(clip);
            if (audioTrack.GetClips().ToList().Count == 0) timeline.DeleteTrack(audioTrack);
            EditorUtility.SetDirty(timeline);
        }

        internal void RemoveTimelineClipXml(ComponentComparisonData data)
        {
            var xmlData = data.ServerData != null ? data.ServerData : data.LocalData;
            var xAsset = XRoot.Elements().FirstOrDefault(x =>
            data.AssetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            var xComponent = xAsset.Elements().FirstOrDefault(x => XNode.DeepEquals(x, xmlData));
            if (xComponent == null) return;
            AsScriptingHelper.RemoveComponentXml(xComponent);
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Inspector Operations
        //-----------------------------------------------------------------------------------------
        internal void RemoveComponentXml(string assetPath, TimelineClip clip)
        {
            ReadXmlData();
            var xAsset = XRoot.Elements().FirstOrDefault(x => 
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null) return;
            var xComponent = FindComponentNode(xAsset, clip);
            if (xComponent == null) return;
            AsScriptingHelper.RemoveComponentXml(xComponent);
            AsScriptingHelper.WriteXml(DefaultXmlPath, XRoot);
        }

        internal bool SaveLocalDataToServer(string assetPath, TimelineClip clip, WwiseTimelineClip component)
        {
            var trackName = clip.parentTrack.name;
            var xAsset = FindAssetNode(assetPath, true);
            var xComponent = FindComponentNode(xAsset, clip);
            if (xComponent != null)
            {
                var xTemp = ParseComponent(component, trackName, clip);
                if (XNode.DeepEquals(xTemp, xComponent)) return false;
                xComponent.ReplaceWith(xTemp);
            }
            else
                xAsset.Add(ParseComponent(component, trackName, clip));
            AsScriptingHelper.WriteXml(DefaultXmlPath, XRoot);
            return true;
        }

        internal bool RevertXmlDataToComponent(string assetPath, TimelineClip clip, WwiseTimelineClip component)
        {
            var xAsset = FindAssetNode(assetPath, true);
            var xComponent = FindComponentNode(xAsset, clip);
            return xComponent != null ?
                _asComponentImporter.WwiseTimelineClipImporter(component, clip, xComponent) :
                SaveLocalDataToServer(assetPath, clip, component);
        }

        private XElement FindAssetNode(string assetPath, bool createIfMissing)
        {
            ReadXmlData();
            var xAsset = XRoot.Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null && createIfMissing)
            {
                xAsset = new XElement("Timeline");
                xAsset.SetAttributeValue("AssetPath", assetPath);
                XRoot.Add(xAsset);
            }
            return xAsset;
        }

        private static XElement FindComponentNode(XElement xAsset, TimelineClip clip)
        {
            var xComponents = xAsset.Elements().Where(x =>
            clip.parentTrack.name == AsScriptingHelper.GetXmlAttribute(x, "TrackName"));
            foreach (var xComponent in xComponents)
            {
                var startTime = AsScriptingHelper.StringToFloat(AsScriptingHelper.
                    GetXmlAttribute(xComponent.Element("Settings"), "StartTime"));
                //var clipName = AsScriptingHelper.GetXmlAttribute(xComponent, "ClipName");
                if (Math.Abs(startTime - clip.start) < 0.01f)
                    return xComponent;
            }
            return null;
        }

        internal bool ComponentBackedUp(string assetPath, TimelineClip clip)
        {
            var xAsset = FindAssetNode(assetPath, false);
            if (xAsset == null) return false;

            var xComponent = FindComponentNode(xAsset, clip);
            if (xComponent == null) return false;

            var currentClipData = ParseComponent(clip.asset as WwiseTimelineClip,
                clip.parentTrack.name, clip);
            if (XNode.DeepEquals(currentClipData, xComponent))
                return true;
            else return false;
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        private static void SaveSingleModifiedComponent(string assetPath, string clipName,
            XElement inServerData, XElement inLocalData, ComponentStatus inComponentStatus)
        {
            var data = new TimelineComparisonData
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
            _compareWindow = GetWindow<AsCompareTimelineWindow>();
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
                    RemoveTimelineClipXml(data);
                    RemoveLocalTimelineClip(data);
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