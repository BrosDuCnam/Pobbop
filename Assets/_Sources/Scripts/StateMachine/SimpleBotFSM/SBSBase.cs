using UnityEngine;

public class SBSBase : FSMState<SBStateInfo>
{
    public override void doState(ref SBStateInfo infos)
    {
        if (infos.bot.IsHoldingObject)
        {
            addAndActivateSubState<SBHasBall>();
        }
    }
}