using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

/// <summary>
/// Handles creation of a Roster. Most important trait is the "Roster Seed Offset", which is consistent across the network
/// </summary>
public class RosterGen : NetworkBehaviour
{
    public static RosterGen instance;

    public int numberOfCharacters;
    Roster roster;

    private bool readyToUse = true;

    public static event Action<Roster> rosterCreationDone;

    // Roster offset. Would rather have these in roster itself, but since it needs to be a network variable, it's convenient to have here
    const ulong TOTAL_ROSTER_PERMUTATIONS = 999999; // How many different rosters can there be?
    public NetworkVariable<ulong> rosterOffset = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer) Debug.Log("Answer key, coming in from the SERVER");
        else Debug.Log("Answer key, coming in from the CLIENT");

        readyToUse = true;
    }

    [ServerRpc]
    private void SetRosterOffset_ServerRpc(ushort sizeBracket)
    {
        rosterOffset.Value = (ulong)UnityEngine.Random.Range(0, TOTAL_ROSTER_PERMUTATIONS);
        createRoster_ClientRpc(sizeBracket);
    }

    public void createRoster(ushort sizeBracket)
    {
        Debug.Log("[Y] In RosterGen");
        if (NetworkManager.IsHost)
        {
            SetRosterOffset_ServerRpc(sizeBracket);
        }
    }

    [ClientRpc]
    private void createRoster_ClientRpc(ushort sizeBracket)
    {
        StartCoroutine(createRosterWhenReady(sizeBracket));
    }

    private IEnumerator createRosterWhenReady(ushort sizeBracket)
    {
        while(!readyToUse || instance == null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("[Y] Called roster creation thingy: " + (instance != null));
        roster = new Roster(sizeBracket);

        rosterCreationDone += (_) => { };

        rosterCreationDone.Invoke(roster);
    }
}
