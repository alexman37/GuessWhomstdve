using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

/// <summary>
/// For everything related to the player and their interaction with multiplayer services,
/// Excluding lobbies.
/// </summary>
public class MultiplayerSetup : NetworkBehaviour
{
    public static MultiplayerSetup instance;

    Player me;

    [SerializeField] private GameObject panel1;
    [SerializeField] private GameObject panel2;

    [SerializeField] private GameObject playerBaseTemplate;
    [SerializeField] private GameObject playerBaseRoot;

    // Things to modify in the setup screen
    [SerializeField] private TextMeshProUGUI roomCodeTxt;

    [SerializeField] private MenuItemAdjustor[] stubs;

    // The one lobby this player owns (if any).
    // They would also be a part of it (LobbyManager.partOfLobby)
    Unity.Services.Lobbies.Models.Lobby activeLobby;

    private string lobbyIdCache;

    private object updateLock = new object();

    private async void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        PlayerDataObject pdoName = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, SettingsMenu.confData.name);
        me = new Player(id: AuthenticationService.Instance.PlayerId, data: new Dictionary<string, PlayerDataObject> {
            { "Name", pdoName }
        });

        NetworkManager.Singleton.OnClientConnectedCallback += onClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += onClientDisconnected;
    }

    public void UpdateName(string to)
    {
        me.Data["Name"].Value = to;
    }

    public void StartNewLobby()
    {
        LobbyManager.instance.CreateLobby(me);
    }

    public void LeaveLobby()
    {
        NetworkManager.Singleton.Shutdown();
        LeaveLobbyHelper(LobbyManager.instance.partOfLobby, me.Id);
    }

    public void LobbySetup(Lobby lobby)
    {
        activeLobby = lobby;
        OnCreatedOrJoinedLobby(lobby);
        NetworkManager.Singleton.StartHost();
    }

    public void onClientConnected(ulong newPlayerId)
    {
        Debug.Log("Client connect: " + newPlayerId);
        UpdateLobbyInfo();
    }

    public void onClientDisconnected(ulong dcPlayerId)
    {
        Debug.Log("Client disconnect: " + dcPlayerId);
        UpdateLobbyInfo();
    }

    private async void UpdateLobbyInfo()
    {
        // If the async method does not complete in time, it's no big deal since it will just update itself the next cycle.
        try
        {
            if(lobbyIdCache != null)
            {
                // TODO why does this throw (seemingly harmless) errors sometimes?
                Lobby lob = await Lobbies.Instance.GetLobbyAsync(lobbyIdCache);
                LobbyManager.instance.partOfLobby = lob;

                lock (updateLock)
                {
                    for (int i = playerBaseRoot.transform.childCount - 1; i >= 0; i--)
                    {
                        Destroy(playerBaseRoot.transform.GetChild(i).gameObject);
                    }

                    Debug.Log("How many active players? " + LobbyManager.instance.partOfLobby.Players.Count);
                    foreach (Player p in LobbyManager.instance.partOfLobby.Players)
                    {
                        GameObject go = GameObject.Instantiate(playerBaseTemplate, playerBaseRoot.transform);
                        // The host cannot be kicked.
                        go.GetComponent<PlayerInfo>().setPlayerInfo(p, p.Id == LobbyManager.instance.partOfLobby.HostId, IsHost);
                    }
                }
            } else
            {
                Debug.LogWarning("Didn't update lobby screen; the lobby info could not be fetched yet. Try again soon");
            }
        }
        catch (System.Exception e)
        {
            // TODO
            Debug.LogError("Lobby connection issues..." + e.ToString());
        }
    }

    public void EndLobby()
    {
        foreach(var player in activeLobby.Players)
        {
            // TODO Lobby Shutdown
            //NetworkManager.Singleton.DisconnectClient(player)
        }
        activeLobby = null;
        LobbyManager.instance.DestroyLobby(activeLobby.Id);
        OnLeftLobby();
    }

    public async void JoinLobbyFromClick(string id)
    {
        try
        {
            Lobby lob = await Lobbies.Instance.JoinLobbyByIdAsync(id, new JoinLobbyByIdOptions()
            {
                Player = me
            });

            OnCreatedOrJoinedLobby(lob);
            NetworkManager.Singleton.StartClient();
        } catch(LobbyServiceException e)
        {
            Debug.LogError("Could not join lobby: " + e);
        }
    }

    public async void JoinLobbyFromCode(string code)
    {
        try
        {
            Lobby lob = await Lobbies.Instance.JoinLobbyByCodeAsync(code, new JoinLobbyByCodeOptions()
            {
                Player = me
            });

            OnCreatedOrJoinedLobby(lob);
            NetworkManager.Singleton.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Could not join lobby: " + e);
        }
    }

    private async void LeaveLobbyHelper(Lobby lob, string playId)
    {
        try
        {
            // Host left: destroy lobby
            if(lob.HostId == playId)
            {
                await Lobbies.Instance.DeleteLobbyAsync(lob.Id);
            }
            // Other player left: disconnect them but keep lobby intact
            else
            {
                await Lobbies.Instance.RemovePlayerAsync(lob.Id, playId);
            }

            OnLeftLobby();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Could not join lobby: " + e);
        }
    }

    public async void KickThisPlayer(string kickedId)
    {
        await Lobbies.Instance.RemovePlayerAsync(lobbyIdCache, kickedId);
        // TODO Only the kicked player should be disconnected
    }

    private void OnCreatedOrJoinedLobby(Lobby lob)
    {
        LobbyManager.instance.partOfLobby = lob;
        lobbyIdCache = lob.Id;

        panel1.SetActive(false);
        panel2.SetActive(true);
        roomCodeTxt.text = lob.LobbyCode;

        UpdateLobbyInfo();
    }

    private void OnLeftLobby()
    {
        panel1.SetActive(true);
        panel2.SetActive(false);
        LobbyManager.instance.GetAllActiveLobbies(true);
        LobbyManager.instance.partOfLobby = null;
        lobbyIdCache = null;
    }
}

public struct GameLobbyData
{
    public string roomCode6;
    public string hostID;
    public string id;
    public bool priv;
}