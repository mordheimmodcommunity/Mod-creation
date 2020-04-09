using UnityEngine;

public class FadeAction : MonoBehaviour
{
	public delegate void OnFadeCallback();

	private Color Transparent = new Color(0f, 0f, 0f, 0f);

	private Color Black = new Color(0f, 0f, 0f, 1f);

	public int bleeding = 10;

	public float fadeDuration = 2f;

	public bool destroy = true;

	private bool _started;

	private Texture2D _texture;

	private Color _currentColor;

	private Rect _rect;

	private float _time;

	private bool _isFadeIn;

	private OnFadeCallback onFade;

	private OnFadeCallback onFadeFinish;

	public void Fade(OnFadeCallback callback, OnFadeCallback finishCallback = null)
	{
		onFade = callback;
		onFadeFinish = finishCallback;
		Object.DontDestroyOnLoad(base.gameObject);
		_isFadeIn = true;
		_time = fadeDuration;
		_rect = new Rect(-bleeding, -bleeding, Screen.width + bleeding, Screen.height + bleeding);
		_texture = new Texture2D(1, 1);
		_texture.SetPixel(0, 0, Transparent);
		_texture.Apply();
		_started = true;
	}

	private void OnGUI()
	{
		if (_started)
		{
			GUI.depth = 0;
			GUI.DrawTexture(_rect, _texture, ScaleMode.StretchToFill);
		}
	}

	private void Update()
	{
		if (!_started)
		{
			return;
		}
		_time -= Time.smoothDeltaTime;
		if (_time < 0f)
		{
			if (_isFadeIn)
			{
				_time = fadeDuration;
				_isFadeIn = false;
				if (onFade != null)
				{
					onFade();
					onFade = null;
				}
				return;
			}
			_started = false;
			if (destroy)
			{
				if (onFadeFinish != null)
				{
					onFadeFinish();
					onFadeFinish = null;
				}
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			if (_isFadeIn)
			{
				_currentColor = Color.Lerp(Black, Transparent, _time / fadeDuration);
			}
			else
			{
				_currentColor = Color.Lerp(Transparent, Black, _time / fadeDuration);
			}
			_texture.SetPixel(0, 0, _currentColor);
			_texture.Apply();
		}
	}
}
