using System.Diagnostics;
using UnityEditor;
using System.IO;
using System.Linq;
using AudioStudio.Components;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AudioStudio.Tools
{
    public partial class AsToolbarMenus
    {
        #region Configs
        [MenuItem("AudioStudio/Configs/Audio Init &F1")]
        private static void OpenAudioInitSettings()
        {
            Selection.activeObject = AudioInitSettings.Instance;
        }

        [MenuItem("AudioStudio/Configs/Wwise Paths &F2")]
        private static void OpenWwisePathSettings()
        {
            Selection.activeObject = WwisePathSettings.Instance;
        }
        
        [MenuItem("AudioStudio/Configs/DLC SoundBank List &F3")]
        public static void OpenDLCBankList()
        {
            Selection.activeObject = DLCBankList.Instance;
        }
        
        [MenuItem("AudioStudio/Configs/AkSoundEngine &F4")]
        private static void OpenWwiseInitializationSettings()
        {
            Selection.activeObject = AkWwiseInitializationSettings.Instance;
        }
        #endregion
        
        #region Game
        [MenuItem("AudioStudio/Game/Start Game")]
        public static void StartGame()
        {
            EditorSceneManager.OpenScene("Assets/" + WwisePathSettings.Instance.StartScenePath);
            EditorApplication.isPlaying = true;
        }
        
        [MenuItem("AudioStudio/Game/Load Default Banks")]
        public static void LoadDefaultBanks()
        {
            AsAssetLoader.LoadAudioInitData();
        }
        
        [MenuItem("AudioStudio/Game/Reload All Banks")]
        public static void ReloadAllBanks()
        {
            BankManager.RefreshAllBanks();
        }
        #endregion

        #region Open
        [MenuItem("AudioStudio/Open/Wwise Picker &F5")]
        public static void WwisePicker()
        {
            EditorWindow.GetWindow<AkWwisePicker>("Wwise Picker", true,
                typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow"));
        }

        [MenuItem("AudioStudio/Open/Audio Profiler &F6")]
        public static void AudioProfiler()
        {
            EditorWindow.GetWindow<AudioProfiler>("Audio Profiler", true,
                typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow"));
        }
        
        [MenuItem("AudioStudio/Open/Wwise Project &F7")]
        public static void OpenWwiseProject()
        {
            if (File.Exists(WwisePathSettings.Instance.WwiseProjectFilePathFull))
                Process.Start(WwisePathSettings.Instance.WwiseProjectFilePathFull);
            else
                EditorUtility.DisplayDialog("Error", "Can't find Wwise Project at " + WwisePathSettings.Instance.WwiseProjectFilePathFull, "OK");
        }

        [MenuItem("AudioStudio/Open/SFX Folder &F8")]
        private static void OpenSfxFolder()
        {
            var folderPath = AsScriptingHelper.CombinePath(WwisePathSettings.Instance.WwiseProjectFolderPath, "Originals/SFX");
            if (Directory.Exists(folderPath))
                Process.Start(folderPath);
            else
                EditorUtility.DisplayDialog("Error", "Can't find Wwise Project at " + WwisePathSettings.Instance.WwiseProjectFilePathFull, "OK");
        }
        #endregion

        #region Tools
        //-----------------------------------------------------------------------------------------
        [MenuItem("AudioStudio/Tools/Update Scene Object Positions")]
        public static void UpdateSceneObjectPositions()
        {
            var editedCount = 0;
            var currentScene = SceneManager.GetActiveScene();
            foreach (var rootGameObject in currentScene.GetRootGameObjects())
            {
                if (rootGameObject.name.Contains("Audio"))
                {
                    var followers = rootGameObject.GetComponentsInChildren<AudioTransformFollower>();
                    editedCount += followers.Count(follower => follower.UpdatePosition());
                }
            }
            if (editedCount > 0)
            {
                EditorSceneManager.SaveScene(currentScene);
                EditorUtility.DisplayDialog("Success", "Updated " + editedCount + " emitter positions/rotations.", "OK");
            }
            else
                EditorUtility.DisplayDialog("Don't worry", "All emitter positions/rotations are up to date.", "OK");
        }

        [MenuItem("AudioStudio/Tools/Implementation Backup  &F9")]
        public static void ImplementationBackUp()
        {
            var window = EditorWindow.GetWindow<AsBackupEditor>();
            window.position = new Rect(500, 300, 550, 500);
            window.titleContent = new GUIContent("Implementation BackUp");
        }

        [MenuItem("AudioStudio/Tools/Animation Player &F10")]
        public static void AnimationPlayer()
        {
            var window = EditorWindow.GetWindow<AsAnimationPlayer>();
            window.position = new Rect(500, 300, 320, 400);
            window.titleContent = new GUIContent("Animation Player");
        }
        
        [MenuItem("AudioStudio/Tools/Script Migration &F11")]
        public static void ScriptMigration()
        {
            var window = EditorWindow.GetWindow<AsScriptMigration>();
            window.position = new Rect(500, 300, 690, 360);
            window.titleContent = new GUIContent("Script Migration");
        }
        
        [MenuItem("AudioStudio/Tools/DLL Migration")]
        public static void DllMigration()
        {
            var sourcePath = EditorUtility.OpenFolderPanel("Root Folder","", "");
            try
            {
                CopyDllFile(sourcePath, "AudioStudio", true, false);
                CopyDllFile(sourcePath, "AudioStudio_Editor", true, false);
                CopyDllFile(sourcePath, "AudioStudio_Deployment", false, true);
                CopyDllFile(sourcePath, "AudioStudio_Deployment", true, true);
            }
#pragma warning disable 168
            catch(FileNotFoundException e)
#pragma warning restore 168
            {
                EditorUtility.DisplayDialog("Error", "Dll file not found!", "OK");
            }
        }
        
        private static void CopyDllFile(string sourcePath, string dllName, bool debugBuild, bool inSubFolder)
        {
            var subFolder = debugBuild ? "Debug" : "Release";
            var dllSource = AsScriptingHelper.CombinePath(sourcePath, dllName, "bin/" + subFolder, dllName + ".dll");
            var dllTarget = inSubFolder 
                ? AsScriptingHelper.CombinePath(WwisePathSettings.Instance.WwisePluginsPathFull, subFolder, dllName + ".dll") 
                : AsScriptingHelper.CombinePath(WwisePathSettings.Instance.WwisePluginsPathFull, dllName + ".dll");
            AsScriptingHelper.CheckoutLockedFile(dllTarget);
            File.Copy(dllSource, dllTarget, true);
            if (debugBuild)
            {
                var pdbSource = dllSource.Replace(".dll", ".pdb");
                var pdbTarget = dllTarget.Replace(".dll", ".pdb");
                AsScriptingHelper.CheckoutLockedFile(pdbTarget);
                File.Copy(pdbSource, pdbTarget, true);
            }
        }

        [MenuItem("AudioStudio/Tools/Replace by Regex in Text File")]
        public static void RegexReplacer()
        {
            var window = EditorWindow.GetWindow<RegexResultReplacer>();
            window.position = new Rect(500, 300, 300, 150);
            window.titleContent = new GUIContent("Regex Replacer");
        }

        [MenuItem("AudioStudio/Tools/Field Upgrade")]
        public static void FieldUpgrade()
        {
            var window = EditorWindow.GetWindow<SerializedFieldUpdater>();
            window.position = new Rect(500, 300, 300, 150);
            window.titleContent = new GUIContent("Filed Upgrade");
        }
        
        [MenuItem("AudioStudio/Tools/Script Reference Update")]
        public static void ScriptReferenceUpdate()
        {
            var window = EditorWindow.GetWindow<ScriptReferenceGuidUpdater>();
            window.position = new Rect(500, 300, 400, 100);
            window.titleContent = new GUIContent("Script Reference Update");
        }

        [MenuItem("AudioStudio/Tools/Remove Missing and Duplicate Components")]
        public static void RemoveMissingDuplicate()
        {
            var window = EditorWindow.GetWindow<MissingDuplicateComponentsRemover>();
            window.position = new Rect(500, 300, 500, 120);
            window.titleContent = new GUIContent("Remove");
        }

        [MenuItem("AudioStudio/Tools/Search Linked Components")]
        private static void SearchLinkedComponents()
        {
            var window = EditorWindow.GetWindow<ComponentPairsManipulator>();
            window.position = new Rect(500, 300, 500, 500);
        }

        [MenuItem("AudioStudio/Tools/Character Component Sync")]
        public static void CharacterComponentSync()
        {
            var window = EditorWindow.GetWindow<PrefabComponentsSynchronizer>();
            window.position = new Rect(500, 300, 300, 100);
            window.titleContent = new GUIContent("Character Component Sync");
        }
        //-----------------------------------------------------------------------------------------
        #endregion

        #region VersionControl
#if UNITY_EDITOR_WIN
        [MenuItem("AudioStudio/SVN/Update Game Project")]
        public static void SvnUpdateProject()
        {            
            var arguments = string.Format("/command:update /path:\"{0}\"", Directory.GetParent(Application.dataPath));
            AsScriptingHelper.RunCommand("TortoiseProc.exe", arguments);                      
        }
        
        [MenuItem("AudioStudio/SVN/Revert Game Project")]
        public static void SvnRevertProject()
        {            
            var arguments = string.Format("/command:revert /path:\"{0}\"", Directory.GetParent(Application.dataPath));
            AsScriptingHelper.RunCommand("TortoiseProc.exe", arguments);                      
        }
        
        [MenuItem("AudioStudio/SVN/Submit SoundBanks")]
        public static void SvnSubmitSoundBank()
        {
            var path = WwisePathSettings.Instance.EditorBankLoadPathFull;
            AsScriptingHelper.RunCommand("TortoiseProc.exe", string.Format("/command:add /path:\"{0}\"", path));      
            AsScriptingHelper.RunCommand("TortoiseProc.exe", string.Format("/command:commit /path:\"{0}\" /logmsg \"{1}\"", path, SubmitBankDescription));
        }
                
        [MenuItem("AudioStudio/SVN/Submit AudioStudio &F12")]
        public static void SvnSubmitAudioStudio()
        {
            var path = WwisePathSettings.AudioStudioLibraryPathFull;
            AsScriptingHelper.RunCommand("TortoiseProc.exe", string.Format("/command:add /path:\"{0}\"", path));      
            AsScriptingHelper.RunCommand("TortoiseProc.exe", string.Format("/command:commit /path:\"{0}\" /logmsg \"{1}\"", path, SubmitAudioStudioDescription));
        }                
        
        [MenuItem("AudioStudio/SVN/Add Android and iOS Plugin files")]
        public static void SvnAddPlugins()
        {
            var path = WwisePathSettings.Instance.WwisePluginsPathFull;
            AddToSvn(path, "so");
            AddToSvn(path, "a");
            AddToSvn(path, "dll");
            AsScriptingHelper.RunCommand("TortoiseProc.exe", string.Format("/command:commit /path:\"{0}\" /logmsg \"{1}\"", path, SubmitAudioStudioDescription));
        }

        private static void AddToSvn(string pluginPath, string suffix)
        {
            string[] files = Directory.GetFiles(pluginPath, "*." + suffix, SearchOption.AllDirectories);
            var title = "SvnCommit";
            EditorUtility.DisplayCancelableProgressBar(title, "wait...", 0);
            for (var i = 0; i < files.Length; i++)
            {
                var fullPath = Path.GetFullPath(files[i]);
                if (EditorUtility.DisplayCancelableProgressBar(title, fullPath, (i + 1) * 1.0f / files.Length))
                {
                    break;
                }

                var arguments = string.Format("/command:add /path:\"{0}\" /closeonend", fullPath);
                AsScriptingHelper.RunCommand("TortoiseProc.exe", arguments);
            }
            EditorUtility.ClearProgressBar();
        }
#endif

        [MenuItem("AudioStudio/Perforce/Update Game Project")]
        public static void P4UpdateProject()
        {                
            GetWorkSpaceName();
            Process.Start("p4", string.Format("-p {0} sync {1}/...#head", PerforcePort, Directory.GetParent(Application.dataPath)));
        }
                
        [MenuItem("AudioStudio/Perforce/Submit AudioStudio Library")]
        public static void P4SubmitAudioStudio()
        {
            GetWorkSpaceName();
            var path = WwisePathSettings.AudioStudioLibraryPathFull;
            var command = string.Format("-p {0} submit -f revertunchanged -d \"{1}\" {2}/...", PerforcePort, SubmitAudioStudioDescription, path);
            Process.Start("p4", command);            
        }

        [MenuItem("AudioStudio/Perforce/Checkout SoundBanks")]
        public static void P4CheckoutBanks()
        {                
            GetWorkSpaceName();
            var path = WwisePathSettings.Instance.EditorBankLoadPathFull;   
            Process.Start("p4", string.Format("-p {0} edit {1}/...", PerforcePort, path));
        }
                
        [MenuItem("AudioStudio/Perforce/Submit SoundBanks")]
        public static void P4SubmitSoundBanks()
        {           
            GetWorkSpaceName();
            var path = WwisePathSettings.Instance.EditorBankLoadPathFull;          
            Process.Start("p4", string.Format("-p {0} submit -f revertunchanged -d \"{1}\" {2}/...", PerforcePort, SubmitBankDescription, path));
        }
                
        private static void GetWorkSpaceName()
        {
            var task = Provider.UpdateSettings();
            task.Wait();
            var workSpaceName = task.messages[87].message.Split('"')[1];
            Process.Start("p4", string.Format("set p4client={0}", workSpaceName));
        }
        #endregion

    }
}