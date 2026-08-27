using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turn Driver: Manages a normal turn of gameplay
/// </summary>
public class TurnDriver : MonoBehaviour
{
    public static TurnDriver instance;

    public TurnDriverPhase currentPhase = TurnDriverPhase.PlayerTurns;

    public Roster currentRoster;


    // TODO make doable for many
    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        } else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        RosterGen.rosterCreationDone += onRosterCreation;
        InfoBar.timerFinished += TimedPhaseCycle;
    }

    private void OnDisable()
    {
        RosterGen.rosterCreationDone -= onRosterCreation;
        InfoBar.timerFinished -= TimedPhaseCycle;
    }

    private void onRosterCreation(Roster rost)
    {
        currentRoster = rost;
        roundSetup();
    }

    public void BeginGame()
    {
        // This gets us started on the first phase
        currentPhase = TurnDriverPhase.PassiveInfo;
        TimedPhaseCycle();
    }

    // Go to the next phase, do all necessary steps.
    public void TimedPhaseCycle()
    {
        switch (currentPhase)
        {
            case TurnDriverPhase.PlayerTurns:
                currentPhase = TurnDriverPhase.ServerResponse;
                InfoBar.instance.setReadout("Server response phase");
                InfoBar.instance.setTimer(5);
                break;
            case TurnDriverPhase.ServerResponse:
                currentPhase = TurnDriverPhase.PassiveInfo;
                InfoBar.instance.setReadout("PassiveInfo Phase");
                InfoBar.instance.setTimer(5);
                break;
            case TurnDriverPhase.PassiveInfo:
                currentPhase = TurnDriverPhase.PlayerTurns;
                InfoBar.instance.setReadout("Turn phase");
                InfoBar.instance.setTimer(15);
                break;
        }
    }


    // TODO: Start everyone's turn. Not just the first.
    private void roundSetup()
    {
        
    }
}

public enum TurnDriverPhase
{
    PlayerTurns,
    ServerResponse,
    PassiveInfo
}