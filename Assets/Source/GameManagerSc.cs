using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Netcode;

// The bridge between main menu and game start,
// And manager of the highest-level problems in the game
public class GameManagerSc : NetworkBehaviour
{
    public const int MAX_PLAYER_CT = 8;

    public static GameManagerSc instance;
    NetworkVariable<MainGameParameters> gameParameters = new NetworkVariable<MainGameParameters>(value: new MainGameParameters {
        playerSetupInfo = new PlayerSetupInfo[8],
        humanPlayerCount = 0,
        rosterSizeZeroes = 3,
        roundsToWin = 1
    });

    NetworkList<int> winsPerPlayer;

    public NetworkList<ulong> winnersThisRound;

    NetworkVariable<ushort> connectedHumanPlayersCt = new NetworkVariable<ushort>(0);

    private bool rosterReady = false;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        DontDestroyOnLoad(this.gameObject);

        winsPerPlayer = new NetworkList<int>();
        winnersThisRound = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("GameManagerSC has been spawned on the network.");
        for(int i = 0; i < 8; i++)
        {
            winsPerPlayer.Add(0);
        }
    }

    private void OnEnable()
    {
        Roster.rosterReady += SetRosterReady;
    }

    private void OnDisable()
    {
        Roster.rosterReady -= SetRosterReady;
    }

    private void SetRosterReady()
    {
        rosterReady = true;
    }

    // Set up game parameters established in the main menu, and wait for all components to be set up
    public void SetGameParameters(MainGameParameters mgp)
    {
        gameParameters.Value = mgp;
        ShipAndSetup_ClientRpc();
    }

    // Ship all players off to the next scene and begin the game setup task for each player
    [ClientRpc]
    private void ShipAndSetup_ClientRpc()
    {
        Debug.Log("Begin setup task for the player ");
        StartCoroutine(SetupTask());
    }
    
    private IEnumerator SetupTask()
    {
        Debug.Log("In co ");
        if (NetworkManager.LocalClientId == 0)
        {
            WaitForAllPlayersToSetup_ServerRpc();
            NetworkManager.SceneManager.LoadScene("GW_Main", LoadSceneMode.Single);
        }

        while (!rosterReady)
            yield return null;

        while (UI_Playerbase.instance == null)
            yield return null;
        (int humanAndBotPlayers, int humanPlayers) pc = UI_Playerbase.instance.redrawPlayerbase(gameParameters.Value.playerSetupInfo);

        while (InfoBar.instance == null)
            yield return null;
        while (TurnDriverClient.instance == null)
            yield return null;

        while (AnswerKey.instance == null)
            yield return null;
        if (NetworkManager.LocalClientId == 0)
        {
            // No need - it should happen on the server automatically, if set up correctly
            //AnswerKey.instance.GetComponent<NetworkObject>().Spawn();

            while (AnswerKey.readyToUse == false)
                yield return null;
            AnswerKey.instance.SetAnswerKey(Roster.instance.simulatedTotalRosterSize);
        }

        while (TurnDriverServer.instance == null)
            yield return null;
        Debug.Log("Local client ID is " + NetworkManager.LocalClientId);
        if (NetworkManager.LocalClientId == 0)
        {
            Debug.Log("I'm the host, so I create TurnDriverServer instance here");
            TurnDriverServer.instance.InitPassiveInfoSystem(pc.humanAndBotPlayers, pc.humanPlayers);
        }

        // TODO - we have the player names, just gotta use them
        HumanPlayer.self = new HumanPlayer("TestPlayer");

        while (UI_Roster.instance == null)
            yield return null;

        while (RosterForm.instance == null)
            yield return null;
        RosterForm.instance.Setup();

        Debug.Log("Reached near end of setup");
        MarkPlayerAsConnected_ServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void MarkPlayerAsConnected_ServerRpc()
    {
        Debug.Log("Server RPC calling ownership: " + NetworkManager.Singleton.LocalClientId);
        connectedHumanPlayersCt.Value += 1;
    }

    [ServerRpc]
    private void WaitForAllPlayersToSetup_ServerRpc()
    {
        StartCoroutine(WaitForAllPlayersToSetup());
    }

    public void MarkPlayerAsWinner(ulong idOfRoundWinner)
    {
        MarkWinner_ServerRpc(idOfRoundWinner);
    }

    [ServerRpc]
    private void MarkWinner_ServerRpc(ulong idOfRoundWinner)
    {
        winnersThisRound.Add(idOfRoundWinner);
    }

    [ServerRpc]
    private void ProcessWinners_ServerRpc()
    {
        // Give those who won the round +1 wins for the game
        if(winnersThisRound.Count > 0)
        {
            ulong[] temp = new ulong[winnersThisRound.Count];
            int count = 0;
            foreach (ulong id in winnersThisRound)
            {
                winsPerPlayer[(int)id] += 1;
                temp[count++] = id;
            }

            // If anyone meets the win total, they win the game
            ulong[] idsOfGameWinners = new ulong[8];
            int numGameWinners = 0;
            for (int i = 0; i < winsPerPlayer.Count; i++)
            {
                if (winsPerPlayer[i] >= gameParameters.Value.roundsToWin)
                {
                    idsOfGameWinners[numGameWinners] = (ulong)i;
                    numGameWinners++;
                }
            }

            // If anyone won the game, end it
            if (numGameWinners > 0)
            {
                EndGame_ClientRpc(idsOfGameWinners);
            }
            else
            {
                EndRound_ClientRpc(temp);
            }
            winnersThisRound.Clear();
        }
    }

    [ClientRpc]
    private void EndRound_ClientRpc(ulong[] idsOfRoundWinners)
    {
        UI_RoundOverPopup.instance.ShowEndOfRound(idsOfRoundWinners);
    }

    [ClientRpc]
    private void EndGame_ClientRpc(ulong[] idsOfGameWinners)
    {
        UI_RoundOverPopup.instance.ShowEndOfGame(idsOfGameWinners);
    }

    private IEnumerator WaitForAllPlayersToSetup()
    {
        while(connectedHumanPlayersCt.Value < gameParameters.Value.humanPlayerCount)
        {
            Debug.Log("Not all players connected yet... " + connectedHumanPlayersCt.Value + "/" + gameParameters.Value.humanPlayerCount);
            yield return new WaitForSeconds(1);
        }
        KickOff();
    }

    // When everything has been loaded, begin the game for real
    private void KickOff()
    {
        Debug.Log("Let the game begin.");
        TurnDriverServer.instance.BeginGame();
    }
}

public struct MainGameParameters : INetworkSerializable
{
    public PlayerSetupInfo[] playerSetupInfo;
    public ushort humanPlayerCount;
    public ulong rosterSizeZeroes;
    public ushort roundsToWin;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref rosterSizeZeroes);
        serializer.SerializeValue(ref humanPlayerCount);
        serializer.SerializeValue(ref roundsToWin);
        serializer.SerializeValue(ref playerSetupInfo);
    }
}