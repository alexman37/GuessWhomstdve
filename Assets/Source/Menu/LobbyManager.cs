using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class LobbyManager : MonoBehaviour
{
    const int maxLobbiesToShow = 50;
    const float refreshCooldown = 3;

    float refreshTicker = 0;
    IEnumerator activeHeartbeat = null;

    // The lobby this player is a part of.
    // Unless they're the host, they do not also own it (MultiplayerSetup.activeLobby)
    public Lobby partOfLobby = null;

    public static LobbyManager instance;

    [SerializeField] private GameObject lobbyInfoTemplate;
    [SerializeField] private GameObject lobbyInfoRoot;

    // Start is called before the first frame update
    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public async void CreateLobby(Player whoCreates)
    {
        Lobby lob = null;
        try
        {
            lob = await LobbyService.Instance.CreateLobbyAsync("TestLobby", 8, new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = whoCreates
            });

            Debug.Log("Created lobby with ID " + lob.Id + ", Code " + lob.LobbyCode + ", Name " + lob.Name);
            activeHeartbeat = LobbyHeartbeat(lob);
            StartCoroutine(activeHeartbeat);
            MultiplayerSetup.instance.LobbySetup(lob);

            partOfLobby = lob;
        }
        catch(LobbyServiceException e)
        {
            // TODO give the player an error msg
            Debug.LogError(e);
        }
    }

    public async void DestroyLobby(string id)
    {
        StopCoroutine(activeHeartbeat);
        await LobbyService.Instance.DeleteLobbyAsync(id);
        partOfLobby = null;
    }

    /// <summary>
    /// Not necessary to query this consistently. Call only on refreshes.
    /// </summary>
    public async void GetAllActiveLobbies(bool force)
    {
        if(Application.isPlaying)
        {
            if(force || refreshTicker >= refreshCooldown)
            {
                QueryResponse qr = await Lobbies.Instance.QueryLobbiesAsync();

                Debug.Log("After querying, found " + qr.Results.Count + " lobbies in existence");

                // Clear existing lobbies in list
                for (int i = lobbyInfoRoot.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(lobbyInfoRoot.transform.GetChild(i).gameObject);
                }

                int count = 0;
                foreach (Lobby l in qr.Results)
                {
                    Debug.Log("Found lobby " + l.ToString() + " w code " + l.LobbyCode);
                    GameObject go = GameObject.Instantiate(lobbyInfoTemplate, lobbyInfoRoot.transform);
                    go.GetComponent<LobbyInfo>().setLobbyInfo(l);
                    count++;
                    if (count > maxLobbiesToShow) break;
                }

                refreshTicker = 0;
            } else
            {
                Debug.Log("Did not refresh list. Cooldown");
            }
        }
    }

    private IEnumerator LobbyHeartbeat(Lobby lob)
    {
        while (Application.isPlaying)
        {
            if (lob == null) StopCoroutine(activeHeartbeat);

            yield return LobbyService.Instance.SendHeartbeatPingAsync(lob.Id);

            yield return new WaitForSeconds(15);
        }
    }

    private void Update()
    {
        if(refreshTicker < refreshCooldown)
        {
            refreshTicker += Time.deltaTime;
        }
    }
}
