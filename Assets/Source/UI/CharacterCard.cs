using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class CharacterCard : MonoBehaviour
{
    [SerializeField] private SpriteRenderer coloredBorder;
    [SerializeField] protected Material drawMat;

    public const int NUM_JOB_PORTRAITS = 64;

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
        if(!float.IsNaN(uvCoord))
            StartCoroutine(flipAndRedraw(c, uvCoord * redrawDelay));
        else
            StartCoroutine(flipAndRedraw(c, 0));
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
        Debug.Log(c.simulatedId + " has eye color " + c.getVariantNameofCharacteristic(CPD_Type.EyeColor));
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


    // Helper methods for setting parameter values
    protected int SetIntFieldAndAdvance(Character c, CPD_Type cpdType, string propName, int additional = 0)
    {
        if (CPD.registry.ContainsKey(cpdType))
        {
            int v = c.getCategoryIndexofCharacteristic(cpdType) + additional;
            drawMat.SetInt(propName, v);
            return v;
        }
        else return -1;
    }

    protected ulong SetColorFieldAndAdvance_R(Character c, ulong inputSeed, CPD_Type cpdType, string propName)
    {
        if (CPD.registry.ContainsKey(cpdType))
        {
            (ulong s, Color v) crv = c.getColorField(inputSeed, cpdType);
            drawMat.SetColor(propName, crv.v);
            return crv.s;
        }
        else return inputSeed;
    }

    // Hairstyle can depend on what gender the character is (if defined). Otherwise just assume male...
    protected ulong SetGenderedHair_R(Character c, ulong inputSeed)
    {
        bool hasHairlen = CPD.registry.ContainsKey(CPD_Type.HairStyle);
        bool hasGender  = CPD.registry.ContainsKey(CPD_Type.Gender);
        // Case 1: Both hair len and gender used
        if (hasHairlen && hasGender)
        {
            int Hairlen = c.getCategoryIndexofCharacteristic(CPD_Type.HairStyle);
            int gender = c.getCategoryIndexofCharacteristic(CPD_Type.Gender);
            drawMat.SetInt("_HairLength", Hairlen);
            (ulong s, int v) crv = CharRandomValue.randomHairIndex(inputSeed, Hairlen, gender);
            drawMat.SetInt("_HairIdx", crv.v);
            return crv.s;
        }
        // Case 2: Just hair len used, assume male
        else if (hasHairlen)
        {
            int Hairlen = c.getCategoryIndexofCharacteristic(CPD_Type.HairStyle);
            int gender = 0;
            drawMat.SetInt("_HairLength", Hairlen);
            (ulong s, int v) crv = CharRandomValue.randomHairIndex(inputSeed, Hairlen, gender);
            drawMat.SetInt("_HairIdx", crv.v);
            return crv.s;
        }
        // Case 3: Just gender used, assume default hairstyle
        // Case 4: Neither, do nothing
        else return inputSeed;
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
