using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AudioStudio.Tools
{
	internal abstract class AsSearchers : EditorWindow
	{
        #region Field
		//-----------------------------------------------------------------------------------------
        internal bool searchPrefab;
		internal bool searchScene;

		protected string _defaultSearchingPath = "Assets";
		protected List<string> _paths = new List<string>();
		/// <summary>
		/// Count total edited objects
		/// </summary>
		protected int EditedCount;
		/// <summary>
		/// Count total searched objects
		/// </summary>
		protected int TotalCount;
		public string _importedXmlPath;
		/// <summary>
		/// Root XML Element
		/// </summary>
		public XElement XRoot;

		protected string XmlDocDirectory
		{
			get { return WwisePathSettings.EditorConfigPathFull; }
		}

		protected virtual string DefaultXmlPath
		{
			get { return ""; }
		}

		//-----------------------------------------------------------------------------------------
		#endregion

		#region init
		//-----------------------------------------------------------------------------------------
		private void Awake()
		{
			searchPrefab = true;
			searchScene = false;
			XRoot = new XElement("Root");
		}
		//-----------------------------------------------------------------------------------------
		#endregion

		#region Management
		//-----------------------------------------------------------------------------------------
		internal void OpenXmlFile()
		{
			var filePath = EditorUtility.OpenFilePanel("Open XML File", XmlDocDirectory, "xml");
			Process.Start(filePath);

			/*
			if (File.Exists(DefaultXmlPath))
				Process.Start(DefaultXmlPath);
			else
				EditorUtility.DisplayDialog("Error", "Default xml file does not exist!", "OK");
			*/
		}

		/// <summary>
		/// reset fields to prevent duplicate
		/// </summary>
		protected void CleanUp()
		{
			XRoot = new XElement("Root");
			_importedXmlPath = "";
			TotalCount = 0;
			EditedCount = 0;
		}
		
		protected bool ReadXmlData(string fileName = "")
		{
			if (string.IsNullOrEmpty(fileName))
				LoadOrCreateXmlDoc();
			else
			{
				if (!File.Exists(fileName))
					return false;
				XRoot = XDocument.Load(fileName).Root;
				if (XRoot == null)
				{
					EditorUtility.DisplayDialog("Error", "Xml format is invalid!", "OK");
					return false;
				}
			}
			return true;
		}

		protected void LoadOrCreateXmlDoc()
		{
			try
			{
				XRoot = XDocument.Load(DefaultXmlPath).Root;
			}
#pragma warning disable 168
			catch (FileNotFoundException e)
#pragma warning restore 168
			{
				XRoot = new XElement("Root");
				AsScriptingHelper.WriteXml(DefaultXmlPath, XRoot);
			}
		}

		protected bool FindFiles(Action<string> parser, 
								 string progressBarTitle, 
								 string extension, 
								 string searchFolder = "")
		{
			try
			{
				if (searchFolder == "")
					searchFolder = _defaultSearchingPath;
				EditorUtility.DisplayCancelableProgressBar
					(progressBarTitle, "Loading assets...", 0);
				string[] allFilesPaths = Directory.GetFiles
					(searchFolder, extension, SearchOption.AllDirectories);
				for (var i = 0; i < allFilesPaths.Length; i++)
				{
					var shortPath = AsScriptingHelper.ShortPath(allFilesPaths[i]);
					if (EditorUtility.DisplayCancelableProgressBar(progressBarTitle, 
						shortPath, (i + 1) * 1.0f / allFilesPaths.Length))
					{
						EditorUtility.ClearProgressBar();
						return false;
					}
					parser(shortPath);
				}
				EditorUtility.ClearProgressBar();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				EditorUtility.ClearProgressBar();
			}
			return true;
		}

		public string GetGameObjectPath(Transform transform, Transform until = null)
		{
			if (transform.parent == null || transform == until || transform.parent.name.StartsWith("Prefab Mode in Context"))
				return transform.name;
			var fullPath = GetGameObjectPath(transform.parent, until) + "/" + transform.name;
			//in Unity 2018.4 and later, opening UI prefabs would create a temp canvas
			if (fullPath.StartsWith("Canvas (Environment)"))
				fullPath = fullPath.Substring(21);
			return fullPath;
		}

		/// <summary>
		/// Get game object from the path in the prefab
		/// </summary>
		/// <param name="go"></param>
		/// <param name="gameObjectPath"></param>
		/// <returns></returns>
		protected GameObject GetGameObject(GameObject go, string gameObjectPath)
		{
			if (go.name == gameObjectPath)
				return go;
			var names = gameObjectPath.Split('/');
			return go.name != names[0] ? null : GetGameObject(go, names, 1);
		}

		private GameObject GetGameObject(GameObject go, string[] names, int index)
		{
			if (index > names.Length) return null;

			foreach (Transform child in go.transform)
			{
				if (child.gameObject.name == names[index])
				{
					return index == names.Length - 1 ? 
						child.gameObject : GetGameObject(child.gameObject, names, index + 1);
				}
			}

			return null;
		}

		protected GameObject GetRootGameObject(Transform trans)
		{
			return trans.parent ? GetRootGameObject(trans.parent) : trans.gameObject;
		}

        //-----------------------------------------------------------------------------------------
        #endregion

        #region AssetOperations
        //-----------------------------------------------------------------------------------------
        internal void SaveSelectedAssets(IEnumerable<string> assetPaths, Action<string> parser)
		{
			foreach (var assetPath in assetPaths)
			{
				parser(assetPath);
			}
		}
		
		internal void ExportSelectedAssets(IEnumerable<string> assetPaths, Action<string> parser)
		{
			CleanUp();
			var xmlPath = EditorUtility.SaveFilePanel("Export to", XmlDocDirectory, "Selection.xml", "xml");
			if (string.IsNullOrEmpty(xmlPath)) return;
			foreach (var assetPath in assetPaths)
			{
				parser(assetPath);
			}
			AsScriptingHelper.WriteXml(xmlPath, XRoot);
			if (EditorUtility.DisplayDialog("Success!", "Found " + TotalCount + " items!", "Open", "OK"))
				Process.Start(xmlPath);
		}
		
		internal void RevertSelectedAssets(IEnumerable<string> assetPaths, Action<string> importer)
		{
			foreach (var assetPath in assetPaths)
			{
				importer(assetPath);
			}
		}
		
		internal void RemoveSelectedAssets(IEnumerable<string> assetPaths, Action<string> remover)
		{
			foreach (var assetPath in assetPaths)
			{
				remover(assetPath);
			}
		}

		//-----------------------------------------------------------------------------------------
		#endregion
		
		
	}		
}