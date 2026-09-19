using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Handles creation of a Roster.
/// TODO maybe more to do with "remaking" the roster on subsequent plays.
/// </summary>
public class RosterGen : MonoBehaviour
{
    public static RosterGen instance;

    public int numberOfCharacters;
    Roster roster;

    public static event Action<Roster> rosterCreationDone;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void createRoster(ushort sizeBracket)
    {
        roster = new Roster(sizeBracket);

        rosterCreationDone += (_) => { };

        rosterCreationDone.Invoke(roster);
    }
}
