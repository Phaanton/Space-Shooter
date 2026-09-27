using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameInputScreen : MonoBehaviour
{
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject background;
    [SerializeField] private UIManager uiManager;

    private int currentScore;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmName);
        nameInputPanel.SetActive(false);
    }

    public void ShowNameInput(int score)
    {
        currentScore = score;
        scoreText.text = score.ToString("D7");
        nameInput.text = "";
        nameInputPanel.SetActive(true);
    }

    private void OnConfirmName()
    {
        string playerName = string.IsNullOrEmpty(nameInput.text) ? "Anonymous" : nameInput.text;

        // Limita o tamanho do nome
        if (playerName.Length > 15)
            playerName = playerName.Substring(0, 15);

        HighscoreManager.Instance.AddHighscore(playerName, currentScore);
        uiManager.StopAllSountrack();
        nameInputPanel.SetActive(false);

        // Volta para o menu
        background.SetActive(true);
        mainMenu.SetActive(true);

        uiManager.LoadMainMenuScene();
    }
}