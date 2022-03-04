using System.Linq;
using UnityEngine;

public class SBSBallChasing : FSMState<SBStateInfo>
{
    public override void doState(ref SBStateInfo infos)
    {
        Vector3 position = infos.bot.transform.position;
        //Sort balls by distance
        var balls = infos.ballHistory.OrderBy(entry => Utils.NavMeshDistance(position, entry.Value)).ToList();

        while (balls.Count > 0 && Vector3.Distance(position, balls[0].Value) < 5)
        {
            infos.ballHistory.Remove(balls[0].Key);
            balls.RemoveAt(0);
        }

        if (balls.Count > 0 && infos.controller.destination != balls[0].Value)
        {
            infos.controller.GoTo(balls[0].Value);
        }
    }
}