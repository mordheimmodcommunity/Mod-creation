using UnityEngine;

public class MainMenuUnit : MonoBehaviour
{
	public UnitId unitId;

	public WarbandId warbandId;

	public AudioSource audioSource;

	private Animator animator;

	private Dissolver dissolver;

	private ShaderSetter shaderSetter;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		dissolver = GetComponent<Dissolver>();
		shaderSetter = GetComponent<ShaderSetter>();
	}

	private void Start()
	{
		animator.Rebind();
		shaderSetter.ApplyShaderParams();
	}

	public void LaunchAction(UnitActionId id, bool success, UnitStateId stateId, int variation = 0)
	{
		animator.SetTrigger("shout");
	}

	public void Hide(bool visible)
	{
		dissolver.Hide(visible, force: true);
	}

	public void EventSound(string soundName)
	{
	}

	public void EventShout(string soundName)
	{
		soundName = soundName.Replace("(unit)", unitId.ToLowerString());
		PlaySound(soundName);
	}

	private void PlaySound(string soundName)
	{
		PandoraSingleton<Pan>.Instance.GetSound(soundName, cache: true, delegate(AudioClip clip)
		{
			if (clip != null && audioSource != null)
			{
				audioSource.PlayOneShot(clip);
			}
		});
	}
}
