using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class MultiplayerSetup : MonoBehaviour
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

    private IEnumerator lobbyWaitingCo;

    // Start is called before the first frame update
    async void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        // Create profile
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        PlayerDataObject pdoName = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, SettingsMenu.confData.name);

        me = new Player(id: AuthenticationService.Instance.PlayerId, data: new Dictionary<string, PlayerDataObject> {
            { "Name", pdoName }
        });
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
        LeaveLobby(LobbyManager.instance.partOfLobby, me.Id);
    }

    public void LobbySetup(Lobby lobby)
    {
        activeLobby = lobby;
        OnCreatedOrJoinedLobby(lobby);
        NetworkManager.Singleton.StartHost();
    }

    public IEnumerator UpdateLobbyInfo()
    {
        // TODO need more formal check
        while(true)
        {
            for (int i = playerBaseRoot.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(playerBaseRoot.transform.GetChild(i).gameObject);
            }

            Debug.Log("How many active players? " + LobbyManager.instance.partOfLobby.Players.Count);
            foreach (Player p in LobbyManager.instance.partOfLobby.Players)
            {
                GameObject go = GameObject.Instantiate(playerBaseTemplate, playerBaseRoot.transform);
                go.GetComponent<PlayerInfo>().setPlayerInfo(p);
            }

            yield return new WaitForSeconds(3);
        }
    }

    public void EndLobby()
    {
        foreach(var player in activeLobby.Players)
        {
            //NetworkManager.Singleton.DisconnectClient();
        }
        activeLobby = null;
        LobbyManager.instance.DestroyLobby(activeLobby.Id);
        OnLeftLobby();
    }

    public async void JoinLobbyFromClick(string id)
    {
        try
        {
            // Rejoin lobby if you've already joined it
            Lobby lob;
            /*if (Lobbies.Instance.GetLobbyAsync(id).Result.Players.Find(p => p.Id == me.Id) != null)
            {
                lob = await Lobbies.Instance.ReconnectToLobbyAsync(id);
            }
            // Else, joining for first time
            else
            {
            }*/
            lob = await Lobbies.Instance.JoinLobbyByIdAsync(id, new JoinLobbyByIdOptions()
            {
                Player = me
            });
            NetworkManager.Singleton.StartClient();

            OnCreatedOrJoinedLobby(lob);
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

    private async void LeaveLobby(Lobby lob, string playId)
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

    private void OnCreatedOrJoinedLobby(Lobby lob)
    {
        LobbyManager.instance.partOfLobby = lob;
        lobbyWaitingCo = UpdateLobbyInfo();
        StartCoroutine(lobbyWaitingCo);

        panel1.SetActive(false);
        panel2.SetActive(true);
        roomCodeTxt.text = lob.LobbyCode;
    }

    private void OnLeftLobby()
    {
        StopCoroutine(lobbyWaitingCo);
        panel1.SetActive(true);
        panel2.SetActive(false);
        LobbyManager.instance.GetAllActiveLobbies(true);
        LobbyManager.instance.partOfLobby = null;
    }
}

public struct GameLobbyData
{
    public string roomCode6;
    public string hostID;
    public string id;
    public bool priv;
}