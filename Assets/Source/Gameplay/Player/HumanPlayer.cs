using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class HumanPlayer : GD_Player
{
    // Each connected client differs on this
    public static HumanPlayer self;

    public static event Action<int> playerUpdateProgress = (_) => { };

    public bool investigationReceived = false;

    public HumanPlayer(string playerName)
    {
        // TODO player's name
        agentName = playerName;

        id = 0;

        rosterConstraints = new RosterConstraints();
        rosterConstraints.clearAllConstraints(true);

        Roster.clearAllConstraints += clearConstraints;
        TurnDriver.dispatchInvestigations += investigation_Send;
        TurnDriver.resetInvestigations += resetInvestigation;
        //Roster.guessedWrongCharacter += guessTarget;
    }

    ~HumanPlayer()
    {
        Roster.clearAllConstraints -= clearConstraints;
        TurnDriver.dispatchInvestigations -= investigation_Send;
        TurnDriver.resetInvestigations -= resetInvestigation;
        //Roster.guessedWrongCharacter -= guessTarget;
    }

    // Initial actions before the player's turn.
    public override void markAsReady()
    {
        Debug.Log("It's the player's turn.");
    }

    public override void addToInvestigation((CPD_Type cpdType, string cat) entry)
    {
        if(currentInvestigation.Count < AnswerKey.instance.maxGuesses)
        {
            currentInvestigation.Add(entry);
        }
    }

    public override void investigation_Send()
    {
        AnswerKey.instance.processInvestigation(currentInvestigation, NetworkManager.Singleton.LocalClientId);
    }

    public override void investigation_Receive(int numHits)
    {
        Debug.Log("[RESP] Found " + numHits + " hits.");
        investigationReceived = true;
    }

    public override void resetInvestigation()
    {
        base.resetInvestigation();
        investigationReceived = false;
    }

    // When a target has been guessed, do these actions
    // Some are performed only if it's your turn
    public override void guessTarget_Send(ulong characterId)
    {
        AnswerKey.instance.targetIdMatchOne(characterId, NetworkManager.Singleton.LocalClientId);
    }

    public override void guessTarget_Receive(bool success)
    {
        if (success)
        {
            Debug.Log("YOU WIN!");
            // TODO
        }
        else
        {
            Debug.Log("Wrong guy!");
            endOfTurn();
        }
    }

    // When turn is over do these actions
    public override void endOfTurn()
    {
        Debug.Log("The player's turn has ended.");
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
