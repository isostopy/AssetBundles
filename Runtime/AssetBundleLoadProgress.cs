using UnityEngine;

namespace Isostopy.AssetBundles
{
	/// <summary> Referencia al proceso de carga asincrona de un asset bundle. </summary>
	public class AssetBundleLoadProgress
	{
		/// <summary> Operacion asincrona que esta cargando el asset bundle. </summary>
		public AsyncOperation asyncOperation = null;

		/// <summary> Valor entre 0 y 1 que indica que progreso de la descarga del asset bundle. </summary>
		public float progress
		{
			get
			{
				if (asyncOperation == null)
					return 1;
				return asyncOperation.progress;
			}
		}

		/// <summary> ¿Ha terminado de cargarse el asset bundle? </summary>
		public bool isDone
		{
			get
			{
				if (asyncOperation == null)
					return true;
				return asyncOperation.isDone;
			}
		}
	}
}
