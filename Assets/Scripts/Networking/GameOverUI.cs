using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    public void Show(bool isHost)
    {
        panel.SetActive(true);

        if (isHost)
        {
            messageText.text = "Все игроки погибли!\nРешите, что делать дальше.";
            restartButton.gameObject.SetActive(true);
            quitButton.gameObject.SetActive(true);

            restartButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();

            restartButton.onClick.AddListener(() =>
            {
                panel.SetActive(false);
                // вызывать респавн игроков на сервере
                GameManager.Instance.RespawnAllPlayersServerRpc();
            });

            quitButton.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }
        else
        {
            messageText.text = "Все игроки погибли!\nОжидаем решение хоста...";
            restartButton.gameObject.SetActive(false);
            quitButton.gameObject.SetActive(false);
        }
    }
}
