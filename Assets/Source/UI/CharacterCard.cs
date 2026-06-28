using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterCard : MonoBehaviour
{
    public int characterId;

    [SerializeField] SpriteRenderer portraitFrame;
    [SerializeField] SpriteRenderer portrait;
    [SerializeField] Material drawMat;

    public static event Action<int> charCardClicked = (_) => { };

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

        drawMat.SetInt("_BodyIdx", UnityEngine.Random.Range(0, 3));
        drawMat.SetInt("_HeadIdx", UnityEngine.Random.Range(0, 8));
        drawMat.SetInt("_FaceIdx", UnityEngine.Random.Range(0, 14));

        int Hairlen = c.getCategoryIndexofCharacteristic(CPD_Type.HairStyle);
        drawMat.SetInt("_HairLength", Hairlen);
        drawMat.SetInt("_HairIdx", CharRandomValue.randomHairIndex(Hairlen));

        drawMat.SetColor("_HairColor", c.getColorField(CPD_Type.HairColor));
        drawMat.SetColor("_SkinColor", c.getColorField(CPD_Type.SkinTone));
        // TODO favorite color / shirt color
        drawMat.SetColor("_BodyColor", new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1));
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
