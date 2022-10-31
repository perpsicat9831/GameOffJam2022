using AK.Wwise;
using AudioStudio.Tools;
using System;
using UnityEngine;

namespace AudioStudio
{
    public static class AudioStudioWrapper
    {
        #region Sound
        public static uint PlaySound(string eventName, GameObject gameObj)
        {
            return AkSoundEngine.PostEvent(eventName, gameObj);
        }

        public static uint PlaySound(string eventName, GameObject gameObj, uint flags, AkCallbackManager.EventCallback callback, object in_pCookie)
        {
            return AkSoundEngine.PostEvent(eventName, gameObj, flags, callback, in_pCookie);
        }

        public static AKRESULT ExecuteActionOnEvent(string eventName, AkActionOnEventType aoeType, GameObject gameObj, int fadeTime, AkCurveInterpolation cureve = AkCurveInterpolation.AkCurveInterpolation_Linear)
        {
            return AkSoundEngine.ExecuteActionOnEvent(eventName, aoeType, gameObj, fadeTime, cureve);
        }

        public static AKRESULT SeekOnEvent(string eventName, GameObject gameObj, int in_pos)
        {
            return AkSoundEngine.SeekOnEvent(eventName, gameObj, in_pos);
        }
        #endregion

        #region State
        public static AKRESULT SetState(string stateGroup, string state)
        {
            return AkSoundEngine.SetState(stateGroup, state);
        }

        public static AKRESULT SetState(uint in_stateGroup, uint in_state)
        {
            return AkSoundEngine.SetState(in_stateGroup, in_state);
        }

        public static AKRESULT GetState(string stateGroup, out uint currentId)
        {
            return AkSoundEngine.GetState(stateGroup, out currentId);
        }
        #endregion

        #region Switch
        public static AKRESULT SetSwitch(string switchGroup, string switchName, GameObject gameObj)
        {
            return AkSoundEngine.SetSwitch(switchGroup, switchName, gameObj);
        }
        #endregion

        #region RTPC
        public static AKRESULT SetRTPCValue(string parameterName, float value)
        {
            return AkSoundEngine.SetRTPCValue(parameterName, value);
        }

        public static AKRESULT SetRTPCValue(string parameterName, float value, GameObject gameObj)
        {
            return AkSoundEngine.SetRTPCValue(parameterName, value, gameObj);
        }
        #endregion

        #region Trigger
        public static AKRESULT PostTrigger(string triggerName, GameObject gameObj)
        {
            return AkSoundEngine.PostTrigger(triggerName, gameObj);
        }
        #endregion

        #region Common
        public static ulong GetAkGameObjectID(UnityEngine.GameObject gameObj)
        {
            return AkSoundEngine.GetAkGameObjectID(gameObj);
        }

        public static AKRESULT Suspend()
        {
            return AkSoundEngine.Suspend();
        }

        public static AKRESULT WakeupFromSuspend()
        {
            return AkSoundEngine.WakeupFromSuspend();
        }

        public static void MuteBackgroundMusic(bool bMute)
        {
            AkSoundEngine.MuteBackgroundMusic(bMute);
        }

        public static AKRESULT SetCurrentLanguage(string lan)
        {
            return AkSoundEngine.SetCurrentLanguage(lan);
        }

        public static void StopAll(GameObject gameObj)
        {
            AkSoundEngine.StopAll(gameObj);
        }

        public static uint GetIDFromString(string name)
        {
            return AkSoundEngine.GetIDFromString(name);
        }

        public static AKRESULT RegisterGameObj(GameObject gameObj, string name = null)
        {
            if (string.IsNullOrEmpty(name))
                return AkSoundEngine.RegisterGameObj(gameObj);

            return AkSoundEngine.RegisterGameObj(gameObj, name);
        }

        public static AKRESULT UnregisterGameObj(GameObject gameObj)
        {
            return AkSoundEngine.UnregisterGameObj(gameObj);
        }

        public static AKRESULT AddDefaultListener(GameObject gameObj)
        {
            return AkSoundEngine.AddDefaultListener(gameObj);
        }

        public static AKRESULT RemoveDefaultListener(GameObject gameObj)
        {
            return AkSoundEngine.RemoveDefaultListener(gameObj);
        }

        public static AKRESULT RemoveListener(GameObject in_emitterGameObj, GameObject in_listenerGameObj)
        {
            return AkSoundEngine.RemoveListener(in_emitterGameObj, in_listenerGameObj);
        }

        public static AKRESULT AddListener(GameObject in_emitterGameObj, GameObject in_listenerGameObj)
        {
            return AkSoundEngine.AddListener(in_emitterGameObj, in_listenerGameObj);
        }

        public static AKRESULT SetListeners(GameObject in_emitterGameObj, ulong[] in_pListenerGameObjs, uint in_uNumListeners)
        {
            return AkSoundEngine.SetListeners(in_emitterGameObj, in_pListenerGameObjs, in_uNumListeners);
        }

        public static AKRESULT SetGameObjectOutputBusVolume(GameObject in_emitterObjID, GameObject in_listenerObjID, float in_fControlValue)
        {
            return AkSoundEngine.SetGameObjectOutputBusVolume(in_emitterObjID, in_listenerObjID, in_fControlValue);
        }

        public static void TerminateSoundEngine()
        {
            AkSoundEngineController.Instance.Terminate();
        }

        public static uint GetMajorMinorVersion()
        {
            return AkSoundEngine.GetMajorMinorVersion();
        }

        public static uint GetSubminorBuildVersion()
        {
            return AkSoundEngine.GetSubminorBuildVersion();
        }
        #endregion

