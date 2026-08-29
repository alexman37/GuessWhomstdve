using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Handles investigations and the responses players get back.
/// Also handles aspects of the roster that need to be on server b/c they're common to all players
/// </summary>
public class AnswerKey : NetworkBehaviour
{
    public static AnswerKey instance;
    public static bool readyToUse = false;

    public int maxGuesses = 3;

    private NetworkVariable<ulong> answerId = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
    private Character answerChar;

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer) Debug.Log("Answer key, coming in from the SERVER");
        else Debug.Log("Answer key, coming in from the CLIENT");

        readyToUse = true;
    }

    public void SetAnswerKey(ulong simulatedRosterSize)
    {
        SetAnswerKey_ServerRpc(simulatedRosterSize);
    }

    [ServerRpc]
    public void SetAnswerKey_ServerRpc(ulong simulatedRosterSize)
    {
        // TODO !!! - This may not encompass all possible values. Stitch one together with two strings maybe?
        ulong targetId = (ulong)UnityEngine.Random.Range(0, simulatedRosterSize - 1);
        answerId.Value = targetId;
        answerChar = new Character(-1, targetId);

        Debug.Log("Set TARGET on server to id " + targetId);
    }

    ///
    /// TARGET MATCHING
    /// 

    /// <summary>
    /// Given ONE target, see if they're a match or not.
    /// </summary>
    public void targetIdMatchOne(ulong characterId, ulong requestorId)
    {
        targetIdMatchOne_ServerRpc(characterId, new ServerRpcParams { Receive = { SenderClientId = requestorId } });
    }

    [ClientRpc]
    private void targetIdMatchOne_ClientRpc(bool matched, ClientRpcParams rpcParams)
    {
        HumanPlayer.self.guessTarget_Receive(matched);
    }

    [ServerRpc(RequireOwnership = false)]
    private void targetIdMatchOne_ServerRpc(ulong characterId, ServerRpcParams rpcParams)
    {
        targetIdMatchOne_ClientRpc(characterId == answerId.Value, new ClientRpcParams
        {
            Send =
            {
                TargetClientIds = new ulong[] {rpcParams.Receive.SenderClientId}
            }
        });
    }

    /// <summary>
    /// Given MULTIPLE targets, see if any of them are a match or not.
    /// </summary>
    public void targetIdMatchAny(ulong[] characterIds, ulong requestorId)
    {
        targetIdMatchAny_ServerRpc(characterIds, new ServerRpcParams { Receive = { SenderClientId = requestorId } });
    }

    [ClientRpc]
    private void targetIdMatchAny_ClientRpc(bool matched, ClientRpcParams rpcParams)
    {
        HumanPlayer.self.guessTarget_Receive(matched);
    }

    [ServerRpc(RequireOwnership = false)]
    private void targetIdMatchAny_ServerRpc(ulong[] characterIds, ServerRpcParams rpcParams)
    {
        bool anyMatched = false;
        foreach (ulong ul in characterIds)
        {
            if(ul == answerId.Value)
            {
                anyMatched = true;
                break;
            }
        }
        targetIdMatchAny_ClientRpc(anyMatched, new ClientRpcParams
        {
            Send =
            {
                TargetClientIds = new ulong[] {rpcParams.Receive.SenderClientId}
            }
        });
    }

    ///
    /// INVESTIGATIONS
    /// 

    /// <summary>
    /// A player submits a request to the server, asking for info about some number of categories
    /// </summary>
    public void processInvestigation(HashSet<(CPD_Type cpdType, string cat)> questions, ulong requestorId)
    {
        NetCpdCategory[] processedQs = new NetCpdCategory[questions.Count];
        int count = 0;
        foreach((CPD_Type cpdType, string cat) quest in questions)
        {
            processedQs[count] = new NetCpdCategory {
                cpdType = quest.cpdType,
                catIndex = CPD.registry[quest.cpdType].categoryIndices[quest.cat]
            };
            Debug.Log("Investigating trait " + quest.cat + " in " + quest.cpdType + ": " + processedQs[count].catIndex);
            count++;
        }
        processInvestigation_ServerRpc(processedQs, new ServerRpcParams { Receive = { SenderClientId = requestorId } });
    }


    [ServerRpc(RequireOwnership = false)]
    private void processInvestigation_ServerRpc(NetCpdCategory[] questions, ServerRpcParams rpcParams)
    {
        int count = 0;
        for(int q = 0; q < questions.Length; q++)
        {
            var quest = questions[q];
            Debug.Log("Check if " + answerChar.getCategoryIndexofCharacteristic(quest.cpdType) + " match " + quest.catIndex);
            if (answerChar.getCategoryIndexofCharacteristic(quest.cpdType) == quest.catIndex)
            {
                count++;
            }
        }
        processInvestigation_ClientRpc(count, new ClientRpcParams
        {
            Send =
            {
                TargetClientIds = new ulong[] {rpcParams.Receive.SenderClientId}
            }
        });
    }

    [ClientRpc]
    private void processInvestigation_ClientRpc(int count, ClientRpcParams rpcParams)
    {
        HumanPlayer.self.investigation_Receive(count);
    }
}

public struct NetCpdCategory : INetworkSerializable
{
    public CPD_Type cpdType;
    public int catIndex;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref cpdType);
        serializer.SerializeValue(ref catIndex);
    }
}