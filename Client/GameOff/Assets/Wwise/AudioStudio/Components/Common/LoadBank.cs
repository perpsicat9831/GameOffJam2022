using System.Linq;
using AK.Wwise;
using AudioStudio.Tools;
using UnityEngine;

namespace AudioStudio.Components
{    
     /// <summary>
     /// Load and unload SoundBanks dynamically in game.
     /// </summary>
    [AddComponentMenu("AudioStudio/LoadBank")]
    [DisallowMultipleComponent]
    public class LoadBank : AsTriggerHandler
    {
        public BankExt[] Banks = new BankExt[0];

        public override void Activate(GameObject source = null)
        {
            foreach (var bank in Banks)
            {
                bank.Load(source, AudioTriggerSource.LoadBank);                                        
            }
        }

        public override void Deactivate(GameObject source = null)
        {
            foreach (var bank in Banks)
            {
                if (bank.UnloadOnDisable)
                    bank.Unload(source, AudioTriggerSource.LoadBank);                           
            }
        }

        public override bool IsValid()
        {            
            return Banks.Any(s => s.IsValid());
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
                Banks = new []{new BankExt()};
                Banks[0].SetupReference(reference.ObjectName, reference.Guid);
            }
        }
#endif
    }
}
