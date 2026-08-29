using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormButtonInvestigation : MonoBehaviour
{
    [SerializeField] FormButton formButton;
    [SerializeField] Image back;

    private bool investigating;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void AddToInvestigation()
    {
        investigating = !investigating;
        if(investigating)
        {
            HumanPlayer.self.addToInvestigation((formButton.cpdType, formButton.category));
            back.color = Color.cyan;
        } else
        {
            HumanPlayer.self.removeFromInvestigation((formButton.cpdType, formButton.category));
            back.color = Color.gray;
        }
    }

    // TODO call through an action
    public void ResetInvestigation()
    {
        investigating = false;
        HumanPlayer.self.clearConstraints();
        back.color = Color.gray;
    }
}
