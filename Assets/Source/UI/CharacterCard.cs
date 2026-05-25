using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterCard : MonoBehaviour
{
    public int characterId;

    public static event Action<int> charCardClicked = (_) => { };

    // Start is called before the first frame update
    void Start()
    {
        // TODO: target selection only
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    public void OnClick()
    {
        charCardClicked.Invoke(characterId);
    }
}
