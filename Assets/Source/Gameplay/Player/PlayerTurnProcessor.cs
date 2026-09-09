using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerTurnProcessor : MonoBehaviour
{
    public static PlayerTurnProcessor instance;

    private PlayerTurnAction currentAction = PlayerTurnAction.Nothing;
    private int numberOfActions;

    public static event Action<PlayerTurnAction> switchedAction = (_) => { };

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void ProcessActionChange(PlayerTurnAction newAction, bool additive)
    {
        // Action may be changed entirely
        if (currentAction != newAction)
        {
            currentAction = newAction;
            numberOfActions = 1;
            switchedAction.Invoke(newAction);
            Debug.Log("[PTP] Action Switched to " + newAction);
            return;
        }

        // If "undoing" an action, and no actions are being taken, treat that separately
        if (!additive)
        {
            numberOfActions--;
            if (numberOfActions == 0)
            {
                currentAction = PlayerTurnAction.Nothing;
                Debug.Log("[PTP] No action being taken");
            } else
            {
                Debug.Log("[PTP] Action removed");
            }
        } 
        
        else
        {
            numberOfActions++;
            Debug.Log("[PTP] Action added");
        }
    }

    public void CommitAction()
    {
        switch(currentAction)
        {
            case PlayerTurnAction.Nothing:
                Debug.Log("[PTPF] The player did nothing this turn");
                break;
            case PlayerTurnAction.Investigation:
                Debug.Log("[PTPF] The player chose to make investigations");
                HumanPlayer.self.investigation_Send();
                break;
            case PlayerTurnAction.TargetGuess:
                Debug.Log("[PTPF] The player chose to guess targets");
                HumanPlayer.self.guessTargets_Send();
                break;
        }

        currentAction = PlayerTurnAction.Nothing;
    }
}

public enum PlayerTurnAction
{
    Nothing,
    Investigation,
    TargetGuess
}