using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

/// <summary>
/// HUD игрока
/// </summary>
public class ObjectHUD : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Slider _healthSlider;

    public bool isPlayer = false;

    private NetworkVariable<FixedString32Bytes> _name = new(writePerm: NetworkVariableWritePermission.Server);
    public string enemyName = "";

    private Camera _mainCamera;
    private Health _health;
    public override void OnNetworkSpawn()
    {
        _mainCamera = Camera.main;

        if (IsServer)
        {
            if (!isPlayer)
            
                _name.Value = string.IsNullOrEmpty(enemyName) ? "Enemy" : enemyName;      
        }

        if (IsOwner && isPlayer)
        {
            string playerName = $"Player_{OwnerClientId + 1}";
            SetNameServerRpc(playerName);
        }

        OnNameChanged("", _name.Value);
        _name.OnValueChanged += OnNameChanged;

        _health = GetComponentInParent<Health>();
        if (_health != null)
        {
            UpdateHealthUI(_health.CurrentHealth.Value, _health.MaxHealth);

            _health.CurrentHealth.OnValueChanged += (oldValue, newValue) =>
            {
                UpdateHealthUI(newValue, _health.MaxHealth);
            };  
        }

    }

    public override void OnNetworkDespawn()
    {
        _name.OnValueChanged -= OnNameChanged;
    }

    [ServerRpc]
    private void SetNameServerRpc(string chosenName)
    {
        _name.Value = chosenName;
    } 

    private void OnNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        gameObject.name = newName.ToString();
        SetName(newName.ToString());
    }

    private void LateUpdate()
    {
        if (_mainCamera != null)
        {
            transform.forward = _mainCamera.transform.forward;
        }
    }

    public void SetName(string name)
    {
        _nameText.text = name;
    }

    private void UpdateHealthUI(int current, int max)
    {
        _healthSlider.maxValue = max;
        _healthSlider.value = current;
    }
}
