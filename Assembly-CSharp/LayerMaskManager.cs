using UnityEngine;

public class LayerMaskManager
{
	public static LayerMask rangeTargetMask = (1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("characters")) | (1 << LayerMask.NameToLayer("props_big")) | (1 << LayerMask.NameToLayer("target")) | (1 << LayerMask.NameToLayer("ground"));

	public static LayerMask rangeTargetMaskNoChar = (1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("props_big")) | (1 << LayerMask.NameToLayer("target")) | (1 << LayerMask.NameToLayer("ground"));

	public static LayerMask fowMask = (1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("props_big")) | (1 << LayerMask.NameToLayer("ground"));

	public static LayerMask footMask = (1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("props_big")) | (1 << LayerMask.NameToLayer("ground"));

	public static LayerMask chargeMask = (1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("characters")) | (1 << LayerMask.NameToLayer("engage_circles")) | (1 << LayerMask.NameToLayer("ground")) | (1 << LayerMask.NameToLayer("props_small")) | (1 << LayerMask.NameToLayer("props_big")) | (1 << LayerMask.NameToLayer("collision_wall"));

	public static LayerMask pathMask = (1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("characters")) | (1 << LayerMask.NameToLayer("engage_circles")) | (1 << LayerMask.NameToLayer("ground")) | (1 << LayerMask.NameToLayer("props_small")) | (1 << LayerMask.NameToLayer("props_big")) | (1 << LayerMask.NameToLayer("collision_wall"));

	public static LayerMask decisionMask = (1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("ground")) | (1 << LayerMask.NameToLayer("props_small")) | (1 << LayerMask.NameToLayer("props_big")) | (1 << LayerMask.NameToLayer("collision_wall"));

	public static LayerMask groundMask = (1 << LayerMask.NameToLayer("ground")) | (1 << LayerMask.NameToLayer("environment")) | (1 << LayerMask.NameToLayer("props_big")) | (1 << LayerMask.NameToLayer("props_small"));

	public static LayerMask groundOnlyMask = 1 << LayerMask.NameToLayer("ground");

	public static LayerMask overviewMask = (1 << LayerMask.NameToLayer("characters")) | (1 << LayerMask.NameToLayer("props_big")) | (1 << LayerMask.NameToLayer("props_small")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("mapsystem"));

	public static LayerMask menuNodeMask = (1 << LayerMask.NameToLayer("characters")) | (1 << LayerMask.NameToLayer("props_big")) | (1 << LayerMask.NameToLayer("props_small"));

	public static int charactersLayer = LayerMask.NameToLayer("characters");

	public static int engage_circlesLayer = LayerMask.NameToLayer("engage_circles");
}
