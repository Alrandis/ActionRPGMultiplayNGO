using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button startGameButton;

    [Header("UI Fields")]
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TextMeshProUGUI joinCodeDisplay;

    private void Awake()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        quitButton.onClick.AddListener(() => Application.Quit());

        startGameButton.gameObject.SetActive(false);
        startGameButton.onClick.AddListener(OnStartGameClicked);

        joinCodeDisplay.gameObject.SetActive(false);
    }

    private async void OnHostClicked()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);

        // Создаем лобби и Relay через Bootstrap
        string joinCode = await NetworkBootstrap.Instance.CreateLobbyAndHost();

        // Отображаем join code
        if (!string.IsNullOrEmpty(joinCode) && joinCodeDisplay != null)
        {
            joinCodeDisplay.gameObject.SetActive(true);
            joinCodeDisplay.text = $"Join Code: {joinCode}";
        }

        // Показываем кнопку старта игры
        startGameButton.gameObject.SetActive(true);
    }

    private async void OnClientClicked()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);

        string joinCode = joinCodeInput.text;
        if (!string.IsNullOrEmpty(joinCode))
        {
            await NetworkBootstrap.Instance.JoinLobbyWithCode(joinCode);
        }
    }

    private void OnStartGameClicked()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                "MainScene",
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );
        }
    }
}
