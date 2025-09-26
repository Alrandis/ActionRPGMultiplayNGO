using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class JoinCodePanel : NetworkBehaviour
{
    [SerializeField] private GameObject _joinCodePanel;
    [SerializeField] private TextMeshProUGUI _joinCodeDisplay;
    string joinCode = "";

    // true = панель открыта (игра на паузе), false = панель закрыта
    private NetworkVariable<bool> isPanelOpen = new NetworkVariable<bool>(false);

    private void Start()
    {
        joinCode = NetworkBootstrap.Instance.CurrentJoinCode;
        _joinCodeDisplay.text = $"Join Code: {joinCode}";
        // Подписка на изменение состояния панели
        isPanelOpen.OnValueChanged += OnPanelChanged;
        OnPanelChanged(false, isPanelOpen.Value); // инициализация UI при старте
    }

    // Локальный вызов игроком (например, кнопка UI)
    public void ToggleJoinPanel()
    {
        // Вызываем сервер, чтобы синхронизировать состояние
        SetPanelServerRpc(!isPanelOpen.Value);
    }

    // SERVER: меняем состояние панели
    [ServerRpc(RequireOwnership = false)]
    private void SetPanelServerRpc(bool open)
    {
        isPanelOpen.Value = open; // автоматически синхронизируется у всех клиентов
    }

    // CLIENT & HOST: обновление UI и времени
    private void OnPanelChanged(bool oldValue, bool newValue)
    {
        if (_joinCodePanel != null)
            _joinCodePanel.SetActive(newValue);

        // Ставим время на паузу или возвращаем обратно
        Time.timeScale = newValue ? 0f : 1f;
    }

    public void OpenJoinPanel()
    {
        _joinCodePanel.SetActive(true);
        isPanelOpen.Value = true;
       
    }

}
