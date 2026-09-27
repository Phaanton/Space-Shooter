using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance { get; private set; }

    public bool IsPaused { get; private set; }

private void Awake()
    {
        if (instance == null)
        {             
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f; // Pausa o jogo

        InputManager.PlayerInput.SwitchCurrentActionMap("UI"); // Muda para o mapa de ações de UI
    }

    public void UnpauseGame()
    {
        IsPaused = false;
        Time.timeScale = 1f; // Retoma o jogo

        InputManager.PlayerInput.SwitchCurrentActionMap("PlayerActions"); // Muda de volta para o mapa de ações do jogador
    }
}
