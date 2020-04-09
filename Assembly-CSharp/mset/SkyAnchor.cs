using System;
using UnityEngine;

namespace mset
{
	public class SkyAnchor : MonoBehaviour
	{
		public enum AnchorBindType
		{
			Center,
			Offset,
			TargetTransform,
			TargetSky
		}

		public AnchorBindType BindType;

		public Transform AnchorTransform;

		public Vector3 AnchorOffset = Vector3.zero;

		public Sky AnchorSky;

		public Vector3 CachedCenter = Vector3.zero;

		public SkyApplicator CurrentApplicator;

		private bool isStatic;

		public bool HasLocalSky;

		public bool HasChanged = true;

		[SerializeField]
		private SkyBlender Blender = new SkyBlender();

		private Vector3 LastPosition = Vector3.zero;

		[NonSerialized]
		public Material[] materials;

		private bool firstFrame;

		public Sky CurrentSky => Blender.CurrentSky;

		public Sky PreviousSky => Blender.PreviousSky;

		public float BlendTime
		{
			get
			{
				return Blender.BlendTime;
			}
			set
			{
				Blender.BlendTime = value;
			}
		}

		public bool IsStatic => isStatic;

		private void Start()
		{
			if (BindType != AnchorBindType.TargetSky)
			{
				GetComponent<Renderer>().SetPropertyBlock(new MaterialPropertyBlock());
				SkyManager skyManager = SkyManager.Get();
				skyManager.RegisterNewRenderer(GetComponent<Renderer>());
				skyManager.ApplyCorrectSky(GetComponent<Renderer>());
				BlendTime = skyManager.LocalBlendTime;
				if ((bool)Blender.CurrentSky)
				{
					Blender.SnapToSky(Blender.CurrentSky);
				}
				else
				{
					Blender.SnapToSky(skyManager.GlobalSky);
				}
			}
			materials = GetComponent<Renderer>().materials;
			LastPosition = base.transform.position;
			HasChanged = true;
		}

		private void OnEnable()
		{
			isStatic = base.gameObject.isStatic;
			ComputeCenter(ref CachedCenter);
			firstFrame = true;
		}

		private void LateUpdate()
		{
			if (BindType == AnchorBindType.TargetSky)
			{
				HasChanged = (AnchorSky != Blender.CurrentSky);
				if (AnchorSky != null)
				{
					CachedCenter = AnchorSky.transform.position;
				}
			}
			else if (BindType == AnchorBindType.TargetTransform)
			{
				if ((bool)AnchorTransform)
				{
					Vector3 position = AnchorTransform.position;
					if (position.x == LastPosition.x)
					{
						Vector3 position2 = AnchorTransform.position;
						if (position2.y == LastPosition.y)
						{
							Vector3 position3 = AnchorTransform.position;
							if (position3.z == LastPosition.z)
							{
								goto IL_01df;
							}
						}
					}
					HasChanged = true;
					LastPosition = AnchorTransform.position;
					CachedCenter.x = LastPosition.x;
					CachedCenter.y = LastPosition.y;
					CachedCenter.z = LastPosition.z;
				}
			}
			else if (!isStatic)
			{
				float x = LastPosition.x;
				Vector3 position4 = base.transform.position;
				if (x == position4.x)
				{
					float y = LastPosition.y;
					Vector3 position5 = base.transform.position;
					if (y == position5.y)
					{
						float z = LastPosition.z;
						Vector3 position6 = base.transform.position;
						if (z == position6.z)
						{
							goto IL_01df;
						}
					}
				}
				HasChanged = true;
				LastPosition = base.transform.position;
				ComputeCenter(ref CachedCenter);
			}
			else
			{
				HasChanged = false;
			}
			goto IL_01df;
			IL_01df:
			HasChanged |= firstFrame;
			firstFrame = false;
			if (Blender.IsBlending || Blender.WasBlending(Time.deltaTime))
			{
				Apply();
			}
			else if (BindType == AnchorBindType.TargetSky)
			{
				if (HasChanged || Blender.CurrentSky.Dirty)
				{
					Apply();
				}
			}
			else if (HasLocalSky && (HasChanged || Blender.CurrentSky.Dirty))
			{
				Apply();
			}
		}

		public void UpdateMaterials()
		{
			materials = GetComponent<Renderer>().materials;
		}

		public void CleanUpMaterials()
		{
			if (materials != null)
			{
				Material[] array = materials;
				foreach (Material obj in array)
				{
					UnityEngine.Object.Destroy(obj);
				}
				materials = new Material[0];
			}
		}

		public void SnapToSky(Sky nusky)
		{
			if (!(nusky == null) && BindType != AnchorBindType.TargetSky)
			{
				Blender.SnapToSky(nusky);
				HasLocalSky = true;
			}
		}

		public void BlendToSky(Sky nusky)
		{
			if (!(nusky == null) && BindType != AnchorBindType.TargetSky)
			{
				Blender.BlendToSky(nusky);
				HasLocalSky = true;
			}
		}

		public void SnapToGlobalSky(Sky nusky)
		{
			SnapToSky(nusky);
			HasLocalSky = false;
		}

		public void BlendToGlobalSky(Sky nusky)
		{
			if (HasLocalSky)
			{
				BlendToSky(nusky);
			}
			HasLocalSky = false;
		}

		public void Apply()
		{
			if (BindType == AnchorBindType.TargetSky)
			{
				if ((bool)AnchorSky)
				{
					Blender.SnapToSky(AnchorSky);
				}
				else
				{
					Blender.SnapToSky(SkyManager.Get().GlobalSky);
				}
			}
			Blender.Apply(GetComponent<Renderer>(), materials);
		}

		public void GetCenter(ref Vector3 _center)
		{
			_center.x = CachedCenter.x;
			_center.y = CachedCenter.y;
			_center.z = CachedCenter.z;
		}

		private void ComputeCenter(ref Vector3 _center)
		{
			Vector3 position = base.transform.position;
			_center.x = position.x;
			Vector3 position2 = base.transform.position;
			_center.y = position2.y;
			Vector3 position3 = base.transform.position;
			_center.z = position3.z;
			switch (BindType)
			{
			case AnchorBindType.TargetTransform:
				if ((bool)AnchorTransform)
				{
					Vector3 position7 = AnchorTransform.position;
					_center.x = position7.x;
					Vector3 position8 = AnchorTransform.position;
					_center.y = position8.y;
					Vector3 position9 = AnchorTransform.position;
					_center.z = position9.z;
				}
				break;
			case AnchorBindType.Center:
			{
				Vector3 center = GetComponent<Renderer>().bounds.center;
				_center.x = center.x;
				Vector3 center2 = GetComponent<Renderer>().bounds.center;
				_center.y = center2.y;
				Vector3 center3 = GetComponent<Renderer>().bounds.center;
				_center.z = center3.z;
				break;
			}
			case AnchorBindType.Offset:
			{
				Vector3 vector = base.transform.localToWorldMatrix.MultiplyPoint3x4(AnchorOffset);
				_center.x = vector.x;
				_center.y = vector.y;
				_center.z = vector.z;
				break;
			}
			case AnchorBindType.TargetSky:
				if ((bool)AnchorSky)
				{
					Vector3 position4 = AnchorSky.transform.position;
					_center.x = position4.x;
					Vector3 position5 = AnchorSky.transform.position;
					_center.y = position5.y;
					Vector3 position6 = AnchorSky.transform.position;
					_center.z = position6.z;
				}
				break;
			}
		}

		private void OnDestroy()
		{
			CleanUpMaterials();
		}

		private void OnApplicationQuit()
		{
			CleanUpMaterials();
		}
	}
}
