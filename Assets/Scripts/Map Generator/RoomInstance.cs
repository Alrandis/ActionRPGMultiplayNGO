using System.Collections.Generic;
using UnityEngine;

public class RoomInstance : MonoBehaviour
{
    public string roomId;
    [SerializeField] private List<RoomSocket> sockets;
    public Transform playerSpawnPoint;
    public GameObject statue;

    private void Awake()
    {
        statue.SetActive(false);
    }

    public List<RoomSocket> GetSockets()
    {
        return new List<RoomSocket>(GetComponentsInChildren<RoomSocket>());
    }
}
