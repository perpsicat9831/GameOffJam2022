using System;
using AudioStudio;
using AudioStudio.Tools;
using UnityEngine;

namespace AK.Wwise
{
	/// <summary>
	/// Extension of State with reset on disable feature.
	/// </summary>
	[Serializable]
	public class StateExt : ASState
	{
		public bool ResetOnDisable = true;

		public override bool Equals(object obj)
		{
			var other = obj as StateExt;
			if (other != null)
				return base.Equals(obj) && other.ResetOnDisable == ResetOnDisable;
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public void Reset(GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
		{
			if (!IsValid()) return;
			AudioManager.ResetLastState(GroupName, source, trigger);    
		}
	}

	[Serializable]
	public class ASState : State
	{
		public string GroupName
		{
			get { return GroupWwiseObjectReference.ObjectName; }
		}

		public string ChildName
		{
			get { return ObjectReference.ObjectName; }
		}

		public void SetValue(GameObject source = null, AudioTriggerSource trigger = AudioTriggerSource.Code)
		{
			if (!IsValid()) return;
			AudioManager.SetState(GroupName, ChildName, source, trigger);        
		}

		public override bool Equals(object obj)
		{
			var other = obj as ASState;
			if (other != null)
				return ObjectReference == other.ObjectReference && GroupWwiseObjectReference == other.GroupWwiseObjectReference;
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}