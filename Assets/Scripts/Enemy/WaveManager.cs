using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public SpawnPattern[] patterns;  // Padr�es desta onda

        [Header("Wake Up por Tempo")]
        public float wakeUpTime = 0f;     // Tempo em segundos para ativar a onda (a partir do in�cio do jogo)

        [Header("Spawn Point")]
        public bool useSpecificSpawnPoint = false;
        public int spawnPointIndex = 0;

        [Header("Boss")]
        public bool isBossWave = false;
        public GameObject bossPrefab;

        // Controle interno
        [System.NonSerialized]
        public bool hasBeenTriggered = false;
    }

    [Header("Configura��o das Ondas")]
    [SerializeField] private Wave[] waves;

    [Header("Refer�ncias")]
    [SerializeField] private EnemySpawner spawner;

    [Header("Sistema de Wake Up")]
    [SerializeField] private bool startTimerOnGameStart = true;
    [SerializeField] private float timerMultiplier = 1f; // Para acelerar/diminuir tempo (�til para testes)

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private float currentGameTime = 0f; // Mostra o tempo atual no Inspector

    private float gameTimer = 0f;
    private bool isWaveActive = false;

    private bool gamePaused = false;

    void Start()
    {
        if (spawner == null)
            spawner = FindObjectOfType<EnemySpawner>();

        if (spawner == null)
            Debug.LogError("EnemySpawner n�o encontrado na cena!");

        // Inicializa as ondas
        foreach (Wave wave in waves)
        {
            wave.hasBeenTriggered = false;
        }

        if (startTimerOnGameStart)
        {
            StartTimer();
        }

        if (debugMode)
            Debug.Log("Sistema de wake up por tempo ativado.");
    }

    void Update()
    {
        if (gamePaused) return;

        if (gameTimer > 0 || startTimerOnGameStart)
        {
            gameTimer += Time.deltaTime * timerMultiplier;
            currentGameTime = gameTimer;

            CheckWaveTriggers();
        }
    }

    private void CheckWaveTriggers()
    {
        foreach (Wave wave in waves)
        {
            // Se a onda j� foi triggerada, ignora
            if (wave.hasBeenTriggered)
                continue;

            // Verifica se o tempo atual j� passou do tempo de wake up
            if (gameTimer >= wave.wakeUpTime)
            {
                wave.hasBeenTriggered = true;

                if (debugMode)
                    Debug.Log($"Wake up por tempo! Onda {wave.waveName} ativada no tempo {gameTimer:F2}s");

                // Inicia a onda
                StartCoroutine(StartWave(wave));
            }
        }
    }

    private IEnumerator StartWave(Wave wave)
    {
        isWaveActive = true;

        if (debugMode)
            Debug.Log($"Iniciando onda: {wave.waveName}");

        if (wave.isBossWave)
        {
            yield return StartCoroutine(SpawnBossWave(wave));
        }
        else
        {
            foreach (SpawnPattern pattern in wave.patterns)
            {
                if (pattern != null)
                {
                    yield return StartCoroutine(spawner.ExecutePattern(pattern));
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        isWaveActive = false;
    }

    private IEnumerator SpawnBossWave(Wave wave)
    {
        yield return new WaitForSeconds(2f);

        float cameraHeight = Camera.main.orthographicSize;
        Vector3 bossPosition = new Vector3(0, cameraHeight + 2, 0);

        if (wave.bossPrefab != null)
        {
            Instantiate(wave.bossPrefab, bossPosition, Quaternion.identity);
            if (debugMode)
                Debug.Log("Boss spawnado!");
        }
    }

    // M�todos de controle do timer
    public void StartTimer()
    {
        gameTimer = 0f;
        enabled = true;
        if (debugMode)
            Debug.Log("Timer iniciado!");
    }

    public void PauseTimer()
    {
        enabled = false;
        if (debugMode)
            Debug.Log("Timer pausado!");
    }

    public void ResetTimer()
    {
        gameTimer = 0f;
        currentGameTime = 0f;

        // Reseta o estado das ondas
        foreach (Wave wave in waves)
        {
            wave.hasBeenTriggered = false;
        }

        if (debugMode)
            Debug.Log("Timer resetado!");
    }

    public void SetTimer(float time)
    {
        gameTimer = time;
        if (debugMode)
            Debug.Log($"Timer ajustado para {time}s");
    }

    // M�todos para testes manuais
    public void TriggerWaveManually(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= waves.Length) return;

        Wave wave = waves[waveIndex];
        if (!wave.hasBeenTriggered)
        {
            wave.hasBeenTriggered = true;
            StartCoroutine(StartWave(wave));
        }
    }

    #region M�todos de Debug (acess�veis pelo menu de contexto)

    [ContextMenu("Iniciar Timer")]
    void StartTimerDebug() { StartTimer(); }

    [ContextMenu("Pausar Timer")]
    void PauseTimerDebug() { PauseTimer(); }

    [ContextMenu("Resetar Timer")]
    void ResetTimerDebug() { ResetTimer(); }

    [ContextMenu("Trigger Onda 0")]
    void TriggerWave0() { TriggerWaveManually(0); }

    [ContextMenu("Trigger Onda 1")]
    void TriggerWave1() { TriggerWaveManually(1); }

    [ContextMenu("Trigger Onda 2")]
    void TriggerWave2() { TriggerWaveManually(2); }

    #endregion
}