using Unity.Netcode;
using UnityEngine;

public class PlayerLook : NetworkBehaviour
{
    [SerializeField] private Transform spriteHolder;

    private Camera _mainCamera;

    // Направление взгляда в сеть
    private NetworkVariable<Vector2> _lookDirection = new(
        Vector2.right,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public bool mouseLocked = false;

    public override void OnNetworkSpawn()
    {
        // Камера
        _mainCamera = transform.root.GetComponentInChildren<Camera>(true);
        if (IsOwner)
        {
            if (_mainCamera != null)
            {
                _mainCamera.enabled = true;
                var listener = _mainCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = true;
            }
        }
        else
        {
            if (_mainCamera != null)
            {
                _mainCamera.enabled = false;
                var listener = _mainCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
            }
        }

        // Подписка на изменение
        _lookDirection.OnValueChanged += OnLookDirectionChanged;

        // Для остальных игроков сразу применим текущее значение
        if (!IsOwner)
            ApplyRotation(_lookDirection.Value);
    }

    public override void OnNetworkDespawn()
    {
        _lookDirection.OnValueChanged -= OnLookDirectionChanged;
    }

    void Update()
    {
        if (GameManager.Instance.isWIn) return;
        if (mouseLocked) return;

        if (!IsOwner || _mainCamera == null || !Application.isFocused) return;

        // Получаем курсор в мировых координатах
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        Vector2 dir = (mouseWorld - spriteHolder.position).normalized;
        if (dir == Vector2.zero) return;

        // Локально поворачиваем
        ApplyRotation(dir);

        // Отправляем в сеть (сетевое значение обновляет только владелец)
        _lookDirection.Value = dir;
    }

    private void OnLookDirectionChanged(Vector2 oldDir, Vector2 newDir)
    {
        if (IsOwner) return; // локальный игрок сам поворачивается
        ApplyRotation(newDir);
    }

    private void ApplyRotation(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        spriteHolder.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public Vector2 GetLookDirection() => _lookDirection.Value;
}
