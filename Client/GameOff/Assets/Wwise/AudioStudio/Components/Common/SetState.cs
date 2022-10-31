using System.Linq;
using UnityEngine;
using AK.Wwise;
using AudioStudio.Tools;

namespace AudioStudio.Components
{    
    /// <summary>
    /// Set global Wwise states from a game object.
    /// </summary>
    [AddComponentMenu("AudioStudio/SetState")]
    [DisallowMultipleComponent]
    public class SetState : AsTriggerHandler
    {        
        public StateExt[] OnStates = new StateExt[0];
        public ASState[] OffStates = new ASState[0];
        
        public override void Activate(GameObject source = null)
        {
            foreach (var state in OnStates)
            {
                state.SetValue(source, AudioTriggerSource.SetState);
            }
        }

        public override void Deactivate(GameObject source = null)
        {
            foreach (var state in OnStates)
            {
                if (state.ResetOnDisable) 
                    state.Reset(source, AudioTriggerSource.SetState);
            }
            foreach (var state in OffStates)
            {
                state.SetValue(source, AudioTriggerSource.SetState);
            }
        }

        public override bool IsValid()
        {            
            return OnStates.Any(s => s.IsValid()) || OffStates.Any(s => s.IsValid());
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
                OnStates = new []{new StateExt()};
                OnStates[0].SetupReference(reference.ObjectName, reference.Guid);                
            }
        }
#endif
    }
}
