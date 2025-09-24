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


    [Header("UI Fields")]
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TextMeshProUGUI joinCodeDisplay;

    private void Awake()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
        quitButton.onClick.AddListener(() => Application.Quit());

        joinCodeDisplay.gameObject.SetActive(false);
    }

    private async void OnHostClicked()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);

        // Создаем лобби и Relay через Bootstrap
        string joinCode = await NetworkBootstrap.Instance.CreateLobbyAndHost();
        
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                "MainScene",
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );
        }
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


}
