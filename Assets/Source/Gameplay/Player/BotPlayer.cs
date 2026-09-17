using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BotPlayer : GD_Player
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

    public override bool addToInvestigation((CPD_Type cpdType, string cat) entry)
    {
        if (currentInvestigation.Count < AnswerKey.instance.maxGuesses)
        {
            currentInvestigation.Add(entry);
            return true;
        }
        return false;
    }

    public override void investigation_Send()
    {
        throw new NotImplementedException();
    }

    public override void investigation_Receive(int numHits)
    {
        throw new NotImplementedException();
    }

    public override void investigationPI_Receive(ulong fromPlayerIndex, NetCpdCategory[] questions, int numHits)
    {
        throw new NotImplementedException();
    }

    public override void guessTargets_Send()
    {
        throw new NotImplementedException();
    }

    public override void guessTargets_Receive(bool success)
    {
        if (success)
        {
            Debug.Log("CPU WINS!");
            // TODO
        }
        else
        {
            Debug.Log("Wrong guy!");
        }
    }





    // CPU-specific methods

    public void skipTurn()
    {
        Debug.Log("Skipping CPU " + agentName + "'s turn.");
    }
}