using UnityEngine;

public class ObjectiveActivate : Objective
{
    private PrimaryObjectiveActivateData activateData;

    public ObjectiveActivate(PrimaryObjectiveId id)
        : base(id)
    {
        DataFactory instance = PandoraSingleton<DataFactory>.Instance;
        int num = (int)id;
        activateData = instance.InitData<PrimaryObjectiveActivateData>("fk_primary_objective_id", num.ToString())[0];
        int count = PandoraSingleton<MissionManager>.Instance.GetActivatePoints(activateData.PropsName).Count;
        counter = new Vector2(0f, activateData.PropsCount);
        desc = PandoraSingleton<LocalizationManager>.Instance.GetStringById(base.DescKey, PandoraSingleton<LocalizationManager>.Instance.GetStringById(activateData.PropsName));
    }

    protected override void Track(ref bool objectivesChanged)
    {
    }

    public void UpdateActivatedItem(string name)
    {
        if (name == activateData.PropsName)
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
