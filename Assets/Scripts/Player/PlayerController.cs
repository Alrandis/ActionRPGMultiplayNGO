using UnityEngine;
using Unity.Netcode;

using Unity.Collections;

/// <summary>
/// Обрабатывает ввод и управляет движением
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]

public class PlayerController : NetworkBehaviour
{
    private Vector2 _inputDirection;
    private IPlayerInput _input;
    private PlayerStats _stats;
    private Rigidbody2D _rb;
    private SpriteRenderer _render;

    private NetworkVariable<Vector2> _serverMoveDir = new(writePerm: NetworkVariableWritePermission.Server);


    public override void OnNetworkSpawn()
    {
        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
        _render = GetComponentInChildren<SpriteRenderer>();

        if (_rb == null || _stats == null || _render == null)
        {
            Debug.LogError("PlayerController: необходимые компоненты не найдены!");
            enabled = false;
            return;
        }


        // Заставляю только локального игрока обрабатывать ввод
        if (IsOwner)
        {
            _input = new KeyboardInput();
            
        }
        

    }

    void Update()
    {
        if (!IsOwner || _input == null || GameManager.Instance.isWIn)
            return;

        _inputDirection = _input.GetMoveDirection();

        if (IsOwner)
            UpdateMoveInputServerRpc(_inputDirection);
    }

    [ServerRpc]
    private void UpdateMoveInputServerRpc(Vector2 direction)
    {
        _serverMoveDir.Value = direction;
    }

    private void FixedUpdate()
    {
        if (!IsServer || _rb == null || _stats == null)
            return;

        // Сервер двигает игрока
        _rb.MovePosition(_rb.position + _serverMoveDir.Value * _stats.moveSpeed.Value * Time.fixedDeltaTime);
    }

}

