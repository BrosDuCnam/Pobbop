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
        
        if (infos.controller.hasDestination)
        {
            infos.controller.Run();

            Vector3 forwardPosition = infos.bot.transform.position + (infos.bot.transform.forward * 2);
            
            bool toCrouch = false;/*
            RaycastHit forwardHit = new RaycastHit();
            Debug.DrawRay(forwardPosition, Vector3.down);
            if (Physics.Raycast(forwardPosition, Vector3.down, out forwardHit))
            {
                Vector3 angle = Quaternion.FromToRotation(forwardHit.normal, infos.bot.transform.forward).eulerAngles;
                Debug.Log(angle);
                if (angle.x < 90 || angle.z < 90) //TODO maybe change 0 by another value
                {
                    RaycastHit[] hits = Physics.RaycastAll(infos.bot.transform.position, Vector3.down);
                    hits = hits.Where(h => h.transform.gameObject == forwardHit.transform.gameObject).ToArray();

                    if (hits.Length > 0)
                    {
                        angle = Quaternion.FromToRotation(hits[0].normal, infos.bot.transform.forward).eulerAngles;
                        if (angle.x < 90 || angle.z < 90) //TODO maybe change 0 by another value
                        {
                            toCrouch = true;
                        }
                    }
                }
            }*/
            infos.controller.Crouch(toCrouch);
        }
        
        if (infos.bot.IsHoldingObject)
        {
            addAndActivateSubState<SBSHasBall>();
        }
        else if (infos.ballHistory.Count > 0)
        {
            addAndActivateSubState<SBSBallChasing>();
            Debug.Log("Ball chasing");
        }
        else
        {
            addAndActivateSubState<SBSNavigate>();
            Debug.Log("Random navigation");
        }
    }
}