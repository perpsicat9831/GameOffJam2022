using System.Collections.Generic;
using System.IO;
using AK.Wwise;
using AudioStudio.Editor;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace AudioStudio.Tools
{	
	[CreateAssetMenu(fileName = "DLCBankList", menuName = "AudioStudio/DLC BankList")]
	public class DLCBankList : ScriptableObject
	{
		private static DLCBankList _instance;

		public static DLCBankList Instance
		{
			get
			{
				if (!_instance)
				{
					var path = AsScriptingHelper.CombinePath("Assets", WwisePathSettings.EditorConfigPath, "DLCBankList.asset");
					_instance = AsUnityHelper.GetOrCreateAsset<DLCBankList>(path);
				}
				return _instance;
			}
		}

		public List<Bank> BankList = new List<Bank>();
		public List<Languages> Languages = new List<Languages>();
		public bool PrioritizeLanguage = true;
		private string _outputText;

		private static string TxtFilePath
		{
			get { return AsScriptingHelper.CombinePath(WwisePathSettings.Instance.EditorBankLoadPathFull, "AudioList.txt"); }
		}

		public void GenerateTextFile(bool showDialog)
		{
			_outputText = "";
			var xBanks = SoundBanksInfoReader.SoundBanksNode;
			if (xBanks == null)
			{
				Debug.LogError("SoundBanksInfo.xml load fails!");
				return;
			}
			foreach (var xBank in xBanks.Elements())
			{
				var shortName = AsScriptingHelper.GetXmlElement(xBank, "ShortName");
				var path = AsScriptingHelper.GetXmlElement(xBank, "Path");
				var language = AsScriptingHelper.GetXmlAttribute(xBank, "Language");
				if (!CheckMatchStatus(language, shortName)) continue;
				if (!File.Exists(AsScriptingHelper.CombinePath(WwisePathSettings.GetBankLoadPath(), path))) continue; 
				_outputText += path.Replace('\\', '/') + "\n";
				var xStreams = xBank.Element("ReferencedStreamedFiles");
				if (xStreams == null) continue;
				foreach (var xFile in xStreams.Elements())
				{
					var fileName = AsScriptingHelper.GetXmlAttribute(xFile, "Id");
					if (language == "SFX")
						_outputText += fileName + ".wem\n";
					else
						_outputText += language + "/" + fileName + ".wem\n";
				}
			}					
			AsScriptingHelper.CheckoutLockedFile(TxtFilePath);
			File.WriteAllText(TxtFilePath, _outputText);
			if (showDialog && EditorUtility.DisplayDialog("Success", "Text file generated at " + TxtFilePath, "Open", "OK"))
				System.Diagnostics.Process.Start(TxtFilePath);
		}

		private bool CheckMatchStatus(string language, string bankName)
		{
			var nameMatch = BankList.Any(b => b.Name == bankName);
			if (language == "SFX")
			{
				if (BankList.Any(b => b.Name == bankName)) return true;
			}
			else
			{
				var languageMatch = Languages.Any(l => l.ToString() == language);
				if (PrioritizeLanguage)
				{
					if (languageMatch || nameMatch) return true;
				}
				else
				{
					if (languageMatch && nameMatch) return true;
				}
			}
			return false;
		}
		
		public static void CopyDLCBanks(string targetFolder)
		{
			AsScriptingHelper.CheckDirectoryExist(targetFolder);
			var files = File.ReadAllLines(TxtFilePath);
			foreach (var file in files)
			{
				var originalPath = AsScriptingHelper.CombinePath(WwisePathSettings.Instance.EditorBankLoadPathFull, file);
				if (!File.Exists(originalPath)) continue;
				var targetPath = AsScriptingHelper.CombinePath(targetFolder, file);
				File.Copy(originalPath, targetPath, true);
			}
		}
		
		public static void DeleteDLCBanks(string targetFolder)
		{
			AsScriptingHelper.CheckDirectoryExist(targetFolder);
			
			var excludedFileList = File.ReadAllLines(TxtFilePath);
			for (var i = 0; i < excludedFileList.Length; i++)
			{
				excludedFileList[i] = AsScriptingHelper.CombinePath(targetFolder, excludedFileList[i]);
			}

			var existingFiles = Directory.GetFiles(targetFolder, "*.*", SearchOption.AllDirectories);
			foreach (var filePath in existingFiles)
			{
				if (filePath.EndsWith(".txt") || filePath.EndsWith(".xml") || excludedFileList.Contains(filePath))
					File.Delete(filePath);
			}
		}
	}

	[CustomEditor(typeof(DLCBankList))]
	public class DLCBankListInspector : UnityEditor.Editor
	{
		private DLCBankList _component;

		private void OnEnable()
		{
			_component = target as DLCBankList;		
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			AsGuiDrawer.DrawList(serializedObject.FindProperty("BankList"), 
								"Manually Download Banks:", 
								WwiseObjectType.Soundbank, 
								AddBank);
			AsGuiDrawer.DrawList(serializedObject.FindProperty("Languages"), 
								"Languages:");
			EditorGUILayout.PropertyField(serializedObject.FindProperty("PrioritizeLanguage"));
			serializedObject.ApplyModifiedProperties();
			if (GUILayout.Button("Generate Text File")) _component.GenerateTextFile(true);
			AsGuiDrawer.DrawSaveButton(_component);
		}

		private void AddBank(WwiseObjectReference reference)
		{
			var newBank = new Bank();
			newBank.SetupReference(reference.ObjectName, reference.Guid);			
			_component.BankList.Add(newBank);
		}
	}
}