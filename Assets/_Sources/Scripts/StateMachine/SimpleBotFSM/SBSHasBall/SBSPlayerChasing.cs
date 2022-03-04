using System.Linq;
using UnityEngine;

public class SBSPlayerChasing : FSMState<SBStateInfo>
{
    public override void doState(ref SBStateInfo infos)
    {
        Vector3 position = infos.bot.transform.position;
        //Sort players by distance
        var players = infos.playerHistory.OrderBy(entry => Utils.NavMeshDistance(position, entry.Value)).ToList();

        while (players.Count > 0 && Vector3.Distance(position, players[0].Value) < 5)
        {
            infos.playerHistory.Remove(players[0].Key);
            players.RemoveAt(0);
        }

        if (players.Count > 0 && infos.controller.destination != players[0].Value)
        {
            infos.controller.GoTo(players[0].Value);
        }
    }
}