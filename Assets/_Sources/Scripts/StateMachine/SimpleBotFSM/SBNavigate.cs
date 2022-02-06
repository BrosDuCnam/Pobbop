using UnityEngine;

public class SBNavigate : FSMState<SBStateInfo>
{
    public override void doState(ref SBStateInfo infos)
    {

        BotController controller = (BotController) infos.bot._controller;
        if (!controller.HasDestination)
        {
            controller.SetDestination(Utils.RandomNavmeshLocation(50, infos.bot.transform.position));
        }
    }
}