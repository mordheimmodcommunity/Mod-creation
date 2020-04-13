using System;
using System.Collections.Generic;

[Serializable]
public class UnitInitData
{
    public string name;

    public UnitId id;

    public UnitRankId rank;

    public List<WeaponInitData> weapons;

    public List<SkillId> skills;

    public List<SkillId> spells;

    public List<MutationId> mutations;

    public List<InjuryId> injuries;
}
