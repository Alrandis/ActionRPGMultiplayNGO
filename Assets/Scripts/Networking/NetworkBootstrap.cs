using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;
using System.Threading.Tasks;

public class NetworkBootstrap : MonoBehaviour
{
    public string lobbyName = "TestLobby";
    private Lobby currentLobby;

    async void Start()
    {
        await InitializeServices();
    }

    private async Task InitializeServices()
    {
        try
        {
            await Unity.Services.Core.UnityServices.InitializeAsync();

            // �������������� (���������)
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Services init failed: " + e);
        }
    }

    // �������� ����� � ������� ����� Relay
    public async void CreateLobbyAndHost()
    {
        try
        {
            // ������� �����
            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2);
            Debug.Log("Lobby created: " + currentLobby.Id);

            // Relay allocation
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Relay JoinCode: " + joinCode);

            // ��������� ���������� Netcode
            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            unityTransport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // ������ �����
            NetworkManager.Singleton.StartHost();
        }
        catch (Exception e)
        {
            Debug.LogError("CreateLobbyAndHost failed: " + e);
        }
    }

    // ����������� � Relay � ������������� � ������������� �����
    public async void JoinLobbyWithCode(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            unityTransport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            // ����������� �������
            NetworkManager.Singleton.StartClient();
            Debug.Log("Joined Relay with code: " + joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError("JoinLobbyWithCode failed: " + e);
        }
    }
}
