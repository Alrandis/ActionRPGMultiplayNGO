using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "LevelGen/Room Data")]
public class RoomData : ScriptableObject
{
    public List<RoomConnection> roomConnections;

    public RoomConnection GetConnectionById(string id)
        => roomConnections.Find(rc => rc.roomPrefab.id == id);

    public RoomConnection GetConnectionByPrefab(GameObject prefab)
        => roomConnections.Find(rc => rc.roomPrefab.prefab == prefab);
}
