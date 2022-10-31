using AudioStudio;
using AudioStudio.Editor;
using AudioStudio.Tools;
using UnityEditor;
using UnityEngine;

namespace AK.Wwise.Editor
{
	[CustomPropertyDrawer(typeof(BankExt))]
	public class BankExtDrawer : AudioBankDrawer
	{

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			base.OnGUI(position, property, label);
			DrawAuditionButtons();
			EditorGUILayout.EndHorizontal();

			var unloadOnDisable = property.FindPropertyRelative("UnloadOnDisable");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("UnloadOnDisable", GUILayout.Width(100));
			EditorGUILayout.PropertyField(unloadOnDisable, GUIContent.none, GUILayout.Width(20));
			if (unloadOnDisable.boolValue)
			{
				EditorGUILayout.LabelField("UseCounter", GUILayout.Width(75));
				EditorGUILayout.PropertyField(property.FindPropertyRelative("UseCounter"), GUIContent.none, GUILayout.Width(10));
			}
			EditorGUILayout.EndHorizontal();

			AsGuiDrawer.DrawList(property.FindPropertyRelative("LoadFinishEvents"), "Play On Load Finish:", WwiseObjectType.Event);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(" ");
		}
	}
	
	[CustomPropertyDrawer(typeof(AudioBank))]
	public class AudioBankDrawer : AudioBaseTypeDrawer
	{		
		protected override WwiseObjectType WwiseObjectType { get { return WwiseObjectType.Soundbank; } }
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			base.OnGUI(position, property, label);
			EditorGUI.BeginProperty(position, label, property);
			DrawAuditionButtons();
			EditorGUI.EndProperty();
		}

		protected void DrawAuditionButtons()
		{
			var bankName = GetObjectName(WwiseObjectReference);
			if (!Application.isPlaying || string.IsNullOrEmpty(bankName)) return;
			GUI.contentColor = Color.green;
			if (GUILayout.Button("▶", EditorStyles.miniButtonLeft, GUILayout.Width(20)))
				AudioManager.LoadBank(bankName, null, null, AudioTriggerSource.InspectorAudition);
			GUI.contentColor = Color.red;
			if (GUILayout.Button("■", EditorStyles.miniButtonRight, GUILayout.Width(20)))
				AudioManager.UnloadBank(bankName, false, null, null, AudioTriggerSource.InspectorAudition);
			GUI.contentColor = Color.white;	
		}
	}
}
