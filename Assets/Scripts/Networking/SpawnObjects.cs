using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class SpawnObjects : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform[] spawnPoints;

    /// <summary>
    /// Метод инициализации спавнера, вызывается после того как комната заспавнена и выровнена
    /// </summary>
    public void InitializeSpawner()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        // Находим все дочерние точки спавна, если не заданы вручную
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            spawnPoints = GetComponentsInChildren<Transform>()
                .Where(t => t != transform) // исключаем сам объект комнаты
                .ToArray();
        }

        SpawnNPCs();
    }


    private void SpawnNPCs()
    {
        foreach (Transform point in spawnPoints)
        {
            if (point == null)
                continue;

         

            var pref = Instantiate(prefab, point.position, point.rotation);

            var netObj = pref.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError($"[SpawnObjects] У {pref.name} отсутствует компонент NetworkObject");
                Destroy(pref);
                continue;
            }

            netObj.Spawn();
        }
    }
}
