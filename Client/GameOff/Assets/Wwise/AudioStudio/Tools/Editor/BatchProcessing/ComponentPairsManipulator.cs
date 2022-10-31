using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using AudioStudio.Components;
using AudioStudio.Editor;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

namespace AudioStudio.Tools
{
	/// <summary>
	/// Search for all components of a type and apply operations to them or their paired components.
	/// For example, you can search for all the Buttons and add a ButtonSound to each Button, or remove all the ButtonSounds from them.
	/// </summary>
	internal class ComponentPairsManipulator : AsSearchers
	{				
		// component pair for processing
		[Serializable]
		private class ComponentPair
		{
			internal Type SearchComponent;
			internal Type PairedComponent;
			internal bool WillSearch = true;
			
			internal ComponentPair() {}

			internal ComponentPair(Type search, Type paired)
			{
				SearchComponent = search;
				PairedComponent = paired;
			}

			// toggle displayed in window
			internal string ToggleName
			{
				get
				{
					if (SearchComponent == null)
						return string.Empty;
					var name = SearchComponent.Name;
					if (PairedComponent != null)
						name += " (" + PairedComponent.Name + ")";
					return name;
				}
			}
		}

		//converts string to type
		[Serializable]
		private class CustomTypes
		{
			internal string SearchComponent = "";
			internal string PairedComponent = "";
			internal readonly ComponentPair Pair = new ComponentPair();

			internal void Validate()
			{
				Pair.SearchComponent = AsScriptingHelper.StringToType(SearchComponent);
				Pair.PairedComponent = AsScriptingHelper.StringToType(PairedComponent);				
			}						
			
			internal bool BothTypesFilled()
			{
				return (Pair.PairedComponent != null && Pair.SearchComponent != null);
			}
		}
		
		#region Fields
		// do not search for inherited components
		private bool _explicitType = true;
		private bool _exportLog = true;
		private bool _removePairedAsWell = true;		

		private readonly List<ComponentPair> _componentPairs = new List<ComponentPair>();
		private readonly List<ComponentPair> _customPairs = new List<ComponentPair>();
		private readonly List<CustomTypes> _customTypeList = new List<CustomTypes>();
		
		private enum ActionType {
			RemoveSearchedComponents, 
			RemovePairedComponents,
			RemoveUnpairedComponents,
			AddUnpairedComponents, 
			FindComponents, 
			FindPairedComponents,
			FindUnpairedComponents,			
		}
		private ActionType _actionType;		
		#endregion
		
		#region Init

		private void OnEnable()
		{						
			_componentPairs.Add(new ComponentPair(typeof(Animator), typeof(AnimationSound)));
			_componentPairs.Add(new ComponentPair(typeof(Animation), typeof(LegacyAnimationSound)));
			_componentPairs.Add(new ComponentPair(typeof(PlayableDirector), typeof(TimelineSound)));
			_componentPairs.Add(new ComponentPair(typeof(Button), typeof(ButtonSound)));
			_componentPairs.Add(new ComponentPair(typeof(Dropdown), typeof(DropdownSound)));
			_componentPairs.Add(new ComponentPair(typeof(Slider), typeof(SliderSound)));
			_componentPairs.Add(new ComponentPair(typeof(ScrollRect), typeof(ScrollSound)));
			_componentPairs.Add(new ComponentPair(typeof(Toggle), typeof(ToggleSound)));
			_componentPairs.Add(new ComponentPair(typeof(EventTrigger), typeof(EventSound)));		
			_componentPairs.Add(new ComponentPair(typeof(Camera), typeof(AudioListener3D)));
		}

		private void SwapComponents()
		{
			foreach (var pair in _componentPairs)
			{
				var tempType = pair.SearchComponent;
				pair.SearchComponent = pair.PairedComponent;
				pair.PairedComponent = tempType;
			}
		}
		private void OnDisable()
		{
			_componentPairs.Clear();
		}

		#endregion
		
		#region GUI
		private void OnGUI()
		{
			using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Quick Select Components");
				if (GUILayout.Button("Swap", GUILayout.Width(60))) SwapComponents();
				if (GUILayout.Button("Select All", GUILayout.Width(100))) SelectAllToggles(true);
				if (GUILayout.Button("Deselect All", GUILayout.Width(100))) SelectAllToggles(false);
				GUILayout.EndHorizontal();
			}
			
