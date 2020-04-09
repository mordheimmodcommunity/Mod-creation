using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(LayoutElement))]
[ExecuteInEditMode]
public class ImageLayout : MonoBehaviour
{
	private Image _image;

	private LayoutElement _layout;

	public Rect rect;

	private void Awake()
	{
		_image = GetComponent<Image>();
		_layout = GetComponent<LayoutElement>();
	}
}
