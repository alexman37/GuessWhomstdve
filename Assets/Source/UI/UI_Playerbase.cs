using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Playerbase : MonoBehaviour
{
    public static UI_Playerbase instance;

    [SerializeField] GameObject playerbaseContainer;
    [SerializeField] GameObject playerbaseEntry;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void redrawPlayerbase(List<PlayerSetupInfo> psi)
    {
        for (int i = playerbaseContainer.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(playerbaseContainer.transform.GetChild(i));
        }

        for (int i = 0; i < psi.Count; i++)
        {
            GameObject go = GameObject.Instantiate(playerbaseEntry, playerbaseContainer.transform);
            go.GetComponent<UI_PlayerbaseEntry>().SetParams(psi[i]);
        }
    }
}
