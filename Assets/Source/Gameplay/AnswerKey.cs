using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Handles investigations and the responses players get back.
/// </summary>
public class AnswerKey : NetworkBehaviour
{
    public static AnswerKey instance;

    public int maxGuesses = 3;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
}
