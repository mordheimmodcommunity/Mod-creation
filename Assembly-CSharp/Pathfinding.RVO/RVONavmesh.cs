using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.RVO
{
	[AddComponentMenu("Pathfinding/Local Avoidance/RVO Navmesh")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_r_v_o_1_1_r_v_o_navmesh.php")]
	public class RVONavmesh : GraphModifier
	{
		public float wallHeight = 5f;

		private readonly List<ObstacleVertex> obstacles = new List<ObstacleVertex>();

		private Simulator lastSim;

		public override void OnPostCacheLoad()
		{
			OnLatePostScan();
		}

		public override void OnLatePostScan()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			RemoveObstacles();
			NavGraph[] graphs = AstarPath.active.graphs;
			RVOSimulator rVOSimulator = UnityEngine.Object.FindObjectOfType<RVOSimulator>();
			if (rVOSimulator == null)
			{
				throw new NullReferenceException("No RVOSimulator could be found in the scene. Please add one to any GameObject");
			}
			Simulator simulator = rVOSimulator.GetSimulator();
			for (int i = 0; i < graphs.Length; i++)
			{
				RecastGraph recastGraph = graphs[i] as RecastGraph;
				if (recastGraph != null)
				{
					RecastGraph.NavmeshTile[] tiles = recastGraph.GetTiles();
					foreach (RecastGraph.NavmeshTile ng in tiles)
					{
						AddGraphObstacles(simulator, ng);
					}
				}
				else
				{
					INavmesh navmesh = graphs[i] as INavmesh;
					if (navmesh != null)
					{
						AddGraphObstacles(simulator, navmesh);
					}
				}
			}
			simulator.UpdateObstacles();
		}

		public void RemoveObstacles()
		{
			if (lastSim != null)
			{
				Simulator simulator = lastSim;
				lastSim = null;
				for (int i = 0; i < obstacles.Count; i++)
				{
					simulator.RemoveObstacle(obstacles[i]);
				}
				obstacles.Clear();
			}
		}

		public void AddGraphObstacles(Simulator sim, INavmesh ng)
		{
			if (obstacles.Count > 0 && lastSim != null && lastSim != sim)
			{
				Debug.LogError("Simulator has changed but some old obstacles are still added for the previous simulator. Deleting previous obstacles.");
				RemoveObstacles();
			}
			lastSim = sim;
			int[] uses = new int[20];
			Dictionary<int, int> outline = new Dictionary<int, int>();
			Dictionary<int, Int3> vertexPositions = new Dictionary<int, Int3>();
			HashSet<int> hasInEdge = new HashSet<int>();
			ng.GetNodes(delegate(GraphNode _node)
			{
				TriangleMeshNode triangleMeshNode = _node as TriangleMeshNode;
				uses[0] = (uses[1] = (uses[2] = 0));
				if (triangleMeshNode != null)
				{
					for (int j = 0; j < triangleMeshNode.connections.Length; j++)
					{
						TriangleMeshNode triangleMeshNode2 = triangleMeshNode.connections[j] as TriangleMeshNode;
						if (triangleMeshNode2 != null)
						{
							int num2 = triangleMeshNode.SharedEdge(triangleMeshNode2);
							if (num2 != -1)
							{
								uses[num2] = 1;
							}
						}
					}
					for (int k = 0; k < 3; k++)
					{
						if (uses[k] == 0)
						{
							int i2 = k;
							int i3 = (k + 1) % triangleMeshNode.GetVertexCount();
							outline[triangleMeshNode.GetVertexIndex(i2)] = triangleMeshNode.GetVertexIndex(i3);
							hasInEdge.Add(triangleMeshNode.GetVertexIndex(i3));
							vertexPositions[triangleMeshNode.GetVertexIndex(i2)] = triangleMeshNode.GetVertex(i2);
							vertexPositions[triangleMeshNode.GetVertexIndex(i3)] = triangleMeshNode.GetVertex(i3);
						}
					}
				}
				return true;
			});
			for (int i = 0; i < 2; i++)
			{
				bool flag = i == 1;
				foreach (int item2 in new List<int>(outline.Keys))
				{
					if (flag || !hasInEdge.Contains(item2))
					{
						int key = item2;
						List<Vector3> list = new List<Vector3>();
						list.Add((Vector3)vertexPositions[key]);
						while (outline.ContainsKey(key))
						{
							int num = outline[key];
							outline.Remove(key);
							Vector3 item = (Vector3)vertexPositions[num];
							list.Add(item);
							if (num == item2)
							{
								break;
							}
							key = num;
						}
						if (list.Count > 1)
						{
							sim.AddObstacle(list.ToArray(), wallHeight, flag);
						}
					}
				}
			}
		}
	}
}
