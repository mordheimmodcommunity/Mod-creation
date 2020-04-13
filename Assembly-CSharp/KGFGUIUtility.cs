using System.Collections.Generic;
using UnityEngine;

public class KGFGUIUtility
{
    public enum eStyleButton
    {
        eButton,
        eButtonLeft,
        eButtonRight,
        eButtonTop,
        eButtonBottom,
        eButtonMiddle
    }

    public enum eStyleToggl
    {
        eToggl,
        eTogglStreched,
        eTogglCompact,
        eTogglSuperCompact,
        eTogglRadioStreched,
        eTogglRadioCompact,
        eTogglRadioSuperCompact,
        eTogglSwitch,
        eTogglBoolean,
        eTogglArrow,
        eTogglButton
    }

    public enum eStyleTextField
    {
        eTextField,
        eTextFieldLeft,
        eTextFieldRight
    }

    public enum eStyleBox
    {
        eBox,
        eBoxInvisible,
        eBoxInteractive,
        eBoxLeft,
        eBoxLeftInteractive,
        eBoxRight,
        eBoxRightInteractive,
        eBoxMiddleHorizontal,
        eBoxMiddleHorizontalInteractive,
        eBoxTop,
        eBoxTopInteractive,
        eBoxMiddleVertical,
        eBoxMiddleVerticalInteractive,
        eBoxBottom,
        eBoxBottomInteractive,
        eBoxDark,
        eBoxDarkInteractive,
        eBoxDarkLeft,
        eBoxDarkLeftInteractive,
        eBoxDarkRight,
        eBoxDarkRightInteractive,
        eBoxDarkMiddleHorizontal,
        eBoxDarkMiddleHorizontalInteractive,
        eBoxDarkTop,
        eBoxDarkTopInteractive,
        eBoxDarkBottom,
        eBoxDarkBottomInteractive,
        eBoxDarkMiddleVertical,
        eBoxDarkMiddleVerticalInteractive,
        eBoxDecorated
    }

    public enum eStyleSeparator
    {
        eSeparatorHorizontal,
        eSeparatorVertical,
        eSeparatorVerticalFitInBox
    }

    public enum eStyleLabel
    {
        eLabel,
        eLabelMultiline,
        eLabelTitle,
        eLabelFitIntoBox
    }

    public enum eStyleImage
    {
        eImage,
        eImageFitIntoBox
    }

    public enum eCursorState
    {
        eUp,
        eRight,
        eDown,
        eLeft,
        eCenter,
        eNone
    }

    private static bool itsEnableKGFSkins = true;

    private static string[] itsDefaultGuiSkinPath = new string[2]
    {
        "KGFSkins/default/skins/skin_default_16",
        "KGFSkins/default/skins/skin_default_16"
    };

    private static int itsSkinIndex = 1;

    private static bool[] itsResetPath = new bool[2];

    protected static GUISkin[] itsSkin = new GUISkin[2];

    private static Texture2D itsIcon = null;

    private static Texture2D itsKGFCopyright = null;

    private static Texture2D itsIconHelp = null;

    private static Dictionary<string, AudioClip> itsAudioClips = new Dictionary<string, AudioClip>();

    private static float itsVolume = 1f;

    private static GUIStyle[] itsStyleToggle = new GUIStyle[2];

    private static GUIStyle[] itsStyleTextField = new GUIStyle[2];

    private static GUIStyle[] itsStyleTextFieldLeft = new GUIStyle[2];

    private static GUIStyle[] itsStyleTextFieldRight = new GUIStyle[2];

    private static GUIStyle[] itsStyleTextArea = new GUIStyle[2];

    private static GUIStyle[] itsStyleWindow = new GUIStyle[2];

    private static GUIStyle[] itsStyleHorizontalSlider = new GUIStyle[2];

    private static GUIStyle[] itsStyleHorizontalSliderThumb = new GUIStyle[2];

    private static GUIStyle[] itsStyleVerticalSlider = new GUIStyle[2];

    private static GUIStyle[] itsStyleVerticalSliderThumb = new GUIStyle[2];

    private static GUIStyle[] itsStyleHorizontalScrollbar = new GUIStyle[2];

    private static GUIStyle[] itsStyleHorizontalScrollbarThumb = new GUIStyle[2];

    private static GUIStyle[] itsStyleHorizontalScrollbarLeftButton = new GUIStyle[2];

    private static GUIStyle[] itsStyleHorizontalScrollbarRightButton = new GUIStyle[2];

    private static GUIStyle[] itsStyleVerticalScrollbar = new GUIStyle[2];

    private static GUIStyle[] itsStyleVerticalScrollbarThumb = new GUIStyle[2];

    private static GUIStyle[] itsStyleVerticalScrollbarUpButton = new GUIStyle[2];

    private static GUIStyle[] itsStyleVerticalScrollbarDownButton = new GUIStyle[2];

    private static GUIStyle[] itsStyleScrollView = new GUIStyle[2];

    private static GUIStyle[] itsStyleMinimap = new GUIStyle[2];

    private static GUIStyle[] itsStyleMinimapButton = new GUIStyle[2];

    private static GUIStyle[] itsStyleToggleStreched = new GUIStyle[2];

    private static GUIStyle[] itsStyleToggleCompact = new GUIStyle[2];

    private static GUIStyle[] itsStyleToggleSuperCompact = new GUIStyle[2];

    private static GUIStyle[] itsStyleToggleRadioStreched = new GUIStyle[2];

    private static GUIStyle[] itsStyleToggleRadioCompact = new GUIStyle[2];

    private static GUIStyle[] itsStyleToggleRadioSuperCompact = new GUIStyle[2];

    private static GUIStyle[] itsStyleToggleSwitch = new GUIStyle[2];

    private static GUIStyle[] itsStyleToggleBoolean = new GUIStyle[2];

    private static GUIStyle[] itsStyleToggleArrow = new GUIStyle[2];

    private static GUIStyle[] itsStyleToggleButton = new GUIStyle[2];

    private static GUIStyle[] itsStyleButton = new GUIStyle[2];

    private static GUIStyle[] itsStyleButtonLeft = new GUIStyle[2];

    private static GUIStyle[] itsStyleButtonRight = new GUIStyle[2];

    private static GUIStyle[] itsStyleButtonTop = new GUIStyle[2];

    private static GUIStyle[] itsStyleButtonBottom = new GUIStyle[2];

    private static GUIStyle[] itsStyleButtonMiddle = new GUIStyle[2];

