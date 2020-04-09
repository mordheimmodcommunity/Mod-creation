using UnityEngine;
using UnityEngine.UI;

public class TabIcon : MonoBehaviour
{
	[HideInInspector]
	public ToggleEffects toggle;

	[HideInInspector]
	public CanvasGroup canvasGroup;

	public Image icon;

	public TabsModule.IsAvailable IsAvailableDelegate;

	public GameObject exclamationMark;

	public GameObject impossibleMark;

	public string reason;

	public string titleText;

	public GameObject textContent;

	public Text tabTitle;

	public Text tabReasonTitle;

	public bool available;

	[HideInInspector]
	public HideoutManager.State state;

	[HideInInspector]
	public HideoutCamp.NodeSlot nodeSlot;

	private bool initialized;

	private void Awake()
	{
		Init();
	}

	public void Init()
	{
		if (!initialized)
		{
			initialized = true;
			toggle = GetComponent<ToggleEffects>();
			canvasGroup = GetComponent<CanvasGroup>();
			textContent.gameObject.SetActive(value: false);
		}
	}
}
