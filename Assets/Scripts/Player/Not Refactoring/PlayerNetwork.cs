using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(PlayerController))]
public class PlayerNetwork : NetworkBehaviour
{
    PlayerController controller;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            controller.enabled = false; // ”правл€ет только владелиц
        }
    }
}
