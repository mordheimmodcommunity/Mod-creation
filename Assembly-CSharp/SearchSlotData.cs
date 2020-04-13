using System;

[Serializable]
public class SearchSlotData
{
    public ItemId itemId;

    public ItemQualityId itemQualityId = ItemQualityId.NORMAL;

    public RuneMarkId runeMarkId;

    public AllegianceId allegianceId;

    public RuneMarkQualityId runeMarkQualityId = RuneMarkQualityId.REGULAR;

    public ItemId restrictedItemId;

    public ItemTypeId restrictedItemTypeId;

    public int value;
}
