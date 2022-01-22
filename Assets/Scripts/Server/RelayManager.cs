using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : NetworkBehaviour
{

    public static RelayManager Singleton;

    private string environment = "production";
    private int maxConnections = 2;

    void Awake()
    {
        StartSingleton();
    }

    public UnityTransport Transport => NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

    public bool IsRelayEnabled => Transport != null && Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

    public async Task<RelayHostData> SetUpRelay()
    {
        InitializeAndSignIn();

        Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxConnections);

        RelayHostData relayHostData = new RelayHostData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            IPv4Address = allocation.RelayServer.IpV4,
            ConnectionData = allocation.ConnectionData
        };

        relayHostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(relayHostData.AllocationID);

        Transport.SetRelayServerData(
            relayHostData.IPv4Address,
            relayHostData.Port, 
            relayHostData.AllocationIDBytes, 
            relayHostData.Key, 
            relayHostData.ConnectionData
        );

        Debug.Log("SetUpRelay Join Code = " + relayHostData.JoinCode);

        return relayHostData;
    }

    public async Task<RelayJoinData> JoinRelay(string joinCode)
    {
        InitializeAndSignIn();

        JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

        RelayJoinData relayHostData = new RelayJoinData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            IPv4Address = allocation.RelayServer.IpV4,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData
        };

        Transport.SetRelayServerData(
            relayHostData.IPv4Address, 
            relayHostData.Port, 
            relayHostData.AllocationIDBytes, 
            relayHostData.Key, 
            relayHostData.ConnectionData, 
            relayHostData.HostConnectionData
        );

        return relayHostData;
    }

    private async void InitializeAndSignIn()
    {
        InitializationOptions inicializationOptions = new InitializationOptions().
            SetEnvironmentName(environment);

        await UnityServices.InitializeAsync(inicializationOptions);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
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

    public struct RelayHostData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] Key;
    }

    public struct RelayJoinData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }
}
