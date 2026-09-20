using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharCard16 : CharacterCard
{
    [SerializeField] SpriteRenderer portrait;

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
        drawMat = portrait.material;

        ulong startingSeed = c.drawId;

        // Non-critical - doing these first better randomizes the seed for important values.
        (ulong s, int v) crv1 = CharRandomValue.RangedSeedRandomizer(startingSeed, 0, 3);
        drawMat.SetInt("_BodyIdx", crv1.v);

        // Main portrait
        int w = SetIntFieldAndAdvance(c, CPD_Type.Weight, "_Weight");
        SetIntFieldAndAdvance(c, CPD_Type.Height,    "_Height");
        SetIntFieldAndAdvance(c, CPD_Type.Job,       "_JobIdx", additional: w * 12);
        SetIntFieldAndAdvance(c, CPD_Type.HairStyle, "_HairLength");

        startingSeed = SetColorFieldAndAdvance_R(c, startingSeed, CPD_Type.HairColor, "_HairColor");
        startingSeed = SetColorFieldAndAdvance_R(c, startingSeed, CPD_Type.SkinTone,  "_SkinColor");
        startingSeed = SetColorFieldAndAdvance_R(c, startingSeed, CPD_Type.EyeColor,  "_EyeColor");
        startingSeed = SetColorFieldAndAdvance_R(c, startingSeed, CPD_Type.FavoriteColor, "_BodyColor");

        // Optionals
        if (c.optionalTraits.hasMoustache)
        {
            drawMat.SetVector("_OPT_Stache", new Vector4(1, 0, 0, 0));
        } else drawMat.SetVector("_OPT_Stache", new Vector4(0, 0, 0, 0));
        if (c.optionalTraits.hasBeard)
        {
            drawMat.SetVector("_OPT_Beard", new Vector4(1, 0, 0, 0));
        } else drawMat.SetVector("_OPT_Beard", new Vector4(0, 0, 0, 0));
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
