using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// The bridge between main menu and game start,
// And manager of the highest-level problems in the game
public class GameManagerSc : MonoBehaviour
{
    public static GameManagerSc instance;
    MainGameParameters gameParameters;

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
        gameParameters = mgp;
        StartCoroutine(SetupTask());
    }

    private IEnumerator SetupTask()
    {
        while (UI_Playerbase.instance == null)
            yield return null;
        UI_Playerbase.instance.redrawPlayerbase(gameParameters.playerSetupInfo);

        //SceneManager.UnloadSceneAsync(0);
        KickOff();
    }

    // When everything has been loaded, begin the game for real
    public void KickOff()
    {
        Debug.Log("Let the game begin.");
    }
}

public struct MainGameParameters
{
    public List<PlayerSetupInfo> playerSetupInfo;
    public ulong rosterSizeZeroes;
    public ushort roundsToWin;
}