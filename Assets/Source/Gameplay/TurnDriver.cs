using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Turn Driver: Manages a normal turn of gameplay
/// </summary>
public class TurnDriver : MonoBehaviour
{
    public static TurnDriver instance;

    public TurnDriverPhase currentPhase = TurnDriverPhase.PlayerTurns;

    public Roster currentRoster;

    public static event Action dispatchInvestigations = () => { };
    public static event Action resetInvestigations = () => { };


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
            // Player turns end: Send requests to server and wait til everyone hears back
            case TurnDriverPhase.PlayerTurns:
                currentPhase = TurnDriverPhase.PlayerTurnsIntermission;
                InfoBar.instance.setReadout("Investigating...");
                dispatchInvestigations.Invoke();
                StartCoroutine(waitForPlayersToReceiveInvestigations());
                break;
            // Players all hear back from server: Show them, let them do stuff with info
            case TurnDriverPhase.PlayerTurnsIntermission:
                resetInvestigations.Invoke();
                currentPhase = TurnDriverPhase.PassiveInfo;
                InfoBar.instance.setReadout("Server response phase");
                InfoBar.instance.setTimer(5);
                break;
            // Players all shown info: Show them what other players learned also
            case TurnDriverPhase.ServerResponse:
                currentPhase = TurnDriverPhase.PassiveInfo;
                InfoBar.instance.setReadout("PassiveInfo Phase");
                InfoBar.instance.setTimer(5);
                break;
            // Shown what other players learned: REPEAT
            case TurnDriverPhase.PassiveInfo:
                currentPhase = TurnDriverPhase.PlayerTurns;
                InfoBar.instance.setReadout("Turn phase");
                InfoBar.instance.setTimer(15);
                break;
        }
    }

    // TODO actually wait
    private IEnumerator waitForPlayersToReceiveInvestigations()
    {
        yield return new WaitForSeconds(2);
        TimedPhaseCycle();
    }


    private void roundSetup()
    {
        
    }
}

public enum TurnDriverPhase
{
    PlayerTurns,
    PlayerTurnsIntermission,
    ServerResponse,
    PassiveInfo
}