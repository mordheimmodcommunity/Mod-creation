using UnityEngine;

public class ObjectiveConvert : Objective
{
	private PrimaryObjectiveConvertData convertData;

	public ObjectiveConvert(PrimaryObjectiveId id)
		: base(id)
	{
		DataFactory instance = PandoraSingleton<DataFactory>.Instance;
		int num = (int)id;
		convertData = instance.InitData<PrimaryObjectiveConvertData>("fk_primary_objective_id", num.ToString())[0];
		counter = new Vector2(0f, convertData.ItemCount);
		desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById(base.DescKey, convertData.ItemId.ToLowerString());
	}

	protected override void Track(ref bool objectivesChanged)
	{
	}

	public void UpdateConvertedItems(ItemId id, int count)
	{
		if (id == convertData.ItemId)
		{
			counter.x += count;
			for (int i = 0; i < count; i++)
			{
				PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateObjective(guid, 0u);
			}
		}
	}

	public override void Reload(uint trackedUid)
	{
		counter.x += 1f;
	}
}
