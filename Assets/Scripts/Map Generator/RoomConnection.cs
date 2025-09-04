using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class RoomConnection
{
    public RoomPrefab roomPrefab;
    public List<string> compatibleRoomIDs; // Список ID совместимых комнат
    public bool isCap; // Префаб-заглушка с одним сокетом
}
