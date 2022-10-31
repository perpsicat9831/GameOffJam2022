using System.Xml.Linq;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AudioStudio.Tools
{
	public enum XmlAction
	{
		Save,
		Revert,
		Remove
	}

	internal enum ComponentStatus
	{
		AllRemoved,
		ServerOnly,
		LocalOnly,
		UseServer,
		UseLocal,
		Different,
		NoEvent
	}

	internal class ComponentComparisonData
	{
		internal string AssetPath;
		internal XElement ServerData;
		internal XElement LocalData;
		internal ComponentStatus ComponentStatus;
		internal bool selectionToggle = true;
		internal bool showDetail = false;
		internal bool display = true;

		internal string AssetName
		{
			get { return Path.GetFileNameWithoutExtension(AssetPath); }
		}
	}

	internal class AnimationComparisonData: ComponentComparisonData
	{
		internal string ClipName = "";
	}

	internal class TimelineComparisonData : ComponentComparisonData
	{
		internal string ClipName = "";
	}


	internal class AsXmlInfo : EditorWindow
	{
		private XElement _xComponent;
		private static int _lines;
		private static int _maxChar;
		internal static void Init(XElement node)
		{
			var position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			position.y += 10;
			var window = CreateInstance<AsXmlInfo>();
			_lines = 0;
			_maxChar = 20;
			CountWindowSize(node);
			window.ShowAsDropDown(new Rect(position, Vector2.zero),
				new Vector2(_maxChar * 7, _lines * 21));
			window.titleContent = new GUIContent("Original");
			window._xComponent = node;
		}

		private static void CountWindowSize(XElement node)
		{
			if (node.HasAttributes)
			{
				_lines += node.Attributes().Count() + 1;
				foreach (var attribute in node.Attributes())
				{
					var label = "  " + attribute.Name + ": " + attribute.Value;
					_maxChar = Mathf.Max(_maxChar, label.Length);
				}
			}

			foreach (var child in node.Elements())
				CountWindowSize(child);
		}

		private void OnGUI()
		{
			DisplayXml(_xComponent);
		}

		private void DisplayXml(XElement node)
		{
			if (node.HasAttributes)
			{
				EditorGUILayout.LabelField(node.Name + ": ", EditorStyles.boldLabel);
				using (new GUILayout.VerticalScope("box"))
				{
					foreach (var attribute in node.Attributes())
					{
						var label = "  " + attribute.Name + ": " + attribute.Value;
						EditorGUILayout.LabelField(label);
					}
				}
			}

			foreach (var child in node.Elements())
			{
				DisplayXml(child);
			}
		}

	}

	internal class AsPopupInfo : EditorWindow
	{
		string path = "No path";
		static void init(ComponentComparisonData node)
		{
			var xmlData = node.LocalData == null ? node.ServerData : node.LocalData;
			var path = node.AssetName + "/" + 
				AsScriptingHelper.GetXmlAttribute(xmlData, "GameObject");
			var position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			var window = CreateInstance<AsPopupInfo>();
			window.ShowAsDropDown(new Rect(position, Vector2.zero),
				new Vector2(path.Length * 7, 21));
		}

		private void OnGUI()
		{
			using (new GUILayout.VerticalScope("box"))
			{
				EditorGUILayout.LabelField(path, EditorStyles.boldLabel);
			}
		}
	}
}