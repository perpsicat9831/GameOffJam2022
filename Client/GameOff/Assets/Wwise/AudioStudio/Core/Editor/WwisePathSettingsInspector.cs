using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using AudioStudio.Tools;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(WwisePathSettings))]
    public class WwisePathSettingsInspector : UnityEditor.Editor
    {
        private WwisePathSettings _component;

        private void OnEnable()
        {
            _component = target as WwisePathSettings;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            var currentPlugin = _component.CurrentPlugin;
            _component.CurrentPlugin = (ActivePlugin) EditorGUILayout.EnumPopup("Deployment Plugin Config", _component.CurrentPlugin);
            if (currentPlugin != _component.CurrentPlugin)
            {
                if (_component.CurrentPlugin == ActivePlugin.Debug)
                    AkPluginActivator.ActivateDebug();
                else if (_component.CurrentPlugin == ActivePlugin.Profile)
                    AkPluginActivator.ActivateProfile();
                else
                    AkPluginActivator.ActivateRelease();
            }
            EditorGUILayout.Separator();

            AsGuiDrawer.DrawPathDisplay("AudioStudio Library Path", WwisePathSettings.AUDIO_STUDIO_LIBRARY_PATH, EditConstantPaths, "Assets/");
            AsGuiDrawer.DrawPathDisplay("Wwise Plugins Path", _component.WwisePluginsPath, SetupWwisePluginsPath, "Assets/");
            AsGuiDrawer.DrawPathDisplay("SoundBanks Initial Build Path", WwisePathSettings.INITIAL_BANK_SUB_FOLDER, EditConstantPaths, "StreamingAssets/");
            AsGuiDrawer.DrawPathDisplay("SoundBanks Hot Update Path", WwisePathSettings.UPDATE_BANK_SUB_FOLDER, EditConstantPaths, "PersistentDataPath/");

            AsGuiDrawer.DrawPathDisplay("Wwise Project Path (For Wwise Picker)", _component.WwiseProjectFilePath, SetupWwiseProjectPath);
            if (string.IsNullOrEmpty(_component.WwiseProjectFilePath))
                if (GUILayout.Button("Auto Find Wwise Project"))
                    AutoFindWwiseProject();
            EditorGUILayout.Separator();
            
            AsGuiDrawer.DrawPathDisplay("Game Start Scene Path", _component.StartScenePath, SetupStartScenePath, "Assets/");

            EditorGUILayout.LabelField("Editor SoundBanks Path", EditorStyles.boldLabel);
            AsGuiDrawer.DrawProperty(serializedObject.FindProperty("LoadBankFromWwiseProjectFolder"), "Load from Wwise Project Folder", 220, 30);
            //AkWwiseEditorSettings.Instance.SoundbankPath = WwisePathSettings.GetBankLoadPath();
            if (!_component.LoadBankFromWwiseProjectFolder)
                AsGuiDrawer.DrawPathDisplay("Bank Load Path (Relative to Assets/)", _component.EditorBankLoadPath, SetupSoundBankLoadPath);

            if (AudioInitSettings.Instance.PackageMode)
                AsGuiDrawer.DrawPathDisplay("Audio Package Path", _component.AudioPackagePath, SetupAudioPackageLoadPath);
            EditorGUILayout.Separator();
        
            EditorGUILayout.LabelField("SoundBanks Settings", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                AsGuiDrawer.DrawProperty(serializedObject.FindProperty("SeparateBankPlatforms"), "Different Banks per Platform", 220, 30);
                AsGuiDrawer.DrawProperty(serializedObject.FindProperty("CopyBanksWhenBuild"), "Copy Banks to StreamingAssets", 220, 30);
                AkWwiseEditorSettings.Instance.CopySoundBanksAsPreBuildStep = _component.CopyBanksWhenBuild;
                if (_component.CopyBanksWhenBuild)
                    AsGuiDrawer.DrawProperty(serializedObject.FindProperty("ExcludeDLCBanks"), "Exclude DLC Banks", 220, 30);
            }
            EditorGUILayout.Separator();
            
            EditorGUILayout.LabelField("WAAPI Settings", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                
                try
                {
                    EditorGUILayout.LabelField(GetWwiseVersion());
                }
                catch (DllNotFoundException e)
                {
                    Debug.LogError(e);
                    EditorGUILayout.HelpBox("AkSoundEngine.dll not found, please check AudioStudio library path!", MessageType.Error);
                }
                AsGuiDrawer.DrawProperty(serializedObject.FindProperty("WaapiPort"), "Port");
            }
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Gizmos Settings", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                AsGuiDrawer.DrawProperty(serializedObject.FindProperty("GizmosSphereColor"), "Attenuation Sphere", 120);
                AsGuiDrawer.DrawProperty(serializedObject.FindProperty("GizmosIconScaling"), "Icon Scaling", 120);
            }

            EditorGUILayout.BeginHorizontal();
            GUI.contentColor = Color.yellow;
            if (GUILayout.Button("Open Configs Folder", EditorStyles.toolbarButton))
                Process.Start(WwisePathSettings.EditorConfigPathFull);
            AsGuiDrawer.DrawSaveButton(_component);
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private string GetWwiseVersion()
        {
            var temp = AudioStudioWrapper.GetMajorMinorVersion();
            var temp2 = AudioStudioWrapper.GetSubminorBuildVersion();
            var versionString = (temp >> 16) + "." + (temp & 0xFFFF);
            if (temp2 >> 16 != 0) versionString += "." + (temp2 >> 16);

            versionString += "." + (temp2 & 0xFFFF);
            return "Wwise version:   " + versionString;
        }

        private void SetupWwiseProjectPath()
        {
            var defaultPath = Path.GetDirectoryName(Path.GetFullPath(Path.Combine(Application.dataPath, _component.WwiseProjectFilePath != null ? _component.WwiseProjectFilePath : string.Empty)));
            var WwiseProjectPathNew = EditorUtility.OpenFilePanel("Select your Wwise Project", defaultPath, "wproj");
            if (WwiseProjectPathNew.Length != 0)
            {
                if (WwiseProjectPathNew.EndsWith(".wproj") == false)
                {
                    AkUtilities.IsWwiseProjectAvailable = false;
                    EditorUtility.DisplayDialog("Error", "Please select a valid Wwise project file", "Ok");
                }
                else
                {
                    _component.WwiseProjectFilePath = AkUtilities.MakeRelativePath(Application.dataPath, WwiseProjectPathNew);
                    AkUtilities.IsWwiseProjectAvailable = true;
                    AkWwiseProjectInfo.Populate();
                    AkWwiseEditorSettings.Instance.WwiseProjectPath = _component.WwiseProjectFilePath;
                    AkWwiseEditorSettings.Instance.SaveSettings();
                }
            }
        }

        private static void EditConstantPaths()
        {
            var scriptPath = AsScriptingHelper.CombinePath(Application.dataPath, WwisePathSettings.AUDIO_STUDIO_LIBRARY_PATH, "Extensions/WwisePathSettingsExt.cs");
            Process.Start(scriptPath);
        }

        private void AutoFindWwiseProject()
        {
            if (string.IsNullOrEmpty(_component.WwiseProjectFilePath))
            {
                var projectDir = Path.GetDirectoryName(Application.dataPath);
                if (projectDir != null)
                {
                    var foundWwiseProjects = Directory.GetFiles(projectDir, "*.wproj", SearchOption.AllDirectories);

                    if (foundWwiseProjects.Length > 0)
                        _component.WwiseProjectFilePath = AkUtilities.MakeRelativePath(Application.dataPath, foundWwiseProjects[0]);
                    else
                        EditorUtility.DisplayDialog("Error", "Wwise Project not found!", "OK");
                }
            }
        }

        private void SetupWwisePluginsPath()
        {
            var fullPath = AkUtilities.GetFullPath(Application.dataPath, _component.WwisePluginsPath);
            var defaultPath = Path.GetDirectoryName(fullPath);
            var pluginsPathNew = EditorUtility.OpenFolderPanel("Select your Wwise Plugins folder", defaultPath, "");
            if (pluginsPathNew.Length != 0)
                _component.WwisePluginsPath = AkUtilities.MakeRelativePath(Application.dataPath, pluginsPathNew);
        }

        private void SetupSoundBankLoadPath()
        {
            var fullPath = AkUtilities.GetFullPath(Application.dataPath, _component.EditorBankLoadPath);
            var defaultPath = Path.GetDirectoryName(fullPath);
            var soundBankPathNew = EditorUtility.OpenFolderPanel("Select your SoundBanks export folder", defaultPath, "");
            if (soundBankPathNew.Length != 0)
            {
                _component.EditorBankLoadPath = AkUtilities.MakeRelativePath(Application.dataPath, soundBankPathNew);
                UpdateSoundBankDestinationFolders(_component.WwiseProjectFilePathFull, _component.EditorBankLoadPath);
            }
        }

        private void SetupAudioPackageLoadPath()
        {
            var fullPath = AkUtilities.GetFullPath(Application.dataPath, _component.AudioPackagePath);
            var defaultPath = Path.GetDirectoryName(fullPath);
            var packagePathNew = EditorUtility.OpenFolderPanel("Select your AudioPackage export folder", defaultPath, "");
            if (packagePathNew.Length != 0)
                _component.AudioPackagePath = AkUtilities.MakeRelativePath(Application.dataPath, packagePathNew);
        }
        
        private void SetupStartScenePath()
        {
            var fullPath = AkUtilities.GetFullPath(Application.dataPath, _component.StartScenePath);
            var defaultPath = Path.GetDirectoryName(fullPath);
            var scenePathNew = EditorUtility.OpenFilePanel("Select scene to start game", defaultPath, "unity");
            if (scenePathNew.Length != 0)
                _component.StartScenePath = AkUtilities.MakeRelativePath(Application.dataPath, scenePathNew);
        }

        private void UpdateSoundBankDestinationFolders(string wwiseProjectPath, string soundBanksPath)
        {
            try
            {
                if (wwiseProjectPath.Length == 0)
                    return;

                if (!File.Exists(wwiseProjectPath))
                    return;

                var doc = new XmlDocument();
                doc.Load(wwiseProjectPath);
                var Navigator = doc.CreateNavigator();

                // Navigate the wproj file (XML format) to where generated soundbank paths are stored
                var it = Navigator.Select("//Property[@Name='SoundBankPaths']/ValueList/Value");
                foreach (XPathNavigator node in it)
                {
                    var path = node.Value;
                    WwisePathSettings.FixPathSeparator(ref path);
                    var pf = node.GetAttribute("Platform", "");
                    var platformBankPath = Path.Combine(soundBanksPath, pf);
                    platformBankPath = platformBankPath.Replace("/", @"\");
                    node.SetValue(platformBankPath);
                }

                doc.Save(wwiseProjectPath);
            }
            catch (Exception ex)
            {
                // Error happened, return empty string
                Debug.LogError("Wwise: Error while reading project " + wwiseProjectPath + ".  Exception: " + ex.Message);
            }
        }
    }
}