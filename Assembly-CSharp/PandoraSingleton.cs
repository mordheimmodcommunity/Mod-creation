using UnityEngine;

public class PandoraSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	protected static T instance;

	public static T Instance
	{
		get
		{
			if ((Object)instance == (Object)null)
			{
				instance = (T)Object.FindObjectOfType(typeof(T));
				if ((Object)instance == (Object)null)
				{
					PandoraDebug.LogError("An instance of " + typeof(T) + " is needed in the scene, but there is none.");
				}
			}
			return instance;
		}
	}

	private void Awake()
	{
		instance = GetComponent<T>();
	}

	private void OnDestroy()
	{
		instance = (T)null;
	}

	public static bool Exists()
	{
		return (Object)instance != (Object)null;
	}
}
