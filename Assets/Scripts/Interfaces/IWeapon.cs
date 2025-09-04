using Unity.Netcode;
using UnityEngine;

public interface IWeapon
{
    int Damage { get; }

    void Initialize(GameObject owner);
    void PrimaryAttack();
    void SecondaryAttack();
    void Idle();
}
