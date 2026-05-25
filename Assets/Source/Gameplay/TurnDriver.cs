using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turn Driver: Manages a normal turn of gameplay
/// </summary>
public class TurnDriver : MonoBehaviour
{
    public static TurnDriver instance;

    public List<Player> playersInOrder = new List<Player>();

    public Roster currentRoster;


    // TODO make doable for many
    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        } else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        RosterGen.rosterCreationDone += onRosterCreation;
    }

    private void OnDisable()
    {
        RosterGen.rosterCreationDone -= onRosterCreation;
    }

    private void onRosterCreation(Roster rost)
    {
        currentRoster = rost;
        generatePlayers();
        roundSetup();
    }

    private void generatePlayers()
    {
        HumanPlayer playerHuman = new HumanPlayer();
        playersInOrder.Add(playerHuman);

        BotPlayer playerBot1 = new BotPlayer(1, "Heathers");
        playersInOrder.Add(playerBot1);
    }






    // TODO: Start everyone's turn. Not just the first.
    private void roundSetup()
    {
        playersInOrder[0].markAsReady();
    }
}
