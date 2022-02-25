using UnityEngine;

public class SBSNavigate : FSMState<SBStateInfo>
{
    public override void doState(ref SBStateInfo infos)
    {
        if (!infos.controller.hasDestination)
        {
            infos.controller.GoTo(Utils.RandomNavmeshLocation(20, infos.bot.transform.position));
        }
    }
}