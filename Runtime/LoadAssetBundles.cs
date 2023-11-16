using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Isostopy.AssetBundles
{
	/// <summary> Componente que carga una lista de asset bundles. </summary>
    public class LoadAssetBundles : MonoBehaviour
    {
		/// Bundles que va a cargar este objeto.
        [Space] public List<AssetBundleLoadData> bundlesToLoad = new();
		/// Lista de bundles que estan siendo cargados ahora mismo.
		private Dictionary<AssetBundleLoadData, AssetBundleLoadProgress> loadingBundles = new();

		/// Si se esta o no cargando algun bundle ahora mismo.
		private bool loading = false;
		/// Progreso del proceso de carga de los bundles.
		private float progress = 0;

		/// Si el componente debe cargar los bundles en el start.
		[Space, SerializeField] bool loadOnStart = true;

		/// Evento disparado al terminar de cargar los asset bundles.
		[Space] public UnityEvent onBundlesLoaded = new();


		// ----------------------------------------------------------------------------

		/// <summary> Si estamos o no cargando algun bundle ahora mismo. </summary>
		public bool Loading => loading;

		/// <summary> Progreso del proceso de carga de los bundles. </summary>
		public float Progress => progress;


		// ----------------------------------------------------------------------------

		private void Start()
		{
			if (loadOnStart)
			{
				LoadBundles();
			}
		}

		/// <summary> Carga los asset bundles de la lista. </summary>
		public void LoadBundles()
		{
			if (loading)
			{
				return;
			}

			StartCoroutine(LoadBundlesRoutine());
		}

		/// Corrutina que carga los asset bundle de la lista.
		private IEnumerator LoadBundlesRoutine()
		{
			loading = true;
			progress = 0;

			// Cargar todos los bundles de la lista.
			foreach (AssetBundleLoadData bundle in bundlesToLoad)
			{
				loadingBundles.Add(bundle, null);

				var loadingProgress = AssetBundleLoader.LoadAssetBundle(bundle, (assetBundle) =>
				{
					loadingBundles.Remove(bundle);
				});

				if (loadingBundles.ContainsKey(bundle))
					loadingBundles[bundle] = loadingProgress;
			}

			// Ir actualizando el progreso.
			while (loadingBundles.Count > 0)
			{
				progress = 0;

				foreach (var bundle in bundlesToLoad)
				{
					if (loadingBundles.ContainsKey(bundle))
						progress += loadingBundles[bundle].progress;
					else
						progress += 1;
				}

				progress = bundlesToLoad.Count / progress;
				yield return null;
			}

			// Terminar el proceso de carga.
			progress = 1;
			loading = false;

			onBundlesLoaded.Invoke();
		}
	}
}
