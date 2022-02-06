using System;
using UnityEngine;

[RequireComponent(typeof(BotController))]
public class BotPlayer : BasePlayer
{
    public BotController controller;
    private SBStateInfo _fsmStateInfo = new SBStateInfo();
    private FSMachine<SBSBase, SBStateInfo> _fsm = new FSMachine<SBSBase, SBStateInfo>();
    


    private new void Awake()
    {
        controller = GetComponent<BotController>();
        _fsmStateInfo.bot = this;
        _fsmStateInfo.PeriodUpdate = 0.1f;
    }

    private void Update()
    {
        _fsm.Update(_fsmStateInfo);
    }
}