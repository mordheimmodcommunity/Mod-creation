using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AssetBundleLoader : PandoraSingleton<AssetBundleLoader>
{
	private enum JobType
	{
		ASSET_BUNDLE,
		RESOURCE,
		SCENE
	}

	private class AsyncJob
	{
		public AsyncOperation req;

		public Action<UnityEngine.Object> cb;

		public JobType jobType;

		public bool cache;

		public string name;

		public void Reset()
		{
			req = null;
			cb = null;
			cache = false;
			name = null;
		}
	}

	private class QueuedLoad
	{
		public string assetBundle;

		public string assetName;

		public Action<UnityEngine.Object> cb;

		public bool isScene;

		public void Reset()
		{
			assetBundle = null;
			assetName = null;
			cb = null;
			isScene = false;
		}
	}

	public const string ASSET_BUNDLE_FOLDER = "/asset_bundle/";

	public const string ASSSET_BUNDLE_EXTENSION = ".assetbundle";

	public const int MAX_ITEM_BY_FRAME = 100;

	public Dictionary<string, AssetBundleCreateRequest> assetBundles;

	public Dictionary<string, UnityEngine.Object> cache;

	private Stack<AsyncJob> asyncJobsPool;

	private Stack<QueuedLoad> queuedLoadsPool;

	private List<AsyncJob> jobs;

	private List<QueuedLoad> queues;

	private List<string> loadedScenes;

	private bool initialized;

	private UnityEngine.Object latestObject;

	public bool IsLoading => jobs.Count > 0 || queues.Count > 0;

	private void Awake()
	{
		Init();
	}

	public void Init()
	{
		assetBundles = new Dictionary<string, AssetBundleCreateRequest>();
		cache = new Dictionary<string, UnityEngine.Object>();
		jobs = new List<AsyncJob>(4096);
		queues = new List<QueuedLoad>();
		loadedScenes = new List<string>();
		asyncJobsPool = new Stack<AsyncJob>(4096);
		for (int i = 0; i < 4096; i++)
		{
			asyncJobsPool.Push(new AsyncJob());
		}
		queuedLoadsPool = new Stack<QueuedLoad>(4096);
		for (int j = 0; j < 4096; j++)
		{
			queuedLoadsPool.Push(new QueuedLoad());
		}
		initialized = true;
	}

	public T GetLatestObject<T>() where T : UnityEngine.Object
	{
		UnityEngine.Object @object = latestObject;
		latestObject = null;
		return (T)@object;
	}

	public void Update()
	{
		if (!initialized)
		{
			return;
		}
		int num = 0;
		for (int num2 = jobs.Count - 1; num2 >= 0; num2--)
		{
			if (num <= 100)
			{
				AsyncJob asyncJob = jobs[num2];
				if (asyncJob.req == null)
				{
					PandoraDebug.LogError("Could not load asset " + asyncJob.name);
					PandoraUtils.RemoveBySwap(jobs, num2);
					if (asyncJob.cb != null)
					{
						asyncJob.cb(null);
					}
				}
				else if (asyncJob.req.isDone)
				{
					num++;
					PandoraUtils.RemoveBySwap(jobs, num2);
					UnityEngine.Object @object = null;
					switch (asyncJob.jobType)
					{
					case JobType.ASSET_BUNDLE:
						@object = ((AssetBundleRequest)asyncJob.req).asset;
						break;
					case JobType.RESOURCE:
						@object = ((ResourceRequest)asyncJob.req).asset;
						if (asyncJob.cache)
						{
							cache[asyncJob.name] = @object;
						}
						break;
					case JobType.SCENE:
						loadedScenes.Add(asyncJob.name);
						break;
					}
					asyncJob.cb(@object);
					asyncJob.Reset();
					asyncJobsPool.Push(asyncJob);
				}
			}
		}
		for (int num3 = queues.Count - 1; num3 >= 0; num3--)
		{
			QueuedLoad queuedLoad = queues[num3];
			if (LoadAsync(queuedLoad.assetBundle))
			{
				AsyncJob asyncJob2 = asyncJobsPool.Pop();
				asyncJob2.cb = queuedLoad.cb;
				asyncJob2.name = queuedLoad.assetName;
				if (queuedLoad.isScene)
				{
					asyncJob2.req = SceneManager.LoadSceneAsync(queuedLoad.assetName, LoadSceneMode.Additive);
					asyncJob2.jobType = JobType.SCENE;
				}
				else
				{
					asyncJob2.req = assetBundles[queuedLoad.assetBundle].assetBundle.LoadAssetAsync(queuedLoad.assetName);
					asyncJob2.jobType = JobType.ASSET_BUNDLE;
				}
				if (asyncJob2.req != null)
				{
					jobs.Add(asyncJob2);
					PandoraUtils.RemoveBySwap(queues, num3);
				}
				else
				{
					PandoraDebug.LogError("Could not load asset " + queuedLoad.assetName);
					if (asyncJob2.cb != null)
					{
						asyncJob2.cb(null);
					}
				}
				queuedLoad.Reset();
				queuedLoadsPool.Push(queuedLoad);
			}
		}
	}

	public bool LoadAsync(string assetBundle)
	{
		assetBundle = assetBundle.ToLowerString();
		if (!assetBundles.ContainsKey(assetBundle))
		{
			string path = Application.streamingAssetsPath + "/asset_bundle/" + assetBundle + ".assetbundle";
			AssetBundleCreateRequest value = AssetBundle.LoadFromFileAsync(path);
			assetBundles.Add(assetBundle, value);
			return false;
		}
		if (assetBundles[assetBundle].isDone)
		{
			return true;
		}
		return false;
	}

	public T LoadAsset<T>(string path, AssetBundleId bundleId, string assetName) where T : UnityEngine.Object
	{
		string assetBundle = bundleId.ToLowerString();
		return LoadAsset<T>(path, assetBundle, assetName);
	}

	public T LoadAsset<T>(string path, string assetBundle, string assetName) where T : UnityEngine.Object
	{
		UnityEngine.Object @object = null;
		if (LoadAsync(assetBundle))
		{
			@object = assetBundles[assetBundle].assetBundle.LoadAsset(assetName, typeof(T));
		}
		return (T)@object;
	}

	public IEnumerator LoadAssetAsync<T>(string path, AssetBundleId bundleId, string assetName) where T : UnityEngine.Object
	{
		string assetBundle = bundleId.ToLowerString();
		if (LoadAsync(assetBundle))
		{
			AsyncOperation ao = assetBundles[assetBundle].assetBundle.LoadAssetAsync(assetName, typeof(T));
			yield return ao;
			if (ao != null)
			{
				latestObject = ((AssetBundleRequest)ao).asset;
			}
		}
	}

	public void LoadAssetAsync<T>(string path, AssetBundleId bundleId, string assetName, Action<UnityEngine.Object> callback) where T : UnityEngine.Object
	{
		string assetBundle = bundleId.ToLowerString();
		LoadAssetAsync<T>(path, assetBundle, assetName, callback);
	}

	public void LoadAssetAsync<T>(string path, string assetBundle, string assetName, Action<UnityEngine.Object> callback) where T : UnityEngine.Object
	{
		assetBundle = assetBundle.ToLowerString();
		if (LoadAsync(assetBundle))
		{
			AssetBundleRequest assetBundleRequest = assetBundles[assetBundle].assetBundle.LoadAssetAsync(assetName, typeof(T));
			if (assetBundleRequest != null)
			{
				AsyncJob asyncJob = asyncJobsPool.Pop();
				asyncJob.name = assetName;
				asyncJob.cb = callback;
				asyncJob.req = assetBundleRequest;
				asyncJob.jobType = JobType.ASSET_BUNDLE;
				jobs.Add(asyncJob);
			}
			else
			{
				PandoraDebug.LogError("Could not load asset " + assetName);
				callback?.Invoke(null);
			}
		}
		else
		{
			QueuedLoad queuedLoad = queuedLoadsPool.Pop();
			queuedLoad.assetBundle = assetBundle;
			queuedLoad.assetName = assetName;
			queuedLoad.cb = callback;
			queuedLoad.isScene = false;
			queues.Add(queuedLoad);
		}
	}

	public void LoadSceneAssetAsync(string sceneName, string assetBundleName, Action<UnityEngine.Object> callback)
	{
		assetBundleName = assetBundleName.ToLowerString();
		if (LoadAsync(assetBundleName))
		{
			if (assetBundles[assetBundleName].assetBundle.Contains(sceneName))
			{
				AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
				if (asyncOperation != null)
				{
					AsyncJob asyncJob = asyncJobsPool.Pop();
					asyncJob.cb = callback;
					asyncJob.req = asyncOperation;
					asyncJob.jobType = JobType.SCENE;
					asyncJob.name = sceneName;
					jobs.Add(asyncJob);
				}
				else
				{
					PandoraDebug.LogError("Could not load asset " + sceneName);
					callback?.Invoke(null);
				}
			}
		}
		else
		{
			QueuedLoad queuedLoad = queuedLoadsPool.Pop();
			queuedLoad.assetBundle = assetBundleName;
			queuedLoad.assetName = sceneName;
			queuedLoad.cb = callback;
			queuedLoad.isScene = true;
			queues.Add(queuedLoad);
		}
	}

	public T LoadResource<T>(string name, bool cached = false) where T : UnityEngine.Object
	{
		if (cache.TryGetValue(name, out UnityEngine.Object value))
		{
			return (T)value;
		}
		value = Resources.Load<T>(name);
		if (cached)
		{
			cache[name] = value;
		}
		return (T)value;
	}

	public void LoadResourceAsync<T>(string name, Action<UnityEngine.Object> callback, bool cached = false) where T : UnityEngine.Object
	{
		if (cache.TryGetValue(name, out UnityEngine.Object value))
		{
			callback?.Invoke(value);
			return;
		}
		ResourceRequest resourceRequest = Resources.LoadAsync<T>(name);
		if (resourceRequest != null)
		{
			AsyncJob asyncJob = asyncJobsPool.Pop();
			asyncJob.name = name;
			asyncJob.cb = callback;
			asyncJob.req = resourceRequest;
			asyncJob.jobType = JobType.RESOURCE;
			if (cached)
			{
				asyncJob.cache = true;
			}
			jobs.Add(asyncJob);
		}
		else
		{
			PandoraDebug.LogError("Could not load asset " + name);
			callback?.Invoke(null);
		}
	}

	public void UnloadScenes()
	{
		for (int i = 0; i < loadedScenes.Count; i++)
		{
			SceneManager.UnloadScene(loadedScenes[i]);
		}
		loadedScenes.Clear();
	}

	public void Unload(string assetbundleName)
	{
		if (assetBundles.ContainsKey(assetbundleName))
		{
			assetBundles[assetbundleName].assetBundle.Unload(unloadAllLoadedObjects: false);
			assetBundles.Remove(assetbundleName);
		}
	}

	public IEnumerator UnloadAll(bool allLoadedObjects = false)
	{
		List<AssetBundleCreateRequest> reqs2 = new List<AssetBundleCreateRequest>();
		foreach (KeyValuePair<string, AssetBundleCreateRequest> pair in assetBundles)
		{
			if (pair.Value.assetBundle != null)
			{
				if (pair.Key == "sounds" || pair.Key == "loading")
				{
					reqs2.Add(pair.Value);
				}
				else
				{
					pair.Value.assetBundle.Unload(allLoadedObjects);
				}
			}
		}
		assetBundles.Clear();
		for (int k = 0; k < reqs2.Count; k++)
		{
			assetBundles.Add(reqs2[k].assetBundle.name.Replace(".assetbundle", string.Empty), reqs2[k]);
		}
		reqs2.Clear();
		reqs2 = null;
		for (int j = 0; j < jobs.Count; j++)
		{
			jobs[j].Reset();
			asyncJobsPool.Push(jobs[j]);
		}
		jobs.Clear();
		for (int i = 0; i < queues.Count; i++)
		{
			queues[i].Reset();
			queuedLoadsPool.Push(queues[i]);
		}
		queues.Clear();
		PandoraDebug.LogDebug("UnloadUnusedAssets", "LOADING", this);
		yield return Resources.UnloadUnusedAssets();
		PandoraDebug.LogDebug("GC Collect", "LOADING", this);
		GC.Collect();
		yield return null;
	}
}
