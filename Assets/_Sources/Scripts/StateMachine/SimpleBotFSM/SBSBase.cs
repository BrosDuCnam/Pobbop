using UnityEngine;

public class SBSBase : FSMState<SBStateInfo>
{
    public override void doState(ref SBStateInfo infos)
    {
        addAndActivateSubState<SBNavigate>();
        /*
        if (infos.bot.IsHoldingObject)
        {
            //TODO: Chasing state
        }
        else
        {
            //TODO: Searching state
        }*/
    }
}