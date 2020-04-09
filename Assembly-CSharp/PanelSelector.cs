using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PanelSelector : MonoBehaviour
{
	[Serializable]
	public struct ButtonPanel
	{
		public Toggle but;

		public GameObject panel;

		public GameObject defaultSelection;
	}

	public int startingPanel;

	public List<ButtonPanel> radioButtons = new List<ButtonPanel>();

	private void Awake()
	{
		for (int i = 0; i < radioButtons.Count; i++)
		{
			int index = i;
			ButtonPanel buttonPanel = radioButtons[i];
			((UnityEvent<bool>)(object)buttonPanel.but.onValueChanged).AddListener((UnityAction<bool>)delegate(bool visible)
			{
				if (visible)
				{
					ActivatePanel(index);
				}
				else
				{
					ActivatePanel(-1);
				}
			});
		}
	}

	private void Start()
	{
		for (int i = 0; i < radioButtons.Count; i++)
		{
			ButtonPanel buttonPanel = radioButtons[i];
			buttonPanel.panel.SetActive(value: false);
		}
		if (startingPanel >= 0)
		{
			ButtonPanel buttonPanel2 = radioButtons[startingPanel];
			buttonPanel2.panel.SetActive(value: true);
			ButtonPanel buttonPanel3 = radioButtons[startingPanel];
			if (buttonPanel3.defaultSelection != null)
			{
				ButtonPanel buttonPanel4 = radioButtons[startingPanel];
				buttonPanel4.defaultSelection.SetSelected(force: true);
			}
		}
	}

	private void ActivatePanel(int index)
	{
		for (int i = 0; i < radioButtons.Count; i++)
		{
			ButtonPanel buttonPanel = radioButtons[i];
			buttonPanel.panel.gameObject.SetActive(i == index);
		}
		if (index >= 0 && index < radioButtons.Count)
		{
			ButtonPanel buttonPanel2 = radioButtons[index];
			if (buttonPanel2.defaultSelection != null)
			{
				ButtonPanel buttonPanel3 = radioButtons[index];
				buttonPanel3.defaultSelection.SetSelected(force: true);
			}
		}
	}
}
