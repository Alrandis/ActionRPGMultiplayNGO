using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.InputSystem.HID;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject quitButton;

    public bool isWIn = false;
    private void Awake()
    {
        Instance = this;
    }

    // Проверка поражения
    public void CheckGameOver()
    {
        if (!IsServer) return;

        bool allPlayersDead = FindObjectsOfType<Health>()
            .Where(h => h.CompareTag("Player"))
            .All(h => h.isDead.Value);

        if (allPlayersDead)
            RpcGameOverClientRpc();
    }

    [ClientRpc]
    private void RpcGameOverClientRpc()
    {
        bool isHost = NetworkManager.Singleton.IsHost;
        GameOverUI.Instance.Show(isHost);
    }

    // Вызывается хостом с кнопки "Рестарт"
    [ServerRpc(RequireOwnership = false)]
    public void RespawnAllPlayersServerRpc()
    {
        if (!IsServer) return;

        var levelGen = FindObjectOfType<LevelGenerator>();
        if (levelGen == null || levelGen.placedRooms.Count == 0)
        {
            Debug.LogError("LevelGenerator не найден или нет сгенерированных комнат!");
            return;
        }

        var firstRoom = levelGen.placedRooms[0];
        var spawnPoint = firstRoom.GetComponent<RoomInstance>().playerSpawnPoint;
        if (spawnPoint == null) spawnPoint = firstRoom.transform;

        foreach (var health in FindObjectsOfType<Health>().Where(h => h.CompareTag("Player")))
        {
            var playerObj = health.gameObject;
            health.isDead.Value = false;
            health.currentHealth.Value = health.maxHealth;
            health.ReviveClientRpc();

            var pcFollow = playerObj.GetComponent<PlayerCameraFollow>();
            if (pcFollow != null)
                pcFollow.OnRespawnClientRpc(); // работает, если pcFollow на том же NetworkObject

            // Перемещаем объект — NetworkTransform синхронизирует на клиентах
            playerObj.transform.position = spawnPoint.position;
            playerObj.transform.rotation = spawnPoint.rotation;

        }

        RpcHideGameOverUIClientRpc();
    }

    [ClientRpc]
    private void RpcHideGameOverUIClientRpc()
    {
        if (GameOverUI.Instance != null)
            GameOverUI.Instance.Hide();
    }

    public void QuitGame()
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsHost)
        {
            // Хост: завершает сессию для всех
            NetworkManager.Singleton.Shutdown();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else
        {
            // Клиент: сначала отключаем локальные скрипты, HUD и камеру
            var player = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (player != null)
            {
                foreach (var comp in player.GetComponents<MonoBehaviour>())
                    comp.enabled = false;

                var cameraFollow = player.GetComponent<PlayerCameraFollow>();
                cameraFollow?.HideSpectatorHint();

                // Деактивируем камеру
                var cam = player.GetComponentInChildren<Camera>();
                if (cam != null)
                    cam.gameObject.SetActive(false);
            }

            // Удаляем объект игрока у всех клиентов
            if (player != null)
            {
                var netObj = player.GetComponent<NetworkObject>();
                if (netObj != null && NetworkManager.Singleton.IsServer)
                {
                    netObj.Despawn(); // если сервер
                }
                else if (netObj != null)
                {
                    // если клиент, посылаем серверу запрос на Despawn
                    DespawnRequestServerRpc(netObj.NetworkObjectId);
                }
            }

            // Завершаем соединение клиента
            NetworkManager.Singleton.Shutdown();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnRequestServerRpc(ulong networkObjectId, ServerRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
        {
            netObj.Despawn();
        }
    }

    public void OnBossDefeated()
    {
        if (!IsServer) return;

        isWIn = true;
        // рассылаем клиентам показать победный экран
        ShowVictoryClientRpc();
    }

    [ClientRpc]
    private void ShowVictoryClientRpc()
    {
        victoryUI.SetActive(true);
        quitButton.SetActive(false);
    }
}
