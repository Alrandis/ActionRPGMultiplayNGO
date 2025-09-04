using Unity.Netcode;
using UnityEngine;

public class EnemyAttack : NetworkBehaviour
{
    [SerializeField] private GameObject weaponPrefab;   // оружие врага
    [SerializeField] private Transform weaponHolder;    // точка крепления оружия
    private NetworkObject nO;
    private IWeapon _weapon;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (weaponPrefab != null && weaponHolder != null)
        {
            // Спавним оружие на сервере
            GameObject weaponObj = Instantiate(weaponPrefab, weaponHolder.position, weaponHolder.rotation);
            nO = weaponObj.GetComponent<NetworkObject>();


            // Спавним с владельцем
            nO.SpawnWithOwnership(OwnerClientId);

            // Получаем интерфейс
            _weapon = weaponObj.GetComponent<IWeapon>();
            _weapon?.Initialize(gameObject);
        }
        else
        {
            Debug.LogWarning($"EnemyAttack: Не назначены weaponPrefab или weaponHolder на {gameObject.name}");
        }
    }

    private void LateUpdate()
    {
        // Только владелец «прилипляет» оружие к weaponHolder
        if (nO != null && weaponHolder != null)
        {
            // Привязываем только корень оружия
            nO.transform.position = weaponHolder.position;
            nO.transform.rotation = weaponHolder.rotation;
            nO.transform.Rotate(0f, 0f, 90f);
        }
    }

    public void TriggerAttack()
    {
        if (_weapon != null)
            _weapon.PrimaryAttack();
    }
}
