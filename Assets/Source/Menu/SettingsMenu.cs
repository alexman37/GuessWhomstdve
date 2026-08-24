using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GW.MainMenu
{
    public class SettingsMenu : MonoBehaviour
    {
        // inputs
        [SerializeField] private GameObject inputField;

        // results
        public static PlayerConfigData confData = new PlayerConfigData()
        {
            name = "Anonymous"
        };

        // Start is called before the first frame update
        void Start()
        {

        }

        public void UpdateFields()
        {
            string tp = inputField.GetComponent<TMP_InputField>().text;
            tp = tp.Substring(0, Mathf.Min(tp.Length, 16));

            if (tp != "") confData.name = tp;
            else confData.name = "Anonymous";

            Debug.Log("PLayer name is now " + confData.name);
            MultiplayerSetup.instance.UpdateName(confData.name);
        }
    }

    public struct PlayerConfigData
    {
        public string name;
    }
}