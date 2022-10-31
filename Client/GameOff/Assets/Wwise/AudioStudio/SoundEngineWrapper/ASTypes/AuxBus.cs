using System;
using AudioStudio;
using UnityEngine;

namespace AK.Wwise
{
	/// <summary>
	/// Extension of AuxBus with send amount parameter.
	/// </summary>
	[Serializable]
	public class AuxBusExt : AuxBus
	{
		[Range(0f, 1f)]
		public float SendAmount = 1f;
		
		public override bool Equals(object obj)
		{
			var other = obj as AuxBusExt;
			if (other != null) 
				return base.Equals(obj) && other.SendAmount == SendAmount;
			return false;
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public void Set()
		{
			if (!IsValid()) return;
			EmitterManager.AddAuxBus(this);
			EmitterManager.RefreshGlobalAuxSends();
		}

		public void Reset()
		{
			if (!IsValid()) return;
			EmitterManager.RemoveAuxBus(this);
			EmitterManager.RefreshGlobalAuxSends();
		}		
	}
}