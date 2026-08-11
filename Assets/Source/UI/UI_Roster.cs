using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Roster: This handles drawing character cards in the roster menu.
/// </summary>
public class UI_Roster : MonoBehaviour
{
    // How many characters should we display at a time?
    public const uint MAX_CHARACTERS_TO_SHOW = 512;
    public uint currCharactersToShow = 32;
    public ushort rosterLOD = 2;
    private ulong[] lodCutoffs = new ulong[]
    {
        1000, 1000000
    };

    public static UI_Roster instance;

    private Roster roster;

    // Common Mode: Show roster based on information everyone knows
    // Filtered Mode: Show only characters meeting your constraints in clue form
    private bool inCommonMode = true;
    [SerializeField] private Image commonButton;
    [SerializeField] private Image filteredButton;

    // Roster cards are sprites now, so canvas terms not used anymore
    public GameObject[] characterCardTemplate;
    public TextMeshProUGUI suspectsRemaining;

    private GameObject container;
    private GameObject[] createdCards;
    [SerializeField] private GameObject rosterFormContainer;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        createdCards = new GameObject[MAX_CHARACTERS_TO_SHOW];
    }

    private void OnEnable()
    {
        RosterGen.rosterCreationDone += setRoster;
        FormButton.updatedConstraint += handleUpdatedConstraint;
        FormButton.reinitializeConstraints += handleDeconfirmed;
        Roster.rosterReady += rosterFormCreation;
    }

    private void OnDisable()
    {
        RosterGen.rosterCreationDone -= setRoster;
        FormButton.updatedConstraint -= handleUpdatedConstraint;
        FormButton.reinitializeConstraints -= handleDeconfirmed;
        Roster.rosterReady -= rosterFormCreation;
    }

    void createContainer()
    {
        Destroy(container);

        container = new GameObject();
        container.name = "RosterContainer";

        // TODO set position and such
        container.transform.position = new Vector3(-15.5f, 3.5f, 0);
    }

    void setRoster(Roster rost)
    {
        roster = rost;

        // assume this also means we want to generate cards
        generateAllCharCards(rosterLOD, true);
    }

    public void enableCommonMode()
    {
        inCommonMode = true;
        commonButton.color = Color.yellow;
        filteredButton.color = Color.gray;
        roster.setCommonConstraints(true);
    }

    public void enableFilteredMode()
    {
        inCommonMode = false;
        commonButton.color = Color.gray;
        filteredButton.color = Color.yellow;
        roster.setCommonConstraints(false);
    }

    /// <summary>
    /// Show or hide the roster window
    /// </summary>
    public void toggleRosterWindow()
    {
        bool newVal = gameObject.activeInHierarchy;
        if (newVal == true) generateAllCharCards(rosterLOD, false);
        else
        {
            Destroy(container);
            container = null;
            createContainer();
        }

        gameObject.SetActive(newVal);
    }

    /// <summary>
    /// Change the display at the top of the roster to show a new number
    /// </summary>
    public void updateRosterCount(ulong newCount)
    {
        bool foundCutoff = false;
        ushort newLOD = 999;
        for(ushort c = 0; !foundCutoff && c < lodCutoffs.Length; c++)
        {
            if (newCount < lodCutoffs[c])
            {
                newLOD = c;
                foundCutoff = true;
            }
        }
        if (!foundCutoff)
        {
            newLOD = (ushort)lodCutoffs.Length;
        }
        if(newLOD != rosterLOD)
        {
            generateAllCharCards(newLOD, newLOD < rosterLOD);
            // regenerate will run later. Just need to do this to re-create the container.
        }
        rosterLOD = newLOD;
        suspectsRemaining.text = commafy(newCount) + " Suspects Remaining";
    }

    private string commafy(ulong num)
    {
        string copy = num.ToString();
        if (copy.Length < 4) return copy;

        string temp = copy;
        int count = 0;
        int firstCommaOffset = copy.Length % 3;
        if(firstCommaOffset > 0)
        {
            temp = temp.Substring(0, firstCommaOffset) + "," + temp.Substring(firstCommaOffset);
            count++;
        }
        for(int i = firstCommaOffset + 1; i < copy.Length; i++)
        {
            if((i - firstCommaOffset) % 3 == 0)
            {
                temp = temp.Substring(0, i + count) + "," + temp.Substring(i + count);
                count++;
            }
        }
        return temp;
    }

    /// <summary>
    /// Generate all character cards for the first time
    /// </summary>
    public void generateAllCharCards(int lod, bool downsizing)
    {
        Debug.Log("Main gen");
        createContainer();

        GridViewStats GVS = CharacterCard.GetGridViewStats(lod);
        currCharactersToShow = GVS.charactersToShow;

        uint maxCharactersCanBeDrawn = downsizing ? currCharactersToShow : (uint)roster.shownRoster.Count;
        for (int i = 0; i < maxCharactersCanBeDrawn; i++)
        {
            Character c = roster.shownRoster[i];

            //instantiate card in correct position
            GameObject newCard = GameObject.Instantiate(characterCardTemplate[lod]);
            newCard.transform.SetParent(container.transform);
            newCard.transform.localPosition = new Vector3(
                GVS.startingX + Mathf.Floor(i % GVS.entriesPerRow) * (GVS.cardWidth + GVS.cardOffsetW),
                GVS.startingY - Mathf.Floor(i / GVS.entriesPerRow) * (GVS.cardHeight + GVS.cardOffsetH), 0);
            newCard.gameObject.SetActive(true);

            CharacterCard charCard = newCard.gameObject.GetComponent<CharacterCard>();
            charCard.characterId = c.simulatedId;
            charCard.SetMaterialParams(c);

            //set portrait and name
            if(lod <= 1)
            {
                newCard.GetComponentInChildren<TextMeshProUGUI>().text = c.getDisplayName(true) + "\n (" + roster.shownRoster[i].simulatedId + ")";
            }

            createdCards[i] = newCard.gameObject;
        }

        // The only case these don't match up is when you're "upsizing", drawing more characters than were previously known.
        // Just buy time. Use empty / default portraits, regenerate will be called afterwards and fill them all in.
        for (uint i = maxCharactersCanBeDrawn; i < currCharactersToShow; i++)
        {
            GameObject newCard = GameObject.Instantiate(characterCardTemplate[lod]);
            newCard.transform.SetParent(container.transform);
            newCard.transform.localPosition = new Vector3(
                GVS.startingX + Mathf.Floor(i % GVS.entriesPerRow) * (GVS.cardWidth + GVS.cardOffsetW),
                GVS.startingY - Mathf.Floor(i / GVS.entriesPerRow) * (GVS.cardHeight + GVS.cardOffsetH), 0);
            newCard.gameObject.SetActive(true);

            createdCards[i] = newCard.gameObject;
        }
    }

    // With updated character positions completed by Roster, redraw the whole visual
    public void regenerateCharCards(ulong newNumber, int lod, HashSet<int> replaceIndices)
    {
        GridViewStats GVS = CharacterCard.GetGridViewStats(lod);

        Debug.Log("Re gen");

        if (roster != null)
        {
            int numPortraits = (int)Mathf.Min(newNumber, currCharactersToShow);
            float entriesPerColumn = numPortraits / GVS.entriesPerRow;
            for (int i = 0; i < numPortraits; i++)
            {
                if (!replaceIndices.Contains(i))
                    continue;

                createdCards[i].SetActive(true);
                Character c = roster.shownRoster[i];

                //instantiate card in correct position
                GameObject newCard = createdCards[i];

                // character card -> other char card types
                CharacterCard charCard = newCard.GetComponent<CharacterCard>();
                charCard.characterId = c.simulatedId;
                
                float uvCoord = ((float)(i % GVS.entriesPerRow) / (float)(GVS.entriesPerRow - 1.0f)) +
                    ((float)(int)(i / GVS.entriesPerRow) / entriesPerColumn);
                charCard.RedrawInPlace(c, uvCoord / 2.0f);

                //set portrait and name
                if (lod <= 1)
                    newCard.GetComponentInChildren<TextMeshProUGUI>().text = c.getDisplayName(true) + "\n (" + roster.shownRoster[i].simulatedId + ")";
            }
            for (int i = numPortraits; i < currCharactersToShow; i++)
            {
                createdCards[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// When roster is finished defining all CPDs, we create the RosterForm here
    /// </summary>
    private void rosterFormCreation()
    {
        rosterFormContainer.GetComponent<RosterForm>().enabled = true;
    }

    /// <summary>
    /// When a new constraint is updated from the roster form, we need to relay it to the Roster object
    /// </summary>
    private void handleUpdatedConstraint(CPD_Type cpdType, string value, FormButtonState newState)
    {
        switch (newState)
        {
            case FormButtonState.Unknown:
                // TODO PlayerSelf
                TurnDriver.instance.playersInOrder[0].rosterConstraints.removeConstraint(cpdType, value);
                break;
            case FormButtonState.Eliminated:
                // TODO PlayerSelf
                TurnDriver.instance.playersInOrder[0].rosterConstraints.addConstraint(cpdType, value, false);
                break;
            case FormButtonState.Confirmed:
                // TODO PlayerSelf
                TurnDriver.instance.playersInOrder[0].rosterConstraints.onlyConstraint(cpdType, value);
                break;
        }

        roster.redrawRosterVis();
    }

    /// <summary>
    /// When a roster object is "deconfirmed" we'll reset its constraints list (and then repopulate it with old ones if needed.)
    /// </summary>
    private void handleDeconfirmed(CPD_Type cpdType, List<string> exclude)
    {
        roster.reInitializeVariants(cpdType, exclude);
    }

    public void handleReset(CPD_Type cpdType)
    {
        TurnDriver.instance.playersInOrder[0].rosterConstraints.clearConstraints(cpdType, true);
    }
}
