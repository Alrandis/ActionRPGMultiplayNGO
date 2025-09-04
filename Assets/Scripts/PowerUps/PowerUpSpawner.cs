using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PowerUpSpawner : NetworkBehaviour
{
    public GameObject poweUpPrefab;
    public Transform[] points;

    public override void OnNetworkSpawn() 
    {
        if (IsServer)
        {
            // Находим все дочерние точки спавна, если не заданы вручную
            if (points == null || points.Length == 0)
            {
                points = GetComponentsInChildren<Transform>()
                    .Where(t => t != transform) // исключаем сам объект комнаты
                    .ToArray();
            }

            foreach (Transform point in points)
            {
                SpawnPowerUp(point.position);

            }
        }
    }

    public void SpawnPowerUp(Vector2 position)
    {
        GameObject powerUp = Instantiate(poweUpPrefab, position, Quaternion.identity);
        powerUp.GetComponent<NetworkObject>().Spawn();
    }
}