			using (new EditorGUILayout.VerticalScope(GUI.skin.box))
			{
				foreach (var componentData in _componentPairs)
				{
					componentData.WillSearch = GUILayout.Toggle(componentData.WillSearch, componentData.ToggleName);
				}
			}
			
			using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
			{
				searchPrefab = GUILayout.Toggle(searchPrefab, "Search in prefabs");
				searchScene = GUILayout.Toggle(searchScene, "Search in scenes");								
			}

			using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
			{
				_explicitType = GUILayout.Toggle(_explicitType, "Explicit Type");	
				_exportLog = EditorGUILayout.BeginToggleGroup("Export log to xml", _exportLog);					
				EditorGUILayout.EndToggleGroup();
			}

			using (new EditorGUILayout.VerticalScope(GUI.skin.box))
			{
				GUILayout.Label("Enter custom types, format like 'Button' and 'ButtonSound'");
				CustomTypes toBeRemoved = null;
				foreach (var customPair in _customTypeList)
				{
					GUILayout.BeginHorizontal();
					EditorGUIUtility.labelWidth = 80;
					customPair.SearchComponent = EditorGUILayout.TextField("Type", customPair.SearchComponent);
					customPair.PairedComponent = EditorGUILayout.TextField("Paired Type", customPair.PairedComponent);
					if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(20))) toBeRemoved = customPair;
					GUILayout.EndHorizontal();
				}
				_customTypeList.Remove(toBeRemoved);
				
