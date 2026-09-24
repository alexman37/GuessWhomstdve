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
                string catName = CPD.registry[guesses[i].cpdType].categories[guesses[i].catIndex];
                string cpdName = guesses[i].cpdType.ToString();
                matches.Add(guesses[i].cpdType, $"{cpdName}\n -  {catName}\n");
            }
            else
            {
                string catName = CPD.registry[guesses[i].cpdType].categories[guesses[i].catIndex];
                matches[guesses[i].cpdType] += $" -  {catName}\n";
            }
        }
        foreach(CPD_Type key in matches.Keys) {
            guessString += matches[key];
        }

        passiveInfoText_guesses.text = guessString;
    }
}
