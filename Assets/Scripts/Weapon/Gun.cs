//using Unity.Netcode;
//using UnityEngine;

//public class Gun : NetworkBehaviour, IWeapon
//{
//    [Header("Stats")]
//    [SerializeField] private int damage = 8;
//    [SerializeField] private float attackRange = 15f;
//    [SerializeField] private float cooldown = 0.3f;
//    [SerializeField] private int clipSize = 12;
//    [SerializeField] private float reloadTime = 1.2f;

//    [Header("References")]
//    [SerializeField] private Transform firePoint;
//    [SerializeField] private GameObject bulletPrefab;
//    [SerializeField] private Animator animator;

//    private float _lastShotTime;
//    private int _ammoInClip;
//    private bool _reloading;

//    public int Damage => damage;
//    public float AttackRange => attackRange;
//    public float Cooldown => cooldown;

//    private void Awake()
//    {
//        _ammoInClip = clipSize;
//    }

//    public void PrimaryAttack(NetworkBehaviour owner)
//    {
//        if (_reloading) return;
//        if (Time.time - _lastShotTime < cooldown) return;
//        if (_ammoInClip <= 0)
//        {
//            Reload(owner);
//            return;
//        }

//        _lastShotTime = Time.time;
//        _ammoInClip--;

//        if (animator != null) animator.SetTrigger("Shoot");

//        if (owner.IsOwner)
//        {
//            ShootServerRpc(owner.OwnerClientId, firePoint.position, firePoint.right);
//        }
//    }

//    public void SecondaryAttack(NetworkBehaviour owner)
//    {
//        // Например, альт. огонь = очередь из 3 пуль
//        if (_reloading) return;
//        if (_ammoInClip < 3)
//        {
//            Reload(owner);
//            return;
//        }

//        for (int i = 0; i < 3; i++)
//        {
//            PrimaryAttack(owner);
//        }
//    }

//    private void Reload(NetworkBehaviour owner)
//    {
//        if (_reloading) return;
//        _reloading = true;
//        if (animator != null) animator.SetTrigger("Reload");

//        Invoke(nameof(FinishReload), reloadTime);
//    }

//    private void FinishReload()
//    {
//        _ammoInClip = clipSize;
//        _reloading = false;
//    }

//    [ServerRpc]
//    private void ShootServerRpc(ulong attackerId, Vector3 spawnPos, Vector3 direction)
//    {
//        var bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
//        var bullet = bulletObj.GetComponent<Projectile>();
//        bullet.Init(direction, Damage, attackerId);
//        bulletObj.GetComponent<NetworkObject>().Spawn(true);
//    }
//}
