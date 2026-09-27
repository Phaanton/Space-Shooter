using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private NameInputScreen nameInputScreen;
    private int currentScore;

    public void OnGameOver()
    {
        // currentScore é sua pontuação atual
        if (HighscoreManager.Instance.IsNewHighscore(currentScore))
        {
            // Mostra tela de inserção de nome
            nameInputScreen.ShowNameInput(currentScore);
        }
        else
        {
            // Vai direto para a tabela ou menu
            // SceneManager.LoadScene("HighscoreScene");
        }
    }

    // Método para adicionar pontos durante o jogo
    public void AddScore(int points)
    {
        currentScore = points;
    }
}
