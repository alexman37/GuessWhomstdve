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
    private ulong localClientId;

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
        if(NetworkManager.Singleton.LocalClientId == 0)
        {
            Debug.Log("GameManagerSC has been spawned on the network.");
            for (int i = 0; i < 8; i++)
            {
                winsPerPlayer.Add(0);
            }
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

    // When the player first joins a lobby, save their localClientId for future use.
    public void setLocalClientId(ulong id)
    {
        localClientId = id;
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

    public void ResetRound()
    {
        connectedHumanPlayersCt.Value = 0;
        ResetSetup_ClientRpc();
    }

    // Ship all players off to the next scene and begin the game setup task for each player
    [ClientRpc]
    private void ShipAndSetup_ClientRpc()
    {
        Debug.Log("Begin setup task for the player ");
        StartCoroutine(SetupTask());
    }

    [ClientRpc]
    private void ResetSetup_ClientRpc()
    {
        Debug.Log("Begin reset task for the player ");
        StartCoroutine(ResetTask());
    }

    // Make sure all essential components created before player is ready.
    private IEnumerator SetupTask()
    {
        if (NetworkManager.LocalClientId == 0)
        {
            WaitForAllPlayersToSetup_ServerRpc();
            NetworkManager.SceneManager.LoadScene("GW_Main", LoadSceneMode.Single);
        }

        while (RosterGen.instance == null)
            yield return null;
        RosterGen.instance.createRoster(gameParameters.Value.rosterSizeZeroes);
        Debug.Log("[Y] Used RosterGen");

        while (!rosterReady)
            yield return null;
        Debug.Log("[Y] Roster ready to go");

        while (UI_Playerbase.instance == null)
            yield return null;
        (int humanAndBotPlayers, int humanPlayers) pc = UI_Playerbase.instance.setupPlayerbase(gameParameters.Value.playerSetupInfo);

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

        int playerbaseIndex = -1;
        for(int i = 0; i < gameParameters.Value.playerSetupInfo.Length; i++)
        {
            Debug.Log("Player in order " + i + " w connection ID " + gameParameters.Value.playerSetupInfo[i].playerConnectionId + "(you are " + NetworkManager.Singleton.LocalClientId + ")");
            if(gameParameters.Value.playerSetupInfo[i].playerConnectionId == NetworkManager.Singleton.LocalClientId)
            {
                playerbaseIndex = i;
                break;
            }
        }
        if(playerbaseIndex == -1)
        {
            throw new PlayerPrefsException("Local player not found in game parameters list!");
        } else
        {
            var playerInfo = gameParameters.Value.playerSetupInfo[playerbaseIndex];
            HumanPlayer.self = new HumanPlayer(playerInfo.name.ToString(), playerInfo.orderedId, localClientId);
        }

        while (UI_Roster.instance == null)
            yield return null;

        while (RosterForm.instance == null)
            yield return null;
        RosterForm.instance.Setup();

        MarkPlayerAsConnected_ServerRpc();
    }

    // Reset components after a round before we're ready to play.
    private IEnumerator ResetTask()
    {
        // Assumes connectedPlayersCt has been reset to 0 before called
        if (NetworkManager.LocalClientId == 0)
        {
            WaitForAllPlayersToSetup_ServerRpc();
        }

        Roster.instance.resetRound();
        yield return null;

        if (NetworkManager.LocalClientId == 0)
        {
            TurnDriverServer.instance.ResetRound();
            AnswerKey.instance.SetAnswerKey(Roster.instance.simulatedTotalRosterSize);
        }

        HumanPlayer.self.rosterConstraints.clearAllConstraints(true);

        // Finish by calling Server RPC to mark player as done the setup
        MarkPlayerAsResetted_ServerRpc();
    }

    /// <summary>
    /// Player has connected to the game for the first time, done setup, is ready to play.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void MarkPlayerAsConnected_ServerRpc()
    {
        Debug.Log("Server RPC calling ownership: " + NetworkManager.Singleton.LocalClientId);
        connectedHumanPlayersCt.Value += 1;
    }

    /// <summary>
    /// Host waits for all players to connect and finish setup.
    /// </summary>
    [ServerRpc]
    private void WaitForAllPlayersToSetup_ServerRpc()
    {
        StartCoroutine(WaitForAllPlayersToSetup());
    }

    /// <summary>
    /// Player finishes setup for a new round.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void MarkPlayerAsResetted_ServerRpc()
    {
        connectedHumanPlayersCt.Value += 1;
    }

    /// <summary>
    /// Host waits for players indefinitely
    /// </summary>
    private IEnumerator WaitForAllPlayersToSetup()
    {
        while (connectedHumanPlayersCt.Value < gameParameters.Value.humanPlayerCount)
        {
            Debug.Log("Not all players connected yet... " + connectedHumanPlayersCt.Value + "/" + gameParameters.Value.humanPlayerCount);
            yield return new WaitForSeconds(1);
        }
        KickOff();
    }

    /// <summary>
    /// When everything has been loaded, begin the game for real
    /// </summary>
    private void KickOff()
    {
        Debug.Log("Let the game begin.");
        TurnDriverServer.instance.BeginGame();
    }



    /// <summary>
    /// Mark this player as a winner of the round (there may be others)
    /// </summary>
    public void MarkPlayerAsWinner(ulong idOfRoundWinner)
    {
        MarkWinner_ServerRpc(idOfRoundWinner);
    }

    /// <summary>
    /// Mark this player as a winner of the round (there may be others)
    /// </summary>
    [ServerRpc]
    private void MarkWinner_ServerRpc(ulong idOfRoundWinner)
    {
        winnersThisRound.Add(idOfRoundWinner);
        TurnDriverServer.instance.FinishWaitingForServerUpdate(idOfRoundWinner);
    }

    public void EndAndCalculateWins()
    {
        ProcessWinners_ServerRpc();
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

    public void ExitGame()
    {
        // TODO
    }
}

public struct MainGameParameters : INetworkSerializable
{
    public PlayerSetupInfo[] playerSetupInfo;
    public ushort humanPlayerCount;
    public ushort rosterSizeZeroes;
    public ushort roundsToWin;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref rosterSizeZeroes);
        serializer.SerializeValue(ref humanPlayerCount);
        serializer.SerializeValue(ref roundsToWin);
        serializer.SerializeValue(ref playerSetupInfo);
    }
}