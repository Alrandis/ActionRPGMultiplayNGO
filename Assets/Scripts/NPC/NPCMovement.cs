using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NPCLook))]
public class NPCMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float attackRange = 1.5f; // дистанция атаки (для милишников маленькая, для стрелков побольше)
    [SerializeField] private float stopBuffer = 0.2f;  // небольшой запас, чтобы не дёргался у границы

    private PlayerSearch _playerSearch;
    private NPCLook _npcLook;
    private EnemyAttack _enemyAttack;
    private Transform _target;

    private void Awake()
    {
        _playerSearch = GetComponent<PlayerSearch>();
        _npcLook = GetComponent<NPCLook>();
        _enemyAttack = GetComponent<EnemyAttack>();
    }

    private void Update()
    {
        if (GameManager.Instance.isWIn) return;
        if (!IsServer) return;

        _target = _playerSearch.FindClosestPlayer();

        if (_target != null)
        {
            float dist = Vector2.Distance(transform.position, _target.position);

            if (dist > attackRange + stopBuffer)
            {
                // Идём к игроку
                Vector2 dir = (_target.position - transform.position).normalized;
                transform.Translate(dir * speed * Time.deltaTime, Space.World);
            }
            else
            {
                if (_enemyAttack != null)
                    _enemyAttack.TriggerAttack();
            }

            _npcLook.SetTarget(_target);
        }
        else
        {
            _npcLook.ClearTarget();
        }
    }
}
