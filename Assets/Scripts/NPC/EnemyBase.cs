using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Collections;

[RequireComponent(typeof(NPCLook))]
public abstract class EnemyBase : NetworkBehaviour
{

    private Transform _target;
    private NPCLook _npcLook;

    public abstract void Attack();



    public override void OnNetworkSpawn()
    {

        _npcLook = GetComponentInChildren<NPCLook>();
        
        if (IsServer)
        {
            
        }


    }

    public override void OnNetworkDespawn()
    {
       
    }
}
