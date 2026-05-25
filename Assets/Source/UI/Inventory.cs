using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    [SerializeField] GameObject actionCardTemplate;

    private List<GameObject> actionCardInstances = new List<GameObject>();

    // Start is called before the first frame update
    private void OnEnable()
    {
        //PlayerAgent.playerGotCard += addCardToInventory;
        //PlayerAgent.playerLostCard += removeCardFromInventory;
    }

    private void OnDisable()
    {
        //PlayerAgent.playerGotCard -= addCardToInventory;
        //PlayerAgent.playerLostCard -= removeCardFromInventory;
    }

    public void addCardToInventory()
    {
        
    }

    public void removeCardFromInventory()
    {
        
    }
}
