using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    private Weapon sword;
    private Health health;

    private void Awake()
    {
        sword = GetComponentInParent<Weapon>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        health = other.GetComponentInParent<Health>();

        if (health != null)
            sword.ProcessHit(other); // делегируем обработку
    }
}
