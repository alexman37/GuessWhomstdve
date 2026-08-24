using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

namespace GW.MainMenu
{
    public class MenuItemAdjustor : NetworkBehaviour
    {
        [SerializeField] private string[] availableValues;
        [SerializeField] private ulong[] actualValues;

        [SerializeField] private int startIndex;
        public NetworkVariable<int> currIndex = new NetworkVariable<int>(0);

        [SerializeField] private TextMeshProUGUI display;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsHost)
            {
                currIndex.Value = startIndex;
            }

            display.text = availableValues[currIndex.Value].ToString();
            currIndex.OnValueChanged += OnNetworkVarChanged_ClientRpc;
        }

        public void MoveUp()
        {
            if (IsHost)
            {
                currIndex.Value = (int)Mathf.Min(currIndex.Value + 1, availableValues.Length - 1);
                Debug.Log("Host increases value to " + currIndex.Value);
            }
        }

        public void MoveDown()
        {
            if (IsHost)
            {
                currIndex.Value = (int)Mathf.Max(currIndex.Value - 1, 0);
            }
        }

        [ClientRpc]
        public void OnNetworkVarChanged_ClientRpc(int prev, int curr)
        {
            UpdateDisplay(curr);
        }

        // Should be called when network variable is updated
        private void UpdateDisplay(int newVal)
        {
            display.text = availableValues[newVal];
        }

        public ulong getRealValue()
        {
            return actualValues[currIndex.Value];
        }
    }
}