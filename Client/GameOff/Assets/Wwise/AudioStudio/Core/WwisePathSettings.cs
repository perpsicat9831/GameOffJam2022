using System.IO;
using UnityEngine;

namespace AudioStudio 
{
    /// <summary>
    /// Wwise plugin configuration to activate in builds.
    /// </summary>
    public enum ActivePlugin
    {
        Debug,
        Profile,
        Release,
    }

    /// <summary>
    /// Color of the attenuation sphere in scene view.
    /// </summary>
    public enum GizmosColor
    {
        Disabled,
        Red,
        Yellow,
        Green,
        Blue
    }

    [CreateAssetMenu(fileName = "WwisePathSettings", menuName = "AudioStudio/Wwise Path Settings")]
    public partial class WwisePathSettings : ScriptableObject
    {
        private static WwisePathSettings _instance;

        public static WwisePathSettings Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!_instance)
                {
                    var loadPath = "Assets/" + EditorConfigPath + "/WwisePathSettings.asset";
                    _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<WwisePathSettings>(loadPath);
                }
#endif                
                return _instance;
            }
        }

        public ActivePlugin CurrentPlugin = ActivePlugin.Profile;
        public bool LoadBankFromWwiseProjectFolder = true;
        public bool SeparateBankPlatforms;
        public bool CopyBanksWhenBuild = true;
        public bool ExcludeDLCBanks = true;
        public GizmosColor GizmosSphereColor = GizmosColor.Red;
        public bool GizmosIconScaling = true;
        public uint WaapiPort = 8080;
        public string WwiseProjectFilePath;
        
        public string WwiseProjectFilePathFull
        {
            get { return GetFullPath(Application.dataPath, WwiseProjectFilePath); }
        }

        public string WwiseProjectFolderPath
        {
            get { return Path.GetDirectoryName(WwiseProjectFilePathFull); }
        }

        public string WwiseProjectFileName
        {
            get { return Path.GetFileNameWithoutExtension(WwiseProjectFilePath); }
        }
		
        private string SoundBankExportPath
        {
            get { return Path.Combine(WwiseProjectFolderPath, "GeneratedSoundBanks"); }
        }

        [SerializeField] 
        private string _editorBankLoadPath;
        public string AudioPackagePath;
        
        public string AudioPackagePathFull
        {
            get { return GetFullPath(Application.dataPath, AudioPackagePath); }
        }
        
        public static string AudioStudioLibraryPathFull
        {
            get { return Path.Combine(Application.dataPath, AUDIO_STUDIO_LIBRARY_PATH); }
        }
        
        public string WwisePluginsPath = "Wwise/API/Runtime/Plugins";
        public string StartScenePath;
        
