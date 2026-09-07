using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

/// <summary>
/// Manages a normal turn of gameplay, things that happen specifically for the client:
///   - The timer
///   - Sending UI updates
/// </summary>
public class TurnDriverClient : MonoBehaviour
{
    public static TurnDriverClient instance;

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
        InfoBar.timerFinished += FinishCurrentPhase;
    }

    private void OnDisable()
    {
        InfoBar.timerFinished -= FinishCurrentPhase;
    }

    public void OnTurnPhaseChange(TurnDriverPhase changedTo)
    {
        Debug.Log("The phase was changed to " + changedTo);
        switch (changedTo)
        {
            // Players all hear back from server: Show them, let them do stuff with info
            case TurnDriverPhase.ServerResponse:
                InfoBar.instance.setReadout("Server response phase");
                InfoBar.instance.setTimer(5);
                break;
            case TurnDriverPhase.InvestigationDispatch:
                dispatchInvestigations.Invoke();
                InfoBar.instance.setReadout("Investigating...");
                InfoBar.instance.setTimer(2);
                break;
            // Players all shown info: Show them what other players learned also
            case TurnDriverPhase.PassiveInfo:
                InfoBar.instance.setReadout("PassiveInfo Phase");
                InfoBar.instance.setTimer(5);
                break;
            // Shown what other players learned: REPEAT
            case TurnDriverPhase.PlayerTurns:
                resetInvestigations.Invoke();
                InfoBar.instance.setReadout("Turn phase");
                InfoBar.instance.setTimer(15);
                break;
        }
    }

    // Send a message to server, saying this player is done 
    public void FinishCurrentPhase()
    {
        TurnDriverServer.instance.ReceivePlayerStatusUpdate(NetworkManager.Singleton.LocalClientId);
    }
}
