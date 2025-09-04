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

        // �������� ��� ������� ������
        _camera = GetComponentInChildren<Camera>();
        if (_camera == null)
        {
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.transform.SetParent(transform, false);
            _camera = camObj.AddComponent<Camera>();
        }

        // ����������� ������ � ������ ������
        _target = transform;
        _camera.gameObject.SetActive(true);

        // �������� �� ������� ������
        var health = GetComponent<Health>();
        if (health != null)
            health.OnDeath += SwitchToSpectator;
    }

    void LateUpdate()
    {
        if (!IsOwner || _camera == null) return;

        // ��������� ������ ����� �������, ���� �� �����������
        if (isSpectator)
        {
            UpdateLivePlayers();
        }

        // ������� ������ � ������� ����
        if (_target != null)
        {
            Vector3 desiredPosition = _target.position + offset;
            _camera.transform.position = Vector3.Lerp(
                _camera.transform.position,
                desiredPosition,
                Time.deltaTime * 5f
            );
        }

        // ������������ ������ �� ������ Tab (������ ���� �� � ������ �����������)
        if (isSpectator && Input.GetKeyDown(KeyCode.Tab))
        {
            CycleNextPlayer();
        }
    }

    // ��������� ������ ����� �������
    private void UpdateLivePlayers()
    {
        livePlayers.Clear();
        foreach (var player in FindObjectsOfType<PlayerCameraFollow>())
        {
            if (player == this) continue; // �� ��������� ������ "��������" ������

            var health = player.GetComponent<Health>();
            if (health != null && !health.isDead.Value) // ����� ������ �����
            {
                livePlayers.Add(player.transform);
            }
        }

        if (livePlayers.Count == 0)
        {
            _target = null;
            HideSpectatorHint(); // �������� ���������, ���� ������ ���
            return;
        }
        // ���� ������� ���� ������ � �������������
        if (_target != null)
        {
            var targetHealth = _target.GetComponent<Health>();
            if (targetHealth != null && targetHealth.isDead.Value)
            {
                CycleNextPlayer();
            }
        }
    }

    // ������������ �� ���������� ������
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

    // ���������� ��� ������ �������� ������
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

        // ���������� ��������� ������ ���������� ������
        if (IsOwner && spectatorHintUI != null)
        {
            spectatorHintUI.ShowHint();
        }
    }

    // ���������� ����� �������� ������
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
