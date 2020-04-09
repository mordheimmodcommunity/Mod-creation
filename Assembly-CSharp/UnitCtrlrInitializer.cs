using UnityEngine;

public class UnitCtrlrInitializer : MonoBehaviour
{
	public UnitId id;

	public ItemId itemId;

	public ItemId offItemId;

	public ItemId secItemId;

	public ItemId secOffItemId;

	public ItemId armordId;

	public SkillId areaSpellId;

	public SkillId pointSpellId;

	public SkillId activeSkillId;

	private void Start()
	{
		UnitSave unitSave = new UnitSave(id);
		unitSave.items[2] = new ItemSave(itemId);
		if (offItemId != 0)
		{
			unitSave.items[3] = new ItemSave(offItemId);
		}
		unitSave.items[4] = new ItemSave(secItemId);
		if (secOffItemId != 0)
		{
			unitSave.items[5] = new ItemSave(secOffItemId);
		}
		unitSave.items[1] = new ItemSave(armordId);
		unitSave.spells.Add(areaSpellId);
		unitSave.spells.Add(pointSpellId);
		unitSave.activeSkills.Add(activeSkillId);
		UnitMenuController component = GetComponent<UnitMenuController>();
		if (component != null)
		{
			component.SetCharacter(new Unit(unitSave));
			component.LoadBodyParts();
		}
		UnitController component2 = GetComponent<UnitController>();
		if (component2 != null)
		{
			component2.FirstSyncInit(unitSave, 0u, 0, 0, PlayerTypeId.PLAYER, 0, merge: true);
			GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
		}
		Animator component3 = GetComponent<Animator>();
		component3.stabilizeFeet = true;
	}
}
