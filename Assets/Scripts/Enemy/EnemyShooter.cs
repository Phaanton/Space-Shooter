// EnemyShooter.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShootingPattern
{
    public string patternName = "Novo Padrão";
    public PatternType patternType = PatternType.Basic;

    [Header("Configurações Gerais")]
    public float fireRate = 1f;           // Tiros por segundo
    public float bulletSpeed = 8f;
    public int bulletDamage = 1;

    [Header("Configurações Específicas")]
    public float spreadAngle = 30f;        // Para tiro duplo (ângulo entre tiros)
    public float horizontalSpread = 2f;    // Para tiro de 6 direções

    [Header("Delay Inicial")]
    public float initialDelay = 0f;        // Tempo antes de começar a atirar
}

public enum PatternType
{
    Basic,          // Tiro simples para baixo
    Double,         // Dois tiros para baixo
    Aimed,          // Mira no player
    SixDirections   // 6 direções com múltiplos spawn points
}

public class EnemyShooter : MonoBehaviour
{
    [Header("Configuração de Projétil")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private List<Transform> bulletSpawnPoints = new List<Transform>();

    [Header("Padrões de Tiro")]
    [SerializeField] private List<ShootingPattern> shootingPatterns = new List<ShootingPattern>();

    [Header("Configurações da Área de Gameplay")]
    [SerializeField] private bool useGameplayAreaRestriction = true;
    [SerializeField] private float minX = -3f;
    [SerializeField] private float maxX = 3f;
    [SerializeField] private float minY = -4.8f;
    [SerializeField] private float maxY = 4.8f;

    [Header("Configurações Visuais")]
    [SerializeField] private bool showDebugInfo = false;

    [Header("Sons")]
    [SerializeField] private AudioSource shootSound;

    // Controle interno
    private List<Coroutine> activeCoroutines = new List<Coroutine>();
    private Transform playerTransform;
    private bool isActive = true;
    private bool isInGameplayArea = false;

    void Start()
    {
        // Tenta encontrar o player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Se não tiver spawn points configurados, usa a posição do inimigo
        if (bulletSpawnPoints == null || bulletSpawnPoints.Count == 0)
        {
            bulletSpawnPoints = new List<Transform> { transform };
            if (showDebugInfo)
                Debug.Log($"Inimigo {gameObject.name}: Nenhum spawn point configurado, usando posição do inimigo");
        }

        // Verifica a área de gameplay inicialmente
        CheckGameplayArea();

        // Inicia todos os padrões configurados
        StartAllPatterns();
    }

    void Update()
    {
        // Verifica se o inimigo está na área de gameplay
        CheckGameplayArea();
    }

    void OnDestroy()
    {
        // Para todas as coroutines quando o inimigo for destruído
        foreach (var coroutine in activeCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        activeCoroutines.Clear();
    }

    private void CheckGameplayArea()
    {
        Vector2 pos = transform.position;
        bool wasInArea = isInGameplayArea;

        isInGameplayArea = pos.x >= minX && pos.x <= maxX &&
                           pos.y >= minY && pos.y <= maxY;

        // Se acabou de sair da área, para todos os tiros
        if (wasInArea && !isInGameplayArea)
        {
            StopAllShooting();
            if (showDebugInfo)
                Debug.Log($"Inimigo {gameObject.name} saiu da área de gameplay, parando tiros");
        }
        // Se acabou de entrar na área, recomeça os tiros
        else if (!wasInArea && isInGameplayArea)
        {
            StartAllPatterns();
            if (showDebugInfo)
                Debug.Log($"Inimigo {gameObject.name} entrou na área de gameplay, iniciando tiros");
        }
    }

    private void StopAllShooting()
    {
        // Para todas as coroutines ativas
        foreach (var coroutine in activeCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        activeCoroutines.Clear();
    }

    private void StartAllPatterns()
    {
        // Só inicia os padrões se estiver na área de gameplay e ativo
        if (!isActive || !isInGameplayArea) return;

        foreach (var pattern in shootingPatterns)
        {
            if (pattern.fireRate > 0)
            {
                Coroutine routine = StartCoroutine(ShootRoutine(pattern));
                activeCoroutines.Add(routine);
            }
        }
    }

    private IEnumerator ShootRoutine(ShootingPattern pattern)
    {
        // Delay inicial se configurado
        if (pattern.initialDelay > 0)
        {
            yield return new WaitForSeconds(pattern.initialDelay);
        }

        while (isActive)
        {
            // Só atira se estiver na área de gameplay
            if (isInGameplayArea)
            {
                ExecutePattern(pattern);
            }

            // Aguarda o próximo tiro
            yield return new WaitForSeconds(1f / pattern.fireRate);
        }
    }

    private void ExecutePattern(ShootingPattern pattern)
    {
        if (bulletPrefab == null)
        {
            if (showDebugInfo)
                Debug.LogWarning($"Inimigo {gameObject.name}: bulletPrefab não atribuído!");
            return;
        }

        switch (pattern.patternType)
        {
            case PatternType.Basic:
                ShootBasic(pattern);
                break;

            case PatternType.Double:
                ShootDouble(pattern);
                break;

            case PatternType.Aimed:
                ShootAimed(pattern);
                break;

            case PatternType.SixDirections:
                ShootSixDirections(pattern);
                break;
        }
    }

    #region Padrões de Tiro

    private void ShootBasic(ShootingPattern pattern)
    {
        // Usa o primeiro spawn point ou a posição do inimigo
        Vector2 spawnPos = GetSpawnPosition(0);
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        SetupBullet(bullet, Vector2.down, pattern.bulletSpeed, pattern.bulletDamage);

        shootSound.Play();
    }

    private void ShootDouble(ShootingPattern pattern)
    {
        // Usa o primeiro spawn point ou a posição do inimigo
        Vector2 spawnPos = GetSpawnPosition(0);

        // Calcula ângulos dos tiros
        float angleOffset = pattern.spreadAngle * 0.5f;

        // Tiro esquerdo
        Vector2 directionLeft = Quaternion.Euler(0, 0, -angleOffset) * Vector2.down;
        GameObject bulletLeft = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        SetupBullet(bulletLeft, directionLeft, pattern.bulletSpeed, pattern.bulletDamage);

        // Tiro direito
        Vector2 directionRight = Quaternion.Euler(0, 0, angleOffset) * Vector2.down;
        GameObject bulletRight = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        SetupBullet(bulletRight, directionRight, pattern.bulletSpeed, pattern.bulletDamage);

        shootSound.Play();
    }

    private void ShootAimed(ShootingPattern pattern)
    {
        // Usa o primeiro spawn point ou a posição do inimigo
        Vector2 spawnPos = GetSpawnPosition(0);
        Vector2 direction = Vector2.down;

        if (playerTransform != null)
        {
            // Calcula direção para o player
            direction = (playerTransform.position - (Vector3)spawnPos).normalized;
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning($"Inimigo {gameObject.name}: Player não encontrado, usando direção padrão");
        }

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        SetupBullet(bullet, direction, pattern.bulletSpeed, pattern.bulletDamage);

        shootSound.Play();
    }

    private void ShootSixDirections(ShootingPattern pattern)
    {
        // Define as 6 direções NA NOVA ORDEM:
        // 1. Baixo (índice 0)
        // 2. Cima (índice 1)
        // 3. Esquerda (índice 2)
        // 4. Direita (índice 3)
        // 5. Diagonal inferior esquerda (índice 4)
        // 6. Diagonal inferior direita (índice 5)

        Vector2[] directions = new Vector2[]
        {
        Vector2.down,                               // Baixo (primeiro)
        Vector2.up,                                 // Cima
        Vector2.left,                               // Esquerda
        Vector2.right,                              // Direita
        new Vector2(-0.5f, -0.5f).normalized,       // Diagonal inferior esquerda
        new Vector2(0.5f, -0.5f).normalized         // Diagonal inferior direita
        };

        // Se tiver spread horizontal configurado, ajusta as diagonais
        if (pattern.horizontalSpread > 0)
        {
            directions[4] = new Vector2(-pattern.horizontalSpread, -1).normalized;  // Diagonal esquerda
            directions[5] = new Vector2(pattern.horizontalSpread, -1).normalized;   // Diagonal direita
        }

        // Para cada direção, usa um spawn point diferente
        // Agora a ordem de uso dos spawn points será:
        // Spawn Point 0 → Tiro para baixo
        // Spawn Point 1 → Tiro para cima
        // Spawn Point 2 → Tiro para esquerda
        // Spawn Point 3 → Tiro para direita
        // Spawn Point 4 → Diagonal inferior esquerda
        // Spawn Point 5 → Diagonal inferior direita
        for (int i = 0; i < directions.Length; i++)
        {
            int spawnIndex = i % bulletSpawnPoints.Count;
            Vector2 spawnPos = GetSpawnPosition(spawnIndex);

            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            SetupBullet(bullet, directions[i], pattern.bulletSpeed, pattern.bulletDamage);

            shootSound.Play();
        }
    }

    #endregion

    private Vector2 GetSpawnPosition(int index)
    {
        if (bulletSpawnPoints != null && bulletSpawnPoints.Count > index && bulletSpawnPoints[index] != null)
        {
            return bulletSpawnPoints[index].position;
        }

        // Fallback: usa a posição do inimigo
        return transform.position;
    }

    private void SetupBullet(GameObject bullet, Vector2 direction, float speed, int damage)
    {
        // Tenta encontrar o componente EnemyBullet
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(direction, speed, damage);
        }
        else
        {
            // Fallback: se não tiver o script, usa o método simples
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
        }
    }

    #region Métodos Públicos

    public void AddPattern(ShootingPattern pattern)
    {
        shootingPatterns.Add(pattern);

        // Se já estiver ativo e na área de gameplay, inicia o novo padrão
        if (isActive && isInGameplayArea && pattern.fireRate > 0)
        {
            Coroutine routine = StartCoroutine(ShootRoutine(pattern));
            activeCoroutines.Add(routine);
        }
    }

    public void RemovePattern(int index)
    {
        if (index >= 0 && index < shootingPatterns.Count)
        {
            shootingPatterns.RemoveAt(index);
        }
    }

    public void ClearAllPatterns()
    {
        shootingPatterns.Clear();

        // Para todas as coroutines
        foreach (var coroutine in activeCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        activeCoroutines.Clear();
    }

    public void SetActive(bool active)
    {
        isActive = active;

        // Se desativou, para todos os tiros
        if (!active)
        {
            StopAllShooting();
        }
        else
        {
            // Se reativou e está na área, recomeça todos os padrões
            if (isInGameplayArea)
            {
                StartAllPatterns();
            }
        }
    }

    public void SetBulletPrefab(GameObject prefab)
    {
        bulletPrefab = prefab;
    }

    public void AddSpawnPoint(Transform spawnPoint)
    {
        if (!bulletSpawnPoints.Contains(spawnPoint))
        {
            bulletSpawnPoints.Add(spawnPoint);
        }
    }

    public void ClearSpawnPoints()
    {
        bulletSpawnPoints.Clear();
        // Adiciona a posição do inimigo como fallback
        bulletSpawnPoints.Add(transform);
    }

    public void SetGameplayArea(float xMin, float xMax, float yMin, float yMax)
    {
        minX = xMin;
        maxX = xMax;
        minY = yMin;
        maxY = yMax;

        // Reavalia a área após mudar os limites
        CheckGameplayArea();
    }

    public bool IsInGameplayArea()
    {
        return isInGameplayArea;
    }

    #endregion
}