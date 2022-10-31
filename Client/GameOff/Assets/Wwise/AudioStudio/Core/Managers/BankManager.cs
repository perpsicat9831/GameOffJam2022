using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio
{
    public static class BankManager
    {
        public static readonly Dictionary<string, int> LoadedBankList = new Dictionary<string, int>();
        // mutex for handling multiple load/unload events at same time
        private static readonly Mutex _mutex = new Mutex();

        #region Load
        internal static void LoadBank(string bankName, Action finishCallback = null, GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (LoadedBankList.ContainsKey(bankName))
            {
                // increment sound bank load counter
                LoadedBankList[bankName]++;
                AsUnityHelper.DebugToProfiler(Severity.Warning, AudioObjectType.SoundBank, trigger, AudioAction.Load, bankName, source, "Sound Bank load counter: " + LoadedBankList[bankName]);
            }
            else
            {
                _mutex.WaitOne();
                uint bankID;
                var result = finishCallback == null ?
                    AudioStudioWrapper.LoadBank(bankName, out bankID) :
                    AudioStudioWrapper.LoadBank(bankName, BankCallback, finishCallback, out bankID);
                ValidateBank(bankName, result, AudioAction.Load, source, trigger);
                _mutex.ReleaseMutex();
            }
        }

        private static void BankCallback(uint in_bankID, IntPtr in_InMemoryBankPtr, AKRESULT in_eLoadResult, object method)
        {
            var callback = (Action)method;
            if (callback != null) callback();  
        }
        #endregion
        
        #region Unload
        internal static void UnloadBank(string bankName, bool useCounter = true, Action finishCallback = null, GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            if (!LoadedBankList.ContainsKey(bankName) || LoadedBankList[bankName] == 0)
            {
                AsUnityHelper.DebugToProfiler(Severity.Warning, AudioObjectType.SoundBank, trigger, AudioAction.Unload, bankName, source, "Sound Bank is already unloaded");
                return;
            }

            if (useCounter && LoadedBankList[bankName] > 1)
            {
                LoadedBankList[bankName]--;
                AsUnityHelper.DebugToProfiler(Severity.Warning, AudioObjectType.SoundBank, trigger, AudioAction.Unload, bankName, source, "Sound Bank unload counter: " + LoadedBankList[bankName]);
            }
            else
            {
                _mutex.WaitOne();
                var result = finishCallback == null ?
                    AudioStudioWrapper.UnloadBank(bankName, IntPtr.Zero) :
                    AudioStudioWrapper.UnloadBank(bankName, IntPtr.Zero, BankCallback, finishCallback);
                ValidateBank(bankName, result, AudioAction.Unload, source, trigger);
                _mutex.ReleaseMutex();
            }
        }
        #endregion

        #region Validation
        private static void ValidateBank(string bankName, AKRESULT result, AudioAction action, GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
        {
            switch (result)
            {
                case AKRESULT.AK_Success:
                    AsUnityHelper.DebugToProfiler(Severity.Notification, AudioObjectType.SoundBank, trigger, action, bankName, source);
                    if (action == AudioAction.Unload)
                        LoadedBankList.Remove(bankName);
                    else
                        LoadedBankList[bankName] = 1;
                    break;
                case AKRESULT.AK_BankAlreadyLoaded:
                    AsUnityHelper.DebugToProfiler(Severity.Warning, AudioObjectType.SoundBank, trigger, action, bankName, source, "Sound Bank is already " + (action == AudioAction.Load? "loaded" : "unloaded"));
                    break;
                default:
                    AsUnityHelper.DebugToProfiler(Severity.Error, AudioObjectType.SoundBank, trigger, action, bankName, source, result.ToString());
                    break;
            }  
        }
        #endregion

        #region Reload
        private static void ReloadBank(string bankName)
        {
            if (!LoadedBankList.ContainsKey(bankName)) return;
            if (LoadedBankList[bankName] > 0)
                AudioStudioWrapper.UnloadBank(bankName, IntPtr.Zero);

            uint bankID;
            var result = AudioStudioWrapper.LoadBank(bankName, out bankID);
            ValidateBank(bankName, result, AudioAction.Load);
        }

        public static void OnHotUpdateFinished(IEnumerable<string> updatedFileList)
        {
            foreach (var filePath in updatedFileList)
            {
                if (filePath.EndsWith(".bnk"))
                {
                    var bankName = Path.GetFileNameWithoutExtension(filePath);
                    ReloadBank(bankName);
                }
            }
        }

        public static void RefreshAllBanks()
        {
            var loadedBanks = LoadedBankList.Keys.ToArray();
            foreach (var bank in loadedBanks)
            {
                ReloadBank(bank);
            }
        }

        public static void RefreshVoiceBanks()
        {
            var bankListTemp = new Dictionary<string, int>(LoadedBankList);
            foreach (var bank in bankListTemp)
            {
                if (!bank.Key.StartsWith("Voice")) continue;
                if (bank.Value > 0)
                    AudioStudioWrapper.UnloadBank(bank.Key, IntPtr.Zero);
            }
#if !UNITY_EDITOR
			PackageManager.ReloadVoicePackages();
#endif
            foreach (var bank in bankListTemp)
            {
                if (!bank.Key.StartsWith("Voice")) continue;
                uint bankID;
                var result = AudioStudioWrapper.LoadBank(bank.Key, out bankID);
                ValidateBank(bank.Key, result, AudioAction.Reload);
            }
        }
        #endregion
    }
}