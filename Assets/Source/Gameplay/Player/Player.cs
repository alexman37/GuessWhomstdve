using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class GD_Player
{
    public int id;
    public string agentName;
    public Sprite portrait;

    public int maxActionCardCount = 5;
    // TODO inventory system?
    //public List<ClueCard> inventory = new List<ClueCard>();

    public RosterConstraints rosterConstraints;

    public bool isHuman = false;

    public List<(CPD_Type cpd, string cat)> currentInvestigation = new List<(CPD_Type cpd, string cat)>();


    /// <summary>
    /// It's your turn.
    /// </summary>
    public abstract void markAsReady();

    public abstract void addToInvestigation((CPD_Type cpdType, string cat) entry);

    public virtual void removeFromInvestigation((CPD_Type cpdType, string cat) entry)
    {
        currentInvestigation.Remove(entry);
    }

    public virtual void resetInvestigation()
    {
        currentInvestigation.Clear();
    }

    /// <summary>
    /// Guess the target outright
    /// </summary>
    public abstract void guessTarget(ulong characterId);

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