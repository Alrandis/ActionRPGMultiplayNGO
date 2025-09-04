using System;
using UnityEngine;


[Serializable]
public class RoomSocket : MonoBehaviour
{
    public Transform point;            // ��������� �����
    public SocketDirection direction;  // � ����� ������� ���� �����
    public bool isOccupied = false; // ����������� �� �����


}

public enum SocketDirection
{
    Left,
    Right,
    Top,
    Bottom
}