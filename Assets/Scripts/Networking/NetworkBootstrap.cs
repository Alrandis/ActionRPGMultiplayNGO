using UnityEngine;
using UnityEngine.UI;
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
    public static NetworkBootstrap Instance { get; private set; }

    [Header("UI")]
    public Text joinCodeText;

    [Header("Lobby Settings")]
    public string lobbyName = "TestLobby";
    public int maxPlayers = 10;

    private string currentJoinCode;
    private Lobby currentLobby;
    private bool isHost = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitializeServices();
    }

    private async Task InitializeServices()
    {
        try
        {
            await Unity.Services.Core.UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Services init failed: " + e);
        }
    }

    // -------------------
    // Хостинг
    // -------------------
    public async Task<string> CreateLobbyAndHost()
    {
        if (currentJoinCode != null)
            return currentJoinCode; // Уже есть код

        isHost = true;

        try
        {
            // 1) Создаем Lobby
            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            // 2) Создаем Relay allocation
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            currentJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // 3) Настраиваем транспорт Netcode
            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            unityTransport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // 4) Старт хоста
            NetworkManager.Singleton.StartHost();

            // 5) Вывод join code на UI
            if (joinCodeText != null)
                joinCodeText.text = $"Join Code: {currentJoinCode}";

            return currentJoinCode;
        }
        catch (Exception e)
        {
            Debug.LogError("CreateLobbyAndHost failed: " + e);
            throw;
        }
    }

    // -------------------
    // Присоединение клиента
    // -------------------
    public async Task JoinLobbyWithCode(string joinCode)
    {
        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogError("Join code пустой!");
            return;
        }

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

            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError("JoinLobbyWithCode failed: " + e);
        }
    }
}
