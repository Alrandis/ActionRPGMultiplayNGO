using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class RoomConnection
{
    public RoomPrefab roomPrefab;
    public List<string> compatibleRoomIDs; // ������ ID ����������� ������
    public bool isCap; // ������-�������� � ����� �������
}
