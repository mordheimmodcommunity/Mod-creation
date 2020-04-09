using UnityEngine;

public class KGFConsoleCommands
{
	private static KGFConsoleCommands itsInstance;

	public static void AddCommands()
	{
		itsInstance = new KGFConsoleCommands();
		KGFConsole.AddCommand("u.t.hpe", "HeightmapPixelError", "unity.terrain", itsInstance, "TerrainHeightmapPixelError");
		KGFConsole.AddCommand("u.t.bd", "BasemapDistance", "unity.terrain", itsInstance, "TerrainBasemapDistance");
		KGFConsole.AddCommand("u.t.cs", "CastShadows", "unity.terrain", itsInstance, "TerrainCastShadows");
		KGFConsole.AddCommand("u.t.dodi", "DetailObjectDistance", "unity.terrain", itsInstance, "TerrainDetailObjectDistance");
		KGFConsole.AddCommand("u.t.dode", "DetailObjectDensity", "unity.terrain", itsInstance, "TerrainDetailObjectDensity");
		KGFConsole.AddCommand("u.t.td", "TreeDistance", "unity.terrain", itsInstance, "TerrainTreeDistance");
		KGFConsole.AddCommand("u.t.tbd", "TreeBillboardDistance", "unity.terrain", itsInstance, "TerrainTreeBillboardDistance");
		KGFConsole.AddCommand("u.t.tcfl", "TreeCrossFadeLength", "unity.terrain", itsInstance, "TerrainTreeCrossFadeLength");
		KGFConsole.AddCommand("u.t.tmfl", "TreeMaximumFullLODCount", "unity.terrain", itsInstance, "TerrainTreeMaximumFullLODCount");
		KGFConsole.AddCommand("u.t.hml", "HeightmapMaximumLOD", "unity.terrain", itsInstance, "TerrainHeightmapMaximumLOD");
		KGFConsole.AddCommand("u.a.q", "Quit", "unity.application", itsInstance, "ApplicationQuit");
	}

	public void TerrainHeightmapPixelError(float theHeightmapPixelError)
	{
		if (Terrain.activeTerrain != null)
		{
			Terrain.activeTerrain.heightmapPixelError = theHeightmapPixelError;
		}
	}

	public void TerrainBasemapDistance(float theBasemapDistance)
	{
		if (Terrain.activeTerrain != null)
		{
			Terrain.activeTerrain.basemapDistance = theBasemapDistance;
		}
	}

	public void TerrainCastShadows(bool theCastShadows)
	{
		if (Terrain.activeTerrain != null)
		{
			Terrain.activeTerrain.castShadows = theCastShadows;
		}
	}

	public void TerrainDetailObjectDistance(float theDetailObjectDistance)
	{
		if (Terrain.activeTerrain != null)
		{
			Terrain.activeTerrain.detailObjectDistance = theDetailObjectDistance;
		}
	}

	public void TerrainDetailObjectDensity(float theDetailObjectDensity)
	{
		if (Terrain.activeTerrain != null)
		{
			Terrain.activeTerrain.detailObjectDensity = theDetailObjectDensity;
		}
	}

	public void TerrainTreeDistance(float theTreeDistance)
	{
		if (Terrain.activeTerrain != null)
		{
			Terrain.activeTerrain.treeDistance = theTreeDistance;
		}
	}

	public void TerrainTreeBillboardDistance(float theTreeBillboardDistance)
	{
		if (Terrain.activeTerrain != null)
		{
			Terrain.activeTerrain.treeBillboardDistance = theTreeBillboardDistance;
		}
	}

	public void TerrainTreeCrossFadeLength(float theTreeCrossFadeLength)
	{
		if (Terrain.activeTerrain != null)
		{
			Terrain.activeTerrain.treeCrossFadeLength = theTreeCrossFadeLength;
		}
	}

	public void TerrainTreeMaximumFullLODCount(int theTreeMaximumFullLODCount)
	{
		if (Terrain.activeTerrain != null)
		{
			Terrain.activeTerrain.treeMaximumFullLODCount = theTreeMaximumFullLODCount;
		}
	}

	public void TerrainHeightmapMaximumLOD(int theHeightmapMaximumLOD)
	{
		if (Terrain.activeTerrain != null)
		{
			Terrain.activeTerrain.heightmapMaximumLOD = theHeightmapMaximumLOD;
		}
	}

	public void ApplicationQuit()
	{
		Application.Quit();
	}
}
