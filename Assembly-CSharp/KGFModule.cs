using System;
using UnityEngine;

public abstract class KGFModule : KGFObject, KGFIValidator
{
	private const string itsCopyrightText = "KOLMICH Creations e.U. is a small company based out of Vienna, Austria.\nWhile developing cool unity3d projects we put an immense amount of time \nto create professional tools and professional content. \n\n\nIf you have any ideas on improvements or you just want to give us some feedback use one of the links below.";

	private Version itsCurrentVersion;

	private Version itsMinimumCoreVersion;

	private static KGFModule itsOpenModule;

	public KGFModule(Version theCurrentVersion, Version theMinimumCoreVersion)
	{
		itsCurrentVersion = theCurrentVersion;
		itsMinimumCoreVersion = theMinimumCoreVersion;
		if (KGFCoreVersion.GetCurrentVersion() < itsMinimumCoreVersion)
		{
			Debug.LogError("the KGFCore verison installed in this scene is older than the required version. please update the KGFCore to the latest version");
		}
	}

	public Version GetCurrentVersion()
	{
		return itsCurrentVersion.Clone() as Version;
	}

	public Version GetRequiredCoreVersion()
	{
		return itsMinimumCoreVersion.Clone() as Version;
	}

	public abstract string GetName();

	public abstract Texture2D GetIcon();

	public abstract string GetDocumentationPath();

	public abstract string GetForumPath();

	public abstract KGFMessageList Validate();

	public static void OpenHelpWindow(KGFModule theModule)
	{
		itsOpenModule = theModule;
	}

	public static void RenderHelpWindow()
	{
		if (itsOpenModule != null)
		{
			int num = 512 + (int)KGFGUIUtility.GetSkinHeight() * 2;
			int num2 = 256 + (int)KGFGUIUtility.GetSkinHeight() * 7;
			Rect theRect = new Rect((Screen.width - num) / 2, (Screen.height - num2) / 2, num, num2);
			KGFGUIUtility.Window(12345689, theRect, RenderHelpWindowMethod, itsOpenModule.GetName() + " (part of KOLMICH Game Framework)");
			if (theRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				itsOpenModule = null;
			}
		}
		else
		{
			itsOpenModule = null;
		}
	}

	private static void RenderHelpWindowMethod(int theWindowID)
	{
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxInvisible, GUILayout.ExpandHeight(expand: true));
		KGFGUIUtility.BeginHorizontalPadding();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop, GUILayout.ExpandWidth(expand: true));
		GUILayout.FlexibleSpace();
		GUILayout.Label(KGFGUIUtility.GetLogo(), GUILayout.Height(50f));
		GUILayout.FlexibleSpace();
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxBottom, GUILayout.ExpandWidth(expand: true), GUILayout.ExpandHeight(expand: true));
		GUILayout.Label("KOLMICH Creations e.U. is a small company based out of Vienna, Austria.\nWhile developing cool unity3d projects we put an immense amount of time \nto create professional tools and professional content. \n\n\nIf you have any ideas on improvements or you just want to give us some feedback use one of the links below.", GUILayout.ExpandWidth(expand: true));
		KGFGUIUtility.EndHorizontalBox();
		GUILayout.Space(KGFGUIUtility.GetSkinHeight());
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkTop, GUILayout.ExpandWidth(expand: true));
		KGFGUIUtility.Label(itsOpenModule.GetName() + " version:", KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
		KGFGUIUtility.Label(itsOpenModule.GetCurrentVersion().ToString(), KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
		GUILayout.FlexibleSpace();
		KGFGUIUtility.Label("req. KGFCore version:", KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
		KGFGUIUtility.Label(itsOpenModule.GetRequiredCoreVersion().ToString(), KGFGUIUtility.eStyleLabel.eLabelFitIntoBox);
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.BeginHorizontalBox(KGFGUIUtility.eStyleBox.eBoxDarkBottom, GUILayout.ExpandWidth(expand: true));
		KGFGUIUtility.BeginVerticalPadding();
		if (KGFGUIUtility.Button(KGFGUIUtility.GetHelpIcon(), "documentation", KGFGUIUtility.eStyleButton.eButtonLeft, GUILayout.ExpandWidth(expand: true)))
		{
			Application.OpenURL("http://www.kolmich.at/documentation/" + itsOpenModule.GetDocumentationPath());
			itsOpenModule = null;
		}
		if (KGFGUIUtility.Button(KGFGUIUtility.GetHelpIcon(), "forum", KGFGUIUtility.eStyleButton.eButtonMiddle, GUILayout.ExpandWidth(expand: true)))
		{
			Application.OpenURL("http://www.kolmich.at/forum/" + itsOpenModule.GetForumPath());
			itsOpenModule = null;
		}
		if (KGFGUIUtility.Button(KGFGUIUtility.GetHelpIcon(), "homepage", KGFGUIUtility.eStyleButton.eButtonRight, GUILayout.ExpandWidth(expand: true)))
		{
			Application.OpenURL("http://www.kolmich.at");
			itsOpenModule = null;
		}
		KGFGUIUtility.EndVerticalPadding();
		KGFGUIUtility.EndHorizontalBox();
		KGFGUIUtility.EndHorizontalPadding();
		KGFGUIUtility.EndVerticalBox();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
}
