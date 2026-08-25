using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Netcode;

// The bridge between main menu and game start,
// And manager of the highest-level problems in the game
public class GameManagerSc : NetworkBehaviour
{
    public static GameManagerSc instance;
    NetworkVariable<MainGameParameters> gameParameters = new NetworkVariable<MainGameParameters>(value: new MainGameParameters {
        playerSetupInfo = new PlayerSetupInfo[8],
        rosterSizeZeroes = 3,
        roundsToWin = 1
    });

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        DontDestroyOnLoad(this.gameObject);
    }

    // Set up game parameters established in the main menu, and wait for all components to be set up
    public void SetGameParameters(MainGameParameters mgp)
    {
        gameParameters.Value = mgp;
        ShipAndSetup_ClientRpc();
    }

    // Ship all players off to the next scene and begin the game setup task for each player
    [ClientRpc]
    private void ShipAndSetup_ClientRpc()
    {
        Debug.Log("Begin setup task for the player " + OwnerClientId);
        StartCoroutine(SetupTask());
    }
    
    private IEnumerator SetupTask()
    {
        Debug.Log("In co " + OwnerClientId);
        SceneManager.LoadSceneAsync(1);

        while (UI_Playerbase.instance == null)
            yield return null;
        UI_Playerbase.instance.redrawPlayerbase(gameParameters.Value.playerSetupInfo);

        //SceneManager.UnloadSceneAsync(0);
        KickOff();
    }

    // When everything has been loaded, begin the game for real
    public void KickOff()
    {
        Debug.Log("Let the game begin.");
    }
}

public struct MainGameParameters : INetworkSerializable
{
    public PlayerSetupInfo[] playerSetupInfo;
    public ulong rosterSizeZeroes;
    public ushort roundsToWin;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref rosterSizeZeroes);
        serializer.SerializeValue(ref roundsToWin);
        serializer.SerializeValue(ref playerSetupInfo);
    }
}