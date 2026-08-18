using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    // TODO picture

    public void setPlayerInfo(Player player)
    {
        Debug.Log("A player with id " + player.Id);
        playerName.text = player.Data["Name"].Value;
    }
}
