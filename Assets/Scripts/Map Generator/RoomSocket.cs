using System;
using UnityEngine;


[Serializable]
public class RoomSocket : MonoBehaviour
{
    public Transform point;            // Трансформ точки
    public SocketDirection direction;  // С какой стороны этот поинт
    public bool isOccupied = false; // Использован ли сокет


}

public enum SocketDirection
{
    Left,
    Right,
    Top,
    Bottom
}