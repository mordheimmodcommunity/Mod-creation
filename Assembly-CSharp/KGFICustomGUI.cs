using UnityEngine;

public interface KGFICustomGUI
{
	string GetName();

	string GetHeaderName();

	Texture2D GetIcon();

	void Render();
}
