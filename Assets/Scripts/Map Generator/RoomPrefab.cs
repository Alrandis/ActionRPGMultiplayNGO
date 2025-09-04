using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class RoomPrefab
{
    public string id;                  // ���������� ID �������
    public GameObject prefab;          // ������ �������
    public List<RoomSocket> sockets;   // ����� ����������
}
