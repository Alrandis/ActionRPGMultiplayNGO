using Unity.Netcode;
using UnityEngine;

public class NPCLook : NetworkBehaviour
{
    [SerializeField] private Transform spriteHolder; // ������ NPC
    [SerializeField] private float rotationSpeed = 10f; // ��������� ��������

    private Transform _target;
    public bool lockLocked = false;

    // ���� ��������, ���������������� ��������
    private NetworkVariable<float> lookAngle = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // ��������: ��� ��������� ���� ��������� �������
        lookAngle.OnValueChanged += (oldVal, newVal) =>
        {
            if (!IsServer) // ������� ������ ���������
            {
                spriteHolder.rotation = Quaternion.Euler(0, 0, newVal);
            }
        };
    }

    void Update()
    {
        if (GameManager.Instance.isWIn) return;
        if (!IsServer) return; // ������� ������� ������ ������
        if (_target == null || lockLocked) return;

        Vector2 dir = (_target.position - spriteHolder.position).normalized;
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // ������ ������ �� �������
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);
            spriteHolder.rotation = Quaternion.Lerp(spriteHolder.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // ��������� �������� ���� (�������������� ��������)
            lookAngle.Value = spriteHolder.rotation.eulerAngles.z;
        }
    }

    // �����, ����� ������ ����, �������� ������
    public void SetTarget(Transform target)
    {
        if (IsServer)
            _target = target;
    }

    // ����� ����
    public void ClearTarget()
    {
        if (IsServer)
            _target = null;
    }

    public Vector2 GetLookDirection()
    {
        if (_target == null) return Vector2.zero;
        return (_target.position - spriteHolder.position).normalized;
    }
}
