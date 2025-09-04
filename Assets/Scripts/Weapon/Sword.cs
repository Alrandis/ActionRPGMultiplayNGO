using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;


public class Sword : NetworkBehaviour, IWeapon
{
    private NetworkVariable<NetworkObjectReference> ownerRef = new();
    [SerializeField] private int damage = 20;
    [SerializeField] private Collider2D bladeCollider;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    private NPCLook npcLook;

    private PlayerLook playerLook;
    private Health playerHealth;
    public int Damage => damage;

    private NetworkObject ownerNetwork;
    private bool isAttacking = false;

    private GameObject owner;
    private Collider2D ownerCollider;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            var controller = animator.runtimeAnimatorController;
            if (controller != null)
            {
                foreach (var clip in controller.animationClips)
                    Debug.Log($"Available animation: {clip.name}");
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (networkAnimator == null)
            networkAnimator = GetComponentInChildren<NetworkAnimator>(); 
        Debug.Log($"{name} | IsServer={IsServer} | IsClient={IsClient} | IsOwner={IsOwner} | OwnerClientId={OwnerClientId}");
       
        // Включаем NetworkAnimator только если он есть
        if (networkAnimator != null)
            networkAnimator.enabled = true;

        ownerRef.OnValueChanged += (oldRef, newRef) =>
        {
            if (newRef.TryGet(out var netObj))
            {
                ownerCollider = netObj.GetComponent<Collider2D>();
                ownerNetwork = netObj.GetComponent<NetworkObject>();
                Debug.Log($"Получил ownerNetwork и ownerCollider");
            }
        };

        if (IsOwner)
        {
            var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (localPlayer != null)
                playerLook = localPlayer.GetComponentInChildren<PlayerLook>();
        }

        if (networkAnimator != null)
            StartCoroutine(PlayIdleNextFrame());

  
    }

    private IEnumerator PlayIdleNextFrame()
    {
        yield return null; // ждём конец текущего кадра
        if (networkAnimator != null)
            Idle(); 
    }

    public void Initialize(GameObject weaponOwner)
    {
        ownerNetwork = weaponOwner.GetComponent<NetworkObject>();
        ownerRef.Value = ownerNetwork;

        playerHealth = weaponOwner.GetComponent<Health>();
        if (playerHealth != null)
            playerHealth.OnDeath += HandleOwnerDeath;

        npcLook = weaponOwner.GetComponent<NPCLook>();
    }

    private void HandleOwnerDeath()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>()?.Despawn();
        }
    }

    public void PrimaryAttack()
    {
        if (!IsOwner || isAttacking) return;

        isAttacking = true;
       

        if (networkAnimator != null && networkAnimator.enabled)
            networkAnimator.SetTrigger("Attack1");


    }

    public void SecondaryAttack()
    {
        if (!IsOwner || isAttacking) return;

        isAttacking = true;
        

        if (networkAnimator != null && networkAnimator.enabled)
            networkAnimator.SetTrigger("Attack2");

    }

    public void Idle()
    {
        if (networkAnimator != null && networkAnimator.enabled)
            networkAnimator.SetTrigger("Idle");

        isAttacking = false;
        
    }

    public void ProcessHit(Collider2D other)
    {
        if (!IsOwner || other == ownerCollider) return;

        var targetNO = other.GetComponentInParent<NetworkObject>();
        if (targetNO != null && ownerNetwork != null && targetNO.NetworkObjectId == ownerNetwork.NetworkObjectId)
            return;

        RequestHitServerRpc(targetNO.NetworkObjectId);
    }

    [ServerRpc]
    private void RequestHitServerRpc(ulong targetNetworkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var target)) return;

        var health = target.GetComponent<Health>();
        if (health != null)
            health.TakeDamage(damage);
    }

    // Вызывается только владельцем оружия
    public void OnAttackAnimationEnd()
    {
        if (!IsOwner) return;

        Idle();
        LockMouse(false);
    }

    public void EnableHitbox()
    {
        if (bladeCollider != null) bladeCollider.enabled = true;
        LockMouse(true);
    }

    public void DisableHitbox()
    {
        if (bladeCollider != null) bladeCollider.enabled = false;
        
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDeath -= HandleOwnerDeath;
    }

    private void LockMouse(bool locked)
    {
        if (npcLook != null)
        {
            npcLook.lockLocked = locked;
            return;
        }

        if (playerLook != null) 
            playerLook.mouseLocked = locked;

    }
}
 