using UnityEngine;

public class MenuConfig
{
	public enum ReasonType
	{
		AVAILABLE,
		NO_MONEY,
		NO_HERO_SLOT,
		NO_ABILITY,
		NO_SKILL_PTS,
		NO_LEADER,
		MAX_OUT,
		IN_TRAINING,
		INJURED,
		UPKEEP_NOT_PAID,
		PREREQUISITE,
		WRONG_ALLEGIANCE,
		EMPTY,
		EMPTY_LIST,
		CANNOT_BE_ENCHANTED,
		NOT_ENOUGH_UNITS,
		SOME_UPKEEP,
		CAMPAIGN
	}

	public enum OptionsGraphics
	{
		GENERAL_QUALITY,
		SHADOW_QUALITY,
		TEXTURE_QUALITY,
		ANISOTROPIC,
		ANTI_ALIASING,
		RESOLUTION,
		FULL_SCREEN,
		VERTICAL_SYNC,
		REVERT
	}

	public static Color32 lightGray = new Color32(168, 168, 168, byte.MaxValue);

	public static Color32 darkGray = new Color32(71, 71, 71, byte.MaxValue);

	public static Color32 wyrdstoneGreen = new Color32(74, 137, 96, byte.MaxValue);

	public static Color32 goldenYellow = new Color32(byte.MaxValue, 204, 94, byte.MaxValue);

	public static Color32 disabledRed = new Color32(byte.MaxValue, 48, 68, byte.MaxValue);

	public static Color32 enchantRegular = new Color32(19, 115, 240, byte.MaxValue);

	public static Color32 enchantMaster = new Color32(160, 84, 182, byte.MaxValue);

	public static Color32 colorUnselected = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 230);

	public static float alphaUnavailable = 0.2f;

	public static OptionsGraphics[][] menuOptionsGfx = new OptionsGraphics[5][]
	{
		new OptionsGraphics[2]
		{
			OptionsGraphics.GENERAL_QUALITY,
			OptionsGraphics.RESOLUTION
		},
		new OptionsGraphics[2]
		{
			OptionsGraphics.SHADOW_QUALITY,
			OptionsGraphics.FULL_SCREEN
		},
		new OptionsGraphics[2]
		{
			OptionsGraphics.TEXTURE_QUALITY,
			OptionsGraphics.VERTICAL_SYNC
		},
		new OptionsGraphics[1]
		{
			OptionsGraphics.ANISOTROPIC
		},
		new OptionsGraphics[1]
		{
			OptionsGraphics.ANTI_ALIASING
		}
	};
}
