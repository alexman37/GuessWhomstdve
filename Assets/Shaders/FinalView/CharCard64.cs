using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class CharCard64 : CharacterCard
{
    [SerializeField] SpriteRenderer portraitFrame;
    [SerializeField] SpriteRenderer portrait;
    [SerializeField] SpriteRenderer flag;

    [SerializeField] SpriteRenderer[] simpleIndexBadges;
    [SerializeField] CPD_Type[] simpleIndexOrder;

    [SerializeField] TextMeshProUGUI cityAbbrText;

    // Start is called before the first frame update
    void Start()
    {
    }

    private new void OnEnable()
    {
        base.OnEnable();
    }

    private new void OnDisable()
    {
        base.OnDisable();
    }

    // You will need different versions of this for different LODs...
    public override void SetMaterialParams(Character c)
    {
        cityAbbrText.enabled = false;
        drawMat = portrait.material;

        ulong startingSeed = c.simulatedId;

        // Main portrait
        int w = SetIntFieldAndAdvance(c, CPD_Type.Weight, "_Weight");
        SetIntFieldAndAdvance(c, CPD_Type.Height, "_Height");
        SetIntFieldAndAdvance(c, CPD_Type.Job, "_JobIdx", w * NUM_JOB_PORTRAITS);

        startingSeed = SetGenderedHair_R(c, startingSeed);

        startingSeed = SetColorFieldAndAdvance_R(c, startingSeed, CPD_Type.HairColor, "_HairColor");
        startingSeed = SetColorFieldAndAdvance_R(c, startingSeed, CPD_Type.SkinTone, "_SkinColor");
        startingSeed = SetColorFieldAndAdvance_R(c, startingSeed, CPD_Type.EyeColor, "_EyeColor");
        startingSeed = SetColorFieldAndAdvance_R(c, startingSeed, CPD_Type.FavoriteColor, "_BodyColor");

        // Non-critical
        (ulong s, int v) crv1 = CharRandomValue.RangedSeedRandomizer(startingSeed, 0, 3);
        drawMat.SetInt("_BodyIdx", crv1.v);
        (ulong s, int v) crv2 = CharRandomValue.RangedSeedRandomizer(crv1.s, 0, 8);
        drawMat.SetInt("_HeadIdx", crv2.v);
        (ulong s, int v) crv3 = CharRandomValue.RangedSeedRandomizer(crv2.s, 0, 14);
        drawMat.SetInt("_FaceIdx", crv3.v);
        startingSeed = crv3.s;

        // Locations
        switch (drawMat.GetInt("_LLOD"))
        {
            case 1:
                if(CPD.registry.ContainsKey(CPD_Type.City_L1))
                {
                    int city = c.getCategoryIndexofCharacteristic(CPD_Type.City_L1);
                    drawMat.SetInt("_CityIdx_l1", city);
                }
                break;
            case 2:
                if(CPD.registry.ContainsKey(CPD_Type.Region_L2))
                {
                    string[] locName = c.getVariantNameofCharacteristic(CPD_Type.City_L2).Split('_');

                    string cityAbbr = locName[0].Substring(0, 3).ToUpper();
                    cityAbbrText.enabled = true;
                    cityAbbrText.text = cityAbbr;

                    int flagCode = CountryMap.instance.getCode(locName[1]);
                    flag.material.SetFloat("Ref_MatIndex", flagCode);
                }
                break;
            default:
                break;
        }

        // Simple index mats
        if (simpleIndexBadges.Length == simpleIndexOrder.Length)
        {
            for (int i = 0; i < simpleIndexBadges.Length; i++)
            {
                if(CPD.registry.ContainsKey(simpleIndexOrder[i])) {
                    simpleIndexBadges[i].color = Color.white;
                    simpleIndexBadges[i].material.SetFloat("Ref_MatIndex", c.getCategoryIndexofCharacteristic(simpleIndexOrder[i]));
                } else
                {
                    simpleIndexBadges[i].color = Color.clear;
                }
            }
        }

        // Optionals
        if (c.optionalTraits.hasMoustache)
        {
            (ulong s, int v) opt_moustache = CharRandomValue.RangedSeedRandomizer(startingSeed, 0, 20);
            drawMat.SetVector("_OPT_Stache", new Vector4(1, opt_moustache.v, 0, 0));
            startingSeed = opt_moustache.s;
        } else drawMat.SetVector("_OPT_Stache", new Vector4(0, 0, 0, 0));
        if (c.optionalTraits.hasBeard)
        {
            (ulong s, int v) opt_beard = CharRandomValue.RangedSeedRandomizer(startingSeed, 0, 8);
            drawMat.SetVector("_OPT_Beard", new Vector4(1, opt_beard.v, 0, 0));
            startingSeed = opt_beard.s;
        } else drawMat.SetVector("_OPT_Beard", new Vector4(0, 0, 0, 0));

        // Background
        int backgroundIdx = c.getOneTimeRandomNumber(0, 8);
        drawMat.SetInt("_Background_Idx", backgroundIdx);
    }

    private void OnMouseDown()
    {
        OnClick();
    }

    public override void OnClick()
    {
        base.OnClick();
        Debug.Log("Click " + characterId);
    }
}
