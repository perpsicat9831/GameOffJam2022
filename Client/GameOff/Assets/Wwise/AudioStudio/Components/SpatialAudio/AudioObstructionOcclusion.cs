using AudioStudio.Components;
using UnityEngine;
using System.Collections.Generic;
using AudioStudio;

namespace AudioStudio.Components
{
	/// <summary>
	/// Base class for any Wwise component that uses obstruction/occlusion mechanics
	/// </summary>
	public abstract class AudioObstructionOcclusion : MonoBehaviour
	{
		private readonly List<AudioListener3D> _listenersToRemove = new List<AudioListener3D>();
		private readonly Dictionary<AudioListener3D, ObstructionOcclusionValue> _obstructionOcclusionValues = 
			new Dictionary<AudioListener3D, ObstructionOcclusionValue>();

		[Tooltip("Fade time in seconds")]
		// fade time when obstruction/occlusion activates/deactivates
		public float FadeTime = 0.5f;

		[Tooltip("Layers of obstructers/occluders")]
		public LayerMask LayerMask = -1;

		[Tooltip("Maximum distance to perform the obstruction/occlusion. Negative values mean infinite")]
		public float MaxDistance = -1.0f;

		[Tooltip("The number of seconds between ray casts")]
		public float UpdateInterval = 1;

		private float _updateTimeStamp;

		protected void InitIntervalsAndFadeRates()
		{
			_updateTimeStamp = Random.Range(0.0f, UpdateInterval);
		}

		protected void UpdateObstructionOcclusionValues(List<AudioListener3D> listenerList)
		{
			// add new listeners
			foreach (var listener in listenerList)
			{
				if (!_obstructionOcclusionValues.ContainsKey(listener))
					_obstructionOcclusionValues.Add(listener, new ObstructionOcclusionValue());
			}

			// remove listeners
			foreach (var obsOccPair in _obstructionOcclusionValues)
			{
				if (!listenerList.Contains(obsOccPair.Key))
					_listenersToRemove.Add(obsOccPair.Key);
			}

			foreach (var listener in _listenersToRemove)
				_obstructionOcclusionValues.Remove(listener);
		}

		protected void UpdateObstructionOcclusionValues(AudioListener3D listener)
		{
			if (!listener)
				return;

			// add new listeners
			if (!_obstructionOcclusionValues.ContainsKey(listener))
				_obstructionOcclusionValues.Add(listener, new ObstructionOcclusionValue());

			// remove listeners
			foreach (var obsOccPair in _obstructionOcclusionValues)
			{
				if (listener != obsOccPair.Key)
					_listenersToRemove.Add(obsOccPair.Key);
			}

			foreach (var t in _listenersToRemove)
				_obstructionOcclusionValues.Remove(t);
		}

		private void CastRays()
		{
			// time has passed, new rays should be cast
			if (_updateTimeStamp > UpdateInterval)
			{
				// reset time stamp
				_updateTimeStamp -= UpdateInterval;
				foreach (var pair in _obstructionOcclusionValues)
				{
					var positionDifference = pair.Key.transform.position - transform.position;
					var distance = positionDifference.magnitude;
					if (MaxDistance > 0 && distance > MaxDistance)
						pair.Value.TargetValue = pair.Value.CurrentValue;
					else
						// if ray hits anything in layer mask, activate the obstruction/occlusion, or deactivate them
						pair.Value.TargetValue = Physics.Raycast(transform.position, positionDifference/distance, distance, LayerMask.value) ? 1.0f : 0.0f;
				}
			}

			// increment time stamp
			_updateTimeStamp += Time.deltaTime;
		}

		protected abstract void UpdateObstructionOcclusionValuesForListeners();

		protected abstract void SetObstructionOcclusion(KeyValuePair<AudioListener3D, ObstructionOcclusionValue> obsOccPair);

		private void Update()
		{
			UpdateObstructionOcclusionValuesForListeners();
			CastRays();
			foreach (var obsOccPair in _obstructionOcclusionValues)
			{
				if (obsOccPair.Value.Update(FadeTime))
					SetObstructionOcclusion(obsOccPair);
			}
		}
	}
}