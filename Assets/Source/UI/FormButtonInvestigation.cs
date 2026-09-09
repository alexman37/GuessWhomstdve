using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormButtonInvestigation : MonoBehaviour
{
    [SerializeField] FormButton formButton;
    [SerializeField] Image back;

    public static bool acceptingInput = true;

    private bool investigating;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        PlayerTurnProcessor.switchedAction += OnActionSwitch;
    }

    private void OnDisable()
    {
        PlayerTurnProcessor.switchedAction -= OnActionSwitch;
    }

    public void AddToInvestigation()
    {
        if(acceptingInput)
        {
            investigating = !investigating;
            if (investigating)
            {
                bool success = HumanPlayer.self.addToInvestigation((formButton.cpdType, formButton.category));
                if (success)
                {
                    back.color = Color.cyan;
                    PlayerTurnProcessor.instance.ProcessActionChange(PlayerTurnAction.Investigation, true);
                }
            }
            else
            {
                HumanPlayer.self.removeFromInvestigation((formButton.cpdType, formButton.category));
                PlayerTurnProcessor.instance.ProcessActionChange(PlayerTurnAction.Investigation, false);
                back.color = Color.gray;
            }
        }
    }

    // TODO call through an action
    public void ResetInvestigation()
    {
        investigating = false;
        back.color = Color.gray;
    }

    private void OnActionSwitch(PlayerTurnAction action)
    {
        if(action != PlayerTurnAction.Investigation)
        {
            ResetInvestigation();
        }
    }

}
