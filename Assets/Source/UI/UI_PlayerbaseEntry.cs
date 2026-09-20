using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_PlayerbaseEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title;

    private int orderedId;
    private ulong playerConnectionId;

    private int winCountOfPlayer = 0;

    [SerializeField] TextMeshProUGUI winTotalTxt;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetParams(PlayerSetupInfo psi)
    {
        title.text = psi.name.ToString();
        playerConnectionId = psi.playerConnectionId;
        orderedId = psi.orderedId;
    }

    public void AddWinToTotal()
    {
        winCountOfPlayer++;
        winTotalTxt.text = winCountOfPlayer.ToString();
    }
}
