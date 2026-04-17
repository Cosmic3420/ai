using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectController : MonoBehaviour
{
    public static string SelectedCharacterId { get; private set; } = "Vora";

    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private Image previewImage;
    [SerializeField] private Sprite voraPreview;
    [SerializeField] private Sprite sabraPreview;

    public void SelectVora()
    {
        SelectedCharacterId = "Vora";
        RefreshPreview();
    }

    public void SelectSabra()
    {
        SelectedCharacterId = "Sabra";
        RefreshPreview();
    }

    public void ConfirmSelection()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void Start()
    {
        RefreshPreview();
    }

    private void RefreshPreview()
    {
        if (previewImage == null)
        {
            return;
        }

        previewImage.sprite = SelectedCharacterId == "Sabra" ? sabraPreview : voraPreview;
    }
}
