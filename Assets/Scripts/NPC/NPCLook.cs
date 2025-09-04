using Unity.Netcode;
using UnityEngine;

public class NPCLook : NetworkBehaviour
{
    [SerializeField] private Transform spriteHolder; // спрайт NPC
    [SerializeField] private float rotationSpeed = 10f; // плавность поворота

    private Transform _target;
    public bool lockLocked = false;

    // Угол поворота, синхронизируемый сервером
    private NetworkVariable<float> lookAngle = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // Подписка: при изменении угла обновляем поворот
        lookAngle.OnValueChanged += (oldVal, newVal) =>
        {
            if (!IsServer) // клиенты только применяют
            {
                spriteHolder.rotation = Quaternion.Euler(0, 0, newVal);
            }
        };
    }

    void Update()
    {
        if (GameManager.Instance.isWIn) return;
        if (!IsServer) return; // поворот считает только сервер
        if (_target == null || lockLocked) return;

        Vector2 dir = (_target.position - spriteHolder.position).normalized;
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Лерпим плавно на сервере
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);
            spriteHolder.rotation = Quaternion.Lerp(spriteHolder.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // Сохраняем значение угла (синхронизируем клиентам)
            lookAngle.Value = spriteHolder.rotation.eulerAngles.z;
        }
    }

    // Метод, чтобы задать цель, например игрока
    public void SetTarget(Transform target)
    {
        if (IsServer)
            _target = target;
    }

    // Сброс цели
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
