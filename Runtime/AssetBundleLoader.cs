using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace Isostopy.AssetBundles
{
	/// <summary> Clase que se encarga de cargar assets bundles en memoria. </summary>
	/// Esta clase estatica existe para tener acceso a diferentes fuenciones con las que cargar un asset bundle de forma independiente de la escena.
	public static class AssetBundleLoader
	{
		/// Diccionario con los bundles que ya hemos cargado asociados a su url o path.
		private static Dictionary<string, AssetBundle> loadedBundles = new();
		/// Componente que usamos para iniciar las corrutinas.
		private static MonoBehaviour corroutiner = null;


		// ----------------------------------------------------------------------------

		/// <summary> Carga un asset bundle dado un objeto <see cref="AssetBundleLoadData"/>. </summary>
		public static AssetBundleLoadProgress LoadAssetBundle(AssetBundleLoadData loadData, UnityAction<AssetBundle> completed = null)
		{
			var dataForCurrentPlatform = loadData.GetDataForCurrentPlatform();

			// Comprobar que hay informacion para cargar el bundle para la plataforma actual.
			if (dataForCurrentPlatform == null)
			{
				Debug.LogError("LoadData [" + loadData.name + "] no contiene información para cargar el asset bundle en la la palataforma actual [" + Application.platform + "]\n" +
					" El AssetBundle no se ha cargado");

				completed?.Invoke(null);
				return new AssetBundleLoadProgress();
			}

			// Cargar el asset bundle.
			return LoadAssetBundleFromUrl(loadData.path, loadData.version, completed);
		}


		/// <summary> Carga un asset bundle desde una url. </summary>
		public static AssetBundleLoadProgress LoadAssetBundleFromUrl(string url, uint version = 0, UnityAction<AssetBundle> completed = null)
		{
			var loadProgress = new AssetBundleLoadProgress();
			StartCoroutine(LoadAssetBundleFromUrlRoutine(url, version, loadProgress, completed));
			return loadProgress;
		}

		public static AssetBundleLoadProgress LoadAssetBundleFromUrl(string url, UnityAction<AssetBundle> completed)
		{
			return LoadAssetBundleFromUrl(url, 0, completed);
		}


		/// <summary> Carga un asset bundle desde un archivo local. </summary>
		public static AssetBundleLoadProgress LoadAssetBundleFromFile(string path, UnityAction<AssetBundle> completed = null)
		{
			var loadProgress = new AssetBundleLoadProgress();
			StartCoroutine(LoadAssetBundleFromFileRoutine(path, loadProgress, completed));
			return loadProgress;
		}


		// ----------------------------------------------------------------------------

		/// Corrutina que carga un asset bundle desde una url.
		public static IEnumerator LoadAssetBundleFromUrlRoutine(string url, uint version, AssetBundleLoadProgress loadProgressRef, UnityAction<AssetBundle> completed)
		{
			// Checkear que el AssetBundle no este ya cargado.
			if (loadedBundles.ContainsKey(url))
			{
				completed?.Invoke(loadedBundles[url]);
				yield break;
			}

			// Descargar el AssetBundle desde la url.
			UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url, version, 0);
			yield return loadProgressRef.asyncOperation = request.SendWebRequest();

			if (request.result != UnityWebRequest.Result.Success)
			{
				completed?.Invoke(null);
				yield break;
			}

			// Checkear que el AssetBundle no este ya cargado.
			// Hay que hacerlo otra vez por si otro proceso ha cargado el mismo bundle durante la descarga asincorana.
			if (loadedBundles.ContainsKey(url))
			{
				completed?.Invoke(loadedBundles[url]);
				yield break;
			}

			// Cargar el AssetBundle en memoria y añadirlo a la lista de los que ya estan cargados.
			AssetBundle loadedBundle = DownloadHandlerAssetBundle.GetContent(request);
			loadedBundles.Add(url, loadedBundle);

			completed?.Invoke(loadedBundle);
		}

		/// Corrutina que carga un asset bundle desde un archivo local.
		public static IEnumerator LoadAssetBundleFromFileRoutine(string path, AssetBundleLoadProgress loadProgressRef, UnityAction<AssetBundle> completed)
		{
			// Checkear que el AssetBundle no este ya cargado.
			if (loadedBundles.ContainsKey(path))
			{
				completed?.Invoke(loadedBundles[path]);
				yield break;
			}

			// Cargar un bundle desde su path local.
			AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
			loadProgressRef.asyncOperation = request;
			yield return request;

			AssetBundle loadedBundle = request.assetBundle;
			if (loadedBundle == null)
			{
				completed?.Invoke(null);
				yield break;
			}

			// Añadirlo a la lista de los que ya estan cargados.
			loadedBundles.Add(path, loadedBundle);

			completed?.Invoke(loadedBundle);
		}


		// ----------------------------------------------------------------------------

		private static Coroutine StartCoroutine(IEnumerator routine)
		{
			if (corroutiner == null)
			{
				var go = new GameObject("Asset Bundle Loader");
				corroutiner = go.AddComponent<AssetBundleLoaderComponent>();
			}
			return corroutiner.StartCoroutine(routine);
		}

		private class AssetBundleLoaderComponent : MonoBehaviour
		{
			private void Start() => DontDestroyOnLoad(this);
		}
	}
}
