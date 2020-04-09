using System;
using UnityEngine;
using UnityEngine.UI;

public class TextInputPopup : ConfirmationPopupView
{
	[SerializeField]
	private InputField inputField;

	public void ShowLocalized(string newTitle, string newText, Action<bool, string> callback, bool hideButtons = false, string initialContent = "", int maxLength = 0)
	{
		base.ShowLocalized(newTitle, newText, delegate(bool confirm)
		{
			callback(confirm, inputField.get_text());
		}, hideButtons);
		((Selectable)inputField).Select();
		inputField.set_characterLimit(maxLength);
		inputField.set_text(initialContent);
	}

	public void Show(string titleId, string textId, Action<bool, string> callback, bool hideButtons = false, string initialContent = "", int maxLength = 0)
	{
		base.Show(titleId, textId, delegate(bool confirm)
		{
			callback(confirm, inputField.get_text());
		}, hideButtons);
		((Selectable)inputField).Select();
		inputField.set_characterLimit(maxLength);
		inputField.set_text(initialContent);
	}
}
