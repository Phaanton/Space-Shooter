using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private UIManager uiManager;
    [SerializeField] private ShieldsUI shieldsUI;

    [SerializeField] private Enemy[] enemy;

    [SerializeField] private AudioSource enemyDieSound;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        shieldsUI = FindObjectOfType<ShieldsUI>();
    }

    // Método que o inimigo vai chamar para se cadastrar
    public void RegisterEnemy(Enemy enemy)
    {
        // Inscreve o método UpdateScore do UIManager no evento OnDeath do inimigo
        enemy.OnDeath += uiManager.UpdateScore;
        enemy.OnDieByPlayer += PlayEnemyDieSound;
    }

    // Método para remover o registro (quando o inimigo for destruído)
    public void UnregisterEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            enemy.OnDeath -= uiManager.UpdateScore;
            enemy.OnDieByPlayer -= PlayEnemyDieSound;
        }
    }

    private void PlayEnemyDieSound()
    {
        enemyDieSound.Play();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Seu método específico aqui
        OnSceneChanged(scene);
    }

    void OnSceneChanged(Scene scene)
    {
        uiManager = FindObjectOfType<UIManager>();
    }

    void OnDestroy()
    {
        // IMPORTANTE: Remover o evento para evitar memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
