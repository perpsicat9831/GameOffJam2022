using UnityEngine;

namespace AudioStudio.Components
{
	[AddComponentMenu("AudioStudio/Reverb Zone Connector")]
	[DisallowMultipleComponent]
	public class ReverbZoneConnector : AsComponent
	{
		public const int MAX_ENVIRONMENTS_PER_PORTAL = 2;

		///The axis used to find the contribution of each environment
		public Vector3 Axis = new Vector3(1, 0, 0);

		///The array is already sorted by position.
		///The first environment is on the negative side of the portal(opposite to the direction of the chosen axis)
		///The second environment is on the positive side of the portal
		public ReverbZone[] Environments = new ReverbZone[MAX_ENVIRONMENTS_PER_PORTAL];

		public float GetAuxSendValueForPosition(Vector3 in_position, int index)
		{
			//total length of the portal in the direction of axis
			var portalLength = Vector3.Dot(Vector3.Scale(GetComponent<BoxCollider>().size, transform.lossyScale), Axis);

			//transform axis to world coordinates 
			var axisWorld = Vector3.Normalize(transform.rotation * Axis);

			//Get distance form left side of the portal(opposite to the direction of axis) to the game object in the direction of axisWorld
			var dist = Vector3.Dot(in_position - (transform.position - portalLength * 0.5f * axisWorld), axisWorld);

			//calculate value of the environment referred by index 
			if (index == 0)
				return (portalLength - dist) * (portalLength - dist) / (portalLength * portalLength);

			return dist * dist / (portalLength * portalLength);
		}


#if UNITY_EDITOR
		/// This enables us to detect intersections between portals and environments in the editor
		[System.Serializable]
		public class EnvListWrapper
		{
			public System.Collections.Generic.List<ReverbZone> list = new System.Collections.Generic.List<ReverbZone>();
		}

		/// Unity can't serialize an array of list so we wrap the list in a serializable class 
		public EnvListWrapper[] envList =
		{
			new EnvListWrapper(), //All environments on the negative side of each portal(opposite to the direction of the chosen axis)
			new EnvListWrapper() //All environments on the positive side of each portal(same direction as the chosen axis)
		};
#endif
	}
}