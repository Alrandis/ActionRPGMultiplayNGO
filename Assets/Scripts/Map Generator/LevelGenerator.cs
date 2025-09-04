using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class LevelGenerator : NetworkBehaviour
{
    [Header("Room Settings")]
    public RoomData roomDatabase;          // ScriptableObject с RoomConnection
    public int targetRoomCount = 5;        // Минимум обычных комнат до фазы закрытия
    public int maxSameRoomRepeat = 2;      // Максимум одинаковых подряд

    [Header("Player Settings")]
    public GameObject playerPrefab;        // Префаб игрока для спавна

    public readonly List<GameObject> placedRooms = new();
    private List<RoomSocket> openSockets = new();

    public GameObject bossPrefab;   
    private void Awake()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        GenerateClosedHouseWithRepeats();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        // Хост уже заспавнен выше
        if (clientId == NetworkManager.Singleton.LocalClientId) 
        {
            Debug.Log($"Хост уже заспавлен id = {clientId}");
            return;    
        }
        
        if (placedRooms.Count > 0)
        {
            SpawnPlayerInFirstRoom(placedRooms[0], clientId);
            Debug.Log($"Player client spawned id = {clientId}");
        }
            

    }

    private void GenerateClosedHouseWithRepeats()
    {
        placedRooms.Clear();
        openSockets.Clear();

        // 1) Стартовая комната
        var firstConn = roomDatabase.roomConnections[Random.Range(0, roomDatabase.roomConnections.Count)];
        var firstRoom = InstantiateAndSpawn(firstConn.roomPrefab.prefab, Vector3.zero, Quaternion.identity);
        placedRooms.Add(firstRoom);
        firstRoom.GetComponent<RoomInstance>().statue.SetActive(true);

        var firstSockets = firstRoom.GetComponent<RoomInstance>().GetSockets();
        openSockets.AddRange(firstSockets);

        InitializeRoomSpawners(firstRoom);

        // 2) Генерация остальных комнат
        while (openSockets.Any(s => s != null && !s.isOccupied))
        {
            // Фильтруем актуальные свободные сокеты
            openSockets = openSockets.Where(s => s != null && !s.isOccupied).ToList();
            if (openSockets.Count == 0) break;

            var baseSocket = openSockets[0];
            var baseRoom = baseSocket.GetComponentInParent<RoomInstance>();

            bool mustUseCapsOnly = placedRooms.Count >= targetRoomCount;

            // Получаем совместимые комнаты
            var compatIDs = roomDatabase.GetConnectionById(baseRoom.roomId)?.compatibleRoomIDs;
            if (compatIDs == null || compatIDs.Count == 0) break;

            var candidates = compatIDs.Select(id => roomDatabase.GetConnectionById(id))
                                      .Where(rc => rc != null)
                                      .ToList();

            var caps = candidates.Where(c => c.isCap).ToList();
            var normals = candidates.Where(c => !c.isCap).ToList();

            var ordered = mustUseCapsOnly ? new List<RoomConnection>(caps)
                                          : normals.Concat(caps).ToList();

            // Ограничение повторов
            var recentId = placedRooms.Count >= 1 ? placedRooms[^1].GetComponent<RoomInstance>().roomId : null;
            int repeatCount = placedRooms.Skip(Mathf.Max(0, placedRooms.Count - maxSameRoomRepeat))
                                         .Count(r => r.GetComponent<RoomInstance>().roomId == recentId);
            if (repeatCount >= maxSameRoomRepeat)
                ordered.RemoveAll(c => c.roomPrefab.id == recentId);

            // Если нет подходящей комнаты — fallback: берем любую из candidates
            if (ordered.Count == 0) ordered.AddRange(candidates);

            bool placed = false;
            var oppositeDir = GetOppositeDirection(baseSocket.direction);

            foreach (var conn in ordered)
            {
                var newRoom = InstantiateAndSpawn(conn.roomPrefab.prefab);

                var newSockets = newRoom.GetComponent<RoomInstance>().GetSockets();
                var match = newSockets.FirstOrDefault(s => s.direction == oppositeDir && !s.isOccupied);

                if (match == null)
                {
                    Destroy(newRoom);
                    continue;
                }

                AlignRooms(baseSocket, match, newRoom);

                baseSocket.isOccupied = true;
                match.isOccupied = true;

                // Удаляем занятые сокеты из списка
                openSockets.Remove(baseSocket);
                openSockets.Remove(match);

                placedRooms.Add(newRoom);
                placed = true;

                InitializeRoomSpawners(newRoom);

                // Добавляем новые свободные сокеты
                foreach (var s in newSockets)
                    if (s != null && !s.isOccupied && !openSockets.Contains(s))
                        openSockets.Add(s);

                break;
            }

            if (!placed)
            {
                // Если не удалось разместить комнату, просто помечаем сокет как занятый и продолжаем
                baseSocket.isOccupied = true;
                openSockets.Remove(baseSocket);
            }
        }

        SpawnAllPlayers();
        SpawnBossInLastRoom();
    }


    private GameObject InstantiateAndSpawn(GameObject prefab, Vector3 position = default, Quaternion rotation = default)
    {
        var go = Instantiate(prefab, position, rotation);
        var netObj = go.GetComponent<NetworkObject>();
        if (netObj != null && IsServer)
            netObj.Spawn(true);
        return go;
    }

    private void AlignRooms(RoomSocket baseSocket, RoomSocket newSocket, GameObject newRoom)
    {
        Vector3 baseDir = GetDirectionVector(baseSocket.direction);
        Vector3 newDir = GetDirectionVector(newSocket.direction);

        var rotationDiff = Quaternion.FromToRotation(newDir, -baseDir);
        newRoom.transform.rotation = rotationDiff * newRoom.transform.rotation;

        Vector3 offset = baseSocket.transform.position - newSocket.transform.position;
        newRoom.transform.position += offset;
    }

    private SocketDirection GetOppositeDirection(SocketDirection dir) => dir switch
    {
        SocketDirection.Left => SocketDirection.Right,
        SocketDirection.Right => SocketDirection.Left,
        SocketDirection.Top => SocketDirection.Bottom,
        SocketDirection.Bottom => SocketDirection.Top,
        _ => dir
    };

    private Vector3 GetDirectionVector(SocketDirection dir) => dir switch
    {
        SocketDirection.Left => Vector3.left,
        SocketDirection.Right => Vector3.right,
        SocketDirection.Top => Vector3.up,
        SocketDirection.Bottom => Vector3.down,
        _ => Vector3.zero
    };

    public void SpawnPlayerInFirstRoom(GameObject firstRoom, ulong clientId)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab не назначен!");
            return;
        }

        Transform spawnPoint = firstRoom.GetComponent<RoomInstance>().playerSpawnPoint;
        if (spawnPoint == null) spawnPoint = firstRoom.transform;

        var player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        var netObj = player.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.SpawnAsPlayerObject(clientId);
    }

    public void SpawnAllPlayers()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayerInFirstRoom(placedRooms[0], client);
        }
    }

    private void InitializeRoomSpawners(GameObject room)
    {
        var spawners = room.GetComponentsInChildren<SpawnObjects>();
        foreach (var spawner in spawners)
        {
            if (spawner.IsServer)
                spawner.InitializeSpawner();
        }
    }

    private void SpawnBossInLastRoom()
    {
        if (placedRooms.Count == 0) return;
        GameObject lastRoom = placedRooms[placedRooms.Count - 1];

        Transform spawnPoint = lastRoom.GetComponent<RoomInstance>().playerSpawnPoint; // например, смещение в центре
        var boss = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);

        // если сетка
        var netObj = boss.GetComponent<NetworkObject>();
        if (netObj != null && NetworkManager.Singleton.IsServer)
        {
            netObj.Spawn();
        }
    }
}
