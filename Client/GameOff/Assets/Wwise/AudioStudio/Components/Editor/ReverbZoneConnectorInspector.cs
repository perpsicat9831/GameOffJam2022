using AudioStudio.Components;
using AudioStudio.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ReverbZoneConnector)), CanEditMultipleObjects]
public class ReverbZoneConnectorInspector : AsComponentInspector
{
	private readonly int[] _selectedIndex = new int[2];
	private ReverbZoneConnector _component;

	private void OnEnable()
	{
		_component = target as ReverbZoneConnector;
		CheckDataBackedUp(_component);
		FindOverlappingEnvironments();
		for (var i = 0; i < 2; i++)
		{
			var index = _component.envList[i].list.IndexOf(_component.Environments[i]);
			_selectedIndex[i] = index == -1 ? 0 : index;
		}
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Connected Reverb Zones:", EditorStyles.boldLabel);
		using (new EditorGUILayout.VerticalScope(GUI.skin.box))
		{
			for (var i = 0; i < 2; i++)
			{
				var labels = new string[_component.envList[i].list.Count];

				for (var j = 0; j < labels.Length; j++)
				{
					if (_component.envList[i].list[j] != null)
					{
						labels[j] = j + 1 + ". " + _component.envList[i].list[j].AuxBus.Name + " (" +
						            _component.envList[i].list[j].name + ")";
					}
					else
						_component.envList[i].list.RemoveAt(j);
				}

				_selectedIndex[i] = EditorGUILayout.Popup("Environment #" + (i + 1), _selectedIndex[i], labels);

				_component.Environments[i] = _selectedIndex[i] < 0 || _selectedIndex[i] >= _component.envList[i].list.Count
					? null
					: _component.envList[i].list[_selectedIndex[i]];
			}
		}

		GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

		using (new EditorGUILayout.VerticalScope("box"))
		{
			string[] axisLabels = { "X", "Y", "Z" };

			var index = 0;
			for (var i = 0; i < 3; i++)
			{
				if (_component.Axis[i] == 1)
				{
					index = i;
					break;
				}
			}

			index = EditorGUILayout.Popup("Axis", index, axisLabels);

			if (_component.Axis[index] != 1)
			{
				_component.Axis.Set(0, 0, 0);
				_component.envList = new[] { new ReverbZoneConnector.EnvListWrapper(), new ReverbZoneConnector.EnvListWrapper() };
				_component.Axis[index] = 1;

				//We move and replace the game object to trigger the OnTriggerStay function
				FindOverlappingEnvironments();
			}
		}
		
		AsGuiDrawer.CheckLinkedComponent<Rigidbody>(_component);
		ShowButtons(_component);
	}

	private void FindOverlappingEnvironments()
	{
		var myCollider = _component.gameObject.GetComponent<Collider>();
		if (myCollider == null)
			return;

		var environments = FindObjectsOfType<ReverbZone>();
		foreach (var environment in environments)
		{
			var otherCollider = environment.gameObject.GetComponent<Collider>();
			if (otherCollider == null)
				continue;

			if (myCollider.bounds.Intersects(otherCollider.bounds))
			{
				//if index == 0 => the environment is on the negative side of the portal(opposite to the direction of the chosen axis)
				//if index == 1 => the environment is on the positive side of the portal(same direction as the chosen axis) 
				var index = Vector3.Dot(_component.transform.rotation * _component.Axis, environment.transform.position - _component.transform.position) >= 0 ? 1 : 0;
				if (!_component.envList[index].list.Contains(environment))
				{
					_component.envList[index].list.Add(environment);
					_component.envList[++index % 2].list.Remove(environment);
				}
			}
			else
			{
				for (var i = 0; i < 2; i++)
				{
					_component.envList[i].list.Remove(environment);
				}
			}
		}
	}
}