using System.Linq;
using UnityEngine;

public class SBSHasBall : FSMState<SBStateInfo>
{
    public override void doState(ref SBStateInfo infos)
    {
        if (infos.bot.HasTarget && !infos.bot.throwSystem.IsCharging)
        {
            addAndActivateSubState<SBSShoot>();
        }
        else if (infos.playerHistory.Count > 0)
        {
            addAndActivateSubState<SBSPlayerChasing>();
        }
        else
        {
            addAndActivateSubState<SBSNavigate>();
        }
    }
}