using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SBSBase : FSMState<SBStateInfo>
{
    public override void doState(ref SBStateInfo infos)
    {
        //Save all position of visible player
        List<BasePlayer> visiblePlayer = Utils.GetAllVisibleObject(infos.playerList.Select(x => x.gameObject).ToList(),
            infos.bot.controller.camera, hit => true).Select(x => x.GetComponent<BasePlayer>()).ToList();

        foreach (BasePlayer player in visiblePlayer)
        {
            if (infos.playerHistory.ContainsKey(player)) infos.playerHistory[player] = player.transform.position;
            else infos.playerHistory.Add(player, player.transform.position);
        }
        
        //Save all position of visible ball
        List<ThrowableObject> visibleBall = Utils.GetAllVisibleObject(infos.ballList.Select(x => x.gameObject).ToList(),
            infos.bot.controller.camera, hit => true).Select(x => x.GetComponent<ThrowableObject>()).ToList();

        foreach (ThrowableObject ball in visibleBall)
        {
            if (infos.ballHistory.ContainsKey(ball)) infos.ballHistory[ball] = ball.transform.position;
            else infos.ballHistory.Add(ball, ball.transform.position);
        }
        
        
        if (infos.bot.IsHoldingObject)
        {
            //TODO: Chasing state
        }
        else if (infos.ballHistory.Count > 0)
        {
            addAndActivateSubState<SBSBallChasing>();
        }
        else
        {
            addAndActivateSubState<SBSNavigate>();
        }
    }
}