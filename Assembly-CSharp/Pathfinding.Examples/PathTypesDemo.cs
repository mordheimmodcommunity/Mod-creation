using Pathfinding.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Examples
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_path_types_demo.php")]
	public class PathTypesDemo : MonoBehaviour
	{
		public enum DemoMode
		{
			ABPath,
			MultiTargetPath,
			RandomPath,
			FleePath,
			ConstantPath,
			FloodPath,
			FloodPathTracer
		}

		public DemoMode activeDemo;

		public Transform start;

		public Transform end;

		public Vector3 pathOffset;

		public Material lineMat;

		public Material squareMat;

		public float lineWidth;

		public RichAI[] agents;

		public int searchLength = 1000;

		public int spread = 100;

		public float aimStrength;

		private Path lastPath;

		private List<GameObject> lastRender = new List<GameObject>();

		private List<Vector3> multipoints = new List<Vector3>();

		private Vector2 mouseDragStart;

		private float mouseDragStartTime;

		private FloodPath lastFlood;

		private void Update()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 origin = ray.origin;
			Vector3 direction = ray.direction;
			Vector3 origin2 = ray.origin;
			float y = origin2.y;
			Vector3 direction2 = ray.direction;
			Vector3 vector = origin + direction * (y / (0f - direction2.y));
			end.position = vector;
			if (Input.GetMouseButtonDown(0))
			{
				mouseDragStart = Input.mousePosition;
				mouseDragStartTime = Time.realtimeSinceStartup;
			}
			if (Input.GetMouseButtonUp(0))
			{
				Vector2 a = Input.mousePosition;
				if ((a - mouseDragStart).sqrMagnitude > 25f && Time.realtimeSinceStartup - mouseDragStartTime > 0.3f)
				{
					Rect rect = Rect.MinMaxRect(Mathf.Min(mouseDragStart.x, a.x), Mathf.Min(mouseDragStart.y, a.y), Mathf.Max(mouseDragStart.x, a.x), Mathf.Max(mouseDragStart.y, a.y));
					RichAI[] array = Object.FindObjectsOfType(typeof(RichAI)) as RichAI[];
					List<RichAI> list = new List<RichAI>();
					for (int i = 0; i < array.Length; i++)
					{
						Vector2 point = Camera.main.WorldToScreenPoint(array[i].transform.position);
						if (rect.Contains(point))
						{
							list.Add(array[i]);
						}
					}
					agents = list.ToArray();
				}
				else
				{
					if (Input.GetKey(KeyCode.LeftShift))
					{
						multipoints.Add(vector);
					}
					if (Input.GetKey(KeyCode.LeftControl))
					{
						multipoints.Clear();
					}
					Vector3 mousePosition = Input.mousePosition;
					if (mousePosition.x > 225f)
					{
						DemoPath();
					}
				}
			}
			if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt) && (lastPath == null || lastPath.IsDone()))
			{
				DemoPath();
			}
		}

		public void OnGUI()
		{
			GUILayout.BeginArea(new Rect(5f, 5f, 220f, Screen.height - 10), string.Empty, "Box");
			switch (activeDemo)
			{
			case DemoMode.ABPath:
				GUILayout.Label("Basic path. Finds a path from point A to point B.");
				break;
			case DemoMode.MultiTargetPath:
				GUILayout.Label("Multi Target Path. Finds a path quickly from one point to many others in a single search.");
				break;
			case DemoMode.RandomPath:
				GUILayout.Label("Randomized Path. Finds a path with a specified length in a random direction or biased towards some point when using a larger aim strenggth.");
				break;
			case DemoMode.FleePath:
				GUILayout.Label("Flee Path. Tries to flee from a specified point. Remember to set Flee Strength!");
				break;
			case DemoMode.ConstantPath:
				GUILayout.Label("Finds all nodes which it costs less than some value to reach.");
				break;
			case DemoMode.FloodPath:
				GUILayout.Label("Searches the whole graph from a specific point. FloodPathTracer can then be used to quickly find a path to that point");
				break;
			case DemoMode.FloodPathTracer:
				GUILayout.Label("Traces a path to where the FloodPath started. Compare the claculation times for this path with ABPath!\nGreat for TD games");
				break;
			}
			GUILayout.Space(5f);
			GUILayout.Label("Note that the paths are rendered without ANY post-processing applied, so they might look a bit edgy");
			GUILayout.Space(5f);
			GUILayout.Label("Click anywhere to recalculate the path. Hold Alt to continuously recalculate the path while the mouse is pressed.");
			if (activeDemo == DemoMode.ConstantPath || activeDemo == DemoMode.RandomPath || activeDemo == DemoMode.FleePath)
			{
				GUILayout.Label("Search Distance (" + searchLength + ")");
				searchLength = Mathf.RoundToInt(GUILayout.HorizontalSlider(searchLength, 0f, 100000f));
			}
			if (activeDemo == DemoMode.RandomPath || activeDemo == DemoMode.FleePath)
			{
				GUILayout.Label("Spread (" + spread + ")");
				spread = Mathf.RoundToInt(GUILayout.HorizontalSlider(spread, 0f, 40000f));
				GUILayout.Label(((activeDemo != DemoMode.RandomPath) ? "Flee strength" : "Aim strength") + " (" + aimStrength + ")");
				aimStrength = GUILayout.HorizontalSlider(aimStrength, 0f, 1f);
			}
			if (activeDemo == DemoMode.MultiTargetPath)
			{
				GUILayout.Label("Hold shift and click to add new target points. Hold ctr and click to remove all target points");
			}
			if (GUILayout.Button("A to B path"))
			{
				activeDemo = DemoMode.ABPath;
			}
			if (GUILayout.Button("Multi Target Path"))
			{
				activeDemo = DemoMode.MultiTargetPath;
			}
			if (GUILayout.Button("Random Path"))
			{
				activeDemo = DemoMode.RandomPath;
			}
			if (GUILayout.Button("Flee path"))
			{
				activeDemo = DemoMode.FleePath;
			}
			if (GUILayout.Button("Constant Path"))
			{
				activeDemo = DemoMode.ConstantPath;
			}
			if (GUILayout.Button("Flood Path"))
			{
				activeDemo = DemoMode.FloodPath;
			}
			if (GUILayout.Button("Flood Path Tracer"))
			{
				activeDemo = DemoMode.FloodPathTracer;
			}
			GUILayout.EndArea();
		}

		public void OnPathComplete(Path p)
		{
			if (lastRender == null)
			{
				return;
			}
			if (p.error)
			{
				ClearPrevious();
			}
			else if ((object)p.GetType() == typeof(MultiTargetPath))
			{
				List<GameObject> list = new List<GameObject>(lastRender);
				lastRender.Clear();
				MultiTargetPath multiTargetPath = p as MultiTargetPath;
				for (int i = 0; i < multiTargetPath.vectorPaths.Length; i++)
				{
					if (multiTargetPath.vectorPaths[i] != null)
					{
						List<Vector3> list2 = multiTargetPath.vectorPaths[i];
						GameObject gameObject = null;
						if (list.Count > i && list[i].GetComponent<LineRenderer>() != null)
						{
							gameObject = list[i];
							list.RemoveAt(i);
						}
						else
						{
							gameObject = new GameObject("LineRenderer_" + i, typeof(LineRenderer));
						}
						LineRenderer component = gameObject.GetComponent<LineRenderer>();
						component.sharedMaterial = lineMat;
						component.SetWidth(lineWidth, lineWidth);
						component.SetVertexCount(list2.Count);
						for (int j = 0; j < list2.Count; j++)
						{
							component.SetPosition(j, list2[j] + pathOffset);
						}
						lastRender.Add(gameObject);
					}
				}
				for (int k = 0; k < list.Count; k++)
				{
					Object.Destroy(list[k]);
				}
			}
			else if ((object)p.GetType() == typeof(ConstantPath))
			{
				ClearPrevious();
				ConstantPath constantPath = p as ConstantPath;
				List<GraphNode> allNodes = constantPath.allNodes;
				Mesh mesh = new Mesh();
				List<Vector3> list3 = new List<Vector3>();
				bool flag = false;
				for (int num = allNodes.Count - 1; num >= 0; num--)
				{
					Vector3 a = (Vector3)allNodes[num].position + pathOffset;
					if (list3.Count == 65000 && !flag)
					{
						Debug.LogError("Too many nodes, rendering a mesh would throw 65K vertex error. Using Debug.DrawRay instead for the rest of the nodes");
						flag = true;
					}
					if (flag)
					{
						Debug.DrawRay(a, Vector3.up, Color.blue);
					}
					else
					{
						GridGraph gridGraph = AstarData.GetGraph(allNodes[num]) as GridGraph;
						float d = 1f;
						if (gridGraph != null)
						{
							d = gridGraph.nodeSize;
						}
						list3.Add(a + new Vector3(-0.5f, 0f, -0.5f) * d);
						list3.Add(a + new Vector3(0.5f, 0f, -0.5f) * d);
						list3.Add(a + new Vector3(-0.5f, 0f, 0.5f) * d);
						list3.Add(a + new Vector3(0.5f, 0f, 0.5f) * d);
					}
				}
				Vector3[] array = list3.ToArray();
				int[] array2 = new int[3 * array.Length / 2];
				int l = 0;
				int num2 = 0;
				for (; l < array.Length; l += 4)
				{
					array2[num2] = l;
					array2[num2 + 1] = l + 1;
					array2[num2 + 2] = l + 2;
					array2[num2 + 3] = l + 1;
					array2[num2 + 4] = l + 3;
					array2[num2 + 5] = l + 2;
					num2 += 6;
				}
				Vector2[] array3 = new Vector2[array.Length];
				for (int m = 0; m < array3.Length; m += 4)
				{
					array3[m] = new Vector2(0f, 0f);
					array3[m + 1] = new Vector2(1f, 0f);
					array3[m + 2] = new Vector2(0f, 1f);
					array3[m + 3] = new Vector2(1f, 1f);
				}
				mesh.vertices = array;
				mesh.triangles = array2;
				mesh.uv = array3;
				mesh.RecalculateNormals();
				GameObject gameObject2 = new GameObject("Mesh", typeof(MeshRenderer), typeof(MeshFilter));
				MeshFilter component2 = gameObject2.GetComponent<MeshFilter>();
				component2.mesh = mesh;
				MeshRenderer component3 = gameObject2.GetComponent<MeshRenderer>();
				component3.material = squareMat;
				lastRender.Add(gameObject2);
			}
			else
			{
				ClearPrevious();
				GameObject gameObject3 = new GameObject("LineRenderer", typeof(LineRenderer));
				LineRenderer component4 = gameObject3.GetComponent<LineRenderer>();
				component4.sharedMaterial = lineMat;
				component4.SetWidth(lineWidth, lineWidth);
				component4.SetVertexCount(p.vectorPath.Count);
				for (int n = 0; n < p.vectorPath.Count; n++)
				{
					component4.SetPosition(n, p.vectorPath[n] + pathOffset);
				}
				lastRender.Add(gameObject3);
			}
		}

		private void ClearPrevious()
		{
			for (int i = 0; i < lastRender.Count; i++)
			{
				Object.Destroy(lastRender[i]);
			}
			lastRender.Clear();
		}

		private void OnApplicationQuit()
		{
			ClearPrevious();
			lastRender = null;
		}

		private void DemoPath()
		{
			Path path = null;
			if (activeDemo == DemoMode.ABPath)
			{
				path = ABPath.Construct(start.position, end.position, OnPathComplete);
				if (agents != null && agents.Length > 0)
				{
					List<Vector3> list = ListPool<Vector3>.Claim(agents.Length);
					Vector3 zero = Vector3.zero;
					for (int i = 0; i < agents.Length; i++)
					{
						list.Add(agents[i].transform.position);
						zero += list[i];
					}
					zero /= (float)list.Count;
					for (int j = 0; j < agents.Length; j++)
					{
						List<Vector3> list2;
						List<Vector3> list3 = list2 = list;
						int index;
						int index2 = index = j;
						Vector3 a = list2[index];
						list3[index2] = a - zero;
					}
					PathUtilities.GetPointsAroundPoint(end.position, AstarPath.active.graphs[0] as IRaycastableGraph, list, 0f, 0.2f);
					for (int k = 0; k < agents.Length; k++)
					{
						if (!(agents[k] == null))
						{
							agents[k].target.position = list[k];
							agents[k].UpdatePath();
						}
					}
				}
			}
			else if (activeDemo == DemoMode.MultiTargetPath)
			{
				MultiTargetPath multiTargetPath = MultiTargetPath.Construct(multipoints.ToArray(), end.position, null, OnPathComplete);
				path = multiTargetPath;
			}
			else if (activeDemo == DemoMode.RandomPath)
			{
				RandomPath randomPath = RandomPath.Construct(start.position, searchLength, OnPathComplete);
				randomPath.spread = spread;
				randomPath.aimStrength = aimStrength;
				randomPath.aim = end.position;
				path = randomPath;
			}
			else if (activeDemo == DemoMode.FleePath)
			{
				FleePath fleePath = FleePath.Construct(start.position, end.position, searchLength, OnPathComplete);
				fleePath.aimStrength = aimStrength;
				fleePath.spread = spread;
				path = fleePath;
			}
			else if (activeDemo == DemoMode.ConstantPath)
			{
				StartCoroutine(CalculateConstantPath());
				path = null;
			}
			else if (activeDemo == DemoMode.FloodPath)
			{
				path = (lastFlood = FloodPath.Construct(end.position));
			}
			else if (activeDemo == DemoMode.FloodPathTracer && lastFlood != null)
			{
				FloodPathTracer floodPathTracer = FloodPathTracer.Construct(end.position, lastFlood, OnPathComplete);
				path = floodPathTracer;
			}
			if (path != null)
			{
				AstarPath.StartPath(path);
				lastPath = path;
			}
		}

		public IEnumerator CalculateConstantPath()
		{
			ConstantPath constPath = ConstantPath.Construct(end.position, searchLength, OnPathComplete);
			AstarPath.StartPath(constPath);
			lastPath = constPath;
			yield return constPath.WaitForPath();
		}
	}
}
