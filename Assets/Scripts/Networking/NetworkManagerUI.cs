using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(StartAsHost);
        clientButton.onClick.AddListener(StartAsListener);
        quitButton.onClick.AddListener(() => Application.Quit());
    }

    private void StartAsHost()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);

        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartHost();
        }

        // После старта хоста сразу грузим MainScene
        NetworkManager.Singleton.SceneManager.LoadScene("MainScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void StartAsListener()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);

        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}

