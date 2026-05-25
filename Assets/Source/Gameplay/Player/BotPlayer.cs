using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BotPlayer : Player
{
    // Tracks all relevant info this CPU would need to make decisions.
    // public CPUInfoTracker infoTracker;

    public static event Action<int, int> cpuUpdateProgress = (_, __) => { };

    public BotPlayer(int id, string name)
    {
        this.id = id;
        agentName = name;

        rosterConstraints = new RosterConstraints();
        rosterConstraints.clearAllConstraints(true);

        Roster.clearAllConstraints += clearConstraints;
    }

    ~BotPlayer()
    {
        Roster.clearAllConstraints -= clearConstraints;
    }

    // Initial actions before the player's turn.
    public override void markAsReady()
    {

    }

    public override void guessTarget(int characterId)
    {
        bool correct = TurnDriver.instance.currentRoster.targetId == characterId;

        // TODO obv. gotta do more than just click/respond
        if (correct)
        {
            Debug.Log("CPU WINS!");
            // TODO
        }
        else
        {
            Debug.Log("Wrong guy!");
            endOfTurn();
        }
    }

    public override void endOfTurn()
    {
        Debug.Log("The CPU " + agentName + "'s turn has ended.");
    }





    // CPU-specific methods

    public void skipTurn()
    {
        Debug.Log("Skipping CPU " + agentName + "'s turn.");
        endOfTurn();
    }
}