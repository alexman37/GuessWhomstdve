using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TurnDriverServer : NetworkBehaviour
{
    public static TurnDriverServer instance;
    private bool readyToUse = false;

    public NetworkVariable<TurnDriverPhase> currentPhase = new NetworkVariable<TurnDriverPhase>(TurnDriverPhase.PlayerTurns);
    // Fill this set with the IDs of players who've finished the current phase.
    // When it contains every player, we're ready to move to next phase
    private HashSet<ulong> receivedUpdatesFrom = new HashSet<ulong>();

    public int humanCount;
    public int humanAndBotCount;
    private ulong[] passiveInfoOffsets;
    private int turnCount = 0;

    bool waitingComplete = false; // used for waiting coroutines
    bool killSwitch = false; // set to true to stop the cycle for any reason

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer) Debug.Log("TurnDriverS, coming in from the SERVER");
        else Debug.LogWarning("TurnDriverS, coming in from the CLIENT");

        readyToUse = true;
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    public void BeginGame()
    {
        AnswerKey.instance.SetPassiveInfoOffset(passiveInfoOffsets[0]);
        // This gets us started on the first phase
        currentPhase.Value = TurnDriverPhase.PlayerTurns;
        SendPlayerStartPhaseUpdate_ClientRpc(TurnDriverPhase.PlayerTurns);
        StartCoroutine(waitForPlayersToCompletePhase());
    }

    public void ResetRound()
    {
        ResetRound_ServerRpc();
    }

    [ServerRpc]
    private void ResetRound_ServerRpc()
    {
        killSwitch = false;
        receivedUpdatesFrom.Clear();
        Debug.Log("[P] Resetting server turn driver");
    }

    // Go to the next phase, do all necessary steps.
    public void TimedPhaseCycle()
    {
        switch (currentPhase.Value)
        {
            // Player turns end: Send requests to server and wait til everyone hears back
            case TurnDriverPhase.PlayerTurns:
                currentPhase.Value = TurnDriverPhase.InvestigationDispatch;
                break;
            case TurnDriverPhase.InvestigationDispatch:
                currentPhase.Value = TurnDriverPhase.ServerResponse;
                break;
            // Players all shown info: Show them what other players learned also
            case TurnDriverPhase.ServerResponse:
                currentPhase.Value = TurnDriverPhase.PassiveInfo;
                break;
            // Shown what other players learned: REPEAT
            case TurnDriverPhase.PassiveInfo:
                currentPhase.Value = TurnDriverPhase.PlayerTurns;
                turnCount++;
                if(NetworkManager.Singleton.LocalClientId == 0)
                {
                    ulong offset = passiveInfoOffsets[turnCount % passiveInfoOffsets.Length];
                    AnswerKey.instance.SetPassiveInfoOffset(offset);
                }
                break;
        }
        receivedUpdatesFrom.Clear();
        SendPlayerStartPhaseUpdate_ClientRpc(currentPhase.Value);

        StartCoroutine(waitForPlayersToCompletePhase());
    }

    // TODO actually wait
    private IEnumerator waitForPlayersToCompletePhase()
    {
        Debug.Log("[P] Waiting for phase to end");
        while (receivedUpdatesFrom.Count < humanAndBotCount)
        {
            yield return new WaitForSeconds(1);
        }
        Debug.Log("[P] Done waiting");

        // When we're done waiting, the cycle renews again...except in specific cases
        // if anyone correctly guessed target, end cycle
        if (killSwitch)
        {
            GameManagerSc.instance.EndAndCalculateWins();
            yield break;
        }
        Debug.Log("[P] Continuing");

        // Else, cycle renews
        TimedPhaseCycle();
    }

    /// <summary>
    /// The server receives an update from a player, telling them the player finished their phase
    /// </summary>
    public void ReceivePlayerStatusUpdate(ulong fromWho)
    {
        ReceivePlayerStatusUpdate_ServerRpc(new ServerRpcParams { Receive = { SenderClientId = fromWho } });
    }

    public void ReceivePlayerStatusUpdate(ulong fromWho, PhaseFinishStatusUpdate specialStatus)
    {
        if(specialStatus == PhaseFinishStatusUpdate.PlayerWon)
        {
            // The turn cycle will stop, this round is over
            OverrideCycle_ServerRpc(true);

            GameManagerSc.instance.MarkPlayerAsWinner(fromWho);
            // Need to wait for server to acknowledge player's win
            StartCoroutine(WaitForServerUpdate(fromWho));
        } else
        {
            ReceivePlayerStatusUpdate_ServerRpc(new ServerRpcParams { Receive = { SenderClientId = fromWho } });
        }
    }

    // Need to stop the cycle on the server
    [ServerRpc(RequireOwnership = false)]
    private void OverrideCycle_ServerRpc(bool swit)
    {
        killSwitch = swit;
    }

    private IEnumerator WaitForServerUpdate(ulong fromWho)
    {
        Debug.Log("Player " + fromWho + " won, now confirming on server");
        while (!waitingComplete)
        {
            yield return new WaitForSeconds(1);
        }
        waitingComplete = false;
        Debug.Log("Player " + fromWho + " win confirmed");
        ReceivePlayerStatusUpdate_ServerRpc(new ServerRpcParams { Receive = { SenderClientId = fromWho } });
    }

    public void FinishWaitingForServerUpdate(ulong idOfPlayer)
    {
        FinishWaiting_ClientRpc(new ClientRpcParams { Send = { TargetClientIds = new ulong[] { idOfPlayer } } });
    }

    [ClientRpc]
    private void FinishWaiting_ClientRpc(ClientRpcParams rpcParams)
    {
        waitingComplete = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReceivePlayerStatusUpdate_ServerRpc(ServerRpcParams rpcParams)
    {
        receivedUpdatesFrom.Add(rpcParams.Receive.SenderClientId);
    }

    /// <summary>
    /// The server tells all players to resume actions
    /// </summary>
    [ClientRpc]
    private void SendPlayerStartPhaseUpdate_ClientRpc(TurnDriverPhase currPhase)
    {
        TurnDriverClient.instance.OnTurnPhaseChange(currPhase);
    }

    public void InitPassiveInfoSystem(int humanAndBotCount, int humanCount)
    {
        passiveInfoOffsets = InitPassiveInfoHelper(humanAndBotCount, humanCount);
    }

    private ulong[] InitPassiveInfoHelper(int humanAndBotCount, int humanCount)
    {
        this.humanCount = humanCount;
        this.humanAndBotCount = humanAndBotCount;
        switch (humanAndBotCount)
        {
            case 2:
                return new ulong[3] { 1, 1, 0 };
            default:
                return new ulong[1] { 0 };
        }
    }
}

public enum TurnDriverPhase
{
    PlayerTurns,
    InvestigationDispatch,
    ServerResponse,
    PassiveInfo
}