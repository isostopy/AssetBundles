using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Isostopy.AssetBundles
{
	/// <summary> Componente que puede cargar una lista de AssetBundles desde su url. </summary>
	public class LoadAssetBundlesFromUrl : MonoBehaviour
	{
		/// Si se tienen que cargar los bundles en el start o no.
		[Space][SerializeField]
		private bool loadOnStart = true;

		/// Lista con las url de los bundles que va a cargar este componente.
		public List<string> bundles = new();
		/// Diccionario con los bundles que ya hemos cargado asociados a su url.
		private static Dictionary<string, AssetBundle> loadedBundles = new();

		/// Evento que se dispara al terminar de cargar todos los bundles. 
		[Space]
		public UnityEvent onBundlesLoaded = new();


		// ----------------------------------------------------------------

		private void Start()
		{
			if(loadOnStart)
			{
				LoadBundles();
			}
		}


		// ----------------------------------------------------------------

		/// <summary> Carga todos los AssetBundles de la lista de urls. </summary>
		public void LoadBundles()
		{
			StartCoroutine(LoadAllBundlesRoutine());
		}

		/// Corrutina que carga todos los AssetBundles de la lista.
		private IEnumerator LoadAllBundlesRoutine()
		{
			int bundleCount = bundles.Count;

			// Carga cada AssetBundle y espera a que terminen.
			foreach (string url in bundles)
			{
				StartCoroutine(LoadSingleBundleRoutine( url, () => { bundleCount--; } ));
			}
			while(bundleCount > 0) { yield return null; }

			// Dispara el evento indicando que hemos terminado de cargar los bundles.
			onBundlesLoaded.Invoke();
		}

		/// Corrutina que carga un solo AssetBundle desde la url indicada.
		private IEnumerator LoadSingleBundleRoutine(string url, UnityAction loadEnded)
		{
			// Checkear que el AssetBundle no este ya cargado.
			if (loadedBundles.ContainsKey(url))
			{
				loadEnded();
				yield break;
			}

			// Descargar el AssetBundle desde la url.
			UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
			yield return www.SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError(www.error);
				loadEnded();
				yield break;
			}

			// Checkear que el AssetBundle no este ya cargado.
			// Hay que hacerlo otra vez por si se ha cargado el mismo bundle durante la descarga asincorana.
			if (loadedBundles.ContainsKey(url))
			{
				loadEnded();
				yield break;
			}

			// Cargar el AssetBundle en memoria y añadirlo a la lista de los que ya estan cargados.
			AssetBundle loadedBundle = DownloadHandlerAssetBundle.GetContent(www);
			loadedBundles.Add(url, loadedBundle);

			loadEnded();
		}
	}
}
