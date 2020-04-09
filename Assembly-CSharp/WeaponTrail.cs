using System.Collections.Generic;
using UnityEngine;

public class WeaponTrail : MonoBehaviour
{
	public class Point
	{
		public float timeCreated;

		public Vector3 basePosition;

		public Vector3 tipPosition;

		public bool lineBreak;
	}

	[SerializeField]
	private bool _emit = true;

	[SerializeField]
	private float _emitTime;

	[SerializeField]
	private Material _material;

	[SerializeField]
	private float _lifeTime = 1f;

	[SerializeField]
	private Color[] _colors;

	[SerializeField]
	private float[] _sizes;

	[SerializeField]
	private float _minVertexDistance = 0.1f;

	[SerializeField]
	private float _maxVertexDistance = 10f;

	[SerializeField]
	private float _maxAngle = 3f;

	[SerializeField]
	private bool _autoDestruct;

	[SerializeField]
	private int subdivisions = 5;

	[SerializeField]
	private Transform _base;

	[SerializeField]
	private Transform _tip;

	private List<Point> _points = new List<Point>();

	private List<Point> _smoothedPoints = new List<Point>();

	private GameObject _o;

	private Mesh _trailMesh;

	private Vector3 _lastPosition;

	private bool _lastFrameEmit = true;

	public void Emit(bool activate)
	{
		_emit = activate;
		if (activate)
		{
			base.enabled = activate;
		}
	}

	private void Start()
	{
		_lastPosition = base.transform.position;
		_o = new GameObject(base.name + "_trail");
		_o.transform.SetParent(null);
		_o.transform.position = Vector3.zero;
		_o.transform.rotation = Quaternion.identity;
		_o.transform.localScale = Vector3.one;
		_o.AddComponent(typeof(MeshFilter));
		_o.AddComponent(typeof(MeshRenderer));
		_o.GetComponent<Renderer>().material = _material;
		_trailMesh = new Mesh();
		_trailMesh.name = base.name + "TrailMesh";
		_o.GetComponent<MeshFilter>().mesh = _trailMesh;
	}

	public Material GetMaterial()
	{
		return _o.GetComponent<Renderer>().material;
	}

	private void OnDisable()
	{
		if (_autoDestruct)
		{
			Object.Destroy(_o);
		}
	}

	private void OnDestroy()
	{
		if ((bool)_o)
		{
			Object.Destroy(_o);
		}
	}

