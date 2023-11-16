using UnityEditor;

namespace Isostopy.AssetBundles.Editor
{
	/// <summary> Clase que guarda en el projecto de Unity las settings con las que crear los AssetBundles. </summary>
	
	[FilePath("ProjectSettings/AssetBundleBuilderSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	public class AssetBundleBuilderSettings : ScriptableSingleton<AssetBundleBuilderSettings>
	{
		/// <summary> Path relativo a la carpeta del proyecto donde crear los AssetBundles. </summary>
		public static string BuildPath
		{
			get => instance.buildPath;
			set => instance.buildPath = value;
		}
		public string buildPath = "Assets/StreamingAssets";

		/// <summary> Opciones con las que crear los AssetBundles. </summary>
		public static BuildAssetBundleOptions BuildOptions
		{
			get => instance.buildOptions;
			set => instance.buildOptions = value;
		}
		public BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.None;

		/// <summary> Plataforma para la que crear los AssetBundles. </summary>
		public static BuildTarget BuildTarget
		{
			get
			{
				if (instance.buildTarget == 0)
					instance.buildTarget = BuildTarget.StandaloneWindows;
				return instance.buildTarget;
			}
			set => instance.buildTarget = value;
		}
		public BuildTarget buildTarget = 0;


		// -----------------------------------------------------------------

		/// <summary> Guarda las settings en un archivo para que se conserven entre sesiones. </summary>
		public void Save()
		{
			Save(true);
		}
	}
}
