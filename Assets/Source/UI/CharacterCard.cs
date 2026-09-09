using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class CharacterCard : MonoBehaviour
{
    [SerializeField] private SpriteRenderer coloredBorder;
    private bool selected;
    public static bool acceptingInput = true;

    public ulong characterId;

    private static float redrawDelay = 1f;
    private static float flipTime = 0.4f;
    private static float waitTime = 0.4f;

    public static event Action<ulong> charCardClicked = (_) => { };

    protected void OnEnable()
    {
        PlayerTurnProcessor.switchedAction += OnActionSwitch;
        TurnDriverClient.resetInvestigations += ResetAllSelections;
    }

    protected void OnDisable()
    {
        PlayerTurnProcessor.switchedAction -= OnActionSwitch;
        TurnDriverClient.resetInvestigations -= ResetAllSelections;
    }

    public void RedrawInPlace(Character c, float uvCoord)
    {
        StartCoroutine(flipAndRedraw(c, uvCoord * redrawDelay));
    }

    private IEnumerator flipAndRedraw(Character c, float delay)
    {
        yield return new WaitForSeconds(delay);
        for(float i = 0; i < flipTime; i += Time.deltaTime)
        {
            yield return transform.localRotation = Quaternion.Euler(0, (i / flipTime) * -180, 0);
        }
        transform.localRotation = Quaternion.Euler(0, -180, 0);
        for (float i = 0; i < waitTime; i += Time.deltaTime)
        {
            yield return null;
        }
        SetMaterialParams(c);
        for (float i = 0; i < flipTime; i += Time.deltaTime)
        {
            yield return transform.localRotation = Quaternion.Euler(0, (i / flipTime) * -180 - 180, 0);
        }
        transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public virtual void SetMaterialParams(Character c)
    {
        Debug.LogWarning("If this function ever runs, there's a problem");
    }

    public static GridViewStats GetGridViewStats(int lod)
    {
        switch (lod)
        {
            // 32-pixel
            case 1:
                return new GridViewStats
                {
                    charactersToShow = 112,
                    entriesPerRow = 16,
                    startingX = 0,
                    startingY = 0,
                    cardWidth = 1.5f,
                    cardHeight = 1.7f,
                    cardOffsetW = 0.1f,
                    cardOffsetH = 0.05f
                };
            // 16-pixel
            case 2:
                return new GridViewStats
                {
                    charactersToShow = 384,
                    entriesPerRow = 24,
                    startingX = 0,
                    startingY = 0,
                    cardWidth = 0.75f,
                    cardHeight = 0.75f,
                    cardOffsetW = 0.05f,
                    cardOffsetH = 0.05f
                };
            // 64-pixel
            case 0:
            default:
                return new GridViewStats
                {
                    charactersToShow = 40,
                    entriesPerRow = 10,
                    startingX = 0,
                    startingY = 0,
                    cardWidth = 2.4f,
                    cardHeight = 3f,
                    cardOffsetW = 0.24f,
                    cardOffsetH = 0.3f
                };
        }
    }

    public virtual void OnClick()
    {
        if(acceptingInput)
        {
            selected = HumanPlayer.self.addToTargetsList(characterId);

            if (selected)
            {
                PlayerTurnProcessor.instance.ProcessActionChange(PlayerTurnAction.TargetGuess, true);
                charCardClicked.Invoke(characterId);
                coloredBorder.color = Color.cyan;
            }
            else
            {
                PlayerTurnProcessor.instance.ProcessActionChange(PlayerTurnAction.TargetGuess, false);
                coloredBorder.color = Color.gray;
            }
        }
    }

    private void OnActionSwitch(PlayerTurnAction switchToAction)
    {
        if(switchToAction != PlayerTurnAction.TargetGuess)
        {
            ResetAllSelections();
        }
    }

    private void ResetAllSelections()
    {
        coloredBorder.color = Color.gray;
        if (selected)
        {
            HumanPlayer.self.removeFromTargetsList(characterId);
        }
        selected = false;
    }
}

public struct GridViewStats
{
    public uint charactersToShow;
    public int entriesPerRow;
    public float startingX;
    public float startingY;
    public float cardWidth;
    public float cardHeight;
    public float cardOffsetW;
    public float cardOffsetH;
}
