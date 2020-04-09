using Pathfinding.RVO;
using Pathfinding.RVO.Sampled;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Examples
{
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_group_controller.php")]
	public class GroupController : MonoBehaviour
	{
		private const float rad2Deg = 180f / MathF.PI;

		public GUIStyle selectionBox;

		public bool adjustCamera = true;

		private Vector2 start;

		private Vector2 end;

		private bool wasDown;

		private List<RVOExampleAgent> selection = new List<RVOExampleAgent>();

		private Simulator sim;

		private Camera cam;

		public void Start()
		{
			cam = Camera.main;
			RVOSimulator rVOSimulator = UnityEngine.Object.FindObjectOfType(typeof(RVOSimulator)) as RVOSimulator;
			if (rVOSimulator == null)
			{
				base.enabled = false;
				throw new Exception("No RVOSimulator in the scene. Please add one");
			}
			sim = rVOSimulator.GetSimulator();
		}

		public void Update()
		{
			if (Screen.fullScreen && Screen.width != Screen.resolutions[Screen.resolutions.Length - 1].width)
			{
				Screen.SetResolution(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height, fullscreen: true);
			}
			if (adjustCamera)
			{
				List<Agent> agents = sim.GetAgents();
				float num = 0f;
				for (int i = 0; i < agents.Count; i++)
				{
					Vector2 position = agents[i].Position;
					float a = Mathf.Abs(position.x);
					Vector2 position2 = agents[i].Position;
					float num2 = Mathf.Max(a, Mathf.Abs(position2.y));
					if (num2 > num)
					{
						num = num2;
					}
				}
				float a2 = num / Mathf.Tan(cam.fieldOfView * (MathF.PI / 180f) / 2f);
				float b = num / Mathf.Tan(Mathf.Atan(Mathf.Tan(cam.fieldOfView * (MathF.PI / 180f) / 2f) * cam.aspect));
				float a3 = Mathf.Max(a2, b) * 1.1f;
				a3 = Mathf.Max(a3, 20f);
				cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(0f, a3, 0f), Time.smoothDeltaTime * 2f);
			}
			if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.Mouse0))
			{
				Order();
			}
		}

		private void OnGUI()
		{
			if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && !Input.GetKey(KeyCode.A))
			{
				Select(start, end);
				wasDown = false;
			}
			if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
			{
				end = Event.current.mousePosition;
				if (!wasDown)
				{
					start = end;
					wasDown = true;
				}
			}
			if (Input.GetKey(KeyCode.A))
			{
				wasDown = false;
			}
			if (wasDown)
			{
				Rect position = Rect.MinMaxRect(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y), Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y));
				if (position.width > 4f && position.height > 4f)
				{
					GUI.Box(position, string.Empty, selectionBox);
				}
			}
		}

		public void Order()
		{
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hitInfo))
			{
				float num = 0f;
				for (int i = 0; i < selection.Count; i++)
				{
					num += selection[i].GetComponent<RVOController>().radius;
				}
				float num2 = num / MathF.PI;
				num2 *= 2f;
				for (int j = 0; j < selection.Count; j++)
				{
					float num3 = MathF.PI * 2f * (float)j / (float)selection.Count;
					Vector3 target = hitInfo.point + new Vector3(Mathf.Cos(num3), 0f, Mathf.Sin(num3)) * num2;
					selection[j].SetTarget(target);
					selection[j].SetColor(GetColor(num3));
					selection[j].RecalculatePath();
				}
			}
		}

		public void Select(Vector2 _start, Vector2 _end)
		{
			_start.y = (float)Screen.height - _start.y;
			_end.y = (float)Screen.height - _end.y;
			Vector2 b = Vector2.Min(_start, _end);
			Vector2 a = Vector2.Max(_start, _end);
			if ((a - b).sqrMagnitude < 16f)
			{
				return;
			}
			selection.Clear();
			RVOExampleAgent[] array = UnityEngine.Object.FindObjectsOfType(typeof(RVOExampleAgent)) as RVOExampleAgent[];
			for (int i = 0; i < array.Length; i++)
			{
				Vector2 vector = cam.WorldToScreenPoint(array[i].transform.position);
				if (vector.x > b.x && vector.y > b.y && vector.x < a.x && vector.y < a.y)
				{
					selection.Add(array[i]);
				}
			}
		}

		public Color GetColor(float angle)
		{
			return AstarMath.HSVToRGB(angle * (180f / MathF.PI), 0.8f, 0.6f);
		}
	}
}
