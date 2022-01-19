using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class NetworkStartManager : MonoBehaviour
{
    public TMP_InputField code;

    
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public async void StartClient()
    {
        if (RelayManager.Singleton.IsRelayEnabled && !string.IsNullOrEmpty(code.text))
        {
            await RelayManager.Singleton.JoinRelay(code.text);
        }
        NetworkManager.Singleton.StartClient();
    }

    public async void StartHost()
    {
        if (RelayManager.Singleton.IsRelayEnabled)
        {
            await RelayManager.Singleton.SetUpRelay();
        }

        NetworkManager.Singleton.StartHost();
    }
}
