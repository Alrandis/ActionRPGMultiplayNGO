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

    // �������� ���������
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

    // ���������� ������ � ������ "�������"
    [ServerRpc(RequireOwnership = false)]
    public void RespawnAllPlayersServerRpc()
    {
        if (!IsServer) return;

        var levelGen = FindObjectOfType<LevelGenerator>();
        if (levelGen == null || levelGen.placedRooms.Count == 0)
        {
            Debug.LogError("LevelGenerator �� ������ ��� ��� ��������������� ������!");
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
                pcFollow.OnRespawnClientRpc(); // ��������, ���� pcFollow �� ��� �� NetworkObject

            // ���������� ������ � NetworkTransform �������������� �� ��������
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
            // ����: ��������� ������ ��� ����
            NetworkManager.Singleton.Shutdown();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else
        {
            // ������: ������� ��������� ��������� �������, HUD � ������
            var player = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (player != null)
            {
                foreach (var comp in player.GetComponents<MonoBehaviour>())
                    comp.enabled = false;

                var cameraFollow = player.GetComponent<PlayerCameraFollow>();
                cameraFollow?.HideSpectatorHint();

                // ������������ ������
                var cam = player.GetComponentInChildren<Camera>();
                if (cam != null)
                    cam.gameObject.SetActive(false);
            }

            // ������� ������ ������ � ���� ��������
            if (player != null)
            {
                var netObj = player.GetComponent<NetworkObject>();
                if (netObj != null && NetworkManager.Singleton.IsServer)
                {
                    netObj.Despawn(); // ���� ������
                }
                else if (netObj != null)
                {
                    // ���� ������, �������� ������� ������ �� Despawn
                    DespawnRequestServerRpc(netObj.NetworkObjectId);
                }
            }

            // ��������� ���������� �������
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
        // ��������� �������� �������� �������� �����
        ShowVictoryClientRpc();
    }

    [ClientRpc]
    private void ShowVictoryClientRpc()
    {
        victoryUI.SetActive(true);
        quitButton.SetActive(false);
    }
}
