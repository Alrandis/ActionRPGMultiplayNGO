using UnityEngine;
using Unity.Netcode;
using System;

public class Projectile : NetworkBehaviour
{
    public float speed = 5f;
    public int _damage = 20;
    private Vector2 _direction;

    [SerializeField] private float lifetime = 2f;
    private ulong _attackerId;
    public void Init(Vector3 dir, int damage, ulong attackerId)
    {
        _direction = dir.normalized;
        _damage = damage;
        _attackerId = attackerId;

        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        transform.Translate(_direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (!IsSpawned) return;

        if (collision.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(_damage);
        }

        NetworkObject.Despawn();
    }
}
