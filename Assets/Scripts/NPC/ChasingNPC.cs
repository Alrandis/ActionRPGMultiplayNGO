using UnityEngine;
using Unity.Netcode;
using Unity.Collections;


[RequireComponent(typeof(PlayerSearch))]
public class ChasingNPC : NetworkBehaviour
{
    [SerializeField] private float speed = 1.5f;

    private PlayerSearch _playerSearch;
    private NPCLook _npcLook;
    private NPCMovement npcMovement;

    private Transform _target;
    public override void OnNetworkSpawn()
    {
        _playerSearch = GetComponent<PlayerSearch>();

    }


    private void Update()
    {

    }

    
}
