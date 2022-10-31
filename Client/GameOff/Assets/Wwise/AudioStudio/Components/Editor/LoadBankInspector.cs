using AK.Wwise;
using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(LoadBank)), CanEditMultipleObjects]
    public class LoadBankInspector : AsComponentInspector
    {
        private LoadBank _component;

        private void OnEnable()
        {
            _component = target as LoadBank;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            //ShowSpatialSettings(_component);
            ShowPhysicalSettings(_component, false);
            AsGuiDrawer.DrawList(serializedObject.FindProperty("Banks"), 
                "Banks", WwiseObjectType.Soundbank, AddBank);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }

        private void AddBank(WwiseObjectReference reference)
        {
            var newBank = new BankExt();
            newBank.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.Banks, newBank);
        }
    }
}