using RAIN.Core;
using System.Collections.Generic;

public class AICastSpell : AIConsSkillSpell
{
    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AICastSpell";
    }

    protected override List<UnitActionId> GetRelatedActions()
    {
        return AIController.spellActions;
    }
}
