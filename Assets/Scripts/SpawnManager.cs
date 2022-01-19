using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Singleton;
    public static event Action<ulong, ulong> PlayersSpawned;
    public float minDistanceFromOrigin = 0f;
    public float maxDistanceFromOrigin = 20f;
    public float verticalPosition = -1.5f;

    private NetworkVariable<bool> firstSetUp = new NetworkVariable<bool>(true);
    private NetworkVariable<bool> playerOneIsSetted = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> playerTwoIsSetted = new NetworkVariable<bool>(false);
    private NetworkVariable<ulong> playerOneId = new NetworkVariable<ulong>((ulong)0);
    private NetworkVariable<ulong> playerTwoId = new NetworkVariable<ulong>((ulong)0);
    private NetworkVariable<Vector2> playerOnePosition = new NetworkVariable<Vector2>(Vector2.zero);
    private NetworkVariable<Vector2> playerTwoPosition = new NetworkVariable<Vector2>(Vector2.zero);
    private NetworkVariable<Vector2> playerOneScale = new NetworkVariable<Vector2>(Vector2.one);
    private NetworkVariable<Vector2> playerTwoScale = new NetworkVariable<Vector2>(Vector2.one);

    void Awake()
    {
        StartSingleton();

        NetworkManager.Singleton.OnServerStarted += SetupSpawner;
        NetworkManager.Singleton.OnClientConnectedCallback += SetupPlayer;
        NetworkManager.Singleton.OnClientDisconnectCallback += ResetPlayer;
    }

    void Start()
    {   
    }

    private void SetupPlayer(ulong id)
    {
        if (!IsServer || !IsHost) return;

        NetworkLog.LogInfoServer("SetupPlayer with cliendId=" + id);

        if (!playerOneIsSetted.Value)
        {
            playerOneId.Value = id;
            playerOneIsSetted.Value = true;
        }
        else if (!playerTwoIsSetted.Value)
        {
            playerTwoId.Value = id;
            playerTwoIsSetted.Value = true;
        }

        if (playerOneId.Value == id)
        {
            SetPlayerPositionAndScale(id, playerOnePosition.Value, playerOneScale.Value);
        }
        else if (playerTwoId.Value == id)
        {
            SetPlayerPositionAndScale(id, playerTwoPosition.Value, playerTwoScale.Value);
        }

        if (playerOneIsSetted.Value && playerTwoIsSetted.Value && firstSetUp.Value)
        {
            firstSetUp.Value = false;
            PlayersSpawned?.Invoke(playerOneId.Value, playerTwoId.Value);
        }
    }

    private void ResetPlayer(ulong id)
    {
        if (!IsServer || !IsHost) return;

        NetworkLog.LogInfoServer("ResetPlayer with cliendId=" + id);

        if (playerOneId.Value == id)
        {
            playerOneId.Value = 0;
            playerOneIsSetted.Value = false;
        }
        else if (playerTwoId.Value == id)
        {
            playerTwoId.Value = 0;
            playerTwoIsSetted.Value = false;
        }
    }

    private void SetupSpawner()
    {
        CalculateSpawnPosition();

        ResetForHost();
    }

    private void CalculateSpawnPosition()
    {
        if (!IsServer || !IsHost) return;

        float xDistance = UnityEngine.Random.Range(minDistanceFromOrigin, maxDistanceFromOrigin);

        NetworkLog.LogInfoServer("CalculateSpawnPosition xDistance=" + xDistance);

        playerOnePosition.Value = new Vector2(-xDistance, verticalPosition);
        playerTwoPosition.Value = new Vector2(xDistance, verticalPosition);
        playerOneScale.Value = Vector2.one;
        playerTwoScale.Value = Vector2.one; // new Vector2(-1, 1);
    }

    private void SetPlayerPositionAndScale(ulong clientId, Vector2 position, Vector2 scale)
    {
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.position = position;
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.localScale = scale;
    }

    private void StartSingleton()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void ResetForHost()
    {
        if (IsHost && playerOneIsSetted.Value)
        {
            ResetPlayer(0);
            SetupPlayer(0);
        }
    }
}