    private static GUIStyle[] itsStyleBox = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxInvisible = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxLeft = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxLeftInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxRight = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxRightInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxMiddleHorizontal = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxMiddleHorizontalInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxTop = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxTopInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxBottom = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxBottomInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxMiddleVertical = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxMiddleVerticalInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDark = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkLeft = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkLeftInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkRight = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkRightInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkMiddleHorizontal = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkMiddleHorizontalInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkTop = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkTopInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkBottom = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkBottomInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkMiddleVertical = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDarkMiddleVerticalInteractive = new GUIStyle[2];

    private static GUIStyle[] itsStyleBoxDecorated = new GUIStyle[2];

    private static GUIStyle[] itsStyleSeparatorVertical = new GUIStyle[2];

    private static GUIStyle[] itsStyleSeparatorVerticalFitInBox = new GUIStyle[2];

    private static GUIStyle[] itsStyleSeparatorHorizontal = new GUIStyle[2];

    private static GUIStyle[] itsStyleLabel = new GUIStyle[2];

    private static GUIStyle[] itsStyleLabelMultiline = new GUIStyle[2];

    private static GUIStyle[] itsStyleLabelTitle = new GUIStyle[2];

    private static GUIStyle[] itsStyleLabelFitInToBox = new GUIStyle[2];

    private static GUIStyle[] itsStyleTable = new GUIStyle[2];

    private static GUIStyle[] itsStyleTableHeadingRow = new GUIStyle[2];

    private static GUIStyle[] itsStyleTableHeadingCell = new GUIStyle[2];

    private static GUIStyle[] itsStyleTableRow = new GUIStyle[2];

    private static GUIStyle[] itsStyleTableRowCell = new GUIStyle[2];

    private static GUIStyle[] itsStyleCursor = new GUIStyle[2];

    public static Color itsEditorColorContent => new Color(0.1f, 0.1f, 0.1f);

    public static Color itsEditorColorTitle => new Color(0.1f, 0.1f, 0.1f);

    public static Color itsEditorDocumentation => new Color(0.74f, 0.79f, 0.64f);

    public static Color itsEditorColorDefault => new Color(1f, 1f, 1f);

    public static Color itsEditorColorInfo => new Color(1f, 1f, 1f);

    public static Color itsEditorColorWarning => new Color(1f, 1f, 0f);

    public static Color itsEditorColorError => new Color(0.9f, 0.5f, 0.5f);

    public static int GetSkinIndex()
    {
        return itsSkinIndex;
    }

    public static float GetSkinHeight()
    {
        if (itsSkinIndex == -1)
        {
            return 16f;
        }
        if (itsStyleButton != null && itsSkinIndex < itsStyleButton.Length && itsStyleButton[itsSkinIndex] != null)
        {
            return itsStyleButton[itsSkinIndex].fixedHeight;
        }
        return 16f;
    }

    public static GUISkin GetSkin()
    {
        if (itsSkin[itsSkinIndex] != null)
        {
            return itsSkin[itsSkinIndex];
        }
        return null;
    }

    public static Texture2D GetLogo()
    {
        if (itsIcon == null)
        {
            itsIcon = (Resources.Load("KGFCore/textures/logo") as Texture2D);
        }
        return itsIcon;
    }

    public static Texture2D GetHelpIcon()
    {
        if (itsIconHelp == null)
        {
            itsIconHelp = (Resources.Load("KGFCore/textures/help") as Texture2D);
        }
        return itsIconHelp;
    }

    public static Texture2D GetKGFCopyright()
    {
        if (itsKGFCopyright == null)
        {
            itsKGFCopyright = (Resources.Load("KGFCore/textures/kgf_copyright_512x256") as Texture2D);
        }
        return itsKGFCopyright;
    }

