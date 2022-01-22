using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Singleton;
    public ulong numbersOfPlayers = 2;

    private NetworkVariable<ulong> currentPlayerClientId = new NetworkVariable<ulong>();
    private ulong serverOffset = 1;
    void Awake()
    {
        StartSingleton();

        SpawnManager.PlayersSpawned += StartTurn;
        ArrowBehaviour.ArrowCollided += SwitchTurn;
    }

    private void StartTurn(ulong playerOne, ulong playerTwo)
    {
        if (!IsServer || !IsHost) return;
    
        NetworkLog.LogInfoServer("StartTurn for players with clientId=" + playerOne + " and clientId=" + playerTwo);

        NetworkManager.Singleton.ConnectedClients[playerOne]
            .PlayerObject.GetComponent<PlayerController>()
            .SetCanPlay(true);

        currentPlayerClientId.Value = playerOne;
    }

    private void SwitchTurn()
    {
        if (!IsServer || !IsHost) return;

        ulong nextPlayerClientId = GetNextPlayerClientId();
        NetworkLog.LogInfoServer("SwitchTurn " + nextPlayerClientId);

        NetworkManager.Singleton.ConnectedClients[currentPlayerClientId.Value]
            .PlayerObject.GetComponent<PlayerController>()
            .SetCanPlay(false);

        NetworkManager.Singleton.ConnectedClients[nextPlayerClientId]
            .PlayerObject.GetComponent<PlayerController>()
            .SetCanPlay(true);

        currentPlayerClientId.Value = nextPlayerClientId;
    }

    private ulong GetNextPlayerClientId()
    {
        if(IsHost)
        {
            return (currentPlayerClientId.Value + 1) % numbersOfPlayers;
        }
        return serverOffset + currentPlayerClientId.Value % numbersOfPlayers;
    }

    private void StartSingleton()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(this);
        }
    }
}
