using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class GD_Player
{
    public int id;
    public string agentName;
    public Sprite portrait;

    public int investigationCount = 3;
    public int targetGuessCount = 1;
    // TODO inventory system?
    //public List<ClueCard> inventory = new List<ClueCard>();

    public RosterConstraints rosterConstraints;

    public bool isHuman = false;

    public HashSet<(CPD_Type cpd, string cat)> currentInvestigation = new HashSet<(CPD_Type cpd, string cat)>();
    public HashSet<ulong> currentTargetSelections = new HashSet<ulong>();


    /// <summary>
    /// It's your turn.
    /// </summary>
    public abstract void markAsReady();

    /// <summary>
    /// Return true if succeeded in adding to investigation
    /// </summary>
    public abstract bool addToInvestigation((CPD_Type cpdType, string cat) entry);

    public virtual void removeFromInvestigation((CPD_Type cpdType, string cat) entry)
    {
        currentInvestigation.Remove(entry);
    }

    public virtual bool addToTargetsList(ulong targetId)
    {
        if(currentTargetSelections.Contains(targetId))
        {
            return false;
        } else
        {
            currentTargetSelections.Add(targetId);
            return true;
        }
    }

    public virtual void removeFromTargetsList(ulong targetId)
    {
        currentTargetSelections.Remove(targetId);
    }

    public virtual void resetInvestigation()
    {
        currentInvestigation.Clear();
    }

    public abstract void investigation_Send();
    public abstract void investigation_Receive(int numHits);
    public abstract void investigationPI_Receive(ulong fromPlayerIndex, NetCpdCategory[] questions, int numHits);

    /// <summary>
    /// Guess the target. Since clients don't store this information, you must ask the server
    /// </summary>
    public abstract void guessTargets_Send();

    /// <summary>
    /// Get a response back from the server on above target guess
    /// </summary>
    public abstract void guessTargets_Receive(bool success);

    public virtual void clearConstraints()
    {
        // "Clear" also serves as initialization for the constraints lists if need be
        rosterConstraints = new RosterConstraints();
        foreach (CPD cpd in Roster.cpdConstrainables)
        {
            rosterConstraints.clearConstraints(cpd, true);
        }
    }

    public abstract void endOfTurn();
}