using System;
using System.Collections;
using UnityEngine;

namespace Pathfinding.Examples
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_r_v_o_agent_placer.php")]
	public class RVOAgentPlacer : MonoBehaviour
	{
		private const float rad2Deg = 180f / MathF.PI;

		public int agents = 100;

		public float ringSize = 100f;

		public LayerMask mask;

		public GameObject prefab;

		public Vector3 goalOffset;

		public float repathRate = 1f;

		private IEnumerator Start()
		{
			yield return null;
			int i = 0;
			while (true)
			{
				if (i < agents)
				{
					float angle = (float)i / (float)agents * MathF.PI * 2f;
					Vector3 pos = new Vector3((float)Math.Cos(angle), 0f, (float)Math.Sin(angle)) * ringSize;
					Vector3 antipodal = -pos + goalOffset;
					GameObject go = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.Euler(0f, angle + 180f, 0f)) as GameObject;
					RVOExampleAgent ag = go.GetComponent<RVOExampleAgent>();
					if (ag == null)
					{
						break;
					}
					go.transform.parent = base.transform;
					go.transform.position = pos;
					ag.repathRate = repathRate;
					ag.SetTarget(antipodal);
					ag.SetColor(GetColor(angle));
					i++;
					continue;
				}
				yield break;
			}
			Debug.LogError("Prefab does not have an RVOExampleAgent component attached");
		}

		public Color GetColor(float angle)
		{
			return AstarMath.HSVToRGB(angle * (180f / MathF.PI), 0.8f, 0.6f);
		}
	}
}
