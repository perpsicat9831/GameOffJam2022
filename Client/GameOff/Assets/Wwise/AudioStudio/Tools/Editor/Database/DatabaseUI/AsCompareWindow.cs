using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace AudioStudio.Tools
{
	internal abstract class AsCompareWindow : AsSearchers
	{
		//-----------------------------------------------------------------------------------------
		internal static List<ComponentComparisonData> 
			ModifiedComponents = new List<ComponentComparisonData>();

		protected static readonly HashSet<string> typesFilter = new HashSet<string>();
		public static List<bool> typesFilterSelector = new List<bool>();
		protected static readonly HashSet<string> objectFilter = new HashSet<string>();
		public static List<bool> objectFilterSelector = new List<bool>();
		protected static readonly HashSet<string> statusFilter = new HashSet<string>();
		public static List<bool> statusFilterSelector = new List<bool>();
		private Vector2 _scrollPosition0, _scrollPosition1;


		ComponentFilterList _componentFilterList;
		ObjectFilterList _objectFilterList;
		StatusFilterList _statusFilterList;

		protected int _objectWidth = 200;
		protected int _componentWidth = 120;
		protected int _statusWidth = 100;

		private int maxNumberPerPage = 40;
		private int componentNumAfterFilter = 0;
		private int totalPages = 1;
		private int currentPage = 0;

		public delegate void SaveAllData();
		public SaveAllData saveAllDataEvent;
		public delegate void SaveSingleData(ComponentComparisonData data);
		public SaveSingleData saveSingleDataEvent;

		/*
		public delegate void SaveComponentToXML(ComponentComparisonData data);
		public SaveComponentToXML saveComponentToXMLEvent;
		public delegate void RevertXMLDataToComponent(ComponentComparisonData data);
		public RevertXMLDataToComponent revertXMLDataToComponentEvent;
		public delegate void RemoveAllData(ComponentComparisonData data);
		public RemoveAllData removeAllDataEvent;
		*/

		//-----------------------------------------------------------------------------------------

		#region Initialization
		//-----------------------------------------------------------------------------------------
		private void Awake()
		{
			ProcessFilter();
			CalculateTotalPages();
		}

		protected virtual void ProcessFilter()
		{
			foreach (var component in ModifiedComponents)
			{
				typesFilter.Add(AsScriptingHelper.
					GetXmlAttribute(component.LocalData, "Type"));
				objectFilter.Add(component.AssetName);
			}

			for (int i = 0; i < objectFilter.Count; i++)
			{
				objectFilterSelector.Add(false);
			}

			for (int i = 0; i < typesFilter.Count; i++)
			{
				typesFilterSelector.Add(false);
			}
			

			statusFilter.Add(ComponentStatus.AllRemoved.ToString());
			statusFilter.Add(ComponentStatus.ServerOnly.ToString());
			statusFilter.Add(ComponentStatus.LocalOnly.ToString());
			statusFilter.Add(ComponentStatus.UseServer.ToString());
			statusFilter.Add(ComponentStatus.UseLocal.ToString());
			statusFilter.Add(ComponentStatus.Different.ToString());
			statusFilter.Add(ComponentStatus.NoEvent.ToString());
			for (int i = 0; i < 7; i++)
			{
				statusFilterSelector.Add(false);
			}
				
		}

		private void CalculateTotalPages()
		{
			componentNumAfterFilter = 0;
			foreach (var component in ModifiedComponents)
			{
				if (component.display && !ProcessFilterGroup(component))
					componentNumAfterFilter++;
			}

			totalPages = componentNumAfterFilter / maxNumberPerPage;
			if (componentNumAfterFilter % maxNumberPerPage != 0)
				totalPages++;
		}

		private void OnDestroy()
		{
			ClearBuffer();
		}

		internal static void ClearBuffer()
		{
			ModifiedComponents.Clear();

			typesFilter.Clear();
			typesFilterSelector.Clear();
			objectFilter.Clear();
			objectFilterSelector.Clear();
			statusFilter.Clear();
			statusFilterSelector.Clear();

		}
        //-----------------------------------------------------------------------------------------
        #endregion

        //-----------------------------------------------------------------------------------------
        private void OnGUI()
		{
			using (new EditorGUILayout.VerticalScope(GUI.skin.box))
			{
				DrawFilters();
			}

			using (new EditorGUILayout.VerticalScope(GUI.skin.box))
			{
				DrawComponentList(ref _scrollPosition1);
			}

			using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
			{
				DrawPageTable();
			}

			GroupOperations();
		}

		private void DrawFilters()
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Object", EditorStyles.foldoutPreDrop,
				GUILayout.Width(_objectWidth)))
			{
				var position = GUIUtility.GUIToScreenPoint(_scrollPosition0);
				_objectFilterList = CreateInstance<ObjectFilterList>();
				_objectFilterList.ShowAsDropDown(new Rect(position, new Vector2(_objectWidth, 25)),
									  new Vector2(5 * _objectWidth, (objectFilter.Count / 4) * 19));
				_objectFilterList.refreshEvent = Repaint;
			}

			if (GUILayout.Button("Component", EditorStyles.foldoutPreDrop,
				GUILayout.Width(_componentWidth)))
			{
				var position = GUIUtility.GUIToScreenPoint(
					new Vector2(_scrollPosition0.x + _objectWidth, _scrollPosition0.y));
				_componentFilterList = CreateInstance<ComponentFilterList>();
				_componentFilterList.ShowAsDropDown(new Rect(position, new Vector2(_componentWidth, 25)),
								  new Vector2(_componentWidth + 20, typesFilter.Count * 19));
				_componentFilterList.refreshEvent = Repaint;
			}

			if (GUILayout.Button("Status", EditorStyles.foldoutPreDrop,
				GUILayout.Width(_statusWidth)))
			{
				var position = GUIUtility.GUIToScreenPoint(new Vector2(_scrollPosition0.x +
					_objectWidth + _componentWidth, _scrollPosition0.y));
				_statusFilterList = CreateInstance<StatusFilterList>();
				_statusFilterList.ShowAsDropDown(new Rect(position, new Vector2(_statusWidth, 25)),
								  new Vector2(_statusWidth + 20, statusFilter.Count * 19));
				_statusFilterList.refreshEvent = Repaint;
			}

			/*
			EditorGUILayout.LabelField("Operation", EditorStyles.boldLabel,
				GUILayout.MinWidth(50));
			*/
			EditorGUILayout.EndHorizontal();
		}

		protected virtual void DrawComponentList(ref Vector2 scrollPosition)
		{
			CalculateTotalPages();

			if (totalPages == 0)
			{
				EditorGUILayout.Separator();
				EditorGUILayout.LabelField("No Components Found",
					EditorStyles.boldLabel, GUILayout.Height(40));
				EditorGUILayout.Separator();
				return;
			}

			int listCount = 0;
			int startIndex = 0;

			for(int i = 0; i < ModifiedComponents.Count; i++)
			{
				if (!ModifiedComponents[i].display) continue;
				//Debug.LogWarning(dataIndex + dataList[dataIndex].AssetName);
				if (ProcessFilterGroup(ModifiedComponents[i]))
					continue;
				if (listCount >= currentPage * maxNumberPerPage)
				{
					startIndex = i;
					listCount = 0;
					break;
				}
				listCount++;
			}

			int pageNumbers = maxNumberPerPage;
			if (currentPage == totalPages - 1)
				pageNumbers = componentNumAfterFilter - currentPage * maxNumberPerPage;

			scrollPosition = EditorGUILayout.
				BeginScrollView(scrollPosition, GUILayout.MaxHeight(position.height));
			for (int i = startIndex; i < ModifiedComponents.Count; i++)
			{
				//var dataIndex = i + currentPage * maxNumberPerPage;

				if (!ModifiedComponents[i].display) continue;
				//Debug.LogWarning(dataIndex + dataList[dataIndex].AssetName);
				if (ProcessFilterGroup(ModifiedComponents[i]))
					continue;
				if (listCount >= pageNumbers) break;
				listCount++;

				using (new EditorGUILayout.HorizontalScope("box"))
				{
					DisplayData(ModifiedComponents[i]);
					DrawOperations(ModifiedComponents[i], i+1);
				}

				if (ModifiedComponents[i].showDetail)
					ShowCompareDetail(ModifiedComponents[i]);
			}
			EditorGUILayout.EndScrollView();

		}

		
		private void DrawPageTable()
		{
			GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
			buttonStyle.normal.textColor = Color.white;
			buttonStyle.active.textColor = Color.yellow;
			buttonStyle.fontSize = 10;

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("<<", 
				EditorStyles.toolbarButton, GUILayout.Width(50)))
				currentPage = 0;
			for (int i = 0; i < totalPages; i++)
			{
				if (i == 10) break;
				if (GUILayout.Button((i + 1).ToString(), 
					buttonStyle, GUILayout.Width(50)))
				{
					currentPage = i;
				}
					
			}
			if (GUILayout.Button(">>", 
				EditorStyles.toolbarButton, GUILayout.Width(50)))
				currentPage = totalPages - 1;
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();
		}

		private void GroupOperations()
		{

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Select All", EditorStyles.toolbarButton))
			{
				int selectionNum = ModifiedComponents.Count;
				for (int index = 0; index < selectionNum; index++)
				{
					if (ProcessFilterGroup(ModifiedComponents[index]))
						continue;
					ModifiedComponents[index].selectionToggle = true;
				}

			}
			if (GUILayout.Button("Deselect All", EditorStyles.toolbarButton))
			{
				int selectionNum = ModifiedComponents.Count;
				for (int index = 0; index < selectionNum; index++)
				{
					if (ProcessFilterGroup(ModifiedComponents[index]))
						continue;
					ModifiedComponents[index].selectionToggle = false;
				}

			}

			if (GUILayout.Button("Save All", EditorStyles.toolbarButton))
			{
				var comfirmFlag = EditorUtility.DisplayDialog("Save All", 
					"Are you sure to replace all your server data with " +
					"the local one?", "Yes", "Cancel");
				if (!comfirmFlag) return;

				int selectionNum = ModifiedComponents.Count;
				for (int index = 0; index < selectionNum; index++)
				{
					if (ProcessFilterGroup(ModifiedComponents[index]))
						continue;

					bool hasLocalData = ModifiedComponents[index].
						LocalData == null ? false : true;
					if (!hasLocalData) continue;
					switch (ModifiedComponents[index].ComponentStatus)
					{
						case ComponentStatus.AllRemoved:
							ModifiedComponents[index].ComponentStatus =
								ComponentStatus.UseLocal;
							saveSingleDataEvent(ModifiedComponents[index]);
							break;
						case ComponentStatus.LocalOnly:
							ModifiedComponents[index].ComponentStatus = 
								ComponentStatus.UseLocal;
							saveSingleDataEvent(ModifiedComponents[index]);
							break;
						case ComponentStatus.UseServer:
							ModifiedComponents[index].ComponentStatus =
								ComponentStatus.UseLocal;
							saveSingleDataEvent(ModifiedComponents[index]);
							break;
						case ComponentStatus.Different:
							ModifiedComponents[index].ComponentStatus =
								ComponentStatus.UseLocal;
							saveSingleDataEvent(ModifiedComponents[index]);
							break;
					}
				}
			}
			if (GUILayout.Button("Revert All", EditorStyles.toolbarButton))
			{
				var comfirmFlag = EditorUtility.DisplayDialog("Revert All",
					"Are you sure to replace all your local data with " +
					"the one in the server?", "Yes", "Cancel");
				if (!comfirmFlag) return;

				int selectionNum = ModifiedComponents.Count;
				for (int index = 0; index < selectionNum; index++)
				{
					if (ProcessFilterGroup(ModifiedComponents[index]))
						continue;

					bool hasServerData = ModifiedComponents[index].
						ServerData == null ? false : true;
					if (!hasServerData) continue;
					switch (ModifiedComponents[index].ComponentStatus)
					{
						case ComponentStatus.AllRemoved:
							ModifiedComponents[index].ComponentStatus =
								ComponentStatus.UseServer;
							saveSingleDataEvent(ModifiedComponents[index]);
							break;
						case ComponentStatus.ServerOnly:
							ModifiedComponents[index].ComponentStatus =
								ComponentStatus.UseServer;
							saveSingleDataEvent(ModifiedComponents[index]);
							break;
						case ComponentStatus.UseLocal:
							ModifiedComponents[index].ComponentStatus =
								ComponentStatus.UseServer;
							saveSingleDataEvent(ModifiedComponents[index]);
							break;
						case ComponentStatus.Different:
							ModifiedComponents[index].ComponentStatus =
								ComponentStatus.UseServer;
							saveSingleDataEvent(ModifiedComponents[index]);
							break;
					}
				}
			}
			if (GUILayout.Button("Remove All", EditorStyles.toolbarButton))
			{
				var comfirmFlag = EditorUtility.DisplayDialog("Remove All",
					"Are you sure to remove all your local data and " +
					"server data?", "Yes", "Cancel");
				if (!comfirmFlag) return;

				int selectionNum = ModifiedComponents.Count;
				for (int index = 0; index < selectionNum; index++)
				{
					if (ProcessFilterGroup(ModifiedComponents[index]))
						continue;
					if (ModifiedComponents[index].ComponentStatus == 
						ComponentStatus.AllRemoved ||
						ModifiedComponents[index].ComponentStatus ==
						ComponentStatus.NoEvent)
						continue;
					ModifiedComponents[index].ComponentStatus = 
						ComponentStatus.AllRemoved;
					saveSingleDataEvent(ModifiedComponents[index]);
				}
			}

			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("Close", EditorStyles.toolbarButton))
				Close();
		}

		#region Operations
		//-----------------------------------------------------------------------------------------
		private void DrawOperations(ComponentComparisonData data, int index)
		{
			if (GUILayout.Button("Detail", EditorStyles.toolbarButton))
				data.showDetail = !data.showDetail;

			if (GUILayout.Button("Locate", EditorStyles.toolbarButton))
				LocateComponent(data);

			string finishLabel = "Cancel";
			if (data.ComponentStatus == ComponentStatus.UseLocal ||
				data.ComponentStatus == ComponentStatus.UseServer ||
				data.ComponentStatus == ComponentStatus.AllRemoved)
				finishLabel = "Finish";

			if (GUILayout.Button(finishLabel, EditorStyles.toolbarButton))
			{
				data.showDetail = false;
				data.display = false;
				CalculateTotalPages();
			}

			data.selectionToggle = GUILayout.
				Toggle(data.selectionToggle, index.ToString(), GUILayout.Width(50));

		}

		private void ShowCompareDetail(ComponentComparisonData data)
		{
			DrawCompareDetails(data);

			bool hasServerData = data.ServerData == null ? false : true;
			bool hasLocalData = data.LocalData == null ? false : true;

			switch (data.ComponentStatus)
			{
				case ComponentStatus.AllRemoved:
					AllRemovedOperations(data, hasServerData, hasLocalData);
					break;
				case ComponentStatus.ServerOnly:
					ServerOnlyOperations(data, hasServerData, hasLocalData);
					break;
				case ComponentStatus.LocalOnly:
					LocalOnlyOperations(data, hasServerData, hasLocalData);
					break;
				case ComponentStatus.UseServer:
					UseServerOperations(data, hasServerData, hasLocalData);
					break;
				case ComponentStatus.UseLocal:
					UseLocalOperations(data, hasServerData, hasLocalData);
					break;
				case ComponentStatus.Different:
					DifferentOperations(data, hasServerData, hasLocalData);
					break;
				case ComponentStatus.NoEvent:
					NoEventOperations(data);
					break;
			}
		}

		private void DrawCompareDetails(ComponentComparisonData data)
		{
			if (data.LocalData == null)
			{
				using (new EditorGUILayout.VerticalScope("box"))
				{
					GUI.contentColor = Color.yellow;
					GUILayout.Label("Lack of the local data");
					GUI.contentColor = Color.white;
				}
				return;
			}

			if (data.ServerData == null)
			{
				using (new EditorGUILayout.VerticalScope("box"))
				{
					GUI.contentColor = Color.yellow;
					GUILayout.Label("Lack of the server data");
					GUI.contentColor = Color.white;
				}
				return;
			}

			List<string> localCompareList = new List<string>();
			List<string> serverCompareList = new List<string>();

			MakeCompareList(data.LocalData, data.ServerData, 
				localCompareList, serverCompareList);
			if (localCompareList.Count == 0) return;

			using (new EditorGUILayout.HorizontalScope("box"))
			{
				EditorGUILayout.BeginVertical("box");
				string prefix = "";
				GUILayout.Label("Local", EditorStyles.boldLabel);
				bool firstLabel = true;
				for (int index = 0; index < localCompareList.Count; index++)
				{
					if (localCompareList[index].Contains(": "))
					{
						if (!firstLabel)
						{
							EditorGUILayout.EndVertical();
						}
						if (data.ComponentStatus == ComponentStatus.UseServer)
							GUI.backgroundColor = Color.yellow;
						EditorGUILayout.BeginVertical("box");
						prefix = localCompareList[index];
						firstLabel = false;
						continue;
					}
					if (data.ComponentStatus != ComponentStatus.UseServer)
					{
						if (localCompareList[index] != " ") 
							GUILayout.Label(prefix + localCompareList[index]);
						else GUILayout.Label(prefix + "empty");
					}
					else
					{
						
						if (serverCompareList[index] != " ")
						{
							GUILayout.Label(prefix + serverCompareList[index]);
							GUI.contentColor = Color.red;
							if (localCompareList[index] != " ")
								GUILayout.Label(prefix + localCompareList[index]);
							else GUILayout.Label(prefix + "empty");
							GUI.contentColor = Color.white;
						}
						else
						{
							GUILayout.Label(prefix + "empty");
							GUI.contentColor = Color.red;
							if (localCompareList[index] != " ")
								GUILayout.Label(prefix + localCompareList[index]);
							else GUILayout.Label(prefix + "empty");
							GUI.contentColor = Color.white;
						}
					}
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndVertical();
				GUI.backgroundColor = Color.white;

				//if (data.ComponentStatus == ComponentStatus.UseServer)
				//	GUI.backgroundColor = Color.yellow;
				EditorGUILayout.BeginVertical("box");
				//using (new EditorGUILayout.VerticalScope("box"))
				GUILayout.Label("Server", EditorStyles.boldLabel);

				firstLabel = true;
				for (int index = 0; index < serverCompareList.Count; index++)
				{
					if (serverCompareList[index].Contains(": "))
					{
						if (!firstLabel)
						{
							EditorGUILayout.EndVertical();
						}
						if (data.ComponentStatus == ComponentStatus.UseLocal)
							GUI.backgroundColor = Color.yellow;
						EditorGUILayout.BeginVertical("box");
						prefix = serverCompareList[index];
						firstLabel = false;
						continue;
					}
					if (data.ComponentStatus != ComponentStatus.UseLocal)
					{
						if (serverCompareList[index] != " ")
							GUILayout.Label(prefix + serverCompareList[index]);
						else GUILayout.Label(prefix + "empty");
					}
					else
					{

						if (localCompareList[index] != " ")
						{
							GUILayout.Label(prefix + localCompareList[index]);
							GUI.contentColor = Color.red;
							if (serverCompareList[index] != " ")
								GUILayout.Label(prefix + serverCompareList[index]);
							else GUILayout.Label(prefix + "empty");
							GUI.contentColor = Color.white;
						}
						else
						{
							GUILayout.Label(prefix + "empty");
							GUI.contentColor = Color.red;
							if (serverCompareList[index] != " ")
								GUILayout.Label(prefix + serverCompareList[index]);
							else GUILayout.Label(prefix + "empty");
							GUI.contentColor = Color.white;
						}
					}
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndVertical();
				GUI.backgroundColor = Color.white;

				/*
				foreach (var element in serverCompareList)
				{
					if (element.Contains(": "))
						using (new EditorGUILayout.VerticalScope("box"))
							GUILayout.Label(element);
					else GUILayout.Label(element);
				}
				EditorGUILayout.EndVertical();
				GUI.backgroundColor = Color.white;
				*/
			}
		}

		private void MakeCompareList(XElement localData, XElement serverData, 
			List<string> localCompareList, List<string> serverCompareList)
		{
			WriteAttributes(localData, serverData, localCompareList, serverCompareList);

			var localElements = localData.Elements().ToList();
			var serverElements = serverData.Elements().ToList();

			if (localElements.Count > serverElements.Count)
			{
				for (int i = 0; i < localElements.Count; i++)
				{
					if (i >= serverElements.Count)
						serverElements.Insert(i, new XElement(localElements[i].Name));
				}
				Debug.LogWarning("element count " + localElements.Count + " " + serverElements.Count);
			}
			else if (localElements.Count < serverElements.Count)
			{
				for (int i = 0; i < serverElements.Count; i++)
				{
					if (i >= localElements.Count)
						localElements.Insert(i, new XElement(serverElements[i].Name));
				}
				Debug.LogWarning("element count " + localElements.Count + " " + serverElements.Count);
			}
			else if (localElements.Count == 0 && serverElements.Count == 0)
				return;

			for (int i = 0; i < localElements.Count; i++)
			{
				MakeCompareList(localElements[i], serverElements[i],
					localCompareList, serverCompareList);
			}
		}

		private static void WriteAttributes(XElement localData, XElement serverData, 
			List<string> localCompareList, List<string> serverCompareList)
		{
			var localAttributes = localData.Attributes().ToList();
			var serverAttributes = serverData.Attributes().ToList();

			if (localAttributes.Count == 0 && serverAttributes.Count == 0)
				return;

			bool attributeDifferent = false;

			if (localAttributes.Count > serverAttributes.Count)
			{
				localCompareList.Add(localData.Name.ToString() + ": ");
				serverCompareList.Add(serverData.Name.ToString() + ": ");
				for (int i = 0; i < localAttributes.Count; i++)
				{
					localCompareList.Add(localAttributes[i].ToString());
					serverCompareList.Add(" ");
				}
			}
			else if (localAttributes.Count < serverAttributes.Count)
			{
				localCompareList.Add(localData.Name.ToString() + ": ");
				serverCompareList.Add(serverData.Name.ToString() + ": ");
				for (int i = 0; i < serverAttributes.Count; i++)
				{
					localCompareList.Add(" ");
					serverCompareList.Add(serverAttributes[i].ToString());
				}
			}
			else
			{
				localCompareList.Add(localData.Name.ToString() + ": ");
				serverCompareList.Add(serverData.Name.ToString() + ": ");
				for (int i = 0; i < localAttributes.Count; i++)
				{
					if (localAttributes[i].Value == serverAttributes[i].Value) continue;
					attributeDifferent = true;
					Debug.LogWarning(localAttributes[i] + " " + serverAttributes[i]);
					localCompareList.Add(localAttributes[i].ToString());
					serverCompareList.Add(serverAttributes[i].ToString());
				}
				if (!attributeDifferent &&
					localCompareList[localCompareList.Count - 1].Contains(": "))
				{
					localCompareList.RemoveAt(localCompareList.Count - 1);
					serverCompareList.RemoveAt(serverCompareList.Count - 1);
				}
			}
		}

		private void NoEventOperations(ComponentComparisonData data)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(true);
			GUI.contentColor = Color.green;
			GUILayout.Button("Save", EditorStyles.toolbarButton);
			GUI.contentColor = Color.magenta;
			GUILayout.Button("Revert", EditorStyles.toolbarButton);
			GUI.contentColor = Color.red;
			GUILayout.Button("Remove", EditorStyles.toolbarButton);
			GUI.contentColor = Color.white;
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
		}

		private void LocalOnlyOperations(ComponentComparisonData data,
			bool hasServerData, bool hasLocalData)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(!hasLocalData);
			GUI.contentColor = Color.green;
			if (GUILayout.Button("Save", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.UseLocal;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!hasServerData);
			GUI.contentColor = Color.magenta;
			if (GUILayout.Button("Revert", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.UseServer;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			EditorGUI.EndDisabledGroup();

			GUI.contentColor = Color.red;
			if (GUILayout.Button("Remove", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.AllRemoved;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			GUI.contentColor = Color.white;
			EditorGUILayout.EndHorizontal();
		}

		private void UseLocalOperations(ComponentComparisonData data,
			bool hasServerData, bool hasLocalData)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(true);
			GUI.contentColor = Color.green;
			if (GUILayout.Button("Save", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.UseLocal;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!hasServerData);
			GUI.contentColor = Color.magenta;
			if (GUILayout.Button("Revert", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.UseServer;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			EditorGUI.EndDisabledGroup();

			GUI.contentColor = Color.red;
			if (GUILayout.Button("Remove", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.AllRemoved;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			GUI.contentColor = Color.white;
			EditorGUILayout.EndHorizontal();
		}

		private void UseServerOperations(ComponentComparisonData data,
			bool hasServerData, bool hasLocalData)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(!hasLocalData);
			GUI.contentColor = Color.green;
			if (GUILayout.Button("Save", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.UseLocal;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(true);
			GUI.contentColor = Color.magenta;
			GUILayout.Button("Revert", EditorStyles.toolbarButton);
			EditorGUI.EndDisabledGroup();

			GUI.contentColor = Color.red;
			if (GUILayout.Button("Remove", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.AllRemoved;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			GUI.contentColor = Color.white;
			EditorGUILayout.EndHorizontal();
		}

		private void ServerOnlyOperations(ComponentComparisonData data,
			bool hasServerData, bool hasLocalData)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(!hasLocalData);
			GUI.contentColor = Color.green;
			if (GUILayout.Button("Save", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.UseLocal;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!hasServerData);
			GUI.contentColor = Color.magenta;
			if (GUILayout.Button("Revert", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.UseServer;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			EditorGUI.EndDisabledGroup();

			GUI.contentColor = Color.red;
			if (GUILayout.Button("Remove", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.AllRemoved;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			GUI.contentColor = Color.white;
			EditorGUILayout.EndHorizontal();
		}

		private void DifferentOperations(ComponentComparisonData data,
			bool hasServerData, bool hasLocalData)
		{
			EditorGUILayout.BeginHorizontal();
			GUI.contentColor = Color.green;
			if (GUILayout.Button("Save", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.UseLocal;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			GUI.contentColor = Color.magenta;
			if (GUILayout.Button("Revert", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.UseServer;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}

			GUI.contentColor = Color.red;
			if (GUILayout.Button("Remove", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.AllRemoved;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			GUI.contentColor = Color.white;
			EditorGUILayout.EndHorizontal();
		}

		private void AllRemovedOperations(ComponentComparisonData data, 
			bool hasServerData, bool hasLocalData)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginDisabledGroup(!hasLocalData);
			GUI.contentColor = Color.green;
			if (GUILayout.Button("Save", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.UseLocal;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!hasServerData);
			GUI.contentColor = Color.magenta;
			if (GUILayout.Button("Revert", EditorStyles.toolbarButton))
			{
				data.ComponentStatus = ComponentStatus.UseServer;
				saveSingleDataEvent(data);
				LocateComponent(data);
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(true);
			GUI.contentColor = Color.red;
			GUILayout.Button("Remove", EditorStyles.toolbarButton);
			GUI.contentColor = Color.white;
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
		}

        //-----------------------------------------------------------------------------------------
        #endregion

		protected abstract void DisplayData(ComponentComparisonData data);
		protected abstract void LocateComponent(ComponentComparisonData data);
		protected virtual void RemoveComponentXML(ComponentComparisonData data)
		{

		}

		#region Filter
		//-----------------------------------------------------------------------------------------
		protected virtual bool ProcessFilterGroup(ComponentComparisonData data)
		{
			return !ProcessFilter(AsScriptingHelper.
					GetXmlAttribute(data.LocalData, "Type"),
					typesFilter, typesFilterSelector) ||
					!ProcessFilter(data.AssetName,
					objectFilter, objectFilterSelector) ||
					!ProcessFilter(data.ComponentStatus.ToString(),
					statusFilter, statusFilterSelector);
		}

		protected bool ProcessFilter(string inFilterName,
			HashSet<string> filter, List<bool> filterSelector)
		{
			// Check if no toggle is checked and quit the filter if so
			bool isFiltered = false;
			foreach (bool selector in filterSelector)
			{
				if (selector)
				{
					isFiltered = true;
					break;
				}
			}
			if (!isFiltered) return true;

			// Here does the filter
			var filterList = filter.ToList();
			for (int i = 0; i < filterList.Count; i++)
			{
				if (filterSelector[i] && filterList[i] == inFilterName)
					return true;
			}
			return false;
		}

		

		internal class ObjectFilterList : EditorWindow
		{
			public delegate void Refresh();
			public Refresh refreshEvent;

			public int row = 1;
			private void OnGUI()
			{
				int totalNumber = objectFilter.Count;
				EditorGUILayout.BeginHorizontal();
				for (int index = 0; index < totalNumber; index++)
				{
					var list = objectFilter.ToList();
					EditorGUILayout.BeginHorizontal();
					if (objectFilterSelector[index] =
						EditorGUILayout.ToggleLeft(list[index], objectFilterSelector[index]))
						refreshEvent();
					//EditorGUILayout.LabelField(list[index]);
					EditorGUILayout.EndHorizontal();
					if ((index + 1) % 4 == 0 && index + 1 != totalNumber)
					{
						++row;
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}


		private class ComponentFilterList : EditorWindow
		{
			public delegate void Refresh();
			public Refresh refreshEvent;

			private void OnGUI()
			{
				int totalNumber = typesFilter.Count;
				for (int index = 0; index < totalNumber; index++)
				{
					var list = typesFilter.ToList();
					EditorGUILayout.BeginHorizontal();
					if (typesFilterSelector[index] = 
						EditorGUILayout.Toggle(typesFilterSelector[index]))
						refreshEvent();
					EditorGUILayout.LabelField(list[index]);
					EditorGUILayout.EndHorizontal();
				}
			}
		}

		private class StatusFilterList : EditorWindow
		{
			public delegate void Refresh();
			public Refresh refreshEvent;

			private void OnGUI()
			{
				int totalNumber = statusFilter.Count;
				for (int index = 0; index < totalNumber; index++)
				{
					var list = statusFilter.ToList();
					EditorGUILayout.BeginHorizontal();
					if (statusFilterSelector[index] =
						EditorGUILayout.Toggle(statusFilterSelector[index]))
						refreshEvent();
					EditorGUILayout.LabelField(list[index]);
					EditorGUILayout.EndHorizontal();
				}
			}
		}

		//-----------------------------------------------------------------------------------------
		#endregion
	}
}