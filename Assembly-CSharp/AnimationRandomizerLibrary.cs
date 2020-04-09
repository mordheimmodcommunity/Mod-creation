using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRandomizerLibrary : MonoBehaviour
{
	public List<Animator> animators;

	private void Start()
	{
		StartCoroutine(Randomize());
	}

	public IEnumerator Randomize()
	{
		while (true)
		{
			yield return new WaitForSeconds(PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, 10));
			for (int i = 0; i < animators.Count; i++)
			{
				switch (PandoraSingleton<GameManager>.Instance.LocalTyche.Rand(0, 10))
				{
				case 1:
					animators[i].SetTrigger("random_1");
					break;
				case 2:
					animators[i].SetTrigger("random_2");
					break;
				}
			}
		}
	}
}
