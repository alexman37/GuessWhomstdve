using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class UI_RoundOverPopup : MonoBehaviour
{
    public static UI_RoundOverPopup instance;

    [SerializeField] GameObject mainframe;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI winners;
    [SerializeField] private TextMeshProUGUI nextRoundTxt;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void ShowEndOfRound(ulong[] idsOfRoundWinners)
    {
        mainframe.SetActive(true);
        string titleStr = "Round Over! Winners:";
        string winnerStr = "";
        for (int i = 0; i < idsOfRoundWinners.Length; i++)
        {
            if (NetworkManager.Singleton.LocalClientId == idsOfRoundWinners[i])
            {
                winnerStr = winnerStr + "YOU" + ",";
            }
            else
            {
                // TODO - convert ID to a name
                winnerStr = winnerStr + idsOfRoundWinners[i].ToString() + ",";
            }
        }
        string nextRoundStr = "Next Round";

        title.text = titleStr;
        winners.text = winnerStr;
        nextRoundTxt.text = nextRoundStr;
    }

    public void ShowEndOfGame(ulong[] idsOfGameWinners)
    {
        mainframe.SetActive(true);
        string titleStr = "Game Over! Winners:";
        string winnerStr = "";
        for(int i = 0; i < idsOfGameWinners.Length; i++)
        {
            if (NetworkManager.Singleton.LocalClientId == idsOfGameWinners[i])
            {
                winnerStr = winnerStr + "YOU" + ",";
            } else
            {
                // TODO - convert ID to a name
                winnerStr = winnerStr + idsOfGameWinners[i].ToString() + ",";
            }
        }
        string nextRoundStr = "Exit Game";

        title.text = titleStr;
        winners.text = winnerStr;
        nextRoundTxt.text = nextRoundStr;
    }

    public void closeAndHide()
    {
        mainframe.SetActive(false);
    }
}
