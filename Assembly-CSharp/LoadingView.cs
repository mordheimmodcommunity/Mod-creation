using UnityEngine;
using UnityEngine.UI;

public class LoadingView : MonoBehaviour
{
	public SceneLoadingTypeId id;

	public Image background;

	public AudioSource audioSrc;

	private string dialogName;

	public virtual void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	protected void LoadDialog(string soundName)
	{
		dialogName = soundName;
	}

	public void PlayDialog()
	{
		if (!string.IsNullOrEmpty(dialogName))
		{
			PandoraSingleton<Pan>.Instance.GetSound("voices/loading/english/", dialogName, cache: false, delegate(AudioClip clip)
			{
				audioSrc.clip = clip;
				audioSrc.Play();
			});
			dialogName = string.Empty;
		}
	}

	protected void LoadBackground(string name)
	{
		background.set_sprite(PandoraSingleton<AssetBundleLoader>.Instance.LoadAsset<Sprite>("Assets/gui/assets/loading/bg/", AssetBundleId.LOADING, name + ".png"));
	}
}
