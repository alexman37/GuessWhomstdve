using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class HumanPlayer : GD_Player
{
    public ulong connectionId;

    // Each connected client differs on this
    public static HumanPlayer self;

    public static event Action<int> playerUpdateProgress = (_) => { };

    public bool investigationReceived = false;

    public HumanPlayer(string playerName, int uid, ulong cid)
    {
        // TODO player's name
        agentName = playerName;

        uniqueId = uid;
        connectionId = cid;

        rosterConstraints = new RosterConstraints();
        rosterConstraints.clearAllConstraints(true);
        Roster.clearAllConstraints += clearConstraints;
        TurnDriverClient.resetInvestigations += resetInvestigation;
        //Roster.guessedWrongCharacter += guessTarget;
    }

    ~HumanPlayer()
    {
        Roster.clearAllConstraints -= clearConstraints;
        TurnDriverClient.resetInvestigations -= resetInvestigation;
        //Roster.guessedWrongCharacter -= guessTarget;
    }

    // Initial actions before the player's turn.
    public override void markAsReady()
    {
        Debug.Log("It's the player's turn.");
    }

    public override bool addToInvestigation((CPD_Type cpdType, string cat) entry)
    {
        if(currentInvestigation.Count < AnswerKey.instance.maxGuesses)
        {
            currentInvestigation.Add(entry);
            return true;
        }
        return false;
    }

    public override void investigation_Send()
    {
        Debug.Log(NetworkManager.Singleton.LocalClientId + " SENDS INVESTIGATION");
        AnswerKey.instance.processInvestigation(currentInvestigation, NetworkManager.Singleton.LocalClientId);
    }

    public override void investigation_Receive(int numHits)
    {
        Debug.Log("[RESP] Found " + numHits + " hits.");
        investigationReceived = true;
        TurnDriverClient.instance.FinishCurrentPhase();
    }

    public override void investigationPI_Receive(ulong fromPlayerIndex, NetCpdCategory[] questions, int numHits)
    {
        Debug.Log("[PI] Found " + numHits + " hits.");
        UI_PassiveInfoPopup.instance.showAndUpdateText(
            GameManagerSc.instance.FromNetId_GetPlayerName(fromPlayerIndex),
            questions,
            numHits
        );
    }

    public override void resetInvestigation()
    {
        base.resetInvestigation();
        investigationReceived = false;
    }

    // When a target has been guessed, do these actions
    // Some are performed only if it's your turn
    public override void guessTargets_Send()
    {
        ulong[] characterIds = new ulong[currentTargetSelections.Count];
        int count = 0;
        foreach(ulong charId in currentTargetSelections)
        {
            characterIds[count] = charId;
            count++;
        }
        AnswerKey.instance.targetIdMatchAny(characterIds, NetworkManager.Singleton.LocalClientId);
    }

    public override void guessTargets_Receive(bool success)
    {
        if (success)
        {
            Debug.Log("YOU WIN!");
            // Wait until the server confirms the players' win is counted
            TurnDriverClient.instance.FinishCurrentPhase(PhaseFinishStatusUpdate.PlayerWon);
        }
        else
        {
            Debug.Log("Wrong guy!");
            TurnDriverClient.instance.FinishCurrentPhase();
        }
    }

    // CPU handles their constraints locally.
    private void updateConstraintsFromInfo((CPD_Type cpdType, string cat) info, bool isCorrect)
    {
        if (isCorrect)
        {
            rosterConstraints.onlyConstraint(info.cpdType, info.cat);
        }
        else
        {
            rosterConstraints.addConstraint(info.cpdType, info.cat, true);
        }
    }
}
