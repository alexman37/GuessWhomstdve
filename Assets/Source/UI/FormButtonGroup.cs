using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A FormButtonGroup is created for each constrainable CPD in the game.
/// It's made up of FormButtons which all need to communicate with each other in some situations.
/// </summary>
public class FormButtonGroup : MonoBehaviour
{
    public CPD_Type cpdType;
    public Image img;

    public List<FormButton> formButtons;

    private int dependency = -1;
    private bool blocked = false; // if there is a dependency, cannot interact with CPD until dependency solved

    public static event Action<CPD_Type> resetConstraints;

    // TEMPLATE
    public GameObject formObjectComponent;

    private void Start()
    {
        img = GetComponent<Image>();
        resetConstraints = (_) => { };
    }

    private void OnEnable()
    {
        FormButton.groupConfirmed += onOtherGroupConfirmed;
        FormButton.groupUnconfirmed += onOtherGroupUnconfirmed;
    }

    private void OnDisable()
    {
        FormButton.groupConfirmed -= onOtherGroupConfirmed;
        FormButton.groupUnconfirmed -= onOtherGroupUnconfirmed;
    }

    /// <summary>
    /// Create all form objects from the template.
    /// </summary>
    public float buildFormButtonGroup(CPD_Type cpdType, IEnumerable buttonInstructions, float offset, int dependentOn)
    {
        this.cpdType = cpdType;
        formButtons = new List<FormButton>();

        int count = 0;
        float standardHeight = 0;

        CPD cpd = CPD.registry[cpdType];

        foreach (string cat in buttonInstructions)
        {
            GameObject next = GameObject.Instantiate(formObjectComponent, transform);
            next.SetActive(true);
            RectTransform rt = next.GetComponent<RectTransform>();
            standardHeight = rt.rect.height;
            rt.anchoredPosition += new Vector2(0, count * -standardHeight);

            next.name = cat;

            FormButton formButton = next.GetComponent<FormButton>();
            formButton.cpdType = cpdType;
            formButton.category = cat;
            formButton.categoryID = cpd.categoryIndices[cat];
            formButton.title.text = cat;

            formButton.partOfGroup = this;
            formButtons.Add(formButton);

            count++;
        }

        if (dependentOn > -1)
        {
            dependency = dependentOn;
            blockGroup();
        }

        GetComponent<RectTransform>().anchoredPosition += new Vector2(0, offset);

        return count * standardHeight;
    }

    private void onOtherGroupConfirmed(CPD_Type group, int catId)
    {
        if((int) group == dependency)
        {
            unblockGroup(catId);
        }
    }

    private void onOtherGroupUnconfirmed(CPD_Type group)
    {
        Debug.Log(cpdType + ": Group #" + group + " unconfirmed; looking for match with " + dependency);
        if ((int)group == dependency)
        {
            blockGroup();
        }
    }

    // Cannot interact with groups until their dependencies (if they exist) are solved.
    public void blockGroup()
    {
        blocked = true;
        foreach(FormButton fb in formButtons)
        {
            fb.blockButton();
        }
        Debug.Log("BLOCKED " + cpdType);
        UI_Roster.instance.handleReset(cpdType);
    }

    // Cannot interact with groups until their dependencies (if they exist) are solved.
    public void unblockGroup(int dependencyCatId)
    {
        blocked = false;
        foreach (FormButton fb in formButtons)
        {
            fb.unblockButton(dependencyCatId);
        }
    }
}
