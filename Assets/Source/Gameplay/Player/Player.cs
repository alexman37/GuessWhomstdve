using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Player
{
    public int id;
    public string agentName;
    public Sprite portrait;

    public int maxActionCardCount = 5;
    // TODO inventory system?
    //public List<ClueCard> inventory = new List<ClueCard>();

    public RosterConstraints rosterConstraints;

    public bool isHuman = false;


    /// <summary>
    /// It's your turn.
    /// </summary>
    public abstract void markAsReady();

    /// <summary>
    /// Guess the target outright
    /// </summary>
    public abstract void guessTarget(int characterId);

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