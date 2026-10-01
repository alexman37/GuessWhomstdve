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

    private RectTransform rc;
    private Vector2 startPosition;
    private Vector2 endPosition;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        rc = GetComponent<RectTransform>();
        startPosition = rc.anchoredPosition;
        endPosition = new Vector2(0,0);
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
        StartCoroutine(movePanel(endPosition));
    }

    public void closeAndHide()
    {
        StartCoroutine(movePanel(startPosition));
    }

    private IEnumerator movePanel(Vector2 toDest)
    {
        float maxTime = 1;
        Vector2 startPos = rc.anchoredPosition;
        for (float i = 0; i < maxTime; i += Time.deltaTime)
        {
            rc.anchoredPosition = Vector2.Lerp(startPos, toDest, i / maxTime);
            yield return null;
        }
    }
}
