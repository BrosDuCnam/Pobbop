using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SBSShoot : FSMState<SBStateInfo>
{
    public override void doState(ref SBStateInfo infos)
    {
        if (infos.bot.throwSystem.IsCharging) return;
        
        infos.controller.StopLocomotion.Invoke();
        
        float direction = Random.Range(0, 1) > 0.5f ? 1 : -1;
        float value = Random.Range(10, 180);

        float chargeTime = Random.Range(0.5f, 2f);
        infos.bot.throwSystem.ChargeThrow();

        Vector2 lookDestination = infos.controller.currentLook;
        lookDestination.x += value * direction;

        BasePlayer bot = infos.bot;
        infos.controller.TimedLookAt(lookDestination, chargeTime, () => bot.throwSystem.ReleaseThrow() );
    }
}