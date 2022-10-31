using System;
using UnityEditor;
using UnityEngine;

namespace AK.Wwise.Editor
{
	public abstract class AudioBaseTypeDrawer : BaseTypeDrawer
	{
		protected SerializedProperty WwiseObjectReference;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			WwiseObjectReference = property.FindPropertyRelative("WwiseObjectReference");
			base.OnGUI(position, property, label);
		}

		protected virtual string GetObjectName(SerializedProperty wwiseObjectReference)
		{
			var reference = wwiseObjectReference.objectReferenceValue as WwiseObjectReference;
			return reference ? reference.DisplayName : string.Empty;
		}
		
		protected static Guid GetObjectGuid(SerializedProperty wwiseObjectReference)
		{
			var reference = wwiseObjectReference.objectReferenceValue as WwiseObjectReference;
			return reference ? reference.Guid : Guid.Empty;
		}
	}
}
