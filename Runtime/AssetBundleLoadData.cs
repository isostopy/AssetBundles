using System.Collections.Generic;
using UnityEngine;

namespace Isostopy.AssetBundles
{
	/// <summary> Informacion necesaria para cargar un asset bundle en distintas plataformas. </summary>

	[CreateAssetMenu(menuName ="Isostopy/AssetBundles/Asset Bundle Load Data", fileName = "new AssetBundleData")]
	public class AssetBundleLoadData : ScriptableObject
	{
		/// <summary> Numero con el que se va a guardar la version del bundle que se descargue desde una url. </summary>
		[Space] public uint version = 1;
		/// <summary> Informacion necesaria segun la plataforma. </summary>
		[Space] public List<PlatformData> dataPerPlatform = new() { new PlatformData() };


		// -------------------------------------------------------------------------

		/// <summary> Devuelve la informacion necesaria para cargar el bundle en la plataforma actual. </summary>
		public PlatformData GetDataForCurrentPlatform()
		{
			return GetDataForPlatform(Application.platform);
		}

		/// <summary> Devuelve la informacion necesaria para cargar el bundle en la plataforma indicada. </summary>
		public PlatformData GetDataForPlatform(RuntimePlatform platform)
		{
			foreach (PlatformData data in dataPerPlatform)
			{
				if (data.platform == platform)
					return data;
			}
			return null;
		}

		/// <summary> Metodo con el cargar el asset bundle para la plaforma actual. </summary>
		public LoadMethod loadMethod
		{
			get
			{
				PlatformData platformLoadData = GetDataForCurrentPlatform();
				if (platformLoadData == null)
					return LoadMethod.none;
				return platformLoadData.loadMethod;
			}
		}

		/// <summary> Path desde el que cargar el asset bundle para la plataforma actual. <br/>
		/// Puede ser el path a un archivo en local o la url a un archivo en un servidor. </summary>
		public string path
		{
			get
			{
				PlatformData platformLoadData = GetDataForCurrentPlatform();
				if (platformLoadData == null)
					return "";
				return platformLoadData.fullPath;
			}
		}


		// ---------------------------------------------------------------------------

		/// <summary> Informacion necesaria para cargar un asset bundle en una plataforma concreta. </summary>
		[System.Serializable]
		public class PlatformData
		{
			[SerializeField] private string editorTitle = "";
			public RuntimePlatform platform = RuntimePlatform.WindowsEditor;
			public LoadMethod loadMethod = LoadMethod.URL;
			public string path = "";

			public string fullPath
			{
				get
				{
					switch (loadMethod)
					{
						case LoadMethod.StreamingAssets:
							var streamingAssetsPath = Application.streamingAssetsPath + "/" + path;
							if (Application.platform == RuntimePlatform.IPhonePlayer)
							{
								// iOS necesita este prefijo delante de la ruta que unity devuelve para streaming assets.
								streamingAssetsPath = "file:/" + streamingAssetsPath;
							}
							return streamingAssetsPath;

						case LoadMethod.PersistentData:
							return Application.persistentDataPath + "/" + path;
						default:
							return path;
					}
				}
			}
		}

		/// <summary> Diferentes sitios desde los que se puede cargar un asset bundle. </summary>
		public enum LoadMethod
		{
			none,
			URL,
			StreamingAssets,
			PersistentData,
			LocalPath
		}
	}
}
