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

		/// Posicion del ScrollView que muestra el contenido de los asset bundles.
		private Vector2 _scrollviewPosition;


		// -----------------------------------------------------------------
		#region Open Window

		[MenuItem("Isostopy/AssetBundle Build Settings")]
		public static void OpenWindow()
		{
			EditorWindow.GetWindow(typeof(AssetBundleBuilder), false, "AssetBundle Build Settings");
		}

		private void OnDisable()
		{
			buildSettings.SaveSettings();
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
			DrawAllBundles();

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

		/// <summary> Dibuja una ScrollView con el contenido de todos los AssetBundles. </summary>
		void DrawAllBundles()
		{
			_scrollviewPosition = EditorGUILayout.BeginScrollView(_scrollviewPosition);
			{
				string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
				foreach (string bundleName in bundleNames)
				{
					DrawBundleContent(bundleName);
				}
			}
			EditorGUILayout.EndScrollView();

			EditorGUILayout.Space();
			if (GUILayout.Button("Remove Unused Bundle Names"))
			{
				AssetDatabase.RemoveUnusedAssetBundleNames();
				Repaint();
			}
		}
		
		/// <summary> Dibuja una lista con el contenido del AssetBundle indicado. </summary>
		void DrawBundleContent(string bundleName)
		{
			EditorGUILayout.LabelField("\t" + bundleName);

			string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
			List<string> drawnDependencies = new();

			if (assetPaths.Length == 0)
			{
				EditorGUILayout.LabelField("\t\t (none)");
				return;
			}

			foreach (string assetPath in assetPaths)
			{
				EditorGUILayout.LabelField("\t\t" + Path.GetFileName(assetPath));

				string[] assetDependeincies = AssetDatabase.GetDependencies(assetPath);
				foreach(string assetDependency in assetDependeincies)
				{
					if (drawnDependencies.Contains(assetDependency))
						continue;
					if (AssetDatabase.GetImplicitAssetBundleName(assetDependency) != "")
						continue;
					if (assetDependency.EndsWith(".cs"))
						continue;

					EditorGUILayout.LabelField("\t\t[ " + Path.GetFileName(assetDependency) + " ]");
					drawnDependencies.Add(assetDependency);
				}
			}
		}

		/// <summary> Dibuja los campos con las settings con las que crear los AssetBundles. </summary>
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
		[MenuItem("Isostopy/Build AssetBundles")]
		public static void BuildAssetBundles()
		{
			buildSettings.SaveSettings();

			if (!Directory.Exists(buildPath))
				Directory.CreateDirectory(buildPath);
			
			BuildPipeline.BuildAssetBundles(buildPath, buildOptions, buildTarget);
			AssetDatabase.Refresh();

			if (!buildPath.StartsWith("Assets/"))
				EditorUtility.RevealInFinder(buildPath);
		}

		#endregion
	}
}
