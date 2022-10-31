using UnityEditor;
using UnityEngine;

namespace AK.Wwise.Editor
{
	[CustomPropertyDrawer(typeof(AuxBusExt))]
	public class AudioAuxBusExtDrawer : AudioBaseTypeDrawer
	{				
		protected override WwiseObjectType WwiseObjectType { get { return WwiseObjectType.AuxBus; } }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			base.OnGUI(position, property, label);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(property.FindPropertyRelative("SendAmount"));
		}
	}
}
