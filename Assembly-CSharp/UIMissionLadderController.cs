using UnityEngine;
using UnityEngine.UI;

public class UIMissionLadderController : ContentView<UIMissionLadderGroup, int>
{
	public UIMissionLadderGroup selectedUnit;

	public RectTransform Transform;

	public Image arrow;

	public Vector2 normalSize = new Vector2(75f, 150f);

	public Vector2 currentSize = new Vector2(100f, 200f);

	private int currentIdx;

	protected override void Awake()
	{
		base.Awake();
		((Behaviour)(object)arrow).enabled = false;
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.MISSION_ROUND_START, ShowLadder);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.LADDER_CHANGED, OnLadderChanged);
		PandoraSingleton<NoticeManager>.Instance.RegisterListener(Notices.LADDER_UNIT_CHANGED, OnLadderCurrentIndexChanged);
	}

	private void ShowLadder()
	{
		PandoraSingleton<NoticeManager>.Instance.RemoveListener(Notices.MISSION_ROUND_START, ShowLadder);
		OnEnable();
		currentIdx = PandoraSingleton<MissionManager>.Instance.CurrentLadderIdx;
	}

	private void OnLadderCurrentIndexChanged()
	{
		if (base.Components.Count == 0)
		{
			OnLadderChanged();
		}
		UnitController item = (UnitController)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
		bool force = PandoraSingleton<NoticeManager>.Instance.Parameters.Count > 1 && (bool)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
		currentIdx = PandoraSingleton<MissionManager>.Instance.InitiativeLadder.IndexOf(item);
		if (base.Components.Count <= 0)
		{
			return;
		}
		if (currentIdx >= 0 && currentIdx < base.Components.Count)
		{
			if (currentIdx == 0)
			{
				RectTransform transform = Transform;
				Vector2 anchoredPosition = Transform.anchoredPosition;
				transform.anchoredPosition = new Vector2(0f, anchoredPosition.y);
			}
			else
			{
				RectTransform transform2 = Transform;
				float x = (0f - currentSize.x) * (float)currentIdx;
				Vector2 anchoredPosition2 = Transform.anchoredPosition;
				transform2.anchoredPosition = new Vector2(x, anchoredPosition2.y);
			}
			if (!((Behaviour)(object)arrow).enabled)
			{
				((Behaviour)(object)arrow).enabled = true;
			}
			for (int i = 0; i < base.Components.Count; i++)
			{
				base.Components[i].SetCurrent(i <= currentIdx, force, i == PandoraSingleton<MissionManager>.Instance.CurrentLadderIdx);
				LayoutElement component = base.Components[i].gameObject.GetComponent<LayoutElement>();
				component.set_preferredWidth((i > currentIdx) ? normalSize.x : currentSize.x);
				component.set_preferredHeight((i > currentIdx) ? normalSize.y : currentSize.y);
			}
		}
		else
		{
			RectTransform transform3 = Transform;
			Vector2 anchoredPosition3 = Transform.anchoredPosition;
			transform3.anchoredPosition = new Vector2(0f, anchoredPosition3.y);
			((Behaviour)(object)arrow).enabled = false;
		}
	}

	private void OnLadderChanged()
	{
		for (int i = 0; i < PandoraSingleton<MissionManager>.Instance.InitiativeLadder.Count; i++)
		{
			Add(i);
		}
		OnAddEnd();
	}

	protected override void OnAdd(UIMissionLadderGroup component, int ladderIndex)
	{
		component.Set(PandoraSingleton<MissionManager>.Instance.InitiativeLadder[ladderIndex], ladderIndex == currentIdx);
		LayoutElement component2 = component.gameObject.GetComponent<LayoutElement>();
		component2.set_preferredWidth((ladderIndex > currentIdx) ? normalSize.x : currentSize.x);
		component2.set_preferredHeight((ladderIndex > currentIdx) ? normalSize.y : currentSize.y);
	}
}
