using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

namespace GW.MainMenu
{
    public class PlayerInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerName;
        // TODO picture
        [SerializeField] private GameObject kickButton;

        private Player player;

        public void setPlayerInfo(Player player, bool playerIsHost, bool requestorIsHost)
        {
            this.player = player;
            Debug.Log("A player with id " + player.Id);
            playerName.text = player.Data["Name"].Value;
            // Only the host can kick players, but the host cannot kick themselves
            if (requestorIsHost && !playerIsHost)
            {
                kickButton.SetActive(true);
                kickButton.GetComponent<Button>().onClick.AddListener(() => {
                    kickThisPlayer();
                });
            }
        }

        public void kickThisPlayer()
        {

        }
    }
}