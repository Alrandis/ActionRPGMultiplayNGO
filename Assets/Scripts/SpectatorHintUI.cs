using UnityEngine;
using TMPro; 


public class SpectatorHintUI : MonoBehaviour
{
    [SerializeField] private GameObject hintTextObj; // ������ ������ ���������
    private void Awake()
    {
        hintTextObj.SetActive(false);
    }

    public void ShowHint()
    {
        hintTextObj.SetActive(true);
    }

    public void HideHint()
    {
        hintTextObj.SetActive(false);
    }
}
