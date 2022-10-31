using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Xml.Linq;
using AudioStudio.Components;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace AudioStudio.Tools
{           
    internal class AsComponentBackup : AsSearchers
    {
        #region Fields
        //-----------------------------------------------------------------------------------------
        // Filter components that need or don't need to include
        internal readonly Dictionary<Type, bool> ComponentsToSearch = 
            new Dictionary<Type, bool>();
        // Store one xml file per type of component
        private readonly Dictionary<Type, XElement> _separateComponentXRoot = 
            new Dictionary<Type, XElement>();

        private XElement _removedComponentXRoot = new XElement("Root");
        internal bool IncludePrefabInstance = false;
        internal bool SeparateXmlFiles = false;

        public bool bCombining = false;

        protected override string DefaultXmlPath 
        {
            get { return AsScriptingHelper.CombinePath(XmlDocDirectory, "AudioStudioComponents.xml"); }
        }

        private string DefaultRemovedComponentDataPath
        {
            get { return AsScriptingHelper.CombinePath(XmlDocDirectory, "RemovedComponents.xml"); }
        }
        
        private string IndividualXmlPath(Type type)
        {
            return AsScriptingHelper.CombinePath(XmlDocDirectory, type.Name + ".xml");                    
        }

        private AsComponentImportParser _asComponentImporter;
        private AsComponentExportParser _asComponentExporter;

        // one function per component to import data
        private delegate bool ComponentImporter(AsComponent component, XElement node);
        private static readonly Dictionary<Type, ComponentImporter> _importers =
            new Dictionary<Type, ComponentImporter>();

        // one function per component to export data
        private delegate void ComponentExporter(AsComponent component, XElement node);
        private static readonly Dictionary<Type, ComponentExporter> _exporters =
            new Dictionary<Type, ComponentExporter>();

        private AsCompareComponentWindow _compareWindow;
        //-----------------------------------------------------------------------------------------
        #endregion

        #region Init     
        //-----------------------------------------------------------------------------------------

        private static AsComponentBackup _instance;
        internal static AsComponentBackup Instance
        {
            get
            {
                if (!_instance)
                    _instance = CreateInstance<AsComponentBackup>();
                return _instance;
            }
        }

        public void Awake()
        {
            searchPrefab = true;
            searchScene = false;
            XRoot = new XElement("Root");

            _asComponentImporter = CreateInstance<AsComponentImportParser>();
            _asComponentExporter = CreateInstance<AsComponentExportParser>();

            RegisterComponent<AnimationSound>(_asComponentImporter.ImportSpatialSettings, 
                                              _asComponentExporter.ExportSpatialSettings);
            RegisterComponent<AudioListener3D>(_asComponentImporter.AudioListener3DImporter, 
                                               _asComponentExporter.AudioListener3DExporter);
            //RegisterComponent<AudioRoom>(_asComponentImporter.AudioRoomImporter, 
            //                             _asComponentExporter.AudioRoomExporter);
            RegisterComponent<AudioTag>(_asComponentImporter.AudioTagImporter, 
                                        _asComponentExporter.AudioTagExporter);
            RegisterComponent<ButtonSound>(_asComponentImporter.ButtonSoundImporter, 
                                           _asComponentExporter.ButtonSoundExporter);
            //RegisterComponent<ColliderSound>(_asComponentImporter.ColliderSoundImporter, 
            //                                 _asComponentExporter.ColliderSoundExporter);
            RegisterComponent<DropdownSound>(_asComponentImporter.DropdownSoundImporter, 
                                             _asComponentExporter.DropdownSoundExporter);
            RegisterComponent<EffectSound>(_asComponentImporter.EffectSoundImporter, 
                                           _asComponentExporter.EffectSoundExporter);
            RegisterComponent<EventSound>(_asComponentImporter.EventSoundImporter, 
                                          _asComponentExporter.EventSoundExporter);
            RegisterComponent<EmitterSound>(_asComponentImporter.EmitterSoundImporter, 
                                            _asComponentExporter.EmitterSoundExporter);
            RegisterComponent<LegacyAnimationSound>(_asComponentImporter.LegacyAnimationSoundImporter, 
                                                    _asComponentExporter.LegacyAnimationSoundExporter);
            RegisterComponent<LoadBank>(_asComponentImporter.LoadBankImporter, 
                                        _asComponentExporter.LoadBankExporter);
            RegisterComponent<MenuSound>(_asComponentImporter.MenuSoundImporter, 
                                         _asComponentExporter.MenuSoundExporter);
            RegisterComponent<MusicSwitch>(_asComponentImporter.MusicSwitchImporter, 
                                           _asComponentExporter.MusicSwitchExporter);
            //RegisterComponent<ReverbZone>(_asComponentImporter.ReverbZoneImporter, 
            //                              _asComponentExporter.ReverbZoneExporter);
            RegisterComponent<ScrollSound>(_asComponentImporter.ScrollSoundImporter, 
                                           _asComponentExporter.ScrollSoundExporter);
            RegisterComponent<SliderSound>(_asComponentImporter.SliderSoundImporter, 
                                           _asComponentExporter.SliderSoundExporter);
            //RegisterComponent<SurfaceReflector>(_asComponentImporter.SurfaceReflectorImporter, 
            //                                   _asComponentExporter.SurfaceReflectorExporter);
            RegisterComponent<TimelineSound>(_asComponentImporter.TimelineSoundImporter, 
                                             _asComponentExporter.TimelineSoundExporter);
            RegisterComponent<ToggleSound>(_asComponentImporter.ToggleSoundImporter, 
                                           _asComponentExporter.ToggleSoundExporter);
            RegisterComponent<GlobalAuxSend>(_asComponentImporter.GlobalAuxSendImporter, 
                                             _asComponentExporter.GlobalAuxSendExporter);
            RegisterComponent<SetState>(_asComponentImporter.SetStateImporter, 
                                        _asComponentExporter.SetStateExporter);
            RegisterComponent<SetSwitch>(_asComponentImporter.SetSwitchImporter, 
                                         _asComponentExporter.SetSwitchExporter);
        }

        // initialize delegates and xml roots
        private void RegisterComponent<T>(ComponentImporter importer = null, 
            ComponentExporter exporter = null)
        {
            var t = typeof(T);            
            ComponentsToSearch[t] = false;
            _importers[t] = importer;
            _exporters[t] = exporter;
        }

        // get an xml file for a type of component
        public void LoadOrCreateSeparatedXmlDoc(Type type)
        {
            var xmlPath = IndividualXmlPath(type);
            if (!File.Exists(xmlPath))
                AsScriptingHelper.WriteXml(xmlPath, new XElement("Root"));
            else
            {
                var xRoot = XDocument.Load(xmlPath).Root;
                _separateComponentXRoot[type] = xRoot;
            }                                                       
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Export
        //-----------------------------------------------------------------------------------------
        internal void Export(string inPath = "")
        {
            CleanUp();
            if (SeparateXmlFiles) ExportSeparatedFiles(inPath);
            else ExportSingleFile(inPath);
        }

        internal void ExportSingleFile(string inPath)
        {
            var completed = false;
            // let user select a file to export to
            _importedXmlPath = EditorUtility.SaveFilePanel("Export to", 
                XmlDocDirectory, "AudioStudioComponents.xml", "xml");
            if (string.IsNullOrEmpty(_importedXmlPath)) return;

            // export prefabs
            if (searchPrefab)
                completed |= FindFiles(ParseSinglePrefab, "Exporting Prefabs", "*.prefab", inPath);
            // export scenes
            if (searchScene)
            {
                // save the current scene
                var currentScene = SceneManager.GetActiveScene().path;
                // do the scene parsing
                completed |= FindFiles(ParseSingleScene, "Exporting Scenes", "*.unity", inPath);
                // reopen the scene saved
                EditorSceneManager.OpenScene(currentScene);
            }

            // if user did not cancel the search process, write xml file
            if (!completed) return;
            ShowExportDialog();
            EditorUtility.ClearProgressBar();
        }

        internal void ExportSeparatedFiles(string inPath)
        {
            var completed = false;
            // clear xml files of all components
            foreach (var pair in ComponentsToSearch)
            {
                if (!pair.Value) continue;
                _separateComponentXRoot[pair.Key] = new XElement("Root");
            }

            // export prefabs
            if (searchPrefab)
                completed |= FindFiles(ParseSeparatedPrefabs, 
                    "Exporting Prefabs", "*.prefab", inPath);
            // export scenes
            if (searchScene)
            {
                // save the current scene
                var currentScene = SceneManager.GetActiveScene().path;
                // do the scene parsing
                completed |= FindFiles(ParseSeparatedScenes, 
                    "Exporting Scenes", "*.unity", inPath);
                // reopen the scene saved
                EditorSceneManager.OpenScene(currentScene);
            }

            // if user did not cancel the search process, write xml file
            if (!completed) return;
            ShowExportDialog();
            foreach (var pair in ComponentsToSearch)
            {
                if (!pair.Value) continue;
                AsScriptingHelper.WriteXml(IndividualXmlPath(pair.Key),
                    _separateComponentXRoot[pair.Key]);
            }
        }

        /// <summary>
        /// load a prefab and export all components in it
        /// </summary>
        /// <param name="assetPath"></param>
        internal void ParseSinglePrefab(string assetPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (!prefab) return;

            var xPrefab = new XElement("Prefab");
            xPrefab.SetAttributeValue("AssetPath", assetPath);
            var components = prefab.GetComponentsInChildren<AsComponent>(true);
            foreach (var component in components)
            {
                bool willSearch;
                if (!ComponentsToSearch.TryGetValue(component.GetType(), out willSearch))
                    continue;
                xPrefab.Add(ParseComponent(component));
            }

            if (!xPrefab.HasElements) return;
            // Save the current data into the window buffer
            SaveModifiedComponents(assetPath, null, xPrefab, ComponentStatus.LocalOnly);
            XRoot.Add(xPrefab);
            TotalCount++;
        }

        internal void ParseSeparatedPrefabs(string assetPath)
        {
            var prefab = (GameObject)AssetDatabase.
                LoadAssetAtPath(assetPath, typeof(GameObject));
            if (!prefab) return;

            var found = false;
            // iterate types first because xml files are separate
            foreach (var pair in ComponentsToSearch)
            {
                if (!pair.Value) continue;
                var xPrefab = new XElement("Prefab");
                var components = prefab.GetComponentsInChildren(pair.Key, true);
                foreach (var component in components)
                {
                    xPrefab.Add(ParseComponent((AsComponent)component));
                }
                // component found, add prefab node to xml
                if (xPrefab.HasElements)
                {
                    xPrefab.SetAttributeValue("AssetPath", assetPath);
                    _separateComponentXRoot[pair.Key].Add(xPrefab);
                    found = true;
                }
            }
            if (found)
                TotalCount++;
        }

        /// <summary>
        /// load a scene and export all components in it
        /// </summary>
        /// <param name="assetPath"></param>
        internal void ParseSingleScene(string assetPath)
        {
            var scene = EditorSceneManager.OpenScene(assetPath);
            if (!scene.IsValid()) return;

            var xScene = new XElement("Scene");
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                var components = rootGameObject.GetComponentsInChildren<AsComponent>(true);
                foreach (var component in components)
                {
                    bool willSearch;
                    if (ComponentsToSearch.TryGetValue(component.GetType(), out willSearch))
                    {
                        if (ComponentBelongsToScene(component))
                            xScene.Add(ParseComponent(component));
                    }
                }
            }
            if (!xScene.HasElements) return;
            xScene.SetAttributeValue("AssetPath", assetPath);

            // Save current data into the window buffer
            SaveModifiedComponents(assetPath, null, xScene, ComponentStatus.LocalOnly);
            XRoot.Add(xScene);
            TotalCount++;
        }

        internal void ParseSeparatedScenes(string assetPath)
        {
            var scene = EditorSceneManager.OpenScene(assetPath);
            if (!scene.IsValid()) return;

            var found = false;
            foreach (var pair in ComponentsToSearch)
            {
                if (!pair.Value) continue;
                var xScene = new XElement("Scene");
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    var components = rootGameObject.GetComponentsInChildren(pair.Key, true);
                    foreach (var component in components)
                    {
                        bool willSearch;
                        if (ComponentsToSearch.TryGetValue(component.GetType(), out willSearch))
                        {
                            // make sure component is saved just in scene
                            if (ComponentBelongsToScene(component))
                                xScene.Add(ParseComponent((AsComponent)component));
                        }
                    }
                }
                if (xScene.HasElements)
                {
                    xScene.SetAttributeValue("AssetPath", assetPath);
                    _separateComponentXRoot[pair.Key].Add(xScene);
                    found = true;
                }
            }
            if (found) TotalCount++;
        }

        private void ShowExportDialog()
        {
            int dialog = EditorUtility.DisplayDialogComplex(
                "Process Finished!",
                string.Format("Found {0} components in {1} assets!", EditedCount, TotalCount),
                "Save",
                "Cancel",
                "More Details");

            //Debug.LogWarning("total data:  " + AsCompareWindow.ModifiedComponents.Count);
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
        
        /// <summary>
        /// General import function if press the import button
        /// </summary>
        internal void Import(string inPath = "")
        {
            CleanUp();
            // select an xml file to import from
            _importedXmlPath = EditorUtility.OpenFilePanel("Import from", XmlDocDirectory, "xml");
            if (string.IsNullOrEmpty(_importedXmlPath)) return;
            ReadXmlData(_importedXmlPath);

            // export prefabs
            if (searchPrefab) ImportPrefabs(inPath);
            // export scenes
            if (searchScene) ImportScenes(inPath);
            CreateWindow();
            EditorUtility.ClearProgressBar();
        }

        private void ImportPrefabs(string inPath)
        {
            try
            {
                SearchXMLPrefabAndScene(inPath, ImportPrefab, "Prefab");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            }
        }

        internal bool ImportPrefab(XElement xPrefab)
        {
            var prefabPath = AsScriptingHelper.GetXmlAttribute(xPrefab, "AssetPath");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (!prefab)
            {
                Debug.LogError("Backup Failed: Can't find prefab at " + prefabPath);
                return false;
            }

            var modified = false;
            var xComponents = xPrefab.Elements();
            foreach (var xComponent in xComponents)
            {
                TotalCount++;
                // locate the game object in prefab
                var gameObjectPath = AsScriptingHelper.GetXmlAttribute(xComponent, "GameObject");
                var gameObject = GetGameObject(prefab, gameObjectPath);
                if (!gameObject)
                {
                    Debug.LogError("Backup Failed: Can't find game object at " + 
                        gameObjectPath + " in prefab " + prefabPath);
                    continue;
                }
                
                var type = AsScriptingHelper.
                    StringToType(AsScriptingHelper.GetXmlAttribute(xComponent, "Type"));
                var component = gameObject.GetComponent(type) as AsComponent;
                // Save the component data for future operation
                if (!component)
                {
                    EditedCount++;
                    SaveSingleModifiedComponent(prefabPath, xComponent, null, ComponentStatus.ServerOnly);
                    continue;
                }
                var tempComponent = Instantiate(component);
                if (ComponentModified(tempComponent, xComponent))
                {
                    EditedCount++;
                    modified = true;
                    SaveComponentAsset(prefab, prefabPath);
                    SaveSingleModifiedComponent(prefabPath, xComponent, 
                        ParseComponent(component), ComponentStatus.UseServer);
                }
                DestroyImmediate(tempComponent.gameObject, true);
            }
            return modified;
        }
        
        private void ImportScenes(string inPath)
        {
            try
            {
                // save the current scene
                var currentScene = SceneManager.GetActiveScene().path;
                SearchXMLPrefabAndScene(inPath, ImportScene, "Scene");
                // reopen the scene saved
                EditorSceneManager.OpenScene(currentScene);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            }
        }

        internal bool ImportScene(XElement xScene)
        {
            var scenePath = AsScriptingHelper.GetXmlAttribute(xScene, "AssetPath");
            var scene = EditorSceneManager.OpenScene(scenePath);
            if (!scene.IsValid())
            {
                Debug.LogError("Backup Failed: Can't find scene at " + scenePath);
                return false;
            }
            
            var modified = false;
            var xSceneComponents = xScene.Elements();
            foreach (var xComponent in xSceneComponents)
            {
                TotalCount++;
                var gameObjectPath = AsScriptingHelper.GetXmlAttribute(xComponent, "GameObject");
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    if (!gameObjectPath.Contains(rootGameObject.name)) continue;
                    var gameObject = GetGameObject(rootGameObject, gameObjectPath);
                    if (!gameObject)
                    {
                        Debug.LogError("Backup Failed: Can't find game object at " +
                                        gameObjectPath + " in scene " + scenePath);
                        continue;
                    }
                    
                    var type = AsScriptingHelper.StringToType(AsScriptingHelper.
                        GetXmlAttribute(xComponent, "Type"));
                    var component = gameObject.GetComponent(type) as AsComponent;
                    if (!component)
                    {
                        EditedCount++;
                        //component = gameObject.AddComponent(type) as AsComponent;
                        SaveSingleModifiedComponent(scenePath, xComponent, null, ComponentStatus.ServerOnly);
                        continue;
                    }
                    var tempComponent = Instantiate(component);
                    if (ComponentModified(tempComponent, xComponent))
                    {
                        EditedCount++;
                        modified = true;
                        EditorUtility.SetDirty(component);
                        SaveSingleModifiedComponent(scenePath, xComponent, 
                            ParseComponent(component), ComponentStatus.Different);
                    }
                    DestroyImmediate(tempComponent.gameObject);
                    break;
                }
            }
            if (modified) EditorSceneManager.SaveScene(scene);
            return modified;
        }
        
        internal static void SaveComponentAsset(GameObject inGameObject, string assetPath)
        {
            // if game object is from a prefab, save the prefab
            if (assetPath.EndsWith(".prefab"))
            {
#if UNITY_2018_3_OR_NEWER
#if UNITY_2021_2_OR_NEWER
                var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
                var stage = UnityEditor.Experimental.SceneManagement.
                    PrefabStageUtility.GetCurrentPrefabStage();
#endif
                var prefab = PrefabUtility.GetNearestPrefabInstanceRoot(inGameObject);
                if (prefab != null)
                {
                    if (stage == null)
                    {
                        if (!PrefabUtility.IsPartOfPrefabAsset(prefab))
                            PrefabUtility.ApplyPrefabInstance(prefab, InteractionMode.AutomatedAction);
                        Selection.activeGameObject = inGameObject;
                        //EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                    }
                    else
                    {
                        EditorUtility.SetDirty(prefab);
                    }

                }
                else
                {
                    if (stage != null)
                        EditorUtility.SetDirty(inGameObject);
                    else
                        PrefabUtility.SavePrefabAsset(inGameObject);
                }
                     
                
#else
#if UNITY_2018_1_OR_NEWER
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
#else
                var prefab = PrefabUtility.GetPrefabParent(go);
#endif
                var prefabRoot = PrefabUtility.FindPrefabRoot(go);
                PrefabUtility.ReplacePrefab(prefabRoot, prefab, ReplacePrefabOptions.ConnectToPrefab);
#endif
            }
            // game object belongs to scene, save the scene
            else
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Compare
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// This function will compare the current implementation with one previous xml file backup
        /// </summary>
        internal void Compare(string inPath = "")
        {            
            CleanUp();
            _importedXmlPath = EditorUtility.OpenFilePanel("Import from", XmlDocDirectory, "xml");
            if (string.IsNullOrEmpty(_importedXmlPath)) return;
            ReadXmlData(_importedXmlPath);

            /*
            var elements = XRoot.Elements().OrderBy(e => e.Name.ToString()).ToList();
            XRoot.RemoveAll();
            XRoot.Add(elements);
            AsScriptingHelper.WriteXml(_importedXmlPath, XRoot);
            */

            if (searchPrefab) ComparePrefabs(inPath);
            if (searchScene) CompareScenes(inPath);
            CreateWindow();
            EditorUtility.ClearProgressBar();
        }

        private void ComparePrefabs(string inPath)
        {
            try
            {
                SearchXMLPrefabAndScene(inPath, ComparePrefab, "Prefab");
                FindFiles(FindUnsavedPrefab, "Find Unsaved Components", "*.prefab", inPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            }
        }

        private void FindUnsavedPrefab(string inPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(inPath);
            if (prefab == null) return;
            var components = prefab.GetComponentsInChildren<AsComponent>(true);
            if (components.Length == 0) return;

            var xmlPrefab = XRoot.Elements("Prefab").FirstOrDefault(p =>
            inPath == AsScriptingHelper.GetXmlAttribute(p, "AssetPath"));
            if (xmlPrefab == null)
            {
                foreach (var component in components)
                {
                    if (!SearchThisComponent(component.GetType())) 
                        continue;
                    if (IsPrefabInstance(component.gameObject))
                    {
                        if (IncludePrefabInstance)
                            ComparePrefabInstance(component.gameObject);
                        continue;
                    }
                    SaveUnsavedPrefab(inPath, component);
                }
                return;
            }
            
            foreach (var component in components)
            {
                if (!SearchThisComponent(component.GetType())) 
                    continue;
                if (IsPrefabInstance(component.gameObject))
                {
                    if (IncludePrefabInstance)
                        ComparePrefabInstance(component.gameObject);
                    continue;
                }
                var xmlComponent = xmlPrefab.Elements().FirstOrDefault(c =>
                    AsScriptingHelper.GetXmlAttribute(c, "Type") ==
                    component.GetType().Name &&
                    AsScriptingHelper.GetXmlAttribute(c, "GameObject") ==
                    GetGameObjectPath(component.transform));
                if (xmlComponent == null)
                {
                    SaveUnsavedPrefab(inPath, component);
                }
            }
        }

        private void ComparePrefabInstance(GameObject inInstance)
        {
            // TODO: need to finish the compare process;

            var instance = PrefabUtility.GetNearestPrefabInstanceRoot(inInstance);
            if (instance == null) return;

            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(instance);
            var path = AssetDatabase.GetAssetPath(prefab);
            Debug.Log("The root prefab path: " + 
                AssetDatabase.GetAssetPath(prefab) + 
                " " + AssetDatabase.Contains(prefab));
        }

        private void SaveUnsavedPrefab(string inPath, AsComponent component)
        {
            if (component.IsValid())
                SaveSingleModifiedComponent(inPath, null,
                    ParseComponent(component), ComponentStatus.LocalOnly);
            else
                SaveSingleModifiedComponent(inPath, null,
                    ParseComponent(component), ComponentStatus.NoEvent);
        }

        

        private bool ComparePrefab(XElement xPrefab)
        {
            var prefabPath = AsScriptingHelper.GetXmlAttribute(xPrefab, "AssetPath");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (!prefab) return false;

            var tempPrefab = Instantiate(prefab);
            foreach (var xComponent in xPrefab.Elements())
            {
                var type = AsScriptingHelper.
                    StringToType(AsScriptingHelper.GetXmlAttribute(xComponent, "Type"));
                if (!SearchThisComponent(type)) continue;

                var gameObjectPath = AsScriptingHelper.GetXmlAttribute(xComponent, "GameObject");
                var gameObject = GetGameObject(tempPrefab, gameObjectPath);
                if (!gameObject) continue;
                
                var component = gameObject.GetComponent(type) as AsComponent;
                if (!component)
                {
                    SaveSingleModifiedComponent(prefabPath, xComponent,
                        null, ComponentStatus.ServerOnly);
                }
                else if (IsPrefabInstance(component.gameObject))
                    xComponent.Remove();
                else
                {
                    var oldXComponent = ParseComponent(component);
                    if (ComponentModified(component, xComponent))
                        SaveSingleModifiedComponent(prefabPath, xComponent,
                            oldXComponent, ComponentStatus.Different);
                }
            }
            DestroyImmediate(tempPrefab, true);
            return true;
        }

        private void CompareScenes(string inPath)
        {
            var currentScene = SceneManager.GetActiveScene().path;

            try
            {
                SearchXMLPrefabAndScene(inPath, CompareScene, "Scene");
                FindFiles(FindUnsavedScene, "Find Unsaved Components", "*.unity", inPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.ClearProgressBar();
            }
            EditorSceneManager.OpenScene(currentScene);
        }

        private void FindUnsavedScene(string inPath)
        {
            var scene = EditorSceneManager.OpenScene(inPath);
            if (!scene.IsValid()) return;

            var xScene = XRoot.Elements("Scene").FirstOrDefault(s =>
            inPath == AsScriptingHelper.GetXmlAttribute(s, "AssetPath"));
            if (xScene == null)
            {
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    var components = rootGameObject.
                        GetComponentsInChildren<AsComponent>(true);
                    if (components.Length == 0) continue;
                    foreach (var component in components)
                    {
                        if (!SearchThisComponent(component.GetType())) continue;
                        if (!ComponentBelongsToScene(component))
                        {
                            if (IncludePrefabInstance) 
                                ComparePrefabInstance(component.gameObject);
                            continue;
                        }
                        SaveUnsavedPrefab(inPath, component);
                    }
                }
                return;
            }

            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                var components = rootGameObject.
                    GetComponentsInChildren<AsComponent>(true);
                if (components.Length == 0) continue;
                foreach (var component in components)
                {
                    if (!SearchThisComponent(component.GetType())) continue;
                    if (!ComponentBelongsToScene(component))
                    {
                        if (IncludePrefabInstance)
                            ComparePrefabInstance(component.gameObject);
                        continue;
                    }
                    var xComponent = xScene.Elements().FirstOrDefault(c =>
                    AsScriptingHelper.GetXmlAttribute(c, "Type") ==
                    component.GetType().Name &&
                    AsScriptingHelper.GetXmlAttribute(c, "GameObject") ==
                    GetGameObjectPath(component.transform));
                    if (xComponent == null)
                    {
                        SaveUnsavedPrefab(inPath, component);
                    }
                        
                }

            }
        }

        private bool CompareScene(XElement xScene)
        {
            var scenePath = AsScriptingHelper.GetXmlAttribute(xScene, "AssetPath");
            var scene = EditorSceneManager.OpenScene(scenePath);
            if (!scene.IsValid())
            {
                Debug.LogError("Backup Failed: Can't find scene at " + scenePath);
                return false;
            }

            var modified = false;
            var xSceneComponents = xScene.Elements();
            foreach (var xComponent in xSceneComponents)
            {
                var type = AsScriptingHelper.
                    StringToType(AsScriptingHelper.GetXmlAttribute(xComponent, "Type"));
                if (!SearchThisComponent(type)) continue;

                var gameObjectPath = AsScriptingHelper.
                    GetXmlAttribute(xComponent, "GameObject");
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    var gameObject = GetGameObject(rootGameObject, gameObjectPath);
                    if (!gameObject) continue;

                    var component = gameObject.GetComponent(type) as AsComponent;
                    if (component == null)
                    {
                        SaveSingleModifiedComponent(scenePath, xComponent,
                        null, ComponentStatus.ServerOnly);
                        continue;
                    }

                    if (!ComponentBelongsToScene(component))
                    {
                        xComponent.Remove();
                        continue;
                    }

                    var tempGo = Instantiate(component);
                    var oldXComponent = ParseComponent(component);
                    if (ComponentModified(tempGo, xComponent))
                    {
                        EditorUtility.SetDirty(component);
                        SaveSingleModifiedComponent(scenePath, xComponent, 
                            oldXComponent, ComponentStatus.Different);
                        modified = true;
                    }
                    DestroyImmediate(tempGo.gameObject, true);
                    // go to next component
                    break;
                }
            }
            //EditorSceneManager.SaveScene(scene);
            return modified;
        }

        private bool SearchThisComponent(Type type)
        {
            bool willSearch = false;
            ComponentsToSearch.TryGetValue(type, out willSearch);
            if (willSearch) return true;
            return false;
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Window Operations
        //-----------------------------------------------------------------------------------------

        private void ProcessCompareData(ComponentComparisonData data)
        {
            if (!data.selectionToggle) return;

            switch (data.ComponentStatus)
            {
                case ComponentStatus.AllRemoved:
                    RemoveComponent(data);
                    RemoveServerXml(data);
                    break;
                case ComponentStatus.ServerOnly:
                    if (data.ServerData == null)
                        SaveLocalXmlToServer(data);
                    RemoveComponent(data);
                    break;
                case ComponentStatus.UseServer:
                    SaveServerXMLToComponent(data);
                    SaveServerXmlToServer(data);
                    break;
                case ComponentStatus.UseLocal:
                    SaveLocalXmlToServer(data);
                    SaveLocalXmlToComponent(data);
                    break;
                case ComponentStatus.LocalOnly:
                    if (data.LocalData == null)
                        SaveServerXMLToComponent(data);
                    RemoveServerXml(data);
                    break;
            }

            AsScriptingHelper.WriteXml(_importedXmlPath, XRoot);
        }

        #region Revert
        //-----------------------------------------------------------------------------------------

        internal void SaveServerXmlToServer(ComponentComparisonData data)
        {
            SaveXmlToServer(data.ServerData, data.AssetPath);
        }

        private void SaveLocalXmlToServer(ComponentComparisonData data)
        {
            SaveXmlToServer(data.LocalData, data.AssetPath);
        }

        private void SaveXmlToServer(XElement xmlData, string assetPath)
        {
            if (xmlData == null) return;
            var xAsset = XRoot.Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null)
            {
                // add a node based on the path suffix
                xAsset = new XElement(assetPath.EndsWith(".prefab") ? "Prefab" : "Scene");
                xAsset.SetAttributeValue("AssetPath", assetPath);
                XRoot.Add(xAsset);
            }

            var component = xAsset.Elements().FirstOrDefault(c =>
            AsScriptingHelper.GetXmlAttribute(xmlData, "Type") ==
            AsScriptingHelper.GetXmlAttribute(c, "Type") &&
            AsScriptingHelper.GetXmlAttribute(xmlData, "GameObject") ==
            AsScriptingHelper.GetXmlAttribute(c, "GameObject"));
            if (component == null) xAsset.Add(xmlData);
            else component.ReplaceWith(xmlData);
        }

        internal void SaveLocalXmlToComponent(ComponentComparisonData data)
        {
            if (data.AssetPath.Substring(data.AssetPath.LastIndexOf("/")).Contains(".prefab"))
            {
                SaveXmlDataToPrefab(data.LocalData, data.AssetPath);
            }
            else if (data.AssetPath.Substring(data.AssetPath.LastIndexOf("/")).Contains(".unity"))
            {
                SaveXmlDataToScene(data.LocalData, data.AssetPath);
            }
        }

        private void SaveServerXMLToComponent(ComponentComparisonData data)
        {
            if (data.AssetPath.Substring(data.AssetPath.LastIndexOf("/")).Contains(".prefab"))
            {
                SaveXmlDataToPrefab(data.ServerData, data.AssetPath);
            }
            else if (data.AssetPath.Substring(data.AssetPath.LastIndexOf("/")).Contains(".unity"))
            {
                SaveXmlDataToScene(data.ServerData, data.AssetPath);
            }
        }

        private void SaveXmlDataToScene(XElement inXmlData, string assetPath)
        {
            var scene = EditorSceneManager.OpenScene(assetPath);
            var gameObjectPath = AsScriptingHelper.
                GetXmlAttribute(inXmlData, "GameObject");
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                if (!gameObjectPath.Contains(rootGameObject.name)) continue;
                var gameObject = GetGameObject(rootGameObject, gameObjectPath);
                var type = AsScriptingHelper.StringToType(AsScriptingHelper.
                    GetXmlAttribute(inXmlData, "Type"));
                var component = gameObject.GetComponent(type) as AsComponent;
                if (!component) component = gameObject.AddComponent(type) as AsComponent;
                ComponentModified(component, inXmlData);
                break;
            }
            EditorSceneManager.SaveScene(scene);
        }

        private void SaveXmlDataToPrefab(XElement inXmlData, string assetPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            var gameObjectPath = AsScriptingHelper.
                GetXmlAttribute(inXmlData, "GameObject");
            var gameObject = GetGameObject(prefab, gameObjectPath);
            var type = AsScriptingHelper.StringToType(AsScriptingHelper.
                GetXmlAttribute(inXmlData, "Type"));
            var component = gameObject.GetComponent(type) as AsComponent;
            if (!component) component = gameObject.AddComponent(type) as AsComponent;
            ComponentModified(component, inXmlData);
            PrefabUtility.SavePrefabAsset(prefab);
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Remove
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Remove local component according to the buffer data from the comparing window
        /// </summary>
        /// <param name="data"></param>
        private void RemoveComponent(ComponentComparisonData data)
        {
            var xmlData = data.LocalData != null ? data.LocalData : data.ServerData;

            if (data.AssetPath.Substring(data.AssetPath.LastIndexOf("/")).Contains(".prefab"))
            {
                var gameObjectPath = AsScriptingHelper.GetXmlAttribute(xmlData, "GameObject");
                var type = AsScriptingHelper.
                    StringToType(AsScriptingHelper.GetXmlAttribute(xmlData, "Type"));

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(data.AssetPath);
                if (prefab == null) return;
                /*
                var rootPrefab = PrefabUtility.
                    GetCorrespondingObjectFromSource(prefab.transform.root.gameObject);
                if (rootPrefab == null) return;
                */
                var gameObject = GetGameObject(prefab, gameObjectPath);
                if (gameObject == null) return;
                var component = gameObject.GetComponent(type) as AsComponent;
                if (component == null) return;

                DestroyImmediate(component, true);
                PrefabUtility.SavePrefabAsset(prefab);

                /*
                var stage = UnityEditor.Experimental.SceneManagement.
                    PrefabStageUtility.GetCurrentPrefabStage();
                if (stage == null) PrefabUtility.SavePrefabAsset(rootPrefab);
                else EditorUtility.SetDirty(rootPrefab);
                */
            }
            else if (data.AssetPath.Substring(data.AssetPath.LastIndexOf("/")).Contains(".unity"))
            {
                var scene = EditorSceneManager.OpenScene(data.AssetPath);
                var gameObjectPath = AsScriptingHelper.
                    GetXmlAttribute(xmlData, "GameObject");
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    if (!gameObjectPath.Contains(rootGameObject.name)) continue;
                    var gameObject = GetGameObject(rootGameObject, gameObjectPath);
                    var type = AsScriptingHelper.StringToType(AsScriptingHelper.
                        GetXmlAttribute(xmlData, "Type"));
                    var component = gameObject.GetComponent(type) as AsComponent;
                    DestroyImmediate(component, true);
                    break;
                }
                EditorSceneManager.SaveScene(scene);
            }
        }

        /// <summary>
        /// Remove node from compare window
        /// </summary>
        /// <param name="data"></param>
        internal void RemoveServerXml(ComponentComparisonData data)
        {
            var xmlData = data.ServerData != null ? data.ServerData : data.LocalData;

            var xAsset = XRoot.Elements().FirstOrDefault(x => 
            data.AssetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null) return;

            var xComponent = xAsset.Elements().FirstOrDefault(x =>
                AsScriptingHelper.GetXmlAttribute(xmlData, "Type") ==
                AsScriptingHelper.GetXmlAttribute(x, "Type") &&
                AsScriptingHelper.GetXmlAttribute(xmlData, "GameObject") ==
                AsScriptingHelper.GetXmlAttribute(x, "GameObject"));
            if (xComponent == null) return;

            AsScriptingHelper.RemoveComponentXml(xComponent);
        }
        //-----------------------------------------------------------------------------------------
        #endregion

        //-----------------------------------------------------------------------------------------
        #endregion

        #region Component Inspector Operations
        //-----------------------------------------------------------------------------------------
        internal bool SaveComponentDataToXML(string assetPath, AsComponent component)
        {
            ReadXmlData(DefaultXmlPath);
            // locate the component node from xml
            var xAsset = XRoot.Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null)
            {
                // add a node based on the path suffix
                xAsset = new XElement(assetPath.EndsWith(".prefab") ? "Prefab" : "Scene");
                xAsset.SetAttributeValue("AssetPath", assetPath);
                XRoot.Add(xAsset);
            }

            var xComponent = FindComponentNode(xAsset, component);
            var xTemp = ParseComponent(component);
            // can't find existing node, create a new one
            if (xComponent == null) xAsset.Add(xTemp);
            else xComponent.ReplaceWith(xTemp);
            // overwrite xml file after update
            AsScriptingHelper.WriteXml(DefaultXmlPath, XRoot);
            return true;
        }

        internal bool SaveRemovedComponentDataToXML(GameObject ciObj, AsComponent component)
        {
            if (!File.Exists(DefaultRemovedComponentDataPath))
                AsScriptingHelper.WriteXml(DefaultRemovedComponentDataPath, new XElement("Root"));
            else
            {
                var xRoot = XDocument.Load(DefaultRemovedComponentDataPath).Root;
                _removedComponentXRoot = xRoot;
            }
            string assetPath = AssetDatabase.GetAssetPath(ciObj);
            if (string.IsNullOrEmpty(assetPath))
            {
#if UNITY_2021_2_OR_NEWER
                var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
                var stage = UnityEditor.Experimental.SceneManagement.
                    PrefabStageUtility.GetCurrentPrefabStage();
#endif
                assetPath = stage != null ? stage.assetPath : SceneManager.GetActiveScene().path;
            }

            // locate the component node from xml
            var xAsset = _removedComponentXRoot.Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null)
            {
                // add a node based on the path suffix
                xAsset = new XElement(assetPath.EndsWith(".prefab") ? "Prefab" : "Scene");
                xAsset.SetAttributeValue("AssetPath", assetPath);
                _removedComponentXRoot.Add(xAsset);
            }

            var xComponent = FindComponentNode(xAsset, component, ciObj.transform);
            var xTemp = ParseComponent(component, ciObj.transform);
            // can't find existing node, create a new one
            if (xComponent == null) 
                xAsset.Add(xTemp);
            else 
                xComponent.ReplaceWith(xTemp);
            // overwrite xml file after update
            AsScriptingHelper.WriteXml(DefaultRemovedComponentDataPath, _removedComponentXRoot);
            return true;
        }

        internal bool SaveComponentDataToSeparatedXML(string assetPath, AsComponent component)
        {
            var type = component.GetType();
            LoadOrCreateSeparatedXmlDoc(type);
            // locate the component node from xml
            var xAsset = _separateComponentXRoot[type].Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null)
            {
                // add a node based on the path suffix
                xAsset = new XElement(assetPath.EndsWith(".prefab") ? "Prefab" : "Scene");
                xAsset.SetAttributeValue("AssetPath", assetPath);
                _separateComponentXRoot[type].Add(xAsset);
            }

            var xComponent = FindComponentNode(xAsset, component);
            var xTemp = ParseComponent(component);
            // can't find existing node, create a new one
            if (xComponent == null) xAsset.Add(xTemp);
            else xComponent.ReplaceWith(xTemp);
            // overwrite xml file after update
            AsScriptingHelper.WriteXml(IndividualXmlPath(type), _separateComponentXRoot[type]);
            return true;
        }

        internal bool SaveXMLDataToComponent(string assetPath, AsComponent component)
        {
            ReadXmlData(DefaultXmlPath);
            var xAsset = XRoot.Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null) return false;
            var xComponent = FindComponentNode(xAsset, component);
            if (xComponent == null) return false;
            // revert component data to its xml
            bool isModified = ComponentModified(component, xComponent);
            SaveComponentAsset(component.gameObject, assetPath);
            return isModified;
        }

        /// <summary>
        /// Remove node from component inspector
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="component"></param>
        internal void RemoveServerXml(string assetPath, AsComponent component)
        {
            ReadXmlData(DefaultXmlPath);
            var type = component.GetType();
            var xAsset = XRoot.Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null) return;

            var xComponent = FindComponentNode(xAsset, component);
            if (xComponent == null) return;

            AsScriptingHelper.RemoveComponentXml(xComponent);
            AsScriptingHelper.WriteXml(DefaultXmlPath, XRoot);
        }

        internal void RemoveSeparatedXml(string assetPath, AsComponent component)
        {
            var type = component.GetType();
            LoadOrCreateSeparatedXmlDoc(type);
            var xAsset = _separateComponentXRoot[type].Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null) return;

            var xComponent = FindComponentNode(xAsset, component);
            if (xComponent == null) return;

            AsScriptingHelper.RemoveComponentXml(xComponent);
            AsScriptingHelper.WriteXml(IndividualXmlPath(type), _separateComponentXRoot[type]);
        }

        internal void RemoveRemovedData(AsComponent component)
        {
            if (!File.Exists(DefaultRemovedComponentDataPath))
                AsScriptingHelper.WriteXml(DefaultRemovedComponentDataPath, new XElement("Root"));
            else
            {
                var xRoot = XDocument.Load(DefaultRemovedComponentDataPath).Root;
                _removedComponentXRoot = xRoot;
            }
            string assetPath = string.Empty;
#if UNITY_2021_2_OR_NEWER
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
                var stage = UnityEditor.Experimental.SceneManagement.
                    PrefabStageUtility.GetCurrentPrefabStage();
#endif
            assetPath = stage != null ? stage.assetPath : SceneManager.GetActiveScene().path;

            // locate the component node from xml
            var xAsset = _removedComponentXRoot.Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));

            if (xAsset == null) return;

            var xComponent = FindComponentNode(xAsset, component);
            if (xComponent == null) return;

            AsScriptingHelper.RemoveComponentXml(xComponent);
            AsScriptingHelper.WriteXml(DefaultRemovedComponentDataPath, _removedComponentXRoot);
        }

        internal static string FindComponentAssetPath(Component component, bool defaultOnPrefab = false)
        {
            var go = component.gameObject;
            var path = AssetDatabase.GetAssetPath(component);
            // path won't be empty if editing on top level of a prefab
            if (string.IsNullOrEmpty(path))
            {
                if (!PrefabUtility.IsAddedComponentOverride(component))
                    path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(component);
                else
                    path = string.Empty;

                if (string.IsNullOrEmpty(path))
                {
#if UNITY_2021_2_OR_NEWER
                    var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
                var stage = UnityEditor.Experimental.SceneManagement.
                    PrefabStageUtility.GetCurrentPrefabStage();
#endif
                    path = stage != null ? stage.assetPath : SceneManager.GetActiveScene().path;
                    if (string.IsNullOrEmpty(path))
                    {
                        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go.transform.root.gameObject);
                        if (prefab && PrefabUtility.IsPartOfPrefabAsset(prefab))
                            path = AssetDatabase.GetAssetPath(prefab);
                    }
                }
            }
            return path;
        }

        private XElement FindComponentNode(XElement xAsset, Component component, Transform trans = null)
        {
            return xAsset.Elements().FirstOrDefault(x =>
            GetGameObjectPath(trans == null ? component.transform : trans) ==
            AsScriptingHelper.GetXmlAttribute(x, "GameObject") &&
            component.GetType().Name ==
            AsScriptingHelper.GetXmlAttribute(x, "Type"));
        }

        internal bool IsComponentBackedUp(string assetPath, AsComponent component)
        {
            //var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            //string path = stage != null ? stage.assetPath : SceneManager.GetActiveScene().path;
            //if (!assetPath.Equals(path))
            //    return true;
            if (PrefabUtility.IsPartOfPrefabInstance(component))
                return true;
            ReadXmlData();
            var xAsset = XRoot.Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));
            if (xAsset == null) return false;

            var xComponent = FindComponentNode(xAsset, component);
            if (xComponent == null) return false;

            var tempComponent = Instantiate(component);
            if (ComponentModified(tempComponent, xComponent))
            {
                DestroyImmediate(tempComponent.gameObject, true);
                return false;
            }
            else
            {
                DestroyImmediate(tempComponent.gameObject, true);
                return true;
            }
        }
        //-----------------------------------------------------------------------------------------
        #endregion

        #region Asset Operations
        //-----------------------------------------------------------------------------------------

        internal void SaveSelectedComponents(string assetPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (!prefab) return;

            var components = prefab.GetComponentsInChildren<AsComponent>(true);
            foreach (var component in components)
            {
                if (IsPrefabInstance(component.gameObject)) continue;
                SaveComponentDataToXML(assetPath, component);
                SaveComponentDataToSeparatedXML(assetPath, component);
            }
        }

        internal void SaveSelectedScenes(string assetPath)
        {
            var scene = EditorSceneManager.OpenScene(assetPath);
            if (!scene.IsValid()) return;

            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                var components = rootGameObject.GetComponentsInChildren<AsComponent>(true);
                foreach (var component in components)
                {
                    if (!ComponentBelongsToScene(component)) continue;
                    SaveComponentDataToXML(assetPath, component);
                    SaveComponentDataToSeparatedXML(assetPath, component);
                }
            }
        }

        internal bool RevertPrefab(string assetPath, XElement xPrefab, XElement xRemovedComponent)
        {
            bool modified = false;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (!prefab)
            {
                Debug.LogError("Revert Failed: Can't find prefab at " + assetPath);
                return false;
            }
            if (xRemovedComponent != null)
            {
                var xComponents = xRemovedComponent.Elements();
                foreach (var xComponent in xComponents)
                {
                    var gameObjectPath = AsScriptingHelper.GetXmlAttribute(xComponent, "GameObject");
                    GameObject gameobj = GetGameObject(prefab, gameObjectPath);
                    if (gameobj)
                    {
                        var type = AsScriptingHelper.StringToType(AsScriptingHelper.GetXmlAttribute(xComponent, "Type"));
                        var component = gameobj.GetComponent(type) as AsComponent;
                        if (component)
                        {
                            modified = ComponentModified(component, xComponent);
                            if (!modified)
                            {
                                DestroyImmediate(component, true);
                                modified = true;
                            }
                        }
                    }
                }
                Transform[] translist = prefab.transform.GetComponentsInChildren<Transform>();
                foreach (Transform obj in translist)
                {
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(obj.gameObject))
                    {
                        var removedlist = PrefabUtility.GetRemovedComponents(obj.gameObject);
                        foreach (var removedcomponent in removedlist)
                        {
                            if (removedcomponent.assetComponent is AsComponent && removedcomponent.containingInstanceGameObject.Equals(obj.gameObject))
                            {
                                var componentnode = FindComponentNode(xRemovedComponent, (AsComponent)removedcomponent.assetComponent, removedcomponent.containingInstanceGameObject.transform);
                                if (componentnode == null)
                                {
                                    removedcomponent.Revert();
                                    modified = true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Transform[] translist = prefab.transform.GetComponentsInChildren<Transform>();
                foreach (Transform obj in translist)
                {
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(obj.gameObject))
                    {
                        var removedlist = PrefabUtility.GetRemovedComponents(obj.gameObject);
                        foreach (var removedcomponent in removedlist)
                        {
                            if (removedcomponent.assetComponent is AsComponent && removedcomponent.containingInstanceGameObject.Equals(obj.gameObject))
                            {
                                removedcomponent.Revert();
                                modified = true;
                            }
                        }
                    }
                }
            }
            if (xPrefab != null)
            {
                var xComponents = xPrefab.Elements();
                foreach (var xComponent in xComponents)
                {
                    TotalCount++;
                    // locate the game object in prefab
                    var gameObjectPath = AsScriptingHelper.GetXmlAttribute(xComponent, "GameObject");
                    var gameObject = GetGameObject(prefab, gameObjectPath);
                    if (!gameObject)
                    {
                        Debug.LogError("Backup Failed: Can't find game object at " +
                            gameObjectPath + " in prefab " + assetPath);
                        continue;
                    }

                    var type = AsScriptingHelper.
                        StringToType(AsScriptingHelper.GetXmlAttribute(xComponent, "Type"));
                    var component = gameObject.GetComponent(type) as AsComponent;
                    // Save the component data for future operation
                    if (!component)
                    {
                        modified = true;
                        component = gameObject.AddComponent(type) as AsComponent;
                    }
                    if (ComponentModified(component, xComponent))
                    {
                        modified = true;
                        
                    }
                }
            }
            
            if (modified)
                SaveComponentAsset(prefab, assetPath);

            return modified;
        }

        internal GameObject FindGameObjectByName(GameObject prefab, string name)
        {
            for(int i = 0; i < prefab.transform.childCount; i++)
            {
                if (!string.IsNullOrEmpty(name) && name.Equals(prefab.transform.GetChild(i).gameObject.name))
                    return prefab.transform.GetChild(i).gameObject;
                else if (prefab.transform.GetChild(i).childCount > 0)
                    return FindGameObjectByName(prefab.transform.GetChild(i).gameObject, name);
            }
            return null;
        }

        internal void RevertSelectedComponents(string assetPath)
        {
            //var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            //if (!prefab) return;

            //var components = prefab.GetComponentsInChildren<AsComponent>(true);
            //foreach (var component in components)
            //{
            //    if (IsPrefabInstance(component.gameObject)) continue;
            //    SaveXMLDataToComponent(assetPath, component);
            //}
            ReadXmlData(DefaultXmlPath);
            var xAsset = XRoot.Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));

            if (!File.Exists(DefaultRemovedComponentDataPath))
                AsScriptingHelper.WriteXml(DefaultRemovedComponentDataPath, new XElement("Root"));
            else
            {
                var xRoot = XDocument.Load(DefaultRemovedComponentDataPath).Root;
                _removedComponentXRoot = xRoot;
            }

            // locate the component node from xml
            var xRemovedAsset = _removedComponentXRoot.Elements().FirstOrDefault(x =>
            assetPath == AsScriptingHelper.GetXmlAttribute(x, "AssetPath"));

            //if (xAsset == null && xRemovedAsset == null)
            //    return;

            RevertPrefab(assetPath, xAsset, xRemovedAsset);
        }

        internal void RevertSelectedScenes(string assetPath)
        {
            var scene = EditorSceneManager.OpenScene(assetPath);
            if (!scene.IsValid()) return;

            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                var components = rootGameObject.GetComponentsInChildren<AsComponent>(true);
                foreach (var component in components)
                {
                    if (!ComponentBelongsToScene(component)) continue;
                    SaveXMLDataToComponent(assetPath, component);
                }
            }
        }

        internal void RemoveSelectedComponents(string assetPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (!prefab) return;

            var components = prefab.GetComponentsInChildren<AsComponent>(true);
            foreach (var component in components)
            {
                if (IsPrefabInstance(component.gameObject)) continue;
                RemoveServerXml(assetPath, component);
                RemoveSeparatedXml(assetPath, component);
                DestroyImmediate(component, true);
            }
        }

        internal void RemoveSelectedScenes(string assetPath)
        {
            var scene = EditorSceneManager.OpenScene(assetPath);
            if (!scene.IsValid()) return;

            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                var components = rootGameObject.GetComponentsInChildren<AsComponent>(true);
                foreach (var component in components)
                {
                    if (!ComponentBelongsToScene(component)) continue;
                    RemoveServerXml(assetPath, component);
                    RemoveSeparatedXml(assetPath, component);
                    DestroyImmediate(component, true);
                }
            }
        }

        //-----------------------------------------------------------------------------------------
        #endregion

        private bool IsPrefabInstance(GameObject componentObject)
        {
            var instance = PrefabUtility.GetNearestPrefabInstanceRoot(componentObject);
            if (instance == null) return false;
            else
            {
                Debug.Log("instance: " + AssetDatabase.GetAssetPath(instance) + 
                    "  " + instance.name.ToString());
                return true;
            }
        }

        /// <summary>
        /// check if a component is prefab variant or just in scene
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        private bool ComponentBelongsToScene(Component component)
        {
#if UNITY_2018_3_OR_NEWER
            // find the root game object node of prefab
            var prefab = PrefabUtility.GetNearestPrefabInstanceRoot(component.gameObject);
            // game object is not a prefab at all
            if (prefab)
            {
                // check if any override matches this component
                var overrides = PrefabUtility.GetObjectOverrides(prefab, true);
                foreach (var objectOverride in overrides)
                {
                    var c = objectOverride.instanceObject as AsComponent;
                    if (c == component)
                        return true;
                }
                // can't find any overrides
                return false;
            }
#else 
            // find a prefab, see if user wants to include it
            if (PrefabUtility.GetPrefabType(component.gameObject) != PrefabType.None)
                return IncludePrefabsInScene;
#endif
            // not a prefab, must be in scene only
            return true;
        }

        private void SearchXMLPrefabAndScene(string inPath,
            Func<XElement, bool> parser, string parsingType)
        {
            if (string.IsNullOrEmpty(inPath))
            {
                if (!EditorUtility.DisplayDialog("Path Not Set",
                    "The Directoty path is not set, are you going to " +
                    "use the default Asset/ to compare?", "Ok", "Cancel"))
                    return;
                else
                    inPath = _defaultSearchingPath;
            }

            var xmlPrefabs = XRoot.Elements(parsingType).ToList();
            int totalPrefabNum = xmlPrefabs.Count;
            for (var index = 0; index < totalPrefabNum; index++)
            {
                var assetPath = AsScriptingHelper.
                    GetXmlAttribute(xmlPrefabs[index], "AssetPath");
                Debug.LogWarning(assetPath);
                if (!assetPath.Contains(inPath)) continue;
                parser(xmlPrefabs[index]);
                if (EditorUtility.DisplayCancelableProgressBar("Processing",
                    assetPath, (index + 1f) / TotalCount))
                    break;
            }
        }

        /// <summary>
        /// write component node
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        private XElement ParseComponent(AsComponent component, Transform trans = null)
        {
            var type = component.GetType();
            var xComponent = new XElement("Component");
            xComponent.SetAttributeValue("Type", type.Name);
            xComponent.SetAttributeValue("GameObject",GetGameObjectPath(trans == null ? component.transform : trans));
            _exporters[type](component, xComponent);
            EditedCount++;
            return xComponent;
        }

        public bool ComponentModified(AsComponent component, XElement node)
        {
            var type = component.GetType();
            return _importers[type] != null &&
                _importers[type](component, node);
        }

        
        private static void SaveModifiedComponents(string assetPath,
            XElement inServerData, XElement inLocalData, ComponentStatus inComponentStatus)
        {
            if (inServerData != null)
            {
                var xmlComponents = inServerData.Elements();
                foreach (var xmlComponent in xmlComponents)
                {
                    SaveSingleModifiedComponent(assetPath,
                       xmlComponent, null, inComponentStatus);
                }
            }
            else if (inLocalData != null)
            {
                var xmlComponents = inLocalData.Elements();
                foreach (var xmlComponent in xmlComponents)
                {
                    SaveSingleModifiedComponent(assetPath,
                       null, xmlComponent, inComponentStatus);
                }
            }
        }

        private static void SaveSingleModifiedComponent(string assetPath,
            XElement inServerData, XElement inLocalData, ComponentStatus inComponentStatus)
        {
            var data = new ComponentComparisonData
            {
                AssetPath = assetPath,
                ServerData = inServerData,
                LocalData = inLocalData,
                ComponentStatus = inComponentStatus
            };
            AsCompareWindow.ModifiedComponents.Add(data);
        }

        private void CreateWindow()
        {
            _compareWindow = GetWindow<AsCompareComponentWindow>();
            _compareWindow.position = new Rect(500, 300, 800, 600);
            _compareWindow.titleContent = new GUIContent("Compare Components");
            _compareWindow.saveSingleDataEvent = ProcessCompareData;
        }

        

        public void CombineThread(object data)
        {
            AsComponentBackup.Instance.bCombining = true;
            //AsComponentBackup.Instance.Combine();
            AsComponentBackup.Instance.bCombining = false;
        }


    }                
}