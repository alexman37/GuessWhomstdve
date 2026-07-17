using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharCard16 : MonoBehaviour
{
    public ulong characterId;

    [SerializeField] SpriteRenderer portrait;
    [SerializeField] Material drawMat;

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

        int weight = c.getCategoryIndexofCharacteristic(CPD_Type.Weight);
        drawMat.SetInt("_Height", c.getCategoryIndexofCharacteristic(CPD_Type.Height));
        drawMat.SetInt("_Weight", weight);

        drawMat.SetInt("_JobIdx", c.getVariantIndexofCharacteristic(CPD_Type.Job) + (weight * 64));

        int Hairlen = c.getCategoryIndexofCharacteristic(CPD_Type.HairStyle);
        drawMat.SetInt("_HairLength", Hairlen);

        (ulong s, Color v) crv2 = c.getColorField(crv1.s, CPD_Type.HairColor);
        drawMat.SetColor("_HairColor", crv2.v);
        (ulong s, Color v) crv3 = c.getColorField(crv2.s, CPD_Type.SkinTone);
        drawMat.SetColor("_SkinColor", crv3.v);
        (ulong s, Color v) crv4 = c.getColorField(crv3.s, CPD_Type.EyeColor);
        drawMat.SetColor("_EyeColor", crv4.v);
        (ulong s, Color v) crv5 = c.getColorField(crv4.s, CPD_Type.FavoriteColor);
        drawMat.SetColor("_BodyColor", crv5.v);

        ulong workingSeed = crv5.s;

        // Optionals
        if (c.optionalTraits.hasMoustache)
        {
            drawMat.SetVector("_OPT_Stache", new Vector4(1, 0, 0, 0));
        }
        if (c.optionalTraits.hasBeard)
        {
            drawMat.SetVector("_OPT_Beard", new Vector4(1, 0, 0, 0));
        }
    }
}
