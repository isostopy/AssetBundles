using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Isostopy.AssetBundles.Editor
{
	/// <summary> Ventana del editor de Unity que permite crear los AssetBundles. </summary>
	public class AssetBundleBuilder : EditorWindow
	{
		/// Settings con las que vamos a crear los AssetBundles.
		public static AssetBundleBuilderSettings buildSettings => AssetBundleBuilderSettings.instance;
		private static string buildPath
		{
			get => AssetBundleBuilderSettings.BuildPath;
			set => AssetBundleBuilderSettings.BuildPath = value;
		}
		private static BuildAssetBundleOptions buildOptions
		{
			get => AssetBundleBuilderSettings.BuildOptions;
			set => AssetBundleBuilderSettings.BuildOptions = value;
		}
		private static BuildTarget buildTarget
		{
			get => AssetBundleBuilderSettings.BuildTarget;
			set => AssetBundleBuilderSettings.BuildTarget = value;
		}

		/// Lista de string con el contenido de los asset bundle que vamos a mostar en la ventana.
		private List<string> bundlesContent = new();
		/// Posicion del ScrollView que muestra el contenido de los asset bundles.
		private Vector2 _scrollviewPosition;


		// -----------------------------------------------------------------
		#region Open Window

		/// <summary> Abre la ventana para elgir las settings con las que se crean los asset bundles. </summary>
		[MenuItem("Isostopy/AssetBundles/AssetBundle Build Settings")]
		public static void OpenWindow()
		{
			EditorWindow.GetWindow(typeof(AssetBundleBuilder), false, "AssetBundle Build Settings");
		}

		private void OnLostFocus()
		{
			// Cada vez que clickes fuera de la ventana guardamos las settings.
			buildSettings.Save();
		}

		private void OnFocus()
		{
			FillBundleContentList();
		}

		// --------------------------------

		/// Rehacer la lista de contenidos de los asset bundle.
		void FillBundleContentList()
		{
			bundlesContent.Clear();

			string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
			if (bundleNames.Length == 0)
			{
				bundlesContent.Add("\t (none)");
				return;
			}
			foreach (string bundleName in bundleNames)
			{
				AddBundleContentToList(bundleName);
			}
		}

		/// Añadir a la lista el contenido de un solo asset bundle.
		void AddBundleContentToList(string bundleName)
		{
			bundlesContent.Add("\t" + bundleName);

			string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
			if (assetPaths.Length == 0)
			{
				bundlesContent.Add("\t\t (none)");
				return;
			}

			List<string> drawnDependencies = new();
			foreach (string assetPath in assetPaths)
			{
				bundlesContent.Add("\t\t" + Path.GetFileName(assetPath));

				//string[] assetDependencies = AssetDatabase.GetDependencies(assetPath);
				//foreach (string assetDependency in assetDependencies)
				//{
				//	if (drawnDependencies.Contains(assetDependency))
				//		continue;
				//	if (AssetDatabase.GetImplicitAssetBundleName(assetDependency) != "")
				//		continue;
				//	if (assetDependency.EndsWith(".cs"))
				//		continue;

				//	bundlesContent.Add("\t\t <i> [ " + Path.GetFileName(assetDependency) + " ] </i>"); 
				//	drawnDependencies.Add(assetDependency);
				//}
			}
		}

		#endregion


		// -----------------------------------------------------------------
		#region Draw Window

		private void OnGUI()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("-----------------------", GUI.skin.horizontalSlider);

			EditorGUILayout.LabelField("AssetBundles in this project");
			EditorGUILayout.Space();
			DrawBundlesContent();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("-----------------------", GUI.skin.horizontalSlider);
			EditorGUILayout.Space();

			DrawBuildSettings();

			EditorGUILayout.Space();

			if (GUILayout.Button("Build Asset Bundles"))
			{
				BuildAssetBundles();
			}

			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
		}

		/// Dibuja una ScrollView con el contenido de todos los AssetBundles.
		void DrawBundlesContent()
		{
			_scrollviewPosition = EditorGUILayout.BeginScrollView(_scrollviewPosition);
			{
				GUIStyle richTextStyle = new GUIStyle(GUI.skin.label) { richText = true };

				foreach (string line in bundlesContent)
				{
					EditorGUILayout.LabelField(line, richTextStyle);
				}
			}
			EditorGUILayout.EndScrollView();

			EditorGUILayout.Space();

			if (GUILayout.Button("Remove Unused Bundle Names"))
			{
				AssetDatabase.RemoveUnusedAssetBundleNames();
				FillBundleContentList();
				Repaint();
			}
		}

		/// Dibuja los campos con las settings con las que crear los AssetBundles.
		void DrawBuildSettings()
		{
			GUILayout.BeginHorizontal();
			{
				buildPath = EditorGUILayout.TextField("Build Path", buildPath);
				if (GUILayout.Button("    Browse    ", GUILayout.ExpandWidth(false)))
				{
					string selectedPath = EditorUtility.OpenFolderPanel("Build Asset Bundles", buildPath, "");
					if (!string.IsNullOrEmpty(selectedPath))
						buildPath = selectedPath;
				}
			}
			GUILayout.EndHorizontal();

			buildOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField("Build Options", buildOptions);
			buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", AssetBundleBuilderSettings.instance.buildTarget);
		}

		#endregion


		// -----------------------------------------------------------------
		#region Build AssetBundles

		/// <summary> Crea los AssetBundles con las settings guardadas. </summary>
		[MenuItem("Assets/Build AssetBundles")]
		[MenuItem("Isostopy/AssetBundles/Build AssetBundles")]
		public static void BuildAssetBundles()
		{
			buildSettings.Save();

			if (!Directory.Exists(buildPath))
				Directory.CreateDirectory(buildPath);
			
			BuildPipeline.BuildAssetBundles(buildPath, buildOptions, buildTarget);
			AssetDatabase.Refresh();

			if (!buildPath.StartsWith("Assets/"))
				EditorUtility.RevealInFinder(buildPath);
		}

		#endregion


		// --------------------------------
		#region Clear Cache

		/// <summary> Borra los assets bundle en cache. </summary>
		[MenuItem("Isostopy/AssetBundles/Clear AssetBundles Cache")]
		public static void ClearCache()
		{
			Caching.ClearCache();
		}

		#endregion
	}
}
