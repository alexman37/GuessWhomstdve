using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

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
        currIndex.Value = startIndex;
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
        UpdateDisplay();
    }

    // Should be called when network variable is updated
    public void UpdateDisplay()
    {
        Debug.Log("RPC CALL");
        display.text = availableValues[currIndex.Value];
    }

    public ulong getRealValue()
    {
        return actualValues[currIndex.Value];
    }
}
