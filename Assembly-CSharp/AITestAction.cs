using RAIN.Core;
using RAIN.Memory;
using RAIN.Representation;
using UnityEngine;

public class AITestAction : AIBase
{
    public Expression expr;

    public override void Start(AI ai)
    {
        base.Start(ai);
        base.actionName = "AITestAction";
        Debug.Log("valid = " + expr.get_IsValid());
        int num = expr.Evaluate<int>(Time.deltaTime, (RAINMemory)null);
        Debug.Log("evalutat = " + num);
        Debug.Log("to string = " + expr.ToString());
    }
}
