using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavButton : MonoBehaviour
{
    [SerializeField] private GameObject currPanel;
    [SerializeField] private GameObject nextPanel;

    public void switchPanel()
    {
        currPanel.SetActive(false);
        nextPanel.SetActive(true);
    }
}
