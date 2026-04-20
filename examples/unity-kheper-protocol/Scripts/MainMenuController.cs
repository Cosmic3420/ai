using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        ShowMain();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptions()
    {
        SetPanels(isMainVisible: false);
    }

    public void CloseOptions()
    {
        ShowMain();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    private void ShowMain()
    {
        SetPanels(isMainVisible: true);
    }

    private void SetPanels(bool isMainVisible)
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(isMainVisible);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(!isMainVisible);
        }
    }
}