        #region Bank
        public static AKRESULT LoadBank(string bankName, AkCallbackManager.BankCallback in_pfnBankCallback, object in_pCookie, out uint out_bankID)
        {
            return AkSoundEngine.LoadBank(bankName, in_pfnBankCallback, in_pCookie, out out_bankID);
        }

        public static AKRESULT LoadBank(string in_pszString, out uint out_bankID)
        {
            return AkSoundEngine.LoadBank(in_pszString, out out_bankID);
        }

        public static AKRESULT UnloadBank(string in_pszString, global::System.IntPtr in_pInMemoryBankPtr)
        {
            return AkSoundEngine.UnloadBank(in_pszString, in_pInMemoryBankPtr);
        }

        public static AKRESULT UnloadBank(string in_pszString, global::System.IntPtr in_pInMemoryBankPtr, AkCallbackManager.BankCallback in_pfnBankCallback, object in_pCookie)
        {
            return AkSoundEngine.UnloadBank(in_pszString, in_pInMemoryBankPtr, in_pfnBankCallback, in_pCookie);
        }
        #endregion

        #region FilePackage
        public static AKRESULT LoadFilePackage(string in_pszFilePackageName, out uint out_uPackageID)
        {
            return AkSoundEngine.LoadFilePackage(in_pszFilePackageName, out out_uPackageID);
        }

        public static AKRESULT UnloadFilePackage(uint in_uPackageID)
        {
            return AkSoundEngine.UnloadFilePackage(in_uPackageID);
        }

        public static AKRESULT UnloadAllFilePackages()
        {
            return AkSoundEngine.UnloadAllFilePackages();
        }
        #endregion

        #region SpatialAudio
        public static AKRESULT SetRoom(ulong in_RoomID, AkRoomParams in_roomParams, ulong GeometryID, string in_pName)
        {
            return AkSoundEngine.SetRoom(in_RoomID, in_roomParams, GeometryID, in_pName);
        }

        public static AKRESULT RemoveRoom(ulong in_RoomID)
        {
            return AkSoundEngine.RemoveRoom(in_RoomID);
        }

        public static AKRESULT SetObjectObstructionAndOcclusion(UnityEngine.GameObject in_EmitterID, UnityEngine.GameObject in_ListenerID, float in_fObstructionLevel, float in_fOcclusionLevel)
        {
            return AkSoundEngine.SetObjectObstructionAndOcclusion(in_EmitterID, in_ListenerID, in_fObstructionLevel, in_fOcclusionLevel);
        }

        public static AKRESULT SetGameObjectAuxSendValues(UnityEngine.GameObject in_gameObjectID, AkAuxSendArray in_aAuxSendValues, uint in_uNumSendValues)
        {
            return AkSoundEngine.SetGameObjectAuxSendValues(in_gameObjectID, in_aAuxSendValues, in_uNumSendValues);
        }

        public static AKRESULT SetRoomPortal(ulong in_PortalID, ulong FrontRoom, ulong BackRoom, AkTransform Transform, AkExtent Extent, bool bEnabled, string in_name)
        {
            return AkSoundEngine.SetRoomPortal(in_PortalID, FrontRoom, BackRoom, Transform, Extent, bEnabled, in_name);
        }

        public static AKRESULT SetPortalObstructionAndOcclusion(ulong in_PortalID, float in_fObstruction, float in_fOcclusion)
        {
            return AkSoundEngine.SetPortalObstructionAndOcclusion(in_PortalID, in_fObstruction, in_fOcclusion);
        }

        public static AKRESULT SetGeometry(ulong in_GeomSetID, AkTriangleArray Triangles, uint NumTriangles, UnityEngine.Vector3[] Vertices, uint NumVertices, AkAcousticSurfaceArray Surfaces, uint NumSurfaces, ulong RoomID, bool EnableDiffraction, bool EnableDiffractionOnBoundaryEdges, bool EnableTriangles)
        {
            return AkSoundEngine.SetGeometry(in_GeomSetID, Triangles, NumTriangles, Vertices, NumVertices, Surfaces, NumSurfaces, RoomID, EnableDiffraction, EnableDiffractionOnBoundaryEdges, EnableTriangles);
        }
        public static AKRESULT RemoveGeometry(ulong in_SetID)
        {
            return AkSoundEngine.RemoveGeometry(in_SetID);
        }
        public static AKRESULT RegisterSpatialAudioListener(UnityEngine.GameObject in_gameObjectID)
        {
            return AkSoundEngine.RegisterSpatialAudioListener(in_gameObjectID);
        }

        public static AKRESULT UnregisterSpatialAudioListener(UnityEngine.GameObject in_gameObjectID)
        {
            return AkSoundEngine.UnregisterSpatialAudioListener(in_gameObjectID);
        }

        public static AKRESULT SetGameObjectInRoom(GameObject gameObj, ulong highestPriorityRoomId)
        {
            return AkSoundEngine.SetGameObjectInRoom(gameObj, highestPriorityRoomId);
        }

        public static AKRESULT SetObjectPosition(GameObject gameObj, Vector3 Position, Vector3 forward, Vector3 up)
        {
            return AkSoundEngine.SetObjectPosition(gameObj, Position, forward, up);
        }

        public static AKRESULT SetObjectPosition(GameObject gameObj, Transform transform)
        {
            return AkSoundEngine.SetObjectPosition(gameObj, transform);
        }

        public static AKRESULT SetMultiplePositions(GameObject gameObj, AkPositionArray positionArray, ushort arraySize, AkMultiPositionType positionType)
        {
            return AkSoundEngine.SetMultiplePositions(gameObj, positionArray, arraySize, positionType);
        }
        #endregion
    }
}