using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    [SerializeField] private GameState currentState = GameState.MainMenu;
    [SerializeField] private bool freezeGameOnMainMenu = true;

    public GameState CurrentState => currentState;
    public bool IsGameFrozen => currentState == GameState.MainMenu && freezeGameOnMainMenu;

    private bool isInitialized = false;

    private bool gameStartedFired = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        currentState = GameState.MainMenu;

        if (freezeGameOnMainMenu)
        {
            Time.timeScale = 0f;
        }

        isInitialized = true;
    }

    public void StartGame()
    {
        if (!isInitialized) return;

        if (currentState == GameState.MainMenu || currentState == GameState.GameOver)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;

            // Reseta o flag
            gameStartedFired = false;

            // Aguarda um frame antes de disparar o evento
            StartCoroutine(DelayedGameStart());
        }
    }

    private System.Collections.IEnumerator DelayedGameStart()
    {
        yield return null; // Aguarda um frame
        yield return null; // Aguarda mais um frame para garantir

        if (!gameStartedFired)
        {
            gameStartedFired = true;
            Debug.Log("Disparando evento OnGameStarted");
            EventManager.TriggerGameStarted();
        }
    }

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    public void GameOver()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.GameOver;
            Time.timeScale = 0f;
        }
    }

    public void ReturnToMainMenu()
    {
        if (currentState != GameState.MainMenu)
        {
            currentState = GameState.MainMenu;
            Time.timeScale = freezeGameOnMainMenu ? 0f : 1f;
        }
    }
}

public static class EventManager
{
    public static System.Action OnGameStarted;

    public static void TriggerGameStarted()
    {
        OnGameStarted?.Invoke();
    }
}