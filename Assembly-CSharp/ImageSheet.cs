using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ImageSheet : MonoBehaviour
{
	private Image img;

	public List<Sprite> sprites;

	public float speed;

	private float timer;

	private int idx;

	private void Start()
	{
		img = GetComponent<Image>();
		timer = 0f;
		idx = 0;
		img.set_overrideSprite(sprites[idx]);
	}

	private void Update()
	{
		timer += Time.deltaTime;
		if (timer >= speed)
		{
			idx = (idx + 1) % sprites.Count;
			img.set_overrideSprite(sprites[idx]);
			timer = 0f;
		}
	}
}
