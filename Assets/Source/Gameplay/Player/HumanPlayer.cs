using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HumanPlayer : Player
{
    public static event Action<int> playerUpdateProgress = (_) => { };

    public HumanPlayer()
    {
        // TODO player's name
        agentName = "Player";

        id = 0;

        rosterConstraints = new RosterConstraints();
        rosterConstraints.clearAllConstraints(true);

        Roster.clearAllConstraints += clearConstraints;
        Roster.guessedWrongCharacter += guessTarget;
    }

    ~HumanPlayer()
    {
        Roster.clearAllConstraints -= clearConstraints;
        Roster.guessedWrongCharacter -= guessTarget;
    }

    // Initial actions before the player's turn.
    public override void markAsReady()
    {
        Debug.Log("It's the player's turn.");
    }

    // When a target has been guessed, do these actions
    // Some are performed only if it's your turn
    public override void guessTarget(int characterId)
    {
        bool correct = TurnDriver.instance.currentRoster.targetId == characterId;
        // TODO obv. gotta do more than just click/respond
        if (correct)
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
