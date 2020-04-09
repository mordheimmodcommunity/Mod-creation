using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextTutoImageModule : MonoBehaviour
{
	public List<GameObject> tutoSprites;

	public Text title;

	public Text counter;

	public ButtonGroup previous;

	public ButtonGroup next;

	public Sprite prevSprite;

	public Sprite nextSprite;

	private int currentTutorialIndex;

	private int currentMessageIndex;

	private Action backCallback;

	private void Awake()
	{
		for (int i = 0; i < tutoSprites.Count; i++)
		{
			tutoSprites[i].SetActive(value: false);
		}
	}

	public void Set(int index, Action backAction)
	{
		backCallback = backAction;
		this.SetSelected(force: true);
		currentTutorialIndex = Mathf.Clamp(index, 0, tutoSprites.Count - 1);
		for (int i = 0; i < tutoSprites.Count; i++)
		{
			tutoSprites[i].SetActive(i == currentTutorialIndex);
		}
		string name = tutoSprites[currentTutorialIndex].gameObject.name;
		title.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById("tutorial_" + name.Substring(name.Length - 2) + "_title"));
		Transform transform = tutoSprites[currentTutorialIndex].transform;
		for (int j = 0; j < transform.childCount; j++)
		{
			Image component = transform.GetChild(j).GetComponent<Image>();
			if ((UnityEngine.Object)(object)component != null && component.get_sprite() == null)
			{
				component.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadAsset<Sprite>("Assets/gui/assets/tutorial/hideout/pc/", AssetBundleId.LOADING, string.Format("{0}_title_{1}.png", name.Replace("tutorial", "tuto"), (j + 1).ToString("00"))));
			}
		}
		SetMessage(0);
	}

	private string GetPlatformFolder()
	{
		return string.Empty;
	}

	private void SetMessage(int index)
	{
		Transform transform = tutoSprites[currentTutorialIndex].transform;
		int childCount = transform.childCount;
		Debug.Log(currentTutorialIndex + " - " + childCount);
		if (index == childCount)
		{
			backCallback();
			return;
		}
		currentMessageIndex = Mathf.Clamp(index, 0, childCount - 1);
		if (currentMessageIndex == childCount - 1)
		{
			PandoraSingleton<GameManager>.Instance.Profile.CompleteTutorial(currentTutorialIndex + Constant.GetInt(ConstantId.COMBAT_TUTORIALS_COUNT));
			PandoraSingleton<GameManager>.Instance.SaveProfile();
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(i == currentMessageIndex);
		}
		counter.set_text(index + 1 + "/" + childCount);
		previous.gameObject.SetActive(currentMessageIndex != 0);
		next.gameObject.SetActive(value: true);
	}

	public void Setup()
	{
		previous.SetAction("h", "controls_action_prev", 0, negative: true, prevSprite);
		previous.OnAction(PrevUnit, mouseOnly: false);
		next.SetAction("h", "controls_action_next", 0, negative: false, nextSprite);
		next.OnAction(NextUnit, mouseOnly: false);
	}

	private void PrevUnit()
	{
		SetMessage(currentMessageIndex - 1);
	}

	private void NextUnit()
	{
		SetMessage(currentMessageIndex + 1);
	}
}
