using System;
using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;

namespace AudioStudio.Tools
{
    [Serializable]
    public class AttenuationRadius
    {
        public string id;
        public float radius;
    }
    [Serializable]
    public class AttenuationData
    {
        public AttenuationRadius maxRadiusAttenuation;
    }

    internal static class AsWaapiTools
    {
        private static float radius = 0.0f;
        #region Connection
        // check if Wwise application is opened
        private static bool IsWaapiConnected()
        {
            return AkWaapiUtilities.IsConnected();
        }

        #endregion
        
        #region Attenuation
        private static void RadiusCallback(List<AttenuationData> info)
        {
            foreach (var maxRadiusAttenuation in info)
            {
                radius = maxRadiusAttenuation.maxRadiusAttenuation.radius;
            }
        }
        internal static float GetMaxAttenuationRadius(AudioEvent evt)
        {
            if (!IsWaapiConnected())
            {
                WwisePathSettings.Instance.GizmosSphereColor = GizmosColor.Disabled;
                return -1;
            }

            
            //string result;
            var query = AsScriptingHelper.ToJson(new
            {
                @from = new
                {
                    id = new[] { evt.ObjectReference.Guid }
                }
            });

            //AkWaapiClient.Call("ak.wwise.core.object.get", query, options, out result);
            //var returnArgs = AsScriptingHelper.FromJson(result, "return");
            //if (returnArgs.Length < 10) return 0f;
            //var attenuation = AsScriptingHelper.FromJson(returnArgs, "audioSource:maxRadiusAttenuation");
            //var radius = AsScriptingHelper.FromJson(attenuation, "radius");
            //return AsScriptingHelper.StringToFloat(radius);
            var args = new WaqlArgs($"from object \"{{{evt.ObjectReference.Guid.ToString()}}}\"");
            //AkWaapiUtilities.QueueCommandWithReturnType<returns>("ak.wwise.core.object.get", RadiusCallback, query, options);
            ReturnOptions waapiWwiseObjectOptions = new ReturnOptions(new string[] { "maxRadiusAttenuation" });
            AkWaapiUtilities.QueueCommandWithReturnWwiseObjects<AttenuationData>(args, waapiWwiseObjectOptions, RadiusCallback);

            return radius;
        }
        #endregion
        
        #region Playback
        // Play/stop an AudioEvent from inspector buttons
        internal static void StartTransportPlayback(Guid eventGuid)
        {
            if (!IsWaapiConnected()) return;
            AkWaapiUtilities.TogglePlayEvent(WwiseObjectType.Event, eventGuid);
        }
        
        internal static void StopTransportPlayback()
        {
            if (!IsWaapiConnected()) return;
            AkWaapiUtilities.StopAllTransports();
        }
        #endregion
    }
}