	private void Update()
	{
		if (_emit && _emitTime != 0f)
		{
			_emitTime -= Time.deltaTime;
			if (_emitTime == 0f)
			{
				_emitTime = -1f;
			}
			if (_emitTime < 0f)
			{
				_emit = false;
			}
		}
		if (_emit)
		{
			float magnitude = (_lastPosition - base.transform.position).magnitude;
			if (magnitude > _minVertexDistance)
			{
				bool flag = false;
				if (_points.Count < 3)
				{
					flag = true;
				}
				else
				{
					Vector3 from = _points[_points.Count - 2].tipPosition - _points[_points.Count - 3].tipPosition;
					Vector3 to = _points[_points.Count - 1].tipPosition - _points[_points.Count - 2].tipPosition;
					if (Vector3.Angle(from, to) > _maxAngle || magnitude > _maxVertexDistance)
					{
						flag = true;
					}
				}
				if (flag)
				{
					Point point = new Point();
					point.basePosition = _base.position;
					point.tipPosition = _tip.position;
					point.timeCreated = Time.time;
					_points.Add(point);
					_lastPosition = base.transform.position;
					if (_points.Count == 1)
					{
						_smoothedPoints.Add(point);
					}
					else if (_points.Count > 1)
					{
						for (int i = 0; i < 1 + subdivisions; i++)
						{
							_smoothedPoints.Add(point);
						}
					}
					if (_points.Count >= 4)
					{
						IEnumerable<Vector3> collection = Interpolate.NewCatmullRom(new Vector3[4]
						{
							_points[_points.Count - 4].tipPosition,
							_points[_points.Count - 3].tipPosition,
							_points[_points.Count - 2].tipPosition,
							_points[_points.Count - 1].tipPosition
						}, subdivisions, loop: false);
						IEnumerable<Vector3> collection2 = Interpolate.NewCatmullRom(new Vector3[4]
						{
							_points[_points.Count - 4].basePosition,
							_points[_points.Count - 3].basePosition,
							_points[_points.Count - 2].basePosition,
							_points[_points.Count - 1].basePosition
						}, subdivisions, loop: false);
						List<Vector3> list = new List<Vector3>(collection);
						List<Vector3> list2 = new List<Vector3>(collection2);
						float timeCreated = _points[_points.Count - 4].timeCreated;
						float timeCreated2 = _points[_points.Count - 1].timeCreated;
						for (int j = 0; j < list.Count; j++)
						{
							int num = _smoothedPoints.Count - (list.Count - j);
							if (num > -1 && num < _smoothedPoints.Count)
							{
								Point point2 = new Point();
								point2.basePosition = list2[j];
								point2.tipPosition = list[j];
								point2.timeCreated = Mathf.Lerp(timeCreated, timeCreated2, (float)j / (float)list.Count);
								_smoothedPoints[num] = point2;
							}
						}
					}
				}
				else
				{
					_points[_points.Count - 1].basePosition = _base.position;
					_points[_points.Count - 1].tipPosition = _tip.position;
					_smoothedPoints[_smoothedPoints.Count - 1].basePosition = _base.position;
					_smoothedPoints[_smoothedPoints.Count - 1].tipPosition = _tip.position;
				}
			}
			else
			{
				if (_points.Count > 0)
				{
					_points[_points.Count - 1].basePosition = _base.position;
					_points[_points.Count - 1].tipPosition = _tip.position;
				}
				if (_smoothedPoints.Count > 0)
				{
					_smoothedPoints[_smoothedPoints.Count - 1].basePosition = _base.position;
					_smoothedPoints[_smoothedPoints.Count - 1].tipPosition = _tip.position;
				}
			}
		}
		if (!_emit && _lastFrameEmit && _points.Count > 0)
		{
			_points[_points.Count - 1].lineBreak = true;
		}
		_lastFrameEmit = _emit;
		for (int num2 = _points.Count - 1; num2 >= 0; num2--)
		{
			if (Time.time - _points[num2].timeCreated > _lifeTime)
			{
				_points.RemoveAt(num2);
			}
		}
		for (int num3 = _smoothedPoints.Count - 1; num3 >= 0; num3--)
		{
			if (Time.time - _smoothedPoints[num3].timeCreated > _lifeTime)
			{
				_smoothedPoints.RemoveAt(num3);
			}
		}
		List<Point> smoothedPoints = _smoothedPoints;
		if (smoothedPoints.Count > 1)
		{
			Vector3[] array = new Vector3[smoothedPoints.Count * 2];
			Vector2[] array2 = new Vector2[smoothedPoints.Count * 2];
			int[] array3 = new int[(smoothedPoints.Count - 1) * 6];
			Color[] array4 = new Color[smoothedPoints.Count * 2];
			for (int k = 0; k < smoothedPoints.Count; k++)
			{
				Point point3 = smoothedPoints[k];
				float num4 = (Time.time - point3.timeCreated) / _lifeTime;
				Color color = Color.Lerp(Color.white, Color.clear, num4);
				if (_colors != null && _colors.Length > 0)
				{
					float num5 = num4 * (float)(_colors.Length - 1);
					float num6 = Mathf.Floor(num5);
					float num7 = Mathf.Clamp(Mathf.Ceil(num5), 1f, _colors.Length - 1);
					float t = Mathf.InverseLerp(num6, num7, num5);
					if (num6 >= (float)_colors.Length)
					{
						num6 = _colors.Length - 1;
					}
					if (num6 < 0f)
					{
						num6 = 0f;
					}
					if (num7 >= (float)_colors.Length)
					{
						num7 = _colors.Length - 1;
					}
					if (num7 < 0f)
					{
						num7 = 0f;
					}
					color = Color.Lerp(_colors[(int)num6], _colors[(int)num7], t);
				}
				float num8 = 0f;
				if (_sizes != null && _sizes.Length > 0)
				{
					float num9 = num4 * (float)(_sizes.Length - 1);
					float num10 = Mathf.Floor(num9);
					float num11 = Mathf.Clamp(Mathf.Ceil(num9), 1f, _sizes.Length - 1);
					float t2 = Mathf.InverseLerp(num10, num11, num9);
					if (num10 >= (float)_sizes.Length)
					{
						num10 = _sizes.Length - 1;
					}
					if (num10 < 0f)
					{
						num10 = 0f;
					}
					if (num11 >= (float)_sizes.Length)
					{
						num11 = _sizes.Length - 1;
					}
					if (num11 < 0f)
					{
						num11 = 0f;
					}
					num8 = Mathf.Lerp(_sizes[(int)num10], _sizes[(int)num11], t2);
				}
				Vector3 a = point3.tipPosition - point3.basePosition;
				array[k * 2] = point3.basePosition - a * (num8 * 0.5f);
				array[k * 2 + 1] = point3.tipPosition + a * (num8 * 0.5f);
				array4[k * 2] = (array4[k * 2 + 1] = color);
				float x = (float)k / (float)smoothedPoints.Count;
				array2[k * 2] = new Vector2(x, 0f);
				array2[k * 2 + 1] = new Vector2(x, 1f);
				if (k > 0)
				{
					array3[(k - 1) * 6] = k * 2 - 2;
					array3[(k - 1) * 6 + 1] = k * 2 - 1;
					array3[(k - 1) * 6 + 2] = k * 2;
					array3[(k - 1) * 6 + 3] = k * 2 + 1;
					array3[(k - 1) * 6 + 4] = k * 2;
					array3[(k - 1) * 6 + 5] = k * 2 - 1;
				}
			}
			_trailMesh.Clear();
			_trailMesh.vertices = array;
			_trailMesh.colors = array4;
			_trailMesh.uv = array2;
			_trailMesh.triangles = array3;
			_trailMesh.RecalculateNormals();
		}
		else if (!_emit)
		{
			_trailMesh.Clear();
			base.enabled = false;
			if (_autoDestruct)
			{
				Object.Destroy(_o);
				Object.Destroy(base.gameObject);
			}
		}
	}
}
