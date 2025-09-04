using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerCameraFollow : NetworkBehaviour
{
    [SerializeField] private SpectatorHintUI spectatorHintUI;
    private Camera _camera;
    private Transform _target;
    public Vector3 offset = new Vector3(0, 0, -10);

    private List<Transform> livePlayers = new List<Transform>();
    private int currentIndex = 0;
    private bool isSpectator = false;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        if (spectatorHintUI == null)
        {
            spectatorHintUI = FindObjectOfType<SpectatorHintUI>();
        }

        // Получаем или создаем камеру
        _camera = GetComponentInChildren<Camera>();
        if (_camera == null)
        {
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.transform.SetParent(transform, false);
            _camera = camObj.AddComponent<Camera>();
        }

        // Привязываем камеру к своему игроку
        _target = transform;
        _camera.gameObject.SetActive(true);

        // Подписка на событие смерти
        var health = GetComponent<Health>();
        if (health != null)
            health.OnDeath += SwitchToSpectator;
    }

    void LateUpdate()
    {
        if (!IsOwner || _camera == null) return;

        // Обновляем список живых игроков, если мы наблюдатель
        if (isSpectator)
        {
            UpdateLivePlayers();
        }

        // Двигаем камеру к текущей цели
        if (_target != null)
        {
            Vector3 desiredPosition = _target.position + offset;
            _camera.transform.position = Vector3.Lerp(
                _camera.transform.position,
                desiredPosition,
                Time.deltaTime * 5f
            );
        }

        // Переключение камеры по кнопке Tab (только если мы в режиме наблюдателя)
        if (isSpectator && Input.GetKeyDown(KeyCode.Tab))
        {
            CycleNextPlayer();
        }
    }

    // Обновляем список живых игроков
    private void UpdateLivePlayers()
    {
        livePlayers.Clear();
        foreach (var player in FindObjectsOfType<PlayerCameraFollow>())
        {
            if (player == this) continue; // не добавляем своего "мертвого" игрока

            var health = player.GetComponent<Health>();
            if (health != null && !health.isDead.Value) // берем только живых
            {
                livePlayers.Add(player.transform);
            }
        }

        if (livePlayers.Count == 0)
        {
            _target = null;
            HideSpectatorHint(); // скрываем подсказку, если никого нет
            return;
        }
        // Если текущая цель умерла — переключаемся
        if (_target != null)
        {
            var targetHealth = _target.GetComponent<Health>();
            if (targetHealth != null && targetHealth.isDead.Value)
            {
                CycleNextPlayer();
            }
        }
    }

    // Переключение на следующего игрока
    private void CycleNextPlayer()
    {
        if (livePlayers.Count == 0)
        {
            _target = null;
            return;
        }

        currentIndex = (currentIndex + 1) % livePlayers.Count;
        _target = livePlayers[currentIndex];
    }

    // Вызывается при смерти текущего игрока
    public void SwitchToSpectator()
    {
        isSpectator = true;

        UpdateLivePlayers();
        if (livePlayers.Count == 0)
            _target = null;
        else if (_target == null || _target.GetComponent<Health>().isDead.Value)
        {
            currentIndex = 0;
            _target = livePlayers[currentIndex];
        }

        // Показываем подсказку только локальному игроку
        if (IsOwner && spectatorHintUI != null)
        {
            spectatorHintUI.ShowHint();
        }
    }

    // Вызывается после респавна игрока
    [ClientRpc]
    public void OnRespawnClientRpc()
    {
        isSpectator = false;     
        _target = transform;     
        currentIndex = 0;         
        HideSpectatorHint();    
    }

    public void HideSpectatorHint()
    {
        if (IsOwner && spectatorHintUI != null)
            spectatorHintUI.HideHint();
    }
}
