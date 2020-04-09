using UnityEngine;

public class ObjectiveDestroy : Objective
{
	private PrimaryObjectiveDestroyData destroyData;

	public ObjectiveDestroy(PrimaryObjectiveId id)
		: base(id)
	{
		DataFactory instance = PandoraSingleton<DataFactory>.Instance;
		int num = (int)id;
		destroyData = instance.InitData<PrimaryObjectiveDestroyData>("fk_primary_objective_id", num.ToString())[0];
		int count = PandoraSingleton<MissionManager>.Instance.GetDestructibles(destroyData.PropsName).Count;
		counter = new Vector2(0f, destroyData.PropsCount);
		desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById(base.DescKey, PandoraSingleton<LocalizationManager>.Instance.GetStringById(destroyData.PropsName));
	}

	protected override void Track(ref bool objectivesChanged)
	{
	}

	public void UpdateDestroyedItem(string name)
	{
		if (name == destroyData.PropsName)
		{
			counter.x += 1f;
			PandoraSingleton<MissionManager>.Instance.MissionEndData.UpdateObjective(guid, 0u);
		}
	}

	public override void Reload(uint trackedUid)
	{
		counter.x += 1f;
	}
}
