using Pathfinding.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
	public class MordheimTileHandlerHelper : MonoBehaviour
	{
		private List<TileHandler> handlers;

		public float updateInterval;

		private float lastUpdateTime = -999f;

		private List<Bounds> forcedReloadBounds = new List<Bounds>();

		private void OnEnable()
		{
			NavmeshCut.OnDestroyCallback += HandleOnDestroyCallback;
		}

		private void OnDisable()
		{
			NavmeshCut.OnDestroyCallback -= HandleOnDestroyCallback;
		}

		public void DiscardPending()
		{
			List<NavmeshCut> all = NavmeshCut.GetAll();
			for (int i = 0; i < all.Count; i++)
			{
				if (all[i].RequiresUpdate())
				{
					all[i].NotifyUpdated();
				}
			}
		}

		private void Awake()
		{
			if (UnityEngine.Object.FindObjectsOfType(typeof(TileHandlerHelper)).Length > 1)
			{
				Debug.LogError("There should only be one TileHandlerHelper per scene. Destroying.");
				UnityEngine.Object.Destroy(this);
				return;
			}
			handlers = new List<TileHandler>();
			if (AstarPath.active == null || AstarPath.active.astarData.recastGraph == null)
			{
				Debug.LogWarning("No AstarPath object in the scene or no RecastGraph on that AstarPath object");
			}
			foreach (RecastGraph item in AstarPath.active.astarData.FindGraphsOfType(typeof(RecastGraph)))
			{
				TileHandler tileHandler = new TileHandler(item);
				tileHandler.CreateTileTypesFromGraph();
				handlers.Add(tileHandler);
			}
		}

		private void HandleOnDestroyCallback(NavmeshCut obj)
		{
			forcedReloadBounds.Add(obj.LastBounds);
			lastUpdateTime = -999f;
		}

		private void Update()
		{
			if (updateInterval != -1f && !(Time.realtimeSinceStartup - lastUpdateTime < updateInterval) && handlers.Count != 0)
			{
				ForceUpdate();
			}
		}

		public void ForceUpdate()
		{
			AstarPath.active.maxGraphUpdateFreq = 1.5f;
			if (handlers.Count == 0)
			{
				throw new Exception("Cannot update graphs. No TileHandler. Do not call this method in Awake.");
			}
			lastUpdateTime = Time.realtimeSinceStartup;
			List<NavmeshCut> all = NavmeshCut.GetAll();
			if (forcedReloadBounds.Count == 0)
			{
				int num = 0;
				for (int i = 0; i < all.Count; i++)
				{
					if (all[i].RequiresUpdate())
					{
						num++;
						break;
					}
				}
				if (num == 0)
				{
					return;
				}
			}
			for (int j = 0; j < handlers.Count; j++)
			{
				TileHandler tileHandler = handlers[j];
				bool flag = tileHandler.StartBatchLoad();
				if (!flag)
				{
					continue;
				}
				for (int k = 0; k < forcedReloadBounds.Count; k++)
				{
					tileHandler.ReloadInBounds(forcedReloadBounds[k]);
				}
				forcedReloadBounds.Clear();
				for (int l = 0; l < all.Count; l++)
				{
					if (all[l].enabled)
					{
						if (all[l].RequiresUpdate())
						{
							tileHandler.ReloadInBounds(all[l].LastBounds);
							tileHandler.ReloadInBounds(all[l].GetBounds());
						}
					}
					else if (all[l].RequiresUpdate())
					{
						tileHandler.ReloadInBounds(all[l].LastBounds);
					}
				}
				if (flag)
				{
					tileHandler.EndBatchLoad();
				}
			}
			for (int m = 0; m < all.Count; m++)
			{
				if (all[m].RequiresUpdate())
				{
					all[m].NotifyUpdated();
				}
			}
		}
	}
}
