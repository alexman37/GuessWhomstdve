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
    public const uint CHARACTERS_TO_SHOW = 32;

    public static UI_Roster instance;

    private Roster roster;

    // Common Mode: Show roster based on information everyone knows
    // Filtered Mode: Show only characters meeting your constraints in clue form
    private bool inCommonMode = true;
    [SerializeField] private Image commonButton;
    [SerializeField] private Image filteredButton;

    // Roster cards are sprites now, so canvas terms not used anymore
    public GameObject characterCardTemplate;
    public TextMeshProUGUI suspectsRemaining;

    private GameObject container;
    private GameObject[] createdCards;
    [SerializeField] private GameObject rosterFormContainer;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        createdCards = new GameObject[CHARACTERS_TO_SHOW];
    }

    private void OnEnable()
    {
        RosterGen.rosterCreationDone += setRoster;
        Roster.constrainedResult += updateRosterCount;
        FormButton.updatedConstraint += handleUpdatedConstraint;
        FormButton.reinitializeConstraints += handleDeconfirmed;
        Roster.rosterReady += rosterFormCreation;
    }

    private void OnDisable()
    {
        RosterGen.rosterCreationDone -= setRoster;
        Roster.constrainedResult -= updateRosterCount;
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
        container.transform.position = new Vector3(-14, 3.5f, 0);
    }

    void setRoster(Roster rost)
    {
        roster = rost;

        // assume this also means we want to generate cards
        generateAllCharCards();
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
        if (newVal == true) generateAllCharCards();
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
    public void generateAllCharCards()
    {
        createContainer();

        int entriesPerRow = 8;
        float startingX = 0;
        float startingY = 0;
        float cardWidth = 2.4f;
        float cardHeight = 3.2f;
        float cardOffsetW = cardWidth / 10f;
        float cardOffsetH = cardHeight / 10f;


        for (int i = 0; i < CHARACTERS_TO_SHOW; i++)
        {
            Character c = roster.shownRoster[i];

            //instantiate card in correct position
            GameObject newCard = GameObject.Instantiate(characterCardTemplate);
            newCard.transform.SetParent(container.transform);
            newCard.transform.localPosition = new Vector3(
                startingX + Mathf.Floor(i % entriesPerRow) * (cardWidth + cardOffsetW), 
                startingY - Mathf.Floor(i / entriesPerRow) * (cardHeight + cardOffsetH), 0);
            newCard.gameObject.SetActive(true);

            // character card -> other character card types
            CharacterCard charCard = newCard.gameObject.GetComponent<CharacterCard>();
            charCard.characterId = c.simulatedId;
            charCard.SetMaterialParams(c);

            //roster.shownRosterSprites[i].name = i.ToString();

            // TODO roster.shownRosterSprites
            

            //set portrait and name
            newCard.GetComponentInChildren<TextMeshProUGUI>().text = c.getDisplayName(true) + "\n (" + roster.shownRoster[i].simulatedId + ")";

            createdCards[i] = newCard.gameObject;
        }
    }

    // TODO - fancier animations for this - one day.
    public void regenerateCharCards(ulong newNumber)
    {
        if(roster != null)
        {
            int numPortraits = (int)Mathf.Min(newNumber, CHARACTERS_TO_SHOW);
            for (int i = 0; i < numPortraits; i++)
            {
                createdCards[i].SetActive(true);
                Character c = roster.shownRoster[i];

                //instantiate card in correct position
                GameObject newCard = createdCards[i];

                //roster.shownRosterSprites[i].name = i.ToString();

                // TODO roster.shownRosterSprites

                // character card -> other char card types
                CharacterCard charCard = newCard.GetComponent<CharacterCard>();
                charCard.characterId = c.simulatedId;
                charCard.SetMaterialParams(c);

                //set portrait and name
                newCard.GetComponentInChildren<TextMeshProUGUI>().text = c.getDisplayName(true) + "\n (" + roster.shownRoster[i].simulatedId + ")";
            }
            for(int i = numPortraits; i < CHARACTERS_TO_SHOW; i++)
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
