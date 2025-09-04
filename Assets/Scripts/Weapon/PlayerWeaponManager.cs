using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerWeaponManager : NetworkBehaviour
{
    [SerializeField] private Transform weaponHolder; // ������ ������ � ����� ������ (��������� ������ � playerLook)
    [SerializeField] private List<GameObject> weaponPrefabs; // ������ �������� ������

    private NetworkObject currentWeaponNO;
    private IWeapon currentWeapon;

    [SerializeField] private PlayerAttack playerAttack;

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipWeaponServerRpc(0);
            
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipWeaponServerRpc(1);
            
        }
    }

    private void LateUpdate()
    {
        // ������ �������� ����������� ������ � weaponHolder
        if (currentWeaponNO != null && weaponHolder != null)
        {
            // ����������� ������ ������ ������
            currentWeaponNO.transform.position = weaponHolder.position;
            currentWeaponNO.transform.rotation = weaponHolder.rotation;
            currentWeaponNO.transform.Rotate(0f, 0f, 90f);
        }
    }

    [ServerRpc]
    private void EquipWeaponServerRpc(int index)
    {
        if (index < 0 || index >= weaponPrefabs.Count) return;

        // ������� ������� ������
        if (currentWeaponNO != null)
        {
            currentWeaponNO.Despawn();
            currentWeaponNO = null;
            currentWeapon = null;
        }

        // ������ ����� ������ ��� ��������
        GameObject newWeapon = Instantiate(weaponPrefabs[index], weaponHolder.position, weaponHolder.rotation);

        NetworkObject no = newWeapon.GetComponent<NetworkObject>();
        if (no == null)
        {
            Debug.LogError("������ ������ ��������� NetworkObject!");
            Destroy(newWeapon);
            return;
        }

        // ������� � ����������
        no.SpawnWithOwnership(OwnerClientId);

        currentWeaponNO = no;
        currentWeapon = newWeapon.GetComponent<IWeapon>();
        currentWeapon?.Initialize(this.gameObject);
          
        // �������� �������
        SetWeaponClientRpc(no.NetworkObjectId);
    }

    [ClientRpc]
    public void SetWeaponClientRpc(ulong weaponId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(weaponId, out NetworkObject weaponObj))
        {
            IWeapon weapon = weaponObj.GetComponent<IWeapon>();
            GetComponent<PlayerAttack>().SetWeapon(weapon);

            // ��������� weapon � ��� �������
            currentWeaponNO = weaponObj;
            currentWeapon = weapon;
        }
    }

    public IWeapon GetCurrentWeapon() => currentWeapon;
}
