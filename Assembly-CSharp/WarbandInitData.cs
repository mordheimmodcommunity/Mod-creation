using System;
using System.Collections.Generic;

[Serializable]
public class WarbandInitData
{
	public string name;

	public WarbandId id;

	public int team;

	public int rank;

	public PlayerTypeId playerId;

	public DeploymentScenarioSlotId deployId;

	public PrimaryObjectiveTypeId objectiveTypeId;

	public int objectiveTargetIdx;

	public List<UnitInitData> units;
}
