using System.Collections.Generic;
using AudioStudio.Tools;

namespace AudioStudio
{
    public static class PackageManager
    {
        private static readonly Dictionary<string, uint> _loadedPackageList = new Dictionary<string, uint>();
        
        /// <summary>
        /// Load a audio package by name.
        /// </summary>
        public static void LoadPackage(string packageName)
        {
            uint id;
            var originalPackageName = packageName;
            // get actual package name with language suffix
            if (packageName.StartsWith("Voice"))
                packageName += "_" + AudioManager.VoiceLanguage;
            var result = AudioStudioWrapper.LoadFilePackage(packageName + ".pck", out id);
            if (result == AKRESULT.AK_Success)
            {
                AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.AudioPackage, AudioTriggerSource.Initialization, AudioAction.Load, originalPackageName);
                _loadedPackageList[originalPackageName] = id;
            }
            else
                AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.AudioPackage, AudioTriggerSource.Initialization, AudioAction.Load, originalPackageName, null, result.ToString());
        }

        /// <summary>
        /// Reload a single audio package by name.
        /// </summary>
        public static void ReloadPackage(string packageName)
        {
            // unload file package only works with uint parameter
            var result = AudioStudioWrapper.UnloadFilePackage(_loadedPackageList[packageName]);
            if (result == AKRESULT.AK_Success) 
                LoadPackage(packageName);
        }
        
        /// <summary>
        /// Reload all audio packages related to voice.
        /// </summary>
        public static void ReloadVoicePackages()
        {
            foreach (var package in _loadedPackageList.Keys)
            {
                if (package.StartsWith("Voice")) 
                    ReloadPackage(package);
            }
        }

        /// <summary>
        /// Reload all audio packages.
        /// </summary>
        public static void ReloadAllPackages()
        {
            AudioStudioWrapper.UnloadAllFilePackages();
            foreach (var package in _loadedPackageList.Keys)
            {
                LoadPackage(package);
            }
        }
    }
}