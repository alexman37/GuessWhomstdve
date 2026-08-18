using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class LobbyInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI lobbyHeadcount;
    [SerializeField] private TextMeshProUGUI lobbyPing;

    private string id;

    public void setLobbyInfo(Lobby lobby)
    {
        lobbyName.text = lobby.Name;
        lobbyHeadcount.text = lobby.Players.Count.ToString() + " / " + lobby.MaxPlayers;
        lobbyPing.text = "0 ms"; // TODO
        id = lobby.Id;
    }

    public void JoinLobby()
    {
        Debug.Log("This lobby info represents " + id);
        MultiplayerSetup.instance.JoinLobbyFromClick(id);
    }
}
