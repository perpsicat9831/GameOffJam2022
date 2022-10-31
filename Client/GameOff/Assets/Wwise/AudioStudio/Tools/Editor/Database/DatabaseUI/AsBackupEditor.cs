using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace AudioStudio.Tools
{
    internal class AsBackupEditor : AsSearchers
    {
        #region Field
        //-----------------------------------------------------------------------------------------
        private enum CheckerType
        {
            Component = 0,
            AnimationEvent = 1,
            AudioState = 2,
            Timeline = 3
        }

        private string[] CheckerNames = new string[4]
        {
            "Component",
            "Animation Events",
            "Audio State",
            "Timeline"
        };

        // Data to backup
        private List<bool> _isComponent = new List<bool>();
        private List<bool> _isAnimationevent = new List<bool>();
        private List<bool> _isAudioState = new List<bool>();
        private List<bool> _isTimeline = new List<bool>();
        private List<EditorGUILayout.VerticalScope> _dropAreas = new List<EditorGUILayout.VerticalScope>();
        private List<string> _pathTags = new List<string>();
        private int _totalPathNum = 1;

        private Vector2 _scrollPosition;
        private bool _plusPressed = false;
        private bool _minusPressed = false;

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Init
        //-----------------------------------------------------------------------------------------
        private void Awake()
        {
            LoadBackup();

        }

        private void OnDestroy()
        {
            SaveBackup();

        }

        //-----------------------------------------------------------------------------------------
        #endregion

        private void OnGUI()
        {
            DrawPaths();

            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
            {
                _plusPressed = GUILayout.Button("+");
                _minusPressed = GUILayout.Button("-");
            }

            if (_plusPressed)
            {
                ++_totalPathNum;
                DrawPaths();
            }
            if (_minusPressed)
            {
                if (_totalPathNum > 1) --_totalPathNum;
                DrawPaths();
            }
        }

        #region Path Management
        //-----------------------------------------------------------------------------------------
        public void DrawPaths()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                _scrollPosition = EditorGUILayout.
                BeginScrollView(_scrollPosition, GUILayout.MaxHeight(position.height));

                for (int index = 0; index < _totalPathNum; index++)
                {
                    if (_paths.Count < _totalPathNum) _paths.Add("Assets");
                    else if (_paths.Count > _totalPathNum) _paths.RemoveAt(_paths.Count - 1);
                    DrawPath(index);
                }
                //Debug.LogWarning("Total Paths: " + _paths.Count);
                EditorGUILayout.EndScrollView();
            }
        }

        public void DrawPath(int pathNum)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    // Tag Panel
                    if (_pathTags.Count <= pathNum) _pathTags.Add("Searching Path");
                    _pathTags[pathNum] = GUILayout.TextField(_pathTags[pathNum], EditorStyles.boldLabel);

                    // Path Panel
                    GUILayout.BeginHorizontal();
                    if (_dropAreas.Count < _totalPathNum)
                    {
                        var dropArea = new EditorGUILayout.VerticalScope(GUI.skin.label);
                        using (dropArea)
                        {
                            _paths[pathNum] = AsScriptingHelper.ShortPath(_paths[pathNum]);
                            _paths[pathNum] = GUILayout.TextField(_paths[pathNum], GUI.skin.textField);
                            CheckDragDrop(dropArea.rect, pathNum);
                        }
                        _dropAreas.Add(dropArea);
                    }
                    else
                    {
                        _dropAreas[pathNum] = new EditorGUILayout.VerticalScope(GUI.skin.label);
                        using (_dropAreas[pathNum])
                        {
                            _paths[pathNum] = AsScriptingHelper.ShortPath(_paths[pathNum]);
                            _paths[pathNum] = GUILayout.TextField(_paths[pathNum], GUI.skin.textField);
                            CheckDragDrop(_dropAreas[pathNum].rect, pathNum);
                        }

                    }

                    // Browse Button
                    if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.MaxWidth(100)))
                        _paths[pathNum] = EditorUtility.OpenFolderPanel("Root Folder", _paths[pathNum], "");
                    GUILayout.EndHorizontal();

                    // Backup Type Checker Group
                    GUILayout.BeginHorizontal();
                    if (_isComponent.Count <= pathNum) _isComponent.Add(false);
                    _isComponent[pathNum] = GUILayout.Toggle(_isComponent[pathNum],
                        CheckerNames[(int)CheckerType.Component]);
                    if (_isAnimationevent.Count <= pathNum) _isAnimationevent.Add(false);
                    _isAnimationevent[pathNum] = GUILayout.Toggle(_isAnimationevent[pathNum],
                        CheckerNames[(int)CheckerType.AnimationEvent]);
                    //if (_isAudioState.Count <= pathNum) _isAudioState.Add(false);
                    //_isAudioState[pathNum] = GUILayout.Toggle(_isAudioState[pathNum],
                    //    CheckerNames[(int)CheckerType.AudioState]);
                    if (_isTimeline.Count <= pathNum) _isTimeline.Add(false);
                    _isTimeline[pathNum] = GUILayout.Toggle(_isTimeline[pathNum],
                        CheckerNames[(int)CheckerType.Timeline]);
                    GUILayout.EndHorizontal();

                }
                if (_isComponent[pathNum]) DrawComponents(_paths[pathNum]);
                if (_isAnimationevent[pathNum]) DrawAnimationEvents(_paths[pathNum]);
                //if (_isAudioState[pathNum]) DrawAudioStates(_paths[pathNum]);
                if (_isTimeline[pathNum]) DrawTimeline(_paths[pathNum]);
            }
        }

        private void CheckDragDrop(Rect inDropArea, int pathNum)
        {
            var currentEvent = Event.current;
            if (!inDropArea.Contains(currentEvent.mousePosition)) return;
            if (currentEvent.type != EventType.DragUpdated &&
                currentEvent.type != EventType.DragPerform)
                return;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                // Only support one object drag
                if (DragAndDrop.objectReferences.Length == 1)
                {
                    Debug.LogWarning(DragAndDrop.paths[0]);
                    _paths[pathNum] = DragAndDrop.paths[0];
                    for (int i = _paths[pathNum].Length - 1; i > 0; i--)
                    {
                        if (_paths[pathNum][i] == '/')
                        {
                            _pathTags[pathNum] = _paths[pathNum].Substring(i + 1);
                            Debug.LogWarning(_pathTags[pathNum]);
                            break;
                        }
                    }
                }
                DragAndDrop.PrepareStartDrag();
            }
            currentEvent.Use();
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Path Back Up Management
        //-----------------------------------------------------------------------------------------

        private void LoadBackup()
        {
            // Read preset data into the window
            var isread = ReadXmlData(AsScriptingHelper.CombinePath(XmlDocDirectory, "BackupPaths.xml"));
            if (!isread) return;
            var xmlPaths = XRoot.Elements("Path").ToList();
            for (int index = 0; index < xmlPaths.Count; index++)
            {
                _paths.Add(AsScriptingHelper.GetXmlAttribute(xmlPaths[index], "BackupPath"));
                _pathTags.Add(AsScriptingHelper.GetXmlAttribute(xmlPaths[index], "PathTag"));

                bool tempBool = false;
                if (AsScriptingHelper.GetXmlAttribute(xmlPaths[index], "ComponentBackup") == "true")
                    tempBool = true;
                _isComponent.Add(tempBool);

                tempBool = false;
                if (AsScriptingHelper.GetXmlAttribute(xmlPaths[index], "AnimationEventsBackup") == "true")
                    tempBool = true;
                _isAnimationevent.Add(tempBool);

                tempBool = false;
                if (AsScriptingHelper.GetXmlAttribute(xmlPaths[index], "AudioStateBackup") == "true")
                    tempBool = true;
                _isAudioState.Add(tempBool);

                tempBool = false;
                if (AsScriptingHelper.GetXmlAttribute(xmlPaths[index], "TimelineBackup") == "true")
                    tempBool = true;
                _isTimeline.Add(tempBool);
            }

            _totalPathNum = (int)XRoot.Element("TotalNumber");
        }

        private void SaveBackup()
        {
            // Save all data into the xml file
            XElement XRoot = new XElement("BackupPaths");
            for (int index = 0; index < _paths.Count; index++)
            {
                var xPath = new XElement("Path");
                xPath.SetAttributeValue("BackupPath", _paths[index]);
                xPath.SetAttributeValue("PathTag", _pathTags[index]);
                xPath.SetAttributeValue("ComponentBackup", _isComponent[index]);
                xPath.SetAttributeValue("AnimationEventsBackup", _isAnimationevent[index]);
                xPath.SetAttributeValue("AudioStateBackup", _isAudioState[index]);
                xPath.SetAttributeValue("TimelineBackup", _isTimeline[index]);
                XRoot.Add(xPath);
            }

            var xmlTotalNum = new XElement("TotalNumber", _totalPathNum);
            XRoot.Add(xmlTotalNum);
            AsScriptingHelper.WriteXml(AsScriptingHelper.
                CombinePath(XmlDocDirectory, "BackupPaths.xml"),
                            XRoot);
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Module Back Up Management
        //-----------------------------------------------------------------------------------------
        private void DrawComponents(string inPath)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Components in Prefabs and Scenes", EditorStyles.boldLabel);

                DrawComonentToggles();

                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Search Option", EditorStyles.boldLabel, GUILayout.Width(150));
                DrawToggle(ref AsComponentBackup.Instance.searchPrefab, "Prefabs");
                DrawToggle(ref AsComponentBackup.Instance.searchScene, "Scenes");
                DrawToggle(ref AsComponentBackup.Instance.IncludePrefabInstance, "Prefabs Variants");
                //DrawToggle(ref AsComponentBackup.Instance.SeparateXmlFiles, "Create one xml per component");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Export", EditorStyles.toolbarButton))
                    AsComponentBackup.Instance.Export(inPath);
                if (GUILayout.Button("Import", EditorStyles.toolbarButton))
                    AsComponentBackup.Instance.Import(inPath);
                if (GUILayout.Button("Compare", EditorStyles.toolbarButton))
                    AsComponentBackup.Instance.Compare(inPath);
                if (GUILayout.Button("Open", EditorStyles.toolbarButton))
                    AsComponentBackup.Instance.OpenXmlFile();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawComonentToggles()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var selections = new Dictionary<Type, bool>
                    (AsComponentBackup.Instance.ComponentsToSearch);
                var count = 0;
                EditorGUILayout.BeginHorizontal();
                foreach (var selection in selections)
                {
                    var selected = GUILayout.Toggle(selection.Value, 
                        selection.Key.Name, GUILayout.Width(150));
                    if (selected != selection.Value)
                        AsComponentBackup.Instance.ComponentsToSearch[selection.Key] = selected;
                    count++;
                    if (count % 3 == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All", EditorStyles.toolbarButton))
                {
                    foreach (var selection in selections)
                    {
                        AsComponentBackup.Instance.ComponentsToSearch[selection.Key] = true;
                    }
                }
                if (GUILayout.Button("Deselect All", EditorStyles.toolbarButton))
                {
                    foreach (var selection in selections)
                    {
                        AsComponentBackup.Instance.ComponentsToSearch[selection.Key] = false;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawAnimationEvents(string inPath)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Animation Events in Animation Clips and Models",
                    EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Search Option", GUILayout.Width(150));
                DrawToggle(ref AsAnimationEventBackup.Instance.searchPrefab, "AnimationClip");
                DrawToggle(ref AsAnimationEventBackup.Instance.searchScene, "FBX Prefab");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Export", EditorStyles.toolbarButton))
                    AsAnimationEventBackup.Instance.Export(inPath);
                //if (GUILayout.Button("Import", EditorStyles.toolbarButton))
                //    AsAnimationEventBackup.Instance.Import();
                if (GUILayout.Button("Compare", EditorStyles.toolbarButton))
                    AsAnimationEventBackup.Instance.Compare(inPath);
                if (GUILayout.Button("Open", EditorStyles.toolbarButton))
                    AsAnimationEventBackup.Instance.OpenXmlFile();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawAudioStates(string inPath)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Audio States in Animator Controllers", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Export", EditorStyles.toolbarButton))
                    AsAudioStateBackup.Instance.Export(inPath);
                if (GUILayout.Button("Import", EditorStyles.toolbarButton))
                    AsAudioStateBackup.Instance.Import();
                if (GUILayout.Button("Compare", EditorStyles.toolbarButton))
                    AsAudioStateBackup.Instance.Compare();
                if (GUILayout.Button("Remove All", EditorStyles.toolbarButton))
                    AsAudioStateBackup.Instance.RemoveAll();
                if (GUILayout.Button("Open", EditorStyles.toolbarButton))
                    AsAudioStateBackup.Instance.OpenXmlFile();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawTimeline(string inPath)
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Audio Clips in Timeline", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Export", EditorStyles.toolbarButton))
                    AsTimelineAudioBackup.Instance.Export(inPath);
                if (GUILayout.Button("Compare", EditorStyles.toolbarButton))
                    AsTimelineAudioBackup.Instance.Compare();
                if (GUILayout.Button("Open", EditorStyles.toolbarButton))
                    AsTimelineAudioBackup.Instance.OpenXmlFile();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawToggle(ref bool toggle, string label)
        {
            toggle = GUILayout.Toggle(toggle, label);
        }

        //-----------------------------------------------------------------------------------------
        #endregion
    }
}