				if (GUILayout.Button("+")) _customTypeList.Add(new CustomTypes());													
			}

			using (new EditorGUILayout.VerticalScope(GUI.skin.box))
			{
				_actionType = (ActionType) EditorGUILayout.EnumPopup("Action", _actionType);
				switch (_actionType)
				{
					case ActionType.FindComponents:
						GUILayout.Label("Search for a type of component and export locations of all instances");
						_exportLog = true;
						break;
					case ActionType.FindPairedComponents:
						GUILayout.Label("Search for occurrences where a type of component and its paired\n" +
						                "type of component are on the same GameObject");
						_exportLog = true;
						break;
					case ActionType.FindUnpairedComponents:
						GUILayout.Label("Search for occurrences where a type of component doesn't have its\n" +
						                " paired component on the same GameObject");
						_exportLog = true;
						break;
					case ActionType.RemoveSearchedComponents:
						_removePairedAsWell = GUILayout.Toggle(_removePairedAsWell, "Remove paired component as well");
						GUILayout.Label("Search for a type of component and remove all instances of the component");
						break;
					case ActionType.RemovePairedComponents:
						GUILayout.Label("Search for occurrences where a type of component has its paired\n" +
						                "component on the same GameObject and remove the paired component");
						break;
					case ActionType.RemoveUnpairedComponents:
						GUILayout.Label("Search for occurrences where a type of component doesn't have its\n" +
						                "paired component on the same GameObject and remove the component itself");
						break;
					case ActionType.AddUnpairedComponents:
						GUILayout.Label("Search for occurrences where a type of component doesn't have its\n" +
						                "paired component on the same GameObject and add the paired component");
						break;
				}
			}
			
			AsGuiDrawer.DisplaySearchPath(ref _defaultSearchingPath);
			
			if (GUILayout.Button("Start Searching! (Process can't undo)", EditorStyles.toolbarButton)) 
				StartSearch();		
		}

		private void SelectAllToggles(bool status)
		{
			foreach (var pair in _componentPairs)
			{
				pair.WillSearch = status;
			}
		}
		#endregion				
		
		#region Validate							
		private bool ValidateComponents()
		{			
			_customPairs.Clear();
			switch (_actionType)
			{
				case ActionType.AddUnpairedComponents:
				case ActionType.FindUnpairedComponents:
				case ActionType.FindPairedComponents:
				case ActionType.RemovePairedComponents:
				case ActionType.RemoveUnpairedComponents:
					return TypeCheckBothComponents();					
				case ActionType.FindComponents:
					return TypeCheckSearchedComponent();
				case ActionType.RemoveSearchedComponents:
					return _removePairedAsWell ? TypeCheckBothComponents() : TypeCheckSearchedComponent();
			}
			return false;
		}

		private bool TypeCheckSearchedComponent()
		{
			foreach (var customTypes in _customTypeList)
			{
				customTypes.Validate();
				if (customTypes.Pair.SearchComponent != null)
					_customPairs.Add(customTypes.Pair);
				else
				{
					EditorUtility.DisplayDialog("Error", "Invalid or empty type name!", "OK");
					return false;	
				}							
			}
			return true;
		}

		private bool TypeCheckBothComponents()
		{
			foreach (var customTypes in _customTypeList)
			{
				customTypes.Validate();
				if (customTypes.BothTypesFilled())
					_customPairs.Add(customTypes.Pair);
				else
				{
					EditorUtility.DisplayDialog("Error", "Invalid or empty type name!", "OK");
					return false;	
				}																						
			}
			return true;
		}
		#endregion
		
		#region Search
		private void StartSearch()
		{			
			if (!ValidateComponents()) return;
			
			XRoot.SetAttributeValue("Action", _actionType);
			// search in prefabs
			if (searchPrefab) 
				FindFiles(SearchPrefabs, "Searching Prefabs", "*.prefab");
			// search in scenes
			if (searchScene)
			{
				var currentScene = SceneManager.GetActiveScene().path;
				FindFiles(SearchScenes, "Searching Scenes", "*.unity");
				EditorSceneManager.OpenScene(currentScene);
			}
			if (_exportLog) ExportToFile();
			
			CleanUp();
		}
		
		private void SearchPrefabs(string filePath)
		{				
			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);							
			var xPrefab = new XElement("Prefab");
			xPrefab.SetAttributeValue("AssetPath", filePath);				
			var edited = false;
				
			var allPairs = _componentPairs.Concat(_customPairs);
			foreach (var componentData in allPairs)
			{
				if (!componentData.WillSearch) continue;
				Component[] components = prefab.GetComponentsInChildren(componentData.SearchComponent, true);
				if (components.Length > 0)
				{						
					if (ProcessComponents(components, componentData, xPrefab, true)) 
						edited = true; 
				}
			}
				
			if (edited)
			{
				if (_actionType != ActionType.FindComponents && 
				    _actionType != ActionType.FindPairedComponents && 
				    _actionType != ActionType.FindUnpairedComponents)
					AsComponentBackup.SaveComponentAsset(prefab, filePath);
				XRoot.Add(xPrefab);
			}							
		}

		private void SearchScenes(string filePath)
		{
			EditorSceneManager.OpenScene(filePath);
			var scene = SceneManager.GetActiveScene();
			var rootGameObjects = scene.GetRootGameObjects();
				
			var xScene = new XElement("Scene");
			xScene.SetAttributeValue("AssetPath", filePath);
			var edited = false;				
			foreach (var gameObject in rootGameObjects)
			{
				var allPairs = _componentPairs.Concat(_customPairs);
				foreach (var pair in allPairs)
				{
					if (!pair.WillSearch) continue;
					Component[] components = gameObject.GetComponentsInChildren(pair.SearchComponent, true);
					if (components.Length > 0)
					{							
						if (ProcessComponents(components, pair, xScene, false)) 
							edited = true;
					}
				}
			}

			if (edited)
			{
				if (_actionType != ActionType.FindComponents && 
				    _actionType != ActionType.FindPairedComponents && 
				    _actionType != ActionType.FindUnpairedComponents)
					EditorSceneManager.SaveScene(scene, scene.path, false);	
				XRoot.Add(xScene);					
			}
				
			EditorSceneManager.CloseScene(scene, false);
		}

		private bool ProcessComponents(Component[] components, ComponentPair componentPair, XElement xElement, bool isSearchingPrefab)
		{
			var modified = false;
			foreach (var component in components)
			{
				if (_explicitType && component.GetType() != componentPair.SearchComponent) continue;
#if UNITY_2018_3_OR_NEWER
				if (isSearchingPrefab && PrefabUtility.GetPrefabAssetType(component.gameObject) == PrefabAssetType.NotAPrefab) continue;
#else
				if (isSearchingPrefab && PrefabUtility.GetPrefabType(component.gameObject) == PrefabType.None) continue;
#endif
				TotalCount++;
				Component pairedComponent;
				switch (_actionType)
				{
					case ActionType.AddUnpairedComponents:
						pairedComponent = component.gameObject.GetComponent(componentPair.PairedComponent);
						if (!pairedComponent)
						{
							component.gameObject.AddComponent(componentPair.PairedComponent);
							EditedCount++;
							WriteXNode(xElement, component, componentPair.SearchComponent, componentPair.PairedComponent);
							modified = true;
						}										
						break;
					case ActionType.RemoveSearchedComponents:
						if (componentPair.PairedComponent != null)
						{
							pairedComponent = component.gameObject.GetComponent(componentPair.PairedComponent);
							if (pairedComponent) 
								DestroyImmediate(pairedComponent);	
						}						 
						WriteXNode(xElement, component, componentPair.SearchComponent, componentPair.PairedComponent);
						DestroyImmediate(component, true);	
						modified = true;	
						break;
					case ActionType.RemovePairedComponents:
						pairedComponent = component.gameObject.GetComponent(componentPair.PairedComponent);
						if (pairedComponent)
						{
							EditedCount++;
							WriteXNode(xElement, component, componentPair.SearchComponent, componentPair.PairedComponent);
							DestroyImmediate(pairedComponent, true);
							modified = true;
						}															
						break;	
					case ActionType.RemoveUnpairedComponents:
						pairedComponent = component.gameObject.GetComponent(componentPair.PairedComponent);
						if (!pairedComponent)
						{							
							EditedCount++;
							WriteXNode(xElement, component, componentPair.SearchComponent, componentPair.PairedComponent);
							DestroyImmediate(component, true);
							modified = true;
						}												
						break;
					case ActionType.FindComponents:
						WriteXNode(xElement, component, componentPair.SearchComponent);
						break;
					case ActionType.FindUnpairedComponents:
						pairedComponent = component.gameObject.GetComponent(componentPair.PairedComponent);						
						if (!pairedComponent)
						{
							EditedCount++;
							WriteXNode(xElement, component, componentPair.SearchComponent, componentPair.PairedComponent);
						}
						break;
					case ActionType.FindPairedComponents:
						pairedComponent = component.gameObject.GetComponent(componentPair.PairedComponent);
						if (pairedComponent)
						{
							EditedCount++;
							WriteXNode(xElement, component, componentPair.SearchComponent, componentPair.PairedComponent);
						}
						break;
				}																																			
			}
			return modified;
		}		
		#endregion
		
		#region XML		
		private void WriteXNode(XElement element, Component component, Type searchType, Type pairedType = null)
		{
			if (!_exportLog) return;
			var xNode = new XElement("Component");
			xNode.SetAttributeValue("Type", searchType.Name);	
			if (pairedType != null) xNode.SetAttributeValue("PairedType", pairedType.Name);			
			xNode.SetAttributeValue("GameObject", GetGameObjectPath(component.transform));		
			element.Add(xNode);
		}

		private void ExportToFile()
		{
			var fileName = EditorUtility.SaveFilePanel("Export Log", XmlDocDirectory, "Search Result", "xml");
			if (string.IsNullOrEmpty(fileName)) return;
			AsScriptingHelper.WriteXml(fileName, XRoot);
			switch (_actionType)
			{
				case ActionType.FindComponents:
					EditorUtility.DisplayDialog("Success!", "Found " + TotalCount + " components!", "OK");
					break;
				case ActionType.RemoveSearchedComponents:
					EditorUtility.DisplayDialog("Success!", "Removed " + TotalCount + " components!", "OK");
					break;
				case ActionType.AddUnpairedComponents:
					EditorUtility.DisplayDialog("Success!", "Found " + TotalCount + " components,\n" +
					                                        "Added " + EditedCount + " paired components!", "OK");
					break;
				case ActionType.RemovePairedComponents:
					EditorUtility.DisplayDialog("Success!", "Found " + TotalCount + " components,\n" +
					                                        "Removed " + EditedCount + " paired components!", "OK");
					break;
				case ActionType.RemoveUnpairedComponents:
					EditorUtility.DisplayDialog("Success!", "Found " + TotalCount + " components,\n" +
					                                        "Removed " + EditedCount + " unpaired components!", "OK");
					break;
				case ActionType.FindUnpairedComponents:
					EditorUtility.DisplayDialog("Success!", "Found " + TotalCount + " components,\n" +
					                                        "and " + EditedCount + " unpaired components!", "OK");
					break;
				case ActionType.FindPairedComponents:
					EditorUtility.DisplayDialog("Success!", "Found " + TotalCount + " components,\n" +
					                                        "and " + EditedCount + " paired components!", "OK");
					break;
			}						
		}
		#endregion
	}
}

