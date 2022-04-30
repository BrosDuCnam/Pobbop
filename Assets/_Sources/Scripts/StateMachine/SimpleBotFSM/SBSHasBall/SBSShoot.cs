using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SBSShoot : FSMState<SBStateInfo>
{
    public override void doState(ref SBStateInfo infos)
    {
        if (infos.bot._throw.IsCharging) return;

        float direction = Random.Range(0, 1) > 0.5f ? 1 : -1;
        float value = Random.Range(10, 180);

        float chargeTime = Random.Range(1f, 5f);
        infos.bot._throw.ChargeThrow();

        Vector2 lookDestination = infos.controller.currentLook;
        lookDestination.x += value * direction;

        Player bot = infos.bot;
        infos.controller.TimedLookAt(lookDestination, chargeTime, () => bot._throw.ReleaseThrow() );
        
        infos.controller.GoTo(infos.bot.Target, 1);
    }
}