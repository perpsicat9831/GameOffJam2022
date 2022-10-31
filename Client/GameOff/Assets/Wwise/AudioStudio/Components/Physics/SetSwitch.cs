using System.Linq;
using UnityEngine;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{    
    /// <summary>
    /// Set Wwise switches from a game object.
    /// </summary>
    [AddComponentMenu("AudioStudio/SetSwitch")]
    [DisallowMultipleComponent]
    public class SetSwitch : AsTriggerHandler
    {
        public SwitchEx[] OnSwitches = new SwitchEx[0];
        public SwitchEx[] OffSwitches = new SwitchEx[0];
        
        public override void Activate(GameObject source)
        {
            SetSwitches(OnSwitches, source);
        }

        public override void Deactivate(GameObject source)
        {
            SetSwitches(OffSwitches, source);
        }
        
        private void SetSwitches(SwitchEx[] switches, GameObject go)
        {            
            foreach (var swc in switches)
            {
                swc.SetValue(go, AudioTriggerSource.SetSwitch);
            }
        }
        
        public override bool IsValid()
        {            
            return OnSwitches.Any(s => s.IsValid()) || OffSwitches.Any(s => s.IsValid());
        }
        
#if UNITY_EDITOR
        private void Reset()
        {
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
                return;

            var reference = AkWwiseTypes.DragAndDropObjectReference;
            if (reference)
            {
                GUIUtility.hotControl = 0;
                OnSwitches = new []{new SwitchEx()};
                OnSwitches[0].SetupReference(reference.ObjectName, reference.Guid);                
            }
        }
#endif
    }
}
