using UnityEngine;
using UnityEngine.UI;

public class UILocalizer : MonoBehaviour
{
	public bool waitForInit;

	public LocaleBuild[] overrideLocale;

	private void Start()
	{
		if (!waitForInit)
		{
			Localize();
		}
	}

	private void Update()
	{
		if (waitForInit && PandoraSingleton<Hephaestus>.Instance.IsInitialized())
		{
			Localize();
		}
	}

	private void Localize()
	{
		Text component = base.gameObject.GetComponent<Text>();
		if ((Object)(object)component == null)
		{
			PandoraDebug.LogError("No component text found on object " + base.name, "GUI", this);
		}
		for (int i = 0; i < overrideLocale.Length; i++)
		{
			if (overrideLocale[i].platform == Application.platform && !string.IsNullOrEmpty(overrideLocale[i].overrideString))
			{
				component.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(overrideLocale[i].overrideString));
				return;
			}
		}
		if (!string.IsNullOrEmpty(component.get_text()))
		{
			component.set_text(PandoraSingleton<LocalizationManager>.Instance.GetStringById(component.get_text()));
		}
		Object.Destroy(this);
	}
}
