using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CharacterCameraAreaModule : UIModule, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	private const float MIN_ZOOM_DISTANCE = 2.5f;

	private const float MAX_ZOOM_DISTANCE = 6f;

	private bool pointerInRect;

	private Vector3 cameraOrigin;

	private CameraManager camManager;

	private GameObject defaultTarget;

	public ButtonGroup btnRotateLeft;

	public ButtonGroup btnRotateRight;

	protected override void Awake()
	{
		base.Awake();
		defaultTarget = new GameObject("CharacterCameraAreaModule_default_camera_target");
	}

	protected void Start()
	{
		btnRotateLeft.SetAction(string.Empty, string.Empty);
		btnRotateLeft.OnAction(delegate
		{
			PandoraSingleton<HideoutManager>.Instance.currentUnit.transform.Rotate(0f, -45f, 0f);
		}, mouseOnly: true);
		btnRotateRight.SetAction(string.Empty, string.Empty);
		btnRotateRight.OnAction(delegate
		{
			PandoraSingleton<HideoutManager>.Instance.currentUnit.transform.Rotate(0f, 45f, 0f);
		}, mouseOnly: true);
	}

	private void OnDestroy()
	{
		if (defaultTarget != null)
		{
			Object.Destroy(defaultTarget);
		}
	}

	public void Init(Vector3 cameraOrigin)
	{
		this.cameraOrigin = cameraOrigin;
		camManager = Camera.main.GetComponent<CameraManager>();
	}

	private void Update()
	{
		if (!PandoraSingleton<HideoutManager>.Instance.currentUnit)
		{
			return;
		}
		float num = PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_x") * 5f;
		float num2 = PandoraSingleton<PandoraInput>.Instance.GetAxis("cam_y") / 3f;
		if (pointerInRect)
		{
			num2 += PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_y") / 3f;
			if (PandoraSingleton<PandoraInput>.Instance.GetKey("mouse_click"))
			{
				num += PandoraSingleton<PandoraInput>.Instance.GetAxis("mouse_x") * 5f;
			}
		}
		PandoraSingleton<HideoutManager>.Instance.currentUnit.transform.Rotate(0f, 0f - num, 0f);
		if (num2 == 0f)
		{
			return;
		}
		float distanceToTarget = PandoraSingleton<HideoutManager>.Instance.CamManager.GetDistanceToTarget();
		if (distanceToTarget != 0f)
		{
			if (distanceToTarget - num2 < 2.5f)
			{
				num2 = distanceToTarget - 2.5f;
			}
			else if (distanceToTarget - num2 > 6f)
			{
				num2 = distanceToTarget - 6f;
			}
			PandoraSingleton<HideoutManager>.Instance.CamManager.Zoom2(num2);
		}
		else
		{
			SetCameraLookAtDefault(instantTransition: true);
		}
	}

	public void SetCameraLookAt(Transform target, bool instantTransition)
	{
		float distanceToTarget = camManager.GetDistanceToTarget();
		if (distanceToTarget == 0f || distanceToTarget > 20f)
		{
			camManager.dummyCam.transform.position = cameraOrigin;
		}
		else
		{
			Vector3 normalized = (cameraOrigin - target.transform.position).normalized;
			camManager.dummyCam.transform.position = target.transform.position + normalized * Mathf.Max(2.5f, distanceToTarget);
		}
		float magnitude = (camManager.transform.position - target.transform.position).magnitude;
		bool flag = magnitude < 10f;
		camManager.LookAtFocus(target.transform, overrideCurrentTarget: false, !instantTransition && flag);
	}

	public void SetCameraLookAtDefault(bool instantTransition)
	{
		Vector3 position = PandoraSingleton<HideoutManager>.Instance.currentUnit.transform.position;
		Unit unit = PandoraSingleton<HideoutManager>.Instance.currentUnit.unit;
		if ((unit.WarbandId == WarbandId.SKAVENS || unit.Id == UnitId.GHOUL) && !unit.IsImpressive)
		{
			position.y += 0.7f;
		}
		else
		{
			position.y += 1.4f;
		}
		defaultTarget.transform.position = position;
		SetCameraLookAt(defaultTarget.transform, instantTransition);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		pointerInRect = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		pointerInRect = false;
	}
}
