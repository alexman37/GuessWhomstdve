using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class CharCard32 : MonoBehaviour
{
    public ulong characterId;

    [SerializeField] SpriteRenderer portraitFrame;
    [SerializeField] SpriteRenderer portrait;
    [SerializeField] Material drawMat;

    public static event Action<ulong> charCardClicked = (_) => { };

    // Start is called before the first frame update
    void Start()
    {
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    // You will need different versions of this for different LODs...
    public void SetMaterialParams(Character c)
    {
        drawMat = portrait.material;

        ulong startingSeed = c.simulatedId;

        // Main portrait
        (ulong s, int v) crv1 = CharRandomValue.RangedSeedRandomizer(startingSeed, 0, 3);
        drawMat.SetInt("_BodyIdx", crv1.v);
        (ulong s, int v) crv2 = CharRandomValue.RangedSeedRandomizer(crv1.s, 0, 8);
        drawMat.SetInt("_HeadIdx", crv2.v);
        (ulong s, int v) crv3 = CharRandomValue.RangedSeedRandomizer(crv2.s, 0, 14);
        drawMat.SetInt("_FaceIdx", crv3.v);

        int weight = c.getCategoryIndexofCharacteristic(CPD_Type.Weight);
        drawMat.SetInt("_Height", c.getCategoryIndexofCharacteristic(CPD_Type.Height));
        drawMat.SetInt("_Weight", weight);

        drawMat.SetInt("_JobIdx", c.getVariantIndexofCharacteristic(CPD_Type.Job) + (weight * 64));

        int Hairlen = c.getCategoryIndexofCharacteristic(CPD_Type.HairStyle);
        int gender = c.getCategoryIndexofCharacteristic(CPD_Type.Gender);
        drawMat.SetInt("_HairLength", Hairlen);
        (ulong s, int v) crv4 = CharRandomValue.randomHairIndex(crv3.s, Hairlen, gender);
        drawMat.SetInt("_HairIdx", crv4.v);

        (ulong s, Color v) crv5 = c.getColorField(crv4.s, CPD_Type.HairColor);
        drawMat.SetColor("_HairColor", crv5.v);
        (ulong s, Color v) crv6 = c.getColorField(crv5.s, CPD_Type.SkinTone);
        drawMat.SetColor("_SkinColor", crv6.v);
        (ulong s, Color v) crv7 = c.getColorField(crv6.s, CPD_Type.EyeColor);
        drawMat.SetColor("_EyeColor", crv7.v);
        (ulong s, Color v) crv8 = c.getColorField(crv7.s, CPD_Type.FavoriteColor);
        drawMat.SetColor("_BodyColor", crv8.v);

        ulong workingSeed = crv8.s;

        // Optionals
        if (c.optionalTraits.hasMoustache)
        {
            (ulong s, int v) opt_moustache = CharRandomValue.RangedSeedRandomizer(workingSeed, 0, 20);
            drawMat.SetVector("_OPT_Stache", new Vector4(1, opt_moustache.v, 0, 0));
            workingSeed = opt_moustache.s;
        }
        if (c.optionalTraits.hasBeard)
        {
            (ulong s, int v) opt_beard = CharRandomValue.RangedSeedRandomizer(workingSeed, 0, 8);
            drawMat.SetVector("_OPT_Beard", new Vector4(1, opt_beard.v, 0, 0));
            workingSeed = opt_beard.s;
        }

        // Background
        int backgroundIdx = c.getOneTimeRandomNumber(0, 8);
        drawMat.SetInt("_Background_Idx", backgroundIdx);
    }

    private void OnMouseDown()
    {
        OnClick();
    }

    public void OnClick()
    {
        Debug.Log("Click " + characterId);
        charCardClicked.Invoke(characterId);
    }
}
