using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
	[Serializable]
	[AddComponentMenu("Pathfinding/Modifiers/Raycast Simplifier")]
	public class RayPathModifier : MonoModifier
	{
		public int divideIterations = 2;

		public List<Collider> traversableColliders = new List<Collider>();

		private float radius = 0.2f;

		private List<Vector3> nodes = new List<Vector3>();

		private List<int> graphIndexes = new List<int>();

		public override int Order => 1;

		public void SetRadius(float newRadius)
		{
			radius = newRadius + 0.1f;
		}

		public override void Apply(Path p)
		{
			nodes.Clear();
			graphIndexes.Clear();
			nodes.Add((p as ABPath).originalStartPoint);
			graphIndexes.Add(0);
			for (int i = 0; i < p.path.Count; i++)
			{
				nodes.Add(p.vectorPath[i]);
				graphIndexes.Add((int)p.path[i].GraphIndex);
				if (i + 1 < p.path.Count && p.path[i].GraphIndex == 0)
				{
					Vector3 a = p.vectorPath[i + 1] - p.vectorPath[i];
					a /= (float)(divideIterations + 1);
					for (int j = 1; j <= divideIterations; j++)
					{
						nodes.Add(p.vectorPath[i] + a * j);
						graphIndexes.Add((int)p.path[i].GraphIndex);
					}
				}
			}
			nodes.Add((p as ABPath).originalEndPoint);
			graphIndexes.Add(0);
			int num = 0;
			while (num < nodes.Count - 2)
			{
				if (graphIndexes[num] == graphIndexes[num + 2] && SimplifyPath(num, num + 2))
				{
					nodes.RemoveAt(num + 1);
					graphIndexes.RemoveAt(num + 1);
				}
				else
				{
					num++;
				}
			}
			nodes.RemoveAt(0);
			p.vectorPath.Clear();
			p.vectorPath.AddRange(nodes);
		}

		private bool SimplifyPath(int i, int j)
		{
			float num = Vector3.Distance(nodes[i], nodes[j]);
			if (num < radius)
			{
				return true;
			}
			RaycastHit raycastHit;
			return PandoraUtils.RectCast(nodes[i] + Vector3.up * (radius + 0.2f), nodes[j] - nodes[i], num, radius * 2f, radius * 2f + 0.05f, LayerMaskManager.pathMask, traversableColliders, out raycastHit);
		}
	}
}
