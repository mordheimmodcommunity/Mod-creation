using UnityEngine;

public class AnimationScrollTexture : MonoBehaviour
{
	public float Speed = 0.25f;

	private Renderer r;

	private void Awake()
	{
		r = GetComponent<Renderer>();
	}

	private void FixedUpdate()
	{
		float y = Time.time * (0f - Speed);
		r.material.mainTextureOffset = new Vector2(0f, y);
	}
}
