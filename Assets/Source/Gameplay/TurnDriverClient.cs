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
                lockActions();
                commitAction();
                InfoBar.instance.setReadout("Investigating...");
                InfoBar.instance.setTimer(2);
                break;
            // Players all shown info: Show them what other players learned also
            case TurnDriverPhase.PassiveInfo:
                resetInvestigations.Invoke();
                InfoBar.instance.setReadout("PassiveInfo Phase");
                InfoBar.instance.setTimer(5);
                break;
            // Shown what other players learned: REPEAT
            case TurnDriverPhase.PlayerTurns:
                unlockActions();
                InfoBar.instance.setReadout("Turn phase");
                InfoBar.instance.setTimer(15);
                break;
        }
    }

    // Can't really do this thru actions because it's very important it all happens before processing the turn
    private void lockActions()
    {
        FormButtonInvestigation.acceptingInput = false;
        CharacterCard.acceptingInput = false;
    }

    private void unlockActions()
    {
        FormButtonInvestigation.acceptingInput = true;
        CharacterCard.acceptingInput = true;
    }

    // The player completes their action. PlayerTurnProcessor should do most of the work
    private void commitAction()
    {
        PlayerTurnProcessor.instance.CommitAction();
    }

    // Send a message to server, saying this player is done 
    public void FinishCurrentPhase()
    {
        TurnDriverServer.instance.ReceivePlayerStatusUpdate(NetworkManager.Singleton.LocalClientId);
    }
}
