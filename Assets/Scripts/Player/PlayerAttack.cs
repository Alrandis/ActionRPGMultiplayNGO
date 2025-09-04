using UnityEngine;
using Unity.Netcode;

public class PlayerAttack : NetworkBehaviour
{
    private IWeapon currentWeapon;

    /// <summary>
    /// ���������� PlayerWeaponManager ��� ����� ������
    /// </summary>
    /// <param name="weapon"></param>
    public void SetWeapon(IWeapon weapon)
    {
        currentWeapon = weapon;
        Debug.Log($"��� ������ ������ � ����� SetWeapon � PlayerAttack. ������� ��� - {currentWeapon}");

        // ������� � Idle ����� ��� �����
        currentWeapon?.Idle();
    }

    private void Update()
    {
        if (GameManager.Instance.isWIn) return;
        if (!IsOwner) return;
        if (currentWeapon == null) return;
        // ���������, �������� �� ������ NetworkBehaviour � ����� �� ���������
        if (currentWeapon is NetworkBehaviour networkWeapon && !networkWeapon.IsOwner)
            return;

        // �������� ����� - ����� ������ ����
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"����� ������ ���� ������");
            currentWeapon.PrimaryAttack();
        }

        // �������������� ����� - ������ ������ ����
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log($"������ ������ ���� ������");
            currentWeapon.SecondaryAttack();
        }
    }
}
