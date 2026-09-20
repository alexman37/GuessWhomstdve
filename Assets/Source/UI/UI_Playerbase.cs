using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Playerbase : MonoBehaviour
{
    public static UI_Playerbase instance;

    [SerializeField] GameObject playerbaseContainer;
    [SerializeField] GameObject playerbaseEntry;

    private Dictionary<int, UI_PlayerbaseEntry> entriesById;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    /// <summary>
    /// Return the number of (human or bot players, human players) in-game
    /// </summary>
    public (int, int) setupPlayerbase(PlayerSetupInfo[] psi)
    {
        entriesById = new Dictionary<int, UI_PlayerbaseEntry>();

        for (int i = playerbaseContainer.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(playerbaseContainer.transform.GetChild(i));
        }

        int humanCount = 0;
        for (int i = 0; i < psi.Length; i++)
        {
            // Assumes the list will be ordered...a safe assumption?
            if (psi[i].type == PlayerSetupType.None)
                return (i, humanCount);
            else if (psi[i].type == PlayerSetupType.Human)
                humanCount++;
            GameObject go = GameObject.Instantiate(playerbaseEntry, playerbaseContainer.transform);

            UI_PlayerbaseEntry ent = go.GetComponent<UI_PlayerbaseEntry>();
            ent.SetParams(psi[i]);
            entriesById.Add(psi[i].orderedId, ent);
        }
        return (psi.Length, humanCount);
    }

    public void AddHumanWinToTotal(int forOrderedId)
    {
        entriesById[forOrderedId].AddWinToTotal();
    }
}
