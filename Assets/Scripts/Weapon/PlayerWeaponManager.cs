using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerWeaponManager : NetworkBehaviour
{
    [SerializeField] private Transform weaponHolder; // пустой объект в руках игрока (вращается вместе с playerLook)
    [SerializeField] private List<GameObject> weaponPrefabs; // список префабов оружия

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
        // Только владелец «прилипляет» оружие к weaponHolder
        if (currentWeaponNO != null && weaponHolder != null)
        {
            // Привязываем только корень оружия
            currentWeaponNO.transform.position = weaponHolder.position;
            currentWeaponNO.transform.rotation = weaponHolder.rotation;
            currentWeaponNO.transform.Rotate(0f, 0f, 90f);
        }
    }

    [ServerRpc]
    private void EquipWeaponServerRpc(int index)
    {
        if (index < 0 || index >= weaponPrefabs.Count) return;

        // Удаляем текущее оружие
        if (currentWeaponNO != null)
        {
            currentWeaponNO.Despawn();
            currentWeaponNO = null;
            currentWeapon = null;
        }

        // Создаём новое оружие без родителя
        GameObject newWeapon = Instantiate(weaponPrefabs[index], weaponHolder.position, weaponHolder.rotation);

        NetworkObject no = newWeapon.GetComponent<NetworkObject>();
        if (no == null)
        {
            Debug.LogError("Оружие должно содержать NetworkObject!");
            Destroy(newWeapon);
            return;
        }

        // Спавним с владельцем
        no.SpawnWithOwnership(OwnerClientId);

        currentWeaponNO = no;
        currentWeapon = newWeapon.GetComponent<IWeapon>();
        currentWeapon?.Initialize(this.gameObject);
          
        // Сообщаем клиенту
        SetWeaponClientRpc(no.NetworkObjectId);
    }

    [ClientRpc]
    public void SetWeaponClientRpc(ulong weaponId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(weaponId, out NetworkObject weaponObj))
        {
            IWeapon weapon = weaponObj.GetComponent<IWeapon>();
            GetComponent<PlayerAttack>().SetWeapon(weapon);

            // сохраняем weapon и для клиента
            currentWeaponNO = weaponObj;
            currentWeapon = weapon;
        }
    }

    public IWeapon GetCurrentWeapon() => currentWeapon;
}
