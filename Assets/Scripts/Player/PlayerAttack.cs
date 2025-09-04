using UnityEngine;
using Unity.Netcode;

public class PlayerAttack : NetworkBehaviour
{
    private IWeapon currentWeapon;

    /// <summary>
    /// Вызывается PlayerWeaponManager при смене оружия
    /// </summary>
    /// <param name="weapon"></param>
    public void SetWeapon(IWeapon weapon)
    {
        currentWeapon = weapon;
        Debug.Log($"Дал оружие игроку в метот SetWeapon в PlayerAttack. Ориужие это - {currentWeapon}");

        // Встанет в Idle сразу при смене
        currentWeapon?.Idle();
    }

    private void Update()
    {
        if (GameManager.Instance.isWIn) return;
        if (!IsOwner) return;
        if (currentWeapon == null) return;
        // Проверяем, является ли оружие NetworkBehaviour и имеет ли владельца
        if (currentWeapon is NetworkBehaviour networkWeapon && !networkWeapon.IsOwner)
            return;

        // Основная атака - левая кнопка мыши
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Левая кнопка мыши нажата");
            currentWeapon.PrimaryAttack();
        }

        // Альтернативная атака - правая кнопка мыши
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log($"Правая кнопка мыши нажата");
            currentWeapon.SecondaryAttack();
        }
    }
}
