using System.Linq;
using UnityEngine;

public class SBSShoot : FSMState<SBStateInfo>
{
    public override void doState(ref SBStateInfo infos)
    {
        float direction = Random.Range(0, 1) > 0.5f ? 1 : -1;
        float value = Random.Range(10, 90);

        float chargeTime = Random.Range(0.2f, 1.5f);
        infos.bot.Shoot(chargeTime);

        Vector2 lookDestination = infos.controller.currentLook;
        lookDestination.x += value * direction;
        infos.controller.LookAt(lookDestination);
    }
}