using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class RoomPrefab
{
    public string id;                  // Уникальный ID комнаты
    public GameObject prefab;          // Префаб комнаты
    public List<RoomSocket> sockets;   // Точки состыковки
}
