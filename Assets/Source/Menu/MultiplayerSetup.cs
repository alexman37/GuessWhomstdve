using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

namespace GW.MainMenu
{
    /// <summary>
    /// For everything related to the player and their interaction with multiplayer services,
    /// Excluding lobbies.
    /// </summary>
    public class MultiplayerSetup : NetworkBehaviour
    {
        public static MultiplayerSetup instance;

        Player me;
        List<PlayerSetupInfo> playerbase = new List<PlayerSetupInfo>();
        private object playerbaseLock = new object();

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
            ConnectToRelay(lobby, true);
            OnCreatedOrJoinedLobby(lobby);
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
                Lobby lob = await Lobbies.Instance.GetLobbyAsync(LobbyManager.instance.partOfLobby.Id);
                LobbyManager.instance.partOfLobby = lob;

                lock (updateLock)
                {
                    for (int i = playerBaseRoot.transform.childCount - 1; i >= 0; i--)
                    {
                        Destroy(playerBaseRoot.transform.GetChild(i).gameObject);
                    }
                    playerbase.Clear();

                    Debug.Log("How many active players? " + LobbyManager.instance.partOfLobby.Players.Count);
                    foreach (Player p in LobbyManager.instance.partOfLobby.Players)
                    {
                        GameObject go = GameObject.Instantiate(playerBaseTemplate, playerBaseRoot.transform);
                        // The host cannot be kicked.
                        go.GetComponent<PlayerInfo>().setPlayerInfo(p, p.Id == LobbyManager.instance.partOfLobby.HostId, IsHost);
                        // The host updates the 'playerbase' list consistently
                        if(activeLobby != null)
                        {
                            lock(playerbaseLock)
                            {
                                playerbase.Add(new PlayerSetupInfo_Human(p.Data["Name"].Value));
                            }
                        }
                    }
                }
            }
            catch (LobbyServiceException e)
            {
                // TODO
                Debug.LogError("Lobby connection issues..." + e.ToString());
            }
        }

        public void EndLobby()
        {
            foreach (var player in activeLobby.Players)
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

                ConnectToRelay(lob, false);
                OnCreatedOrJoinedLobby(lob);
            }
            catch (LobbyServiceException e)
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

                ConnectToRelay(lob, false);
                OnCreatedOrJoinedLobby(lob);
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
                if (lob.HostId == playId)
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

        private async void ConnectToRelay(Lobby lob, bool isHost)
        {
            try
            {
                if (!isHost)
                {
                    Debug.Log("[REL] Relay code found " + lob.Data.ContainsKey("RelayCode") + lob.Data["RelayCode"].Value);
                    JoinAllocation jAlloc = await RelayService.Instance.JoinAllocationAsync(lob.Data["RelayCode"].Value);
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                        jAlloc.RelayServer.IpV4,
                        (ushort)jAlloc.RelayServer.Port,
                        jAlloc.AllocationIdBytes,
                        jAlloc.Key,
                        jAlloc.ConnectionData,
                        jAlloc.HostConnectionData
                    );
                    NetworkManager.Singleton.StartClient();
                }
                else
                {
                    var alloc = await RelayService.Instance.CreateAllocationAsync(7);

                    string relayCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);

                    Lobby newLob = await Lobbies.Instance.UpdateLobbyAsync(lob.Id, new UpdateLobbyOptions()
                    {
                        Data = new Dictionary<string, DataObject>
                        {
                            { "RelayCode", new DataObject(visibility: DataObject.VisibilityOptions.Member, value: relayCode) }
                        }
                    });
                    Debug.Log("[REL] Relay code set to " + newLob.Data["RelayCode"].Value);
                    activeLobby = newLob;
                    LobbyManager.instance.partOfLobby = newLob;

                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                        alloc.RelayServer.IpV4,
                        (ushort)alloc.RelayServer.Port,
                        alloc.AllocationIdBytes,
                        alloc.Key,
                        alloc.ConnectionData
                    );

                    NetworkManager.Singleton.StartHost();
                }
            }
            catch (RelayServiceException e)
            {
                Debug.LogError("Relay startup Error: " + e);
            }
        }

        // Side effects of joining a lobby - these tasks are not time dependent
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

        // - Unload the main menu scene
        // - Load the game scene with necessary data arguments
        // - End the lobby (but keep relay)
        public void StartGame()
        {
            SceneManager.LoadSceneAsync(1);

            // Set Game data
            Debug.Log("Trying to set the game here.");
            GameManagerSc.instance.SetGameParameters(new MainGameParameters
            {
                playerSetupInfo = playerbase,
                rosterSizeZeroes = stubs[0].getRealValue(),
                roundsToWin = (ushort)stubs[1].getRealValue(),
            });

            Lobbies.Instance.DeleteLobbyAsync(lobbyIdCache);
        }

        //[ClientRpc]
        //private void StartGame_ClientRpc(string joinCode, AllocationData alloc)
        //{
            // Use for kicking later
        //}
    }

    public struct GameLobbyData
    {
        public string roomCode6;
        public string hostID;
        public string id;
        public bool priv;
    }
}
public abstract class PlayerSetupInfo
{
    public string name;
    public Sprite img;
}

public class PlayerSetupInfo_Human : PlayerSetupInfo
{
    public int winTotal;

    public PlayerSetupInfo_Human(string n)
    {
        name = n;
    }
}

public class PlayerSetupInfo_Bot : PlayerSetupInfo
{
    public int difficulty;

    public PlayerSetupInfo_Bot(string n)
    {
        name = n;
    }
}