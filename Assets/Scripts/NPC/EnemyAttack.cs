using Unity.Netcode;
using UnityEngine;

public class EnemyAttack : NetworkBehaviour
{
    [SerializeField] private GameObject weaponPrefab;   // ������ �����
    [SerializeField] private Transform weaponHolder;    // ����� ��������� ������
    private NetworkObject nO;
    private IWeapon _weapon;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (weaponPrefab != null && weaponHolder != null)
        {
            // ������� ������ �� �������
            GameObject weaponObj = Instantiate(weaponPrefab, weaponHolder.position, weaponHolder.rotation);
            nO = weaponObj.GetComponent<NetworkObject>();


            // ������� � ����������
            nO.SpawnWithOwnership(OwnerClientId);

            // �������� ���������
            _weapon = weaponObj.GetComponent<IWeapon>();
            _weapon?.Initialize(gameObject);
        }
        else
        {
            Debug.LogWarning($"EnemyAttack: �� ��������� weaponPrefab ��� weaponHolder �� {gameObject.name}");
        }
    }

    private void LateUpdate()
    {
        // ������ �������� ����������� ������ � weaponHolder
        if (nO != null && weaponHolder != null)
        {
            // ����������� ������ ������ ������
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
