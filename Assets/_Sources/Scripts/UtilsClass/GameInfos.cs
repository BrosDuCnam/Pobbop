using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameInfos
{
    public enum GameModes
    {
        OneVOne,
        TwoVTwo,
        ThreeVThree,
        FFA
    }

    public static Dictionary<GameModes, int> GameModesPlayers = new Dictionary<GameModes, int>()
    {
        {GameModes.OneVOne, 2},
        {GameModes.TwoVTwo, 4},
        {GameModes.ThreeVThree, 6},
        {GameModes.FFA, 5}
    };
}
