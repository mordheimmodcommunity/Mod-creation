using UnityEngine;

public class KGFGUIUtilityTutorial : MonoBehaviour
{
	private void OnGUI()
	{
		int num = 300;
		int num2 = 250;
		Rect screenRect = new Rect((Screen.width - num) / 2, (Screen.height - num2) / 2, num, num2);
		GUILayout.BeginArea(screenRect);
		KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxInvisible, GUILayout.ExpandHeight(expand: true));
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop);
		GUILayout.FlexibleSpace();
		KGFGUIUtility.Label("KGFGUIUtility Tutorial", KGFGUIUtility.eStyleLabel.eLabel);
		GUILayout.FlexibleSpace();
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxMiddleVertical, GUILayout.ExpandHeight(expand: true));
		GUILayout.FlexibleSpace();
		KGFGUIUtility.BeginHorizontalPadding();
		KGFGUIUtility.Button("Top", KGFGUIUtility.eStyleButton.eButtonTop, GUILayout.ExpandWidth(expand: true));
		KGFGUIUtility.Button("Middle", KGFGUIUtility.eStyleButton.eButtonMiddle, GUILayout.ExpandWidth(expand: true));
		KGFGUIUtility.Button("Bottom", KGFGUIUtility.eStyleButton.eButtonBottom, GUILayout.ExpandWidth(expand: true));
		KGFGUIUtility.EndHorizontalPadding();
		GUILayout.FlexibleSpace();
		KGFGUIUtility.EndVerticalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkBottom);
		KGFGUIUtility.BeginVerticalPadding();
		KGFGUIUtility.Button("Left", KGFGUIUtility.eStyleButton.eButtonLeft, GUILayout.ExpandWidth(expand: true));
		KGFGUIUtility.Button("Center", KGFGUIUtility.eStyleButton.eButtonMiddle, GUILayout.ExpandWidth(expand: true));
		KGFGUIUtility.Button("Right", KGFGUIUtility.eStyleButton.eButtonRight, GUILayout.ExpandWidth(expand: true));
		KGFGUIUtility.EndVerticalPadding();
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.EndVerticalBox();
		GUILayout.EndArea();
	}
}
