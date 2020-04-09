using UnityEngine;

public class test_resolution : MonoBehaviour
{
	public Texture2D boxBg;

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			Application.CaptureScreenshot("Screenshot.png");
		}
	}

	private void OnGUI()
	{
		GUI.skin.box.normal.background = boxBg;
		GUI.skin.box.border = new RectOffset(2, 2, 2, 2);
		GUI.skin.box.margin = new RectOffset(0, 0, 0, 0);
		GUI.skin.box.overflow = new RectOffset(0, 0, 0, 0);
		GUI.skin.box.padding = new RectOffset(0, 0, 0, 0);
		GUI.skin.box.stretchWidth = false;
		GUI.skin.box.stretchHeight = false;
		int num = 0;
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution = resolutions[i];
			GUI.Box(new Rect(Screen.width / 2 - resolution.width / 2, Screen.height / 2 - resolution.height / 2, resolution.width, resolution.height), string.Empty);
			GUI.Label(new Rect(Screen.width / 2 - resolution.width / 2 + 2, Screen.height / 2 - resolution.height / 2, resolution.width, resolution.height), resolution.width + " x " + resolution.height);
			if (GUI.Button(new Rect(0f, num * 25 + 50, 200f, 25f), resolution.width + " x " + resolution.height))
			{
				Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
			}
			num++;
		}
		if (GUI.Button(new Rect(0f, 0f, 200f, 50f), "Toggle Fullscreen"))
		{
			Screen.fullScreen = !Screen.fullScreen;
		}
		GUI.Label(new Rect(0f, num * 25 + 50, 200f, 50f), "Total resolutions = " + Screen.resolutions.Length);
		GUI.Label(new Rect(200f, 0f, 200f, 50f), "Current resolution = " + Screen.width + " x " + Screen.height);
	}
}