    public static GUIStyle GetStyleToggl(eStyleToggl theTogglStyle)
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.toggle;
        }
        Init();
        if (theTogglStyle == eStyleToggl.eTogglStreched && itsStyleToggleStreched[itsSkinIndex] != null)
        {
            return itsStyleToggleStreched[itsSkinIndex];
        }
        if (theTogglStyle == eStyleToggl.eTogglCompact && itsStyleToggleCompact[itsSkinIndex] != null)
        {
            return itsStyleToggleCompact[itsSkinIndex];
        }
        if (theTogglStyle == eStyleToggl.eTogglSuperCompact && itsStyleToggleSuperCompact[itsSkinIndex] != null)
        {
            return itsStyleToggleSuperCompact[itsSkinIndex];
        }
        if (theTogglStyle == eStyleToggl.eTogglRadioStreched && itsStyleToggleRadioStreched[itsSkinIndex] != null)
        {
            return itsStyleToggleRadioStreched[itsSkinIndex];
        }
        if (theTogglStyle == eStyleToggl.eTogglRadioCompact && itsStyleToggleRadioCompact[itsSkinIndex] != null)
        {
            return itsStyleToggleRadioCompact[itsSkinIndex];
        }
        if (theTogglStyle == eStyleToggl.eTogglRadioSuperCompact && itsStyleToggleRadioSuperCompact[itsSkinIndex] != null)
        {
            return itsStyleToggleRadioSuperCompact[itsSkinIndex];
        }
        if (theTogglStyle == eStyleToggl.eTogglSwitch && itsStyleToggleSwitch[itsSkinIndex] != null)
        {
            return itsStyleToggleSwitch[itsSkinIndex];
        }
        if (theTogglStyle == eStyleToggl.eTogglBoolean && itsStyleToggleBoolean[itsSkinIndex] != null)
        {
            return itsStyleToggleBoolean[itsSkinIndex];
        }
        if (theTogglStyle == eStyleToggl.eTogglArrow && itsStyleToggleArrow[itsSkinIndex] != null)
        {
            return itsStyleToggleArrow[itsSkinIndex];
        }
        if (theTogglStyle == eStyleToggl.eTogglButton && itsStyleToggleButton[itsSkinIndex] != null)
        {
            return itsStyleToggleButton[itsSkinIndex];
        }
        if (itsStyleToggle[itsSkinIndex] != null)
        {
            return itsStyleToggle[itsSkinIndex];
        }
        return GUI.skin.toggle;
    }

    public static GUIStyle GetStyleTextField(eStyleTextField theStyleTextField)
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.textField;
        }
        Init();
        if (theStyleTextField == eStyleTextField.eTextField && itsStyleTextField[itsSkinIndex] != null)
        {
            return itsStyleTextField[itsSkinIndex];
        }
        if (theStyleTextField == eStyleTextField.eTextFieldLeft && itsStyleTextFieldLeft[itsSkinIndex] != null)
        {
            return itsStyleTextFieldLeft[itsSkinIndex];
        }
        if (theStyleTextField == eStyleTextField.eTextFieldRight && itsStyleTextFieldRight[itsSkinIndex] != null)
        {
            return itsStyleTextFieldRight[itsSkinIndex];
        }
        return GUI.skin.textField;
    }

    public static GUIStyle GetStyleTextArea()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.textArea;
        }
        Init();
        if (itsStyleTextArea != null)
        {
            return itsStyleTextArea[itsSkinIndex];
        }
        return GUI.skin.textArea;
    }

    public static GUIStyle GetStyleHorizontalSlider()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.horizontalSlider;
        }
        Init();
        if (itsStyleHorizontalSlider[itsSkinIndex] != null)
        {
            return itsStyleHorizontalSlider[itsSkinIndex];
        }
        return GUI.skin.horizontalSlider;
    }

    public static GUIStyle GetStyleHorizontalSliderThumb()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.horizontalSliderThumb;
        }
        Init();
        if (itsStyleHorizontalSliderThumb[itsSkinIndex] != null)
        {
            return itsStyleHorizontalSliderThumb[itsSkinIndex];
        }
        return GUI.skin.horizontalSliderThumb;
    }

    public static GUIStyle GetStyleHorizontalScrollbar()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.horizontalScrollbar;
        }
        Init();
        if (itsStyleHorizontalScrollbar[itsSkinIndex] != null)
        {
            return itsStyleHorizontalScrollbar[itsSkinIndex];
        }
        return GUI.skin.horizontalScrollbar;
    }

    public static GUIStyle GetStyleHorizontalScrollbarThumb()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.horizontalScrollbarThumb;
        }
        Init();
        if (itsStyleHorizontalScrollbarThumb[itsSkinIndex] != null)
        {
            return itsStyleHorizontalScrollbarThumb[itsSkinIndex];
        }
        return GUI.skin.horizontalScrollbarThumb;
    }

    public static GUIStyle GetStyleHorizontalScrollbarLeftButton()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.horizontalScrollbarLeftButton;
        }
        Init();
        if (itsStyleHorizontalScrollbarLeftButton[itsSkinIndex] != null)
        {
            return itsStyleHorizontalScrollbarLeftButton[itsSkinIndex];
        }
        return GUI.skin.horizontalScrollbarLeftButton;
    }

    public static GUIStyle GetStyleHorizontalScrollbarRightButton()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.horizontalScrollbarRightButton;
        }
        Init();
        if (itsStyleHorizontalScrollbarRightButton[itsSkinIndex] != null)
        {
            return itsStyleHorizontalScrollbarRightButton[itsSkinIndex];
        }
        return GUI.skin.horizontalScrollbarRightButton;
    }

    public static GUIStyle GetStyleVerticalSlider()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.verticalSlider;
        }
        Init();
        if (itsStyleVerticalSlider[itsSkinIndex] != null)
        {
            return itsStyleVerticalSlider[itsSkinIndex];
        }
        return GUI.skin.verticalSlider;
    }

    public static GUIStyle GetStyleVerticalSliderThumb()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.verticalSliderThumb;
        }
        Init();
        if (itsStyleVerticalSliderThumb[itsSkinIndex] != null)
        {
            return itsStyleVerticalSliderThumb[itsSkinIndex];
        }
        return GUI.skin.verticalSliderThumb;
    }

    public static GUIStyle GetStyleVerticalScrollbar()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.verticalScrollbar;
        }
        Init();
        if (itsStyleVerticalScrollbar[itsSkinIndex] != null)
        {
            return itsStyleVerticalScrollbar[itsSkinIndex];
        }
        return GUI.skin.verticalScrollbar;
    }

    public static GUIStyle GetStyleVerticalScrollbarThumb()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.verticalScrollbarThumb;
        }
        Init();
        if (itsStyleVerticalScrollbarThumb[itsSkinIndex] != null)
        {
            return itsStyleVerticalScrollbarThumb[itsSkinIndex];
        }
        return GUI.skin.verticalScrollbarThumb;
    }

    public static GUIStyle GetStyleVerticalScrollbarUpButton()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.verticalScrollbarUpButton;
        }
        Init();
        if (itsStyleVerticalScrollbarUpButton[itsSkinIndex] != null)
        {
            return itsStyleVerticalScrollbarUpButton[itsSkinIndex];
        }
        return GUI.skin.verticalScrollbarUpButton;
    }

    public static GUIStyle GetStyleVerticalScrollbarDownButton()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.verticalScrollbarDownButton;
        }
        Init();
        if (itsStyleVerticalScrollbarDownButton[itsSkinIndex] != null)
        {
            return itsStyleVerticalScrollbarDownButton[itsSkinIndex];
        }
        return GUI.skin.verticalScrollbarDownButton;
    }

    public static GUIStyle GetStyleScrollView()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.scrollView;
        }
        Init();
        if (itsStyleScrollView[itsSkinIndex] != null)
        {
            return itsStyleScrollView[itsSkinIndex];
        }
        return GUI.skin.scrollView;
    }

    public static GUIStyle GetStyleMinimapBorder()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.box;
        }
        Init();
        if (itsStyleMinimap[itsSkinIndex] != null)
        {
            return itsStyleMinimap[itsSkinIndex];
        }
        return GUI.skin.box;
    }

    public static GUIStyle GetStyleMinimapButton()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.box;
        }
        Init();
        if (itsStyleMinimapButton[itsSkinIndex] != null)
        {
            return itsStyleMinimapButton[itsSkinIndex];
        }
        return GUI.skin.button;
    }

    public static GUIStyle GetStyleButton(eStyleButton theStyleButton)
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.button;
        }
        Init();
        if (theStyleButton == eStyleButton.eButton && itsStyleButton[itsSkinIndex] != null)
        {
            return itsStyleButton[itsSkinIndex];
        }
        if (theStyleButton == eStyleButton.eButtonLeft && itsStyleButtonLeft[itsSkinIndex] != null)
        {
            return itsStyleButtonLeft[itsSkinIndex];
        }
        if (theStyleButton == eStyleButton.eButtonRight && itsStyleButtonRight[itsSkinIndex] != null)
        {
            return itsStyleButtonRight[itsSkinIndex];
        }
        if (theStyleButton == eStyleButton.eButtonTop && itsStyleButtonTop[itsSkinIndex] != null)
        {
            return itsStyleButtonTop[itsSkinIndex];
        }
        if (theStyleButton == eStyleButton.eButtonBottom && itsStyleButtonBottom[itsSkinIndex] != null)
        {
            return itsStyleButtonBottom[itsSkinIndex];
        }
        if (theStyleButton == eStyleButton.eButtonMiddle && itsStyleButtonMiddle[itsSkinIndex] != null)
        {
            return itsStyleButtonMiddle[itsSkinIndex];
        }
        return GUI.skin.button;
    }

    public static GUIStyle GetStyleBox(eStyleBox theStyleBox)
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.box;
        }
        Init();
        if (theStyleBox == eStyleBox.eBox && itsStyleBox[itsSkinIndex] != null)
        {
            return itsStyleBox[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxInvisible && itsStyleBoxInvisible[itsSkinIndex] != null)
        {
            return itsStyleBoxInvisible[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxInteractive && itsStyleBox[itsSkinIndex] != null)
        {
            return itsStyleBoxInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxLeft && itsStyleBoxLeft[itsSkinIndex] != null)
        {
            return itsStyleBoxLeft[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxLeftInteractive && itsStyleBoxLeft[itsSkinIndex] != null)
        {
            return itsStyleBoxLeftInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxRight && itsStyleBoxRight[itsSkinIndex] != null)
        {
            return itsStyleBoxRight[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxRightInteractive && itsStyleBoxRight[itsSkinIndex] != null)
        {
            return itsStyleBoxRightInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxMiddleHorizontal && itsStyleBoxMiddleHorizontal[itsSkinIndex] != null)
        {
            return itsStyleBoxMiddleHorizontal[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxMiddleHorizontalInteractive && itsStyleBoxMiddleHorizontal[itsSkinIndex] != null)
        {
            return itsStyleBoxMiddleHorizontalInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxTop && itsStyleBoxTop[itsSkinIndex] != null)
        {
            return itsStyleBoxTop[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxTopInteractive && itsStyleBoxTop[itsSkinIndex] != null)
        {
            return itsStyleBoxTopInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxBottom && itsStyleBoxBottom[itsSkinIndex] != null)
        {
            return itsStyleBoxBottom[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxBottomInteractive && itsStyleBoxBottom[itsSkinIndex] != null)
        {
            return itsStyleBoxBottomInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxMiddleVertical && itsStyleBoxMiddleVertical[itsSkinIndex] != null)
        {
            return itsStyleBoxMiddleVertical[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxMiddleVerticalInteractive && itsStyleBoxMiddleVertical[itsSkinIndex] != null)
        {
            return itsStyleBoxMiddleVerticalInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDark && itsStyleBoxDark[itsSkinIndex] != null)
        {
            return itsStyleBoxDark[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkInteractive && itsStyleBoxDark[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkLeft && itsStyleBoxDarkLeft[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkLeft[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkLeftInteractive && itsStyleBoxDarkLeft[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkLeftInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkRight && itsStyleBoxDarkRight[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkRight[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkRightInteractive && itsStyleBoxDarkRight[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkRightInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkMiddleHorizontal && itsStyleBoxDarkMiddleHorizontal[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkMiddleHorizontal[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkMiddleHorizontalInteractive && itsStyleBoxDarkMiddleHorizontal[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkMiddleHorizontalInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkTop && itsStyleBoxDarkTop[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkTop[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkTopInteractive && itsStyleBoxDarkTop[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkTopInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkBottom && itsStyleBoxDarkBottom[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkBottom[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkBottomInteractive && itsStyleBoxDarkBottom[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkBottomInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkMiddleVertical && itsStyleBoxDarkMiddleVertical[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkMiddleVertical[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDarkMiddleVerticalInteractive && itsStyleBoxDarkMiddleVertical[itsSkinIndex] != null)
        {
            return itsStyleBoxDarkMiddleVerticalInteractive[itsSkinIndex];
        }
        if (theStyleBox == eStyleBox.eBoxDecorated && itsStyleBoxDecorated[itsSkinIndex] != null)
        {
            return itsStyleBoxDecorated[itsSkinIndex];
        }
        return GUI.skin.box;
    }

    public static GUIStyle GetStyleSeparator(eStyleSeparator theStyleSeparator)
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.box;
        }
        Init();
        if (theStyleSeparator == eStyleSeparator.eSeparatorHorizontal && itsStyleSeparatorHorizontal[itsSkinIndex] != null)
        {
            return itsStyleSeparatorHorizontal[itsSkinIndex];
        }
        if (theStyleSeparator == eStyleSeparator.eSeparatorVertical && itsStyleSeparatorVertical[itsSkinIndex] != null)
        {
            return itsStyleSeparatorVertical[itsSkinIndex];
        }
        if (theStyleSeparator == eStyleSeparator.eSeparatorVerticalFitInBox && itsStyleSeparatorVerticalFitInBox[itsSkinIndex] != null)
        {
            return itsStyleSeparatorVerticalFitInBox[itsSkinIndex];
        }
        return GUI.skin.label;
    }

    public static GUIStyle GetStyleLabel(eStyleLabel theStyleLabel)
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.label;
        }
        Init();
        if (theStyleLabel == eStyleLabel.eLabel && itsStyleLabel[itsSkinIndex] != null)
        {
            return itsStyleLabel[itsSkinIndex];
        }
        if (theStyleLabel == eStyleLabel.eLabelFitIntoBox && itsStyleLabelFitInToBox[itsSkinIndex] != null)
        {
            return itsStyleLabelFitInToBox[itsSkinIndex];
        }
        if (theStyleLabel == eStyleLabel.eLabelMultiline && itsStyleLabelMultiline[itsSkinIndex] != null)
        {
            return itsStyleLabelMultiline[itsSkinIndex];
        }
        if (theStyleLabel == eStyleLabel.eLabelTitle && itsStyleLabelTitle[itsSkinIndex] != null)
        {
            return itsStyleLabelTitle[itsSkinIndex];
        }
        return GUI.skin.box;
    }

    public static GUIStyle GetStyleWindow()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.window;
        }
        Init();
        if (itsStyleWindow[itsSkinIndex] != null)
        {
            return itsStyleWindow[itsSkinIndex];
        }
        return GUI.skin.window;
    }

    public static GUIStyle GetStyleCursor()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.box;
        }
        Init();
        if (itsStyleCursor[itsSkinIndex] != null)
        {
            return itsStyleCursor[itsSkinIndex];
        }
        return itsStyleCursor[itsSkinIndex];
    }

    public static GUIStyle GetTableStyle()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.box;
        }
        Init();
        if (itsStyleTable[itsSkinIndex] != null)
        {
            return itsStyleTable[itsSkinIndex];
        }
        return GUI.skin.box;
    }

    public static GUIStyle GetTableHeadingRowStyle()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.box;
        }
        Init();
        if (itsStyleTableHeadingRow[itsSkinIndex] != null)
        {
            return itsStyleTableHeadingRow[itsSkinIndex];
        }
        return GUI.skin.box;
    }

    public static GUIStyle GetTableHeadingCellStyle()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.box;
        }
        Init();
        if (itsStyleTableHeadingCell[itsSkinIndex] != null)
        {
            return itsStyleTableHeadingCell[itsSkinIndex];
        }
        return GUI.skin.box;
    }

    public static GUIStyle GetTableRowStyle()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.box;
        }
        Init();
        if (itsStyleTableRow[itsSkinIndex] != null)
        {
            return itsStyleTableRow[itsSkinIndex];
        }
        return GUI.skin.box;
    }

    public static GUIStyle GetTableCellStyle()
    {
        if (itsSkinIndex == -1)
        {
            return GUI.skin.box;
        }
        Init();
        if (itsStyleTableRowCell[itsSkinIndex] != null)
        {
            return itsStyleTableRowCell[itsSkinIndex];
        }
        return GUI.skin.box;
    }

    public static void SetVolume(float theVolume)
    {
        itsVolume = theVolume;
    }

    public static void SetSoundForButton(eStyleButton theButtonStyle, AudioClip theAudioClip)
    {
        SetSound(theButtonStyle.ToString(), theAudioClip);
    }

    public static void SetSoundForToggle(eStyleToggl theTogglStyle, AudioClip theAudioClip)
    {
        SetSound(theTogglStyle.ToString(), theAudioClip);
    }

    private static void SetSound(string theStyle, AudioClip theAudioClip)
    {
        if (theAudioClip != null && itsAudioClips.ContainsKey(theStyle))
        {
            itsAudioClips.Remove(theStyle);
        }
        else
        {
            itsAudioClips[theStyle] = theAudioClip;
        }
    }

    private static void PlaySound(string theStyle)
    {
        if (Application.isPlaying && itsAudioClips.ContainsKey(theStyle))
        {
            AudioSource.PlayClipAtPoint(itsAudioClips[theStyle], Vector3.zero, itsVolume);
        }
    }

    public static void SetEnableKGFSkinsInEdior(bool theSetEnableKGFSkins)
    {
        itsEnableKGFSkins = theSetEnableKGFSkins;
    }

    public static void SetSkinIndex(int theIndex)
    {
        itsSkinIndex = theIndex;
        if (itsSkinIndex == 0 && !itsEnableKGFSkins)
        {
            itsSkinIndex = -1;
        }
    }

    public static void SetSkinPath(string thePath)
    {
        itsDefaultGuiSkinPath[1] = thePath;
        itsResetPath[1] = true;
    }

    public static void SetSkinPathEditor(string thePath)
    {
        itsDefaultGuiSkinPath[0] = thePath;
        itsResetPath[0] = true;
    }

    public static string GetSkinPath()
    {
        return itsDefaultGuiSkinPath[itsSkinIndex];
    }

    private static void Init()
    {
        Init(theForceInit: false);
    }

    private static void Init(bool theForceInit)
    {
        if (itsSkinIndex != -1 && (!(itsSkin[itsSkinIndex] != null) || theForceInit || itsResetPath[itsSkinIndex]))
        {
            itsResetPath[itsSkinIndex] = false;
            Debug.Log("Loading skin: " + itsDefaultGuiSkinPath[itsSkinIndex]);
            itsSkin[itsSkinIndex] = (Resources.Load(itsDefaultGuiSkinPath[itsSkinIndex]) as GUISkin);
            if (itsSkin[itsSkinIndex] == null)
            {
                Debug.Log("Kolmich Game Framework default skin wasn`t found");
                itsSkin[itsSkinIndex] = GUI.skin;
                return;
            }
            GUI.skin = itsSkin[itsSkinIndex];
            itsStyleToggle[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("toggle");
            itsStyleTextField[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("textfield");
            itsStyleTextFieldLeft[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("textfield_left");
            itsStyleTextFieldRight[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("textfield_right");
            itsStyleTextArea[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("textarea");
            itsStyleWindow[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("window");
            itsStyleHorizontalSlider[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("horizontalslider");
            itsStyleHorizontalSliderThumb[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("horizontalsliderthumb");
            itsStyleVerticalSlider[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("verticalslider");
            itsStyleVerticalSliderThumb[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("verticalsliderthumb");
            itsStyleHorizontalScrollbar[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("horizontalscrollbar");
            itsStyleHorizontalScrollbarThumb[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("horizontalscrollbarthumb");
            itsStyleHorizontalScrollbarLeftButton[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("horizontalscrollbarleftbutton");
            itsStyleHorizontalScrollbarRightButton[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("horizontalscrollbarrightbutton");
            itsStyleVerticalScrollbar[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("verticalscrollbar");
            itsStyleVerticalScrollbarThumb[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("verticalscrollbarthumb");
            itsStyleVerticalScrollbarUpButton[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("verticalscrollbarupbutton");
            itsStyleVerticalScrollbarDownButton[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("verticalscrollbardownbutton");
            itsStyleScrollView[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("scrollview");
            itsStyleMinimap[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("minimap");
            itsStyleMinimapButton[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("minimap_button");
            itsStyleToggleStreched[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("toggle_stretched");
            itsStyleToggleCompact[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("toggle_compact");
            itsStyleToggleSuperCompact[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("toggle_supercompact");
            itsStyleToggleRadioStreched[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("toggle_radio_stretched");
            itsStyleToggleRadioCompact[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("toggle_radio_compact");
            itsStyleToggleRadioSuperCompact[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("toggle_radio_supercompact");
            itsStyleToggleSwitch[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("toggle_switch");
            itsStyleToggleBoolean[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("toggle_boolean");
            itsStyleToggleArrow[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("toggle_arrow");
            itsStyleToggleButton[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("toggle_button");
            itsStyleButton[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("Button");
            itsStyleButtonLeft[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("button_left");
            itsStyleButtonRight[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("button_right");
            itsStyleButtonTop[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("button_top");
            itsStyleButtonBottom[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("button_bottom");
            itsStyleButtonMiddle[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("button_middle");
            itsStyleBox[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("Box");
            itsStyleBoxInvisible[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_invisible");
            itsStyleBoxInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_interactive");
            itsStyleBoxLeft[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_left");
            itsStyleBoxLeftInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_left_interactive");
            itsStyleBoxRight[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_right");
            itsStyleBoxRightInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_right_interactive");
            itsStyleBoxMiddleHorizontal[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_middle_horizontal");
            itsStyleBoxMiddleHorizontalInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_middle_horizontal_interactive");
            itsStyleBoxTop[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_top");
            itsStyleBoxTopInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_top_interactive");
            itsStyleBoxBottom[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_bottom");
            itsStyleBoxBottomInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_bottom_interactive");
            itsStyleBoxMiddleVertical[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_middle_vertical");
            itsStyleBoxMiddleVerticalInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_middle_vertical_interactive");
            itsStyleBoxDark[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark");
            itsStyleBoxDarkInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_interactive");
            itsStyleBoxDarkLeft[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_left");
            itsStyleBoxDarkLeftInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_left_interactive");
            itsStyleBoxDarkRight[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_right");
            itsStyleBoxDarkRightInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_right_interactive");
            itsStyleBoxDarkMiddleHorizontal[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_middle_horizontal");
            itsStyleBoxDarkMiddleHorizontalInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_middle_horizontal_interactive");
            itsStyleBoxDarkTop[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_top");
            itsStyleBoxDarkTopInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_top_interactive");
            itsStyleBoxDarkBottom[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_bottom");
            itsStyleBoxDarkBottomInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_bottom_interactive");
            itsStyleBoxDarkMiddleVertical[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_middle_vertical");
            itsStyleBoxDarkMiddleVerticalInteractive[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_dark_middle_vertical_interactive");
            itsStyleBoxDecorated[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("box_decorated");
            itsStyleSeparatorVertical[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("separator_vertical");
            itsStyleSeparatorVerticalFitInBox[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("separator_vertical_fitinbox");
            itsStyleSeparatorHorizontal[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("separator_horizontal");
            itsStyleLabel[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("label");
            itsStyleLabelFitInToBox[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("label_fitintobox");
            itsStyleLabelMultiline[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("label_multiline");
            itsStyleLabelTitle[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("label_title");
            itsStyleCursor[itsSkinIndex] = itsSkin[itsSkinIndex].GetStyle("mouse_cursor");
        }
    }

    public static void BeginWindowHeader(string theTitle, Texture2D theIcon)
    {
        Init();
        BeginHorizontalBox(eStyleBox.eBoxDark);
        Label(string.Empty, theIcon, eStyleLabel.eLabel, GUILayout.Width(GetSkinHeight()));
        Label(theTitle, eStyleLabel.eLabel);
    }

    public static bool EndWindowHeader(bool theCloseButton)
    {
        bool result = false;
        if (theCloseButton)
        {
            Init();
            result = ((itsSkinIndex != -1) ? Button("x", eStyleButton.eButton, GUILayout.Width(GetSkinHeight())) : GUILayout.Button("x", GUILayout.Width(GetSkinHeight())));
        }
        EndHorizontalBox();
        return result;
    }

    public static void RenderDropDownList()
    {
        if (KGFGUIDropDown.itsOpenInstance == null || !KGFGUIDropDown.itsCorrectedOffset)
        {
            return;
        }
        GUI.depth = 0;
        Rect screenRect;
        bool flag;
        if (KGFGUIDropDown.itsOpenInstance.itsDirection == KGFGUIDropDown.eDropDirection.eDown || (KGFGUIDropDown.itsOpenInstance.itsDirection == KGFGUIDropDown.eDropDirection.eAuto && KGFGUIDropDown.itsOpenInstance.itsLastRect.y + GetStyleButton(eStyleButton.eButton).fixedHeight + (float)(double)KGFGUIDropDown.itsOpenInstance.itsHeight < (float)Screen.height))
        {
            screenRect = new Rect(KGFGUIDropDown.itsOpenInstance.itsLastRect.x, KGFGUIDropDown.itsOpenInstance.itsLastRect.y + GetStyleButton(eStyleButton.eButton).fixedHeight, (float)(double)KGFGUIDropDown.itsOpenInstance.itsWidth, (float)(double)KGFGUIDropDown.itsOpenInstance.itsHeight);
            flag = true;
        }
        else
        {
            screenRect = new Rect(KGFGUIDropDown.itsOpenInstance.itsLastRect.x, KGFGUIDropDown.itsOpenInstance.itsLastRect.y - (float)(double)KGFGUIDropDown.itsOpenInstance.itsHeight, (float)(double)KGFGUIDropDown.itsOpenInstance.itsWidth, (float)(double)KGFGUIDropDown.itsOpenInstance.itsHeight);
            flag = false;
        }
        GUILayout.BeginArea(screenRect);
        if (itsSkinIndex == -1)
        {
            KGFGUIDropDown.itsOpenInstance.itsScrollPosition = BeginScrollView(KGFGUIDropDown.itsOpenInstance.itsScrollPosition, false, false, GUILayout.ExpandWidth(expand: true));
        }
        else
        {
            KGFGUIDropDown.itsOpenInstance.itsScrollPosition = GUILayout.BeginScrollView(KGFGUIDropDown.itsOpenInstance.itsScrollPosition, false, false, GUILayout.ExpandWidth(expand: true));
        }
        foreach (string entry in KGFGUIDropDown.itsOpenInstance.GetEntrys())
        {
            if (entry != string.Empty && Button(entry, eStyleButton.eButtonMiddle, GUILayout.ExpandWidth(expand: true)))
            {
                KGFGUIDropDown.itsOpenInstance.SetSelectedItem(entry);
                KGFGUIDropDown.itsOpenInstance = null;
                break;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        if (flag)
        {
            screenRect.y -= GetSkinHeight();
            screenRect.height += GetSkinHeight();
        }
        else
        {
            screenRect.height += GetSkinHeight();
        }
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.y = (float)Screen.height - mousePosition.y;
        if (!screenRect.Contains(mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            KGFGUIDropDown.itsOpenInstance = null;
        }
        if (KGFGUIDropDown.itsOpenInstance != null)
        {
            if (screenRect.Contains(mousePosition))
            {
                KGFGUIDropDown.itsOpenInstance.itsHover = true;
            }
            else
            {
                KGFGUIDropDown.itsOpenInstance.itsHover = false;
            }
        }
    }

    public static void Space()
    {
        GUILayout.Space(GetSkinHeight());
    }

    public static void SpaceSmall()
    {
        GUILayout.Space(GetSkinHeight() / 2f);
    }

    public static void Label(string theText, params GUILayoutOption[] theLayout)
    {
        Label(theText, eStyleLabel.eLabel, theLayout);
    }

    public static void Label(string theText, eStyleLabel theStyleLabel, params GUILayoutOption[] theLayout)
    {
        Label(theText, null, theStyleLabel, theLayout);
    }

    public static void Label(string theText, Texture2D theImage, eStyleLabel theStyleLabel, params GUILayoutOption[] theLayout)
    {
        Init();
        GUIContent gUIContent = null;
        gUIContent = ((!(theImage != null)) ? new GUIContent(theText) : new GUIContent(theText, theImage));
        if (itsSkinIndex == -1)
        {
            GUILayout.Label(gUIContent, theLayout);
        }
        else
        {
            GUILayout.Label(gUIContent, GetStyleLabel(theStyleLabel), theLayout);
        }
    }

    public static void Separator(eStyleSeparator theStyleSeparator, params GUILayoutOption[] theLayout)
    {
        Init();
        if (itsSkinIndex == -1)
        {
            GUILayout.Label("|", theLayout);
        }
        else
        {
            GUILayout.Label(string.Empty, GetStyleSeparator(theStyleSeparator), theLayout);
        }
    }

    public static bool Toggle(bool theValue, string theText, eStyleToggl theToggleStyle, params GUILayoutOption[] theLayout)
    {
        Init();
        bool flag = false;
        flag = ((itsSkinIndex != -1) ? GUILayout.Toggle(theValue, theText, GetStyleToggl(theToggleStyle), theLayout) : GUILayout.Toggle(theValue, theText, theLayout));
        if (flag != theValue)
        {
            PlaySound(theToggleStyle.ToString());
        }
        return flag;
    }

    public static bool Toggle(bool theValue, Texture2D theImage, eStyleToggl theToggleStyle, params GUILayoutOption[] theLayout)
    {
        Init();
        bool flag = false;
        flag = ((itsSkinIndex != -1) ? GUILayout.Toggle(theValue, theImage, GetStyleToggl(theToggleStyle), theLayout) : GUILayout.Toggle(theValue, theImage, theLayout));
        if (flag != theValue)
        {
            PlaySound(theToggleStyle.ToString());
        }
        return flag;
    }

    public static bool Toggle(bool theValue, string theText, Texture2D theImage, eStyleToggl theToggleStyle, params GUILayoutOption[] theLayout)
    {
        Init();
        GUIContent gUIContent = null;
        gUIContent = ((!(theImage != null)) ? new GUIContent(theText) : new GUIContent(theText, theImage));
        bool flag = false;
        flag = ((itsSkinIndex != -1) ? GUILayout.Toggle(theValue, gUIContent, GetStyleToggl(theToggleStyle), theLayout) : GUILayout.Toggle(theValue, gUIContent, theLayout));
        if (flag != theValue)
        {
            PlaySound(theToggleStyle.ToString());
        }
        return flag;
    }

    public static Rect Window(int theId, Rect theRect, GUI.WindowFunction theFunction, string theText, params GUILayoutOption[] theLayout)
    {
        return Window(theId, theRect, theFunction, null, theText, theLayout);
    }

    public static Rect Window(int theId, Rect theRect, GUI.WindowFunction theFunction, Texture theImage, params GUILayoutOption[] theLayout)
    {
        return Window(theId, theRect, theFunction, theImage, string.Empty, theLayout);
    }

    public static Rect Window(int theId, Rect theRect, GUI.WindowFunction theFunction, Texture theImage, string theText, params GUILayoutOption[] theLayout)
    {
        Init();
        GUIContent gUIContent = null;
        gUIContent = ((!(theImage != null)) ? new GUIContent(theText) : new GUIContent(theText, theImage));
        if (itsSkinIndex != -1)
        {
            if (itsStyleWindow[itsSkinIndex] != null)
            {
                return GUILayout.Window(theId, theRect, theFunction, gUIContent, itsStyleWindow[itsSkinIndex], theLayout);
            }
            return GUILayout.Window(theId, theRect, theFunction, gUIContent, theLayout);
        }
        return GUILayout.Window(theId, theRect, theFunction, gUIContent, theLayout);
    }

    public static void Box(string theText, eStyleBox theStyleBox, params GUILayoutOption[] theLayout)
    {
        Box(null, theText, theStyleBox, theLayout);
    }

    public static void Box(Texture theImage, eStyleBox theStyleBox, params GUILayoutOption[] theLayout)
    {
        Box(theImage, string.Empty, theStyleBox, theLayout);
    }

    public static void Box(Texture theImage, string theText, eStyleBox theStyleBox, params GUILayoutOption[] theLayout)
    {
        Init();
        GUIContent gUIContent = null;
        gUIContent = ((!(theImage != null)) ? new GUIContent(theText) : new GUIContent(theText, theImage));
        if (itsSkinIndex == -1)
        {
            GUILayout.Box(gUIContent, theLayout);
        }
        else
        {
            GUILayout.Box(gUIContent, GetStyleBox(theStyleBox), theLayout);
        }
    }

    public static void BeginVerticalBox(eStyleBox theStyleBox, params GUILayoutOption[] theLayout)
    {
        Init();
        if (itsSkinIndex == -1)
        {
            GUILayout.BeginVertical(GUI.skin.box, theLayout);
        }
        else
        {
            GUILayout.BeginVertical(GetStyleBox(theStyleBox), theLayout);
        }
    }

    public static void EndVerticalBox()
    {
        GUILayout.EndVertical();
    }

    public static void BeginVerticalPadding()
    {
        GUILayout.BeginVertical();
        BeginHorizontalBox(eStyleBox.eBoxInvisible);
    }

    public static void EndVerticalPadding()
    {
        EndHorizontalBox();
        GUILayout.EndVertical();
    }

    public static void BeginHorizontalPadding()
    {
        GUILayout.BeginHorizontal();
        BeginVerticalBox(eStyleBox.eBoxInvisible);
    }

    public static void EndHorizontalPadding()
    {
        EndVerticalBox();
        GUILayout.EndHorizontal();
    }

    public static void BeginHorizontalBox(eStyleBox theStyleBox, params GUILayoutOption[] theLayout)
    {
        Init();
        if (itsSkinIndex == -1)
        {
            GUILayout.BeginHorizontal(GUI.skin.box, theLayout);
        }
        else
        {
            GUILayout.BeginHorizontal(GetStyleBox(theStyleBox), theLayout);
        }
    }

    public static void EndHorizontalBox()
    {
        GUILayout.EndHorizontal();
    }

    public static Vector2 BeginScrollView(Vector2 thePosition, bool theHorizontalAlwaysVisible, bool theVerticalAlwaysVisible, params GUILayoutOption[] theLayout)
    {
        Init();
        if (itsSkinIndex != -1)
        {
            GUI.skin = itsSkin[itsSkinIndex];
        }
        if (itsStyleHorizontalScrollbar != null && itsStyleVerticalScrollbar != null && itsSkinIndex != -1)
        {
            return GUILayout.BeginScrollView(thePosition, theHorizontalAlwaysVisible, theVerticalAlwaysVisible, itsStyleHorizontalScrollbar[itsSkinIndex], itsStyleVerticalScrollbar[itsSkinIndex], theLayout);
        }
        return GUILayout.BeginScrollView(thePosition, theHorizontalAlwaysVisible, theVerticalAlwaysVisible, theLayout);
    }

    public static void EndScrollView()
    {
        GUILayout.EndScrollView();
    }

    public static string TextField(string theText, eStyleTextField theStyleTextField, params GUILayoutOption[] theLayout)
    {
        Init();
        if (itsSkinIndex == -1)
        {
            return GUILayout.TextField(theText, theLayout);
        }
        return GUILayout.TextField(theText, GetStyleTextField(theStyleTextField), theLayout);
    }

    public static string TextArea(string theText, params GUILayoutOption[] theLayout)
    {
        Init();
        if (itsStyleTextArea[itsSkinIndex] != null && itsSkinIndex != -1)
        {
            return GUILayout.TextArea(theText, itsStyleTextArea[itsSkinIndex], theLayout);
        }
        return GUILayout.TextArea(theText, theLayout);
    }

    public static bool Button(string theText, eStyleButton theButtonStyle, params GUILayoutOption[] theLayout)
    {
        return Button(null, theText, theButtonStyle, theLayout);
    }

    public static bool Button(Texture theImage, eStyleButton theButtonStyle, params GUILayoutOption[] theLayout)
    {
        return Button(theImage, string.Empty, theButtonStyle, theLayout);
    }

    public static bool Button(Texture theImage, string theText, eStyleButton theButtonStyle, params GUILayoutOption[] theLayout)
    {
        GUIContent gUIContent = null;
        gUIContent = ((!(theImage != null)) ? new GUIContent(theText) : new GUIContent(theText, theImage));
        Init();
        if (itsSkinIndex == -1)
        {
            if (GUILayout.Button(gUIContent, theLayout))
            {
                PlaySound(theButtonStyle.ToString());
                return true;
            }
        }
        else if (GUILayout.Button(gUIContent, GetStyleButton(theButtonStyle), theLayout))
        {
            PlaySound(theButtonStyle.ToString());
            return true;
        }
        return false;
    }

    public static eCursorState Cursor()
    {
        return Cursor(null, null, null, null, null);
    }

    public static eCursorState Cursor(Texture theUp, Texture theRight, Texture theDown, Texture theLeft, Texture theCenter)
    {
        float skinHeight = GetSkinHeight();
        float num = skinHeight * 3f;
        eCursorState result = eCursorState.eNone;
        GUILayout.BeginVertical(GUILayout.ExpandWidth(expand: false), GUILayout.ExpandHeight(expand: false));
        BeginHorizontalBox(eStyleBox.eBoxInvisible);
        GUILayout.BeginVertical(GUILayout.Width(num), GUILayout.Height(num));
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(expand: false), GUILayout.ExpandHeight(expand: false));
        GUILayout.Space(skinHeight);
        if (theUp != null)
        {
            if (Button(theUp, eStyleButton.eButtonTop, GUILayout.Width(skinHeight)))
            {
                result = eCursorState.eUp;
            }
        }
        else if (Button(string.Empty, eStyleButton.eButtonTop, GUILayout.Width(skinHeight)))
        {
            result = eCursorState.eUp;
        }
        GUILayout.Space(skinHeight);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(expand: false), GUILayout.ExpandHeight(expand: false));
        if (theLeft != null)
        {
            if (Button(theLeft, eStyleButton.eButtonLeft, GUILayout.Width(skinHeight)))
            {
                result = eCursorState.eLeft;
            }
        }
        else if (Button(string.Empty, eStyleButton.eButtonLeft, GUILayout.Width(skinHeight)))
        {
            result = eCursorState.eLeft;
        }
        if (theCenter != null)
        {
            if (Button(theCenter, eStyleButton.eButtonLeft, GUILayout.Width(skinHeight)))
            {
                result = eCursorState.eCenter;
            }
        }
        else if (Button(string.Empty, eStyleButton.eButtonMiddle, GUILayout.Width(skinHeight)))
        {
            result = eCursorState.eCenter;
        }
        if (theRight != null)
        {
            if (Button(theRight, eStyleButton.eButtonLeft, GUILayout.Width(skinHeight)))
            {
                result = eCursorState.eRight;
            }
        }
        else if (Button(string.Empty, eStyleButton.eButtonRight, GUILayout.Width(skinHeight)))
        {
            result = eCursorState.eRight;
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(expand: false), GUILayout.ExpandHeight(expand: false));
        GUILayout.Space(skinHeight);
        if (theDown != null)
        {
            if (Button(theDown, eStyleButton.eButtonLeft, GUILayout.Width(skinHeight)))
            {
                result = eCursorState.eDown;
            }
        }
        else if (Button(string.Empty, eStyleButton.eButtonBottom, GUILayout.Width(skinHeight)))
        {
            result = eCursorState.eDown;
        }
        GUILayout.Space(skinHeight);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        EndHorizontalBox();
        GUILayout.EndVertical();
        return result;
    }

    public static float HorizontalSlider(float theValue, float theLeftValue, float theRightValue, params GUILayoutOption[] theLayout)
    {
        Init();
        if (itsStyleHorizontalSlider != null && itsStyleHorizontalSliderThumb != null && itsSkinIndex != -1)
        {
            return GUILayout.HorizontalSlider(theValue, theLeftValue, theRightValue, itsStyleHorizontalSlider[itsSkinIndex], itsStyleHorizontalSliderThumb[itsSkinIndex], theLayout);
        }
        return GUILayout.HorizontalSlider(theValue, theLeftValue, theRightValue, theLayout);
    }

    public static float VerticalSlider(float theValue, float theLeftValue, float theRightValue, params GUILayoutOption[] theLayout)
    {
        Init();
        if (itsStyleVerticalSlider != null && itsStyleVerticalSliderThumb != null && itsSkinIndex != -1)
        {
            return GUILayout.VerticalSlider(theValue, theLeftValue, theRightValue, itsStyleVerticalSlider[itsSkinIndex], itsStyleVerticalSliderThumb[itsSkinIndex], theLayout);
        }
        return GUILayout.VerticalSlider(theValue, theLeftValue, theRightValue, theLayout);
    }
}
