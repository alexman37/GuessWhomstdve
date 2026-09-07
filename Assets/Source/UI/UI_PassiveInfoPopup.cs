using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_PassiveInfoPopup : MonoBehaviour
{
    public static UI_PassiveInfoPopup instance;
    [SerializeField] TextMeshProUGUI passiveInfoText_title;
    [SerializeField] TextMeshProUGUI passiveInfoText_numCorrect;
    [SerializeField] TextMeshProUGUI passiveInfoText_guesses;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void showAndUpdateText(string playerName, NetCpdCategory[] guesses, int numCorrect)
    {
        passiveInfoText_title.text = playerName;
        passiveInfoText_numCorrect.text = numCorrect.ToString();

        string guessString = "";
        Dictionary<CPD_Type, string> matches = new Dictionary<CPD_Type, string>();
        for(int i = 0; i < guesses.Length; i++)
        {
            if(!matches.ContainsKey(guesses[i].cpdType))
            {
                matches.Add(guesses[i].cpdType, "\n" + guesses[i].catIndex + "\n");
            }
            else
            {
                matches[guesses[i].cpdType] += (guesses[i].catIndex + "\n");
            }
        }
        foreach(CPD_Type key in matches.Keys) {
            guessString += matches[key];
        }

        passiveInfoText_guesses.text = guessString;
    }
}
