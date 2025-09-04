using UnityEngine;
using Unity.Netcode;
using System;

public class Health : NetworkBehaviour, IDamageable
{
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject mesh;
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth = new(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isDead = new(writePerm: NetworkVariableWritePermission.Server);

    public event Action OnDeath;
    public int MaxHealth => maxHealth;
    public NetworkVariable<int> CurrentHealth => currentHealth;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        currentHealth.OnValueChanged += OnHealthChanged;

        isDead.OnValueChanged += OnDeathChanged;

    }
    private void OnDeathChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            DeactivatePlayer();

            if (IsOwner)
            {
                var camFollow = GetComponent<PlayerCameraFollow>();
                if (camFollow != null)
                    camFollow.SwitchToSpectator();
            }
        }
    }
    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int oldsValue, int newValue)
    {
        if (newValue <= 0)
            HandleDeath();
    }

    private void HandleDeath()
    {
        if (!IsServer) return;

        OnDeath?.Invoke();
        // Если это не игрок — просто деспавним
        if (GetComponent<PlayerController>() == null)
        {
            // Проверка на босса
            if (CompareTag("Boss"))
            {
                Debug.Log("Босс повержен!");
                // Уведомляем GameManager
                GameManager.Instance.OnBossDefeated();
            }
            // Деспавним босса
            NetworkObject.Despawn();
            return;
        }

        isDead.Value = true;

        GameManager.Instance.CheckGameOver();

    }
    [ClientRpc]
    public void ReviveClientRpc()
    {
        
        Debug.Log("ReviveClientRpc на клиенте сработал в Health");
        //isDead.Value = false;
        //currentHealth.Value = maxHealth;
        
        foreach (var mb in GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb is PlayerCameraFollow) continue;
            mb.enabled = true;
        }

        foreach (var col in GetComponentsInChildren<Collider2D>(true))
            col.enabled = true;

        foreach (var rb in GetComponentsInChildren<Rigidbody2D>(true))
            rb.simulated = true;

        foreach (var anim in GetComponentsInChildren<Animator>(true))
            anim.enabled = true;

        if (hud != null) hud.SetActive(true);
        if (mesh != null) mesh.SetActive(true);
    }

    private void DeactivatePlayer()
    {
        foreach (var mb in GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb is PlayerCameraFollow) continue;
            mb.enabled = false;
        }

        foreach (var col in GetComponentsInChildren<Collider2D>(true))
            col.enabled = false;

        foreach (var rb in GetComponentsInChildren<Rigidbody2D>(true))
            rb.simulated = false;

        foreach (var anim in GetComponentsInChildren<Animator>(true))
            anim.enabled = false;

        if (hud != null) hud.SetActive(false);
        if (mesh != null) mesh.SetActive(false);
    }
    public void TakeDamage(int amount)
    {
        if (!IsServer) return;
        currentHealth.Value -= amount;
    }

    public void Heal(int amount)
    {
        currentHealth.Value = (currentHealth.Value + amount >= maxHealth) ? maxHealth : currentHealth.Value + amount;
    }
}
