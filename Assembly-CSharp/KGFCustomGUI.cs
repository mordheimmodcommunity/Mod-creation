using System;
using System.Collections.Generic;
using UnityEngine;

public class KGFCustomGUI : KGFModule
{
	[Serializable]
	public class KGFDataCustomGUI
	{
		public Texture2D itsUnknownIcon;

		public KeyCode itsModifierKey;

		public KeyCode itsSchortcutKey = KeyCode.F3;

		public bool itsBarVisible = true;
	}

	private static KGFCustomGUI itsInstance = null;

	public KGFDataCustomGUI itsDataModuleCustomGUI = new KGFDataCustomGUI();

	private static List<KGFICustomGUI> itsCustomGuiList = null;

	private static KGFICustomGUI itsCurrentSelectedGUI = null;

	private static Rect itsWindowRectangle = new Rect(50f, 50f, 800f, 600f);

	public KGFCustomGUI()
		: base(new Version(1, 0, 0, 1), new Version(1, 0, 0, 0))
	{
	}

	public static Rect GetItsWindowRectangle()
	{
		return itsWindowRectangle;
	}

	protected override void KGFAwake()
	{
		base.KGFAwake();
		if (itsInstance == null)
		{
			itsInstance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
		KGFAccessor.RegisterAddEvent<KGFICustomGUI>(OnCustomGuiChanged);
		KGFAccessor.RegisterRemoveEvent<KGFICustomGUI>(OnCustomGuiChanged);
		UpdateInternalList();
	}

	private void OnCustomGuiChanged(object theSender, EventArgs theArgs)
	{
		UpdateInternalList();
	}

	private void UpdateInternalList()
	{
		itsCustomGuiList = KGFAccessor.GetObjects<KGFICustomGUI>();
	}

	protected void OnGUI()
	{
		Render();
	}

	protected void Update()
	{
		if ((Input.GetKey(itsDataModuleCustomGUI.itsModifierKey) && Input.GetKeyDown(itsDataModuleCustomGUI.itsSchortcutKey)) || (itsDataModuleCustomGUI.itsModifierKey == KeyCode.None && Input.GetKeyDown(itsDataModuleCustomGUI.itsSchortcutKey)))
		{
			itsDataModuleCustomGUI.itsBarVisible = !itsDataModuleCustomGUI.itsBarVisible;
		}
	}

	public static void Render()
	{
		KGFGUIUtility.SetSkinIndex(0);
		if (itsInstance != null && itsInstance.itsDataModuleCustomGUI.itsBarVisible)
		{
			GUIStyle styleToggl = KGFGUIUtility.GetStyleToggl(KGFGUIUtility.eStyleToggl.eTogglRadioStreched);
			GUIStyle styleBox = KGFGUIUtility.GetStyleBox(KGFGUIUtility.eStyleBox.eBoxDecorated);
			Vector2 contentOffset = styleToggl.contentOffset;
			int num = (int)(contentOffset.x + (float)styleToggl.padding.horizontal + (KGFGUIUtility.GetSkinHeight() - (float)styleToggl.padding.vertical));
			int num2 = (int)((float)(styleBox.margin.top + styleBox.margin.bottom + styleBox.padding.top + styleBox.padding.bottom) + (styleToggl.fixedHeight + (float)styleToggl.margin.top) * (float)itsCustomGuiList.Count);
			GUILayout.BeginArea(new Rect(Screen.width - num, (Screen.height - num2) / 2, num, num2));
			KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBoxDecorated, GUILayout.ExpandWidth(expand: true), GUILayout.ExpandHeight(expand: true));
			GUILayout.FlexibleSpace();
			foreach (KGFICustomGUI itsCustomGui in itsCustomGuiList)
			{
				bool flag = (itsCurrentSelectedGUI != null && itsCurrentSelectedGUI == itsCustomGui) ? true : false;
				Texture2D texture2D = itsCustomGui.GetIcon();
				if (texture2D == null)
				{
					texture2D = itsInstance.itsDataModuleCustomGUI.itsUnknownIcon;
				}
				if (flag != KGFGUIUtility.Toggle(flag, texture2D, KGFGUIUtility.eStyleToggl.eTogglRadioStreched))
				{
					if (flag)
					{
						itsCurrentSelectedGUI = null;
					}
					else
					{
						itsCurrentSelectedGUI = itsCustomGui;
					}
				}
			}
			GUILayout.FlexibleSpace();
			KGFGUIUtility.EndVerticalBox();
			GUILayout.EndArea();
			itsInstance.DrawCurrentCustomGUI(num);
		}
		KGFGUIUtility.SetSkinIndex(1);
	}

	private static KGFCustomGUI GetInstance()
	{
		return itsInstance;
	}

	private void DrawCurrentCustomGUI(float aCustomGuiWidth)
	{
		if (itsCurrentSelectedGUI != null)
		{
			float num = KGFGUIUtility.GetSkinHeight() + (float)KGFGUIUtility.GetStyleButton(KGFGUIUtility.eStyleButton.eButton).margin.vertical + (float)KGFGUIUtility.GetStyleBox(KGFGUIUtility.eStyleBox.eBoxDecorated).padding.vertical;
			GUILayout.BeginArea(new Rect(num, num, (float)Screen.width - aCustomGuiWidth - num, (float)Screen.height - num * 2f));
			KGFGUIUtility.BeginVerticalBox(KGFGUIUtility.eStyleBox.eBox);
			if (itsCurrentSelectedGUI.GetIcon() == null)
			{
				KGFGUIUtility.BeginWindowHeader(itsCurrentSelectedGUI.GetHeaderName(), itsDataModuleCustomGUI.itsUnknownIcon);
			}
			else
			{
				KGFGUIUtility.BeginWindowHeader(itsCurrentSelectedGUI.GetHeaderName(), itsCurrentSelectedGUI.GetIcon());
			}
			GUILayout.FlexibleSpace();
			if (!KGFGUIUtility.EndWindowHeader(theCloseButton: true))
			{
				GUILayout.Space(0f);
				itsCurrentSelectedGUI.Render();
			}
			else
			{
				itsCurrentSelectedGUI = null;
			}
			KGFGUIUtility.EndVerticalBox();
			GUILayout.EndArea();
		}
	}

	public static Texture2D GetDefaultIcon()
	{
		if (itsInstance != null)
		{
			return itsInstance.itsDataModuleCustomGUI.itsUnknownIcon;
		}
		return null;
	}

	public override Texture2D GetIcon()
	{
		return null;
	}

	public override string GetName()
	{
		return "KGFCustomGUI";
	}

	public override string GetDocumentationPath()
	{
		return "KGFCustomGUIManual.html";
	}

	public override string GetForumPath()
	{
		return string.Empty;
	}

	public override KGFMessageList Validate()
	{
		KGFMessageList kGFMessageList = new KGFMessageList();
		if (itsDataModuleCustomGUI.itsUnknownIcon == null)
		{
			kGFMessageList.AddWarning("the unknown icon is missing");
		}
		if (itsDataModuleCustomGUI.itsModifierKey == itsDataModuleCustomGUI.itsSchortcutKey)
		{
			kGFMessageList.AddInfo("the modifier key is equal to the shortcut key");
		}
		return kGFMessageList;
	}
}
