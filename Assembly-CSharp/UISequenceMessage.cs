using System;
using UnityEngine;
using UnityEngine.UI;

public class UISequenceMessage : UIUnitControllerChanged
{
	public const int NO_VALUE = -1;

	public Sprite messageBackground;

	public Sprite warningBackground;

	public Image background;

	public Text title;

	public Image actionIcon;

	public Image mastery;

	public Text damage;

	public Text percent;

	public bool isLeft;

	private float timer;

	private bool hideMessageWithTimer = true;

	private void Awake()
	{
		((Behaviour)(object)actionIcon).enabled = false;
		PandoraSingleton<NoticeManager>.Instance.RegisterListener((!isLeft) ? Notices.COMBAT_RIGHT_MESSAGE : Notices.COMBAT_LEFT_MESSAGE, OnMessage);
	}

	private void OnMessage()
	{
		string titleTextId = null;
		int percentRoll = -1;
		int minDamage = -1;
		int maxDamage = -1;
		for (int i = 0; i < PandoraSingleton<NoticeManager>.Instance.Parameters.Count && i < 4; i++)
		{
			switch (i)
			{
			case 0:
				titleTextId = (string)PandoraSingleton<NoticeManager>.Instance.Parameters[0];
				break;
			case 1:
				percentRoll = (int)PandoraSingleton<NoticeManager>.Instance.Parameters[1];
				break;
			case 2:
				minDamage = (int)PandoraSingleton<NoticeManager>.Instance.Parameters[2];
				break;
			case 3:
				maxDamage = (int)PandoraSingleton<NoticeManager>.Instance.Parameters[3];
				break;
			}
		}
		Message(titleTextId, percentRoll, minDamage, maxDamage);
	}

	private void ActionChanged()
	{
	}

	public void Message(string titleTextId, int percentRoll = -1, int minDamage = -1, int maxDamage = -1)
	{
		background.set_sprite(messageBackground);
		if ((UnityEngine.Object)(object)mastery != null)
		{
			((Component)(object)mastery).gameObject.SetActive(titleTextId.EndsWith("_mstr", StringComparison.OrdinalIgnoreCase));
		}
		title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(titleTextId));
		SetMessage(-1, minDamage, maxDamage);
	}

	private void SetMessage(int percentRoll = -1, int minDamage = -1, int maxDamage = -1, bool hideWithTimer = true)
	{
		hideMessageWithTimer = hideWithTimer;
		OnEnable();
		((Behaviour)(object)title).enabled = true;
		((Behaviour)(object)actionIcon).enabled = false;
		((Behaviour)(object)percent).enabled = (percentRoll != -1);
		if (((Behaviour)(object)percent).enabled)
		{
			percent.set_text(percentRoll + "%");
		}
		((Behaviour)(object)damage).enabled = (minDamage + maxDamage > -1);
		if (((Behaviour)(object)damage).enabled)
		{
			damage.set_text($"{minDamage}-{maxDamage}");
		}
	}

	public void Warning(string titleTextId, int percentRoll = -1, int minDamage = -1, int maxDamage = -1, bool isPotential = true)
	{
		background.set_sprite(warningBackground);
		if (isPotential)
		{
			title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("reaction_potential", "#" + titleTextId));
		}
		else
		{
			title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(titleTextId));
		}
		SetMessage(percentRoll, minDamage, maxDamage);
	}

	public void WarningNoTimer(string titleTextId, int percentRoll = -1, int minDamage = -1, int maxDamage = -1, bool isPotential = true)
	{
		Warning(titleTextId, percentRoll, minDamage, maxDamage, isPotential);
		hideMessageWithTimer = false;
	}

	protected override void OnUnitChanged()
	{
	}

	public void HideWithTimer()
	{
		if (base.IsVisible)
		{
			if (!hideMessageWithTimer)
			{
				OnDisable();
				timer = 0f;
			}
			else if (timer <= 0f)
			{
				timer = 4f;
			}
		}
	}

	private void Update()
	{
		if (timer > 0f)
		{
			timer -= Time.deltaTime;
			if (timer <= 0f)
			{
				OnDisable();
			}
		}
	}
}