#if UNITY_EDITOR
        public string WwisePluginsPathFull
        {
            get { return Path.Combine(Application.dataPath, WwisePluginsPath); }
        }

        public string EditorBankLoadPath
        {
            get
            {
                return LoadBankFromWwiseProjectFolder ? SoundBankExportPath : _editorBankLoadPath;
            }
            set
            {
                _editorBankLoadPath = value;
            }
        }
        
        /// <summary>
        /// Where banks should be loaded in editor.
        /// </summary>
        public string EditorBankLoadPathFull
        {
            get
            {
                var loadPath = GetFullPath(Application.dataPath, EditorBankLoadPath);
                return SeparateBankPlatforms ? Path.Combine(EditorBankLoadPath, GetPlatformName()) : loadPath;
            }
        }

        /// <summary>
        /// Where the xml files and other AudioStudio related config files are at.
        /// </summary>
        public static string EditorConfigPathFull 
        {
            get 
            {
                var path = Path.Combine("Assets", EditorConfigPath);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }
        
        public static string EditorConfigPath
        {
            get { return AUDIO_STUDIO_LIBRARY_PATH + "/Configs/Editor"; }
        }

        public static string WwisePickerImagesPath
        {
            get { return "Assets/" + AUDIO_STUDIO_LIBRARY_PATH + "/Core/Editor/WwiseWindows/TreeViewControl/"; }
        }

        public static string WwiseObjectsPathFull
        {
            get { return Path.Combine(AUDIO_STUDIO_LIBRARY_PATH, "ScriptableObjects"); }
        }
        
        public static string WwiseObjectsPath(WwiseObjectType objectType)
        {
            return Path.Combine(AUDIO_STUDIO_LIBRARY_PATH, "ScriptableObjects/" + objectType);
        }
#endif

        public static string GetPlatformName()
        {
#if UNITY_STANDALONE_WIN || UNITY_WSA
            return "Windows";
#elif UNITY_STANDALONE_OSX
		    return "Mac";
#elif UNITY_STANDALONE_LINUX
		    return "Linux";
#elif UNITY_XBOXONE
		    return "XBoxOne";
#elif UNITY_IOS || UNITY_TVOS
		    return "iOS";
#elif UNITY_ANDROID
		    return "Android";
#elif UNITY_PS4
		    return "PS4";
#elif UNITY_SWITCH
		    return "Switch";
#else
		    return "Undefined platform";
#endif
        }

        /// <summary>
        /// Set the Wwise SoundBanks load path.
        /// </summary>
        public static string SetBasePath()
        {
            var basePathToSet = GetBankLoadPath();
            var initBankFound = true;
#if UNITY_EDITOR || !(UNITY_ANDROID || PLATFORM_LUMIN) // Can't use File.Exists on Android, assume banks are there
            initBankFound = File.Exists(Path.Combine(basePathToSet, "Init.bnk")) || File.Exists(Path.Combine(basePathToSet, "Init.pck"));
#endif

            if (basePathToSet == string.Empty || initBankFound == false)
            {
                Debug.Log("WwiseUnity: Looking for SoundBanks in " + basePathToSet);

#if UNITY_EDITOR
                Debug.LogError("WwiseUnity: Could not locate the SoundBanks. Did you make sure to generate them?");
#else
			    Debug.LogError("WwiseUnity: Could not locate the SoundBanks. Did you make sure to copy them to the StreamingAssets folder?");
#endif
            }
            return basePathToSet;
        }

        /// <summary>
        /// Get SoundBank load path, different in editor and standalone mode.
        /// </summary>
        public static string GetBankLoadPath()
        {
#if UNITY_EDITOR
            var loadPath = Instance.EditorBankLoadPathFull;
#else
            var loadPath = GetStandaloneBankLoadPath();
#endif
            FixPathSeparator(ref loadPath);
            return loadPath;
        }
        
        public static string GetStandaloneBankLoadPath()
        {
            // Get full path of base path
            var fullBasePath = INITIAL_BANK_SUB_FOLDER;

#if UNITY_EDITOR || !UNITY_ANDROID
            fullBasePath = Path.Combine(Application.streamingAssetsPath, fullBasePath);
#endif

#if UNITY_SWITCH
		if (fullBasePath.StartsWith("/"))
			fullBasePath = fullBasePath.Substring(1);
#endif
            FixPathSeparator(ref fullBasePath);
            return fullBasePath;
        }
        
        /// <summary>
        /// Replace path with the correct path separator under current platform.
        /// </summary>
        public static void FixPathSeparator(ref string path)
        {
#if UNITY_WSA
		    var separatorChar = '\\';
#else
            var separatorChar = Path.DirectorySeparatorChar;
#endif
            var badChar = separatorChar == '\\' ? '/' : '\\';

            path = path.Trim().Replace(badChar, separatorChar).TrimStart('\\');

            // Append a trailing slash to play nicely with Wwise
            if (!path.EndsWith(separatorChar.ToString()))
                path += separatorChar;
        }

        /// <summary>
        /// Get the full path from an abbreviated path.
        /// </summary>
        private static string GetFullPath(string BasePath, string RelativePath)
        {
            var wrongSeparator = Path.DirectorySeparatorChar == '/' ? '\\' : '/';
            var path = BasePath;
            if (!string.IsNullOrEmpty(RelativePath))
                path = Path.Combine(BasePath, RelativePath);
            path = Path.GetFullPath(new System.Uri(path).LocalPath);
            return path.Replace(wrongSeparator, Path.DirectorySeparatorChar);
        }
    }
}