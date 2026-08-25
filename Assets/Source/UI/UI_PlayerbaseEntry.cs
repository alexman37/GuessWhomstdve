using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_PlayerbaseEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetParams(PlayerSetupInfo psi)
    {
        title.text = psi.name.ToString();
    }
}
