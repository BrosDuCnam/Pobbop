using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(BotController))]
public class BotPlayer : BasePlayer
{
    [SerializeField] public SBStateInfo fsmStateInfo = new SBStateInfo();
    private FSMachine<SBSBase, SBStateInfo> _fsm = new FSMachine<SBSBase, SBStateInfo>();

    public bool SeeingDangerousEnemy
    {
        get => SeeEnemyWithWeapon();
    }
    
    private new void Awake()
    {
        base.Awake();

        fsmStateInfo.bot = this;
        fsmStateInfo.PeriodUpdate = 0.1f;
    }

    private void Update()
    {
        _fsm.Update(fsmStateInfo);
    }

    public void Shoot(float chargeTime)
    {
        throwSystem.ChargeThrow();
        StartCoroutine(Utils.TimedAction(chargeTime, b =>
        {
            if (!b) throwSystem.ReleaseThrow();
        }));
    }
    
    private bool SeeEnemyWithWeapon()
    {
        return targetSystem.GetVisibleTargets(targetSystem.Targets) // Get visible targets
            .Any(x => x.GetComponent<PickUpDropSystem>() && // If the enemy has PickUpDropSystem
                      x.GetComponent<PickUpDropSystem>().PickableObject != null && // If the enemy has PickUpDropSystem with a PickableObject
                      x.GetComponent<ThrowSystem>()); // If the enemy can throw his PickableObject
    }
}