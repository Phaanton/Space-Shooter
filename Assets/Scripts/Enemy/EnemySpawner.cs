using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configura��es")]
    [SerializeField] private GameObject[] defaultEnemies;
    [SerializeField] private Transform enemyContainer;

    [Header("Posi��es Predefinidas")]
    [SerializeField] private Transform[] spawnPoints;

    // NOVO: Controle do spawn point atual
    private int currentWaveSpawnPointIndex = -1;  // -1 = aleat�rio

    private Camera mainCamera;
    private WaveManager waveManager;

    void Start()
    {
        mainCamera = Camera.main;
        waveManager = FindObjectOfType<WaveManager>();

        if (enemyContainer == null)
        {
            GameObject container = new GameObject("Enemies");
            enemyContainer = container.transform;
        }
    }

    // NOVO: M�todo para o WaveManager configurar o spawn point
    public void SetCurrentWaveSpawnPoint(int index)
    {
        currentWaveSpawnPointIndex = index;
        if (spawnPoints != null)
        {
            string pointName = (index >= 0 && index < spawnPoints.Length) ? spawnPoints[index].name : "Aleat�rio";
            Debug.Log($"Spawn point da onda configurado: {pointName}");
        }
    }

    public IEnumerator ExecutePattern(SpawnPattern pattern)
    {
        yield return new WaitForSeconds(pattern.preDelay);

        // MODIFICADO: Passa o padr�o e o �ndice da onda
        Vector2 basePosition = GetBasePosition(pattern);

        switch (pattern.formation)
        {
            case FormationType.Single:
                yield return StartCoroutine(SpawnSingle(basePosition, pattern));
                break;
            case FormationType.Line:
                if (pattern.useTrainMovement)
                {
                    yield return StartCoroutine(SpawnTrainLine(basePosition, pattern));
                }
                else
                {
                    yield return StartCoroutine(SpawnLine(basePosition, pattern));
                }
                break;
            case FormationType.DoubleLine:
                yield return StartCoroutine(SpawnDoubleLine(basePosition, pattern));
                break;
            case FormationType.Triangle:
                yield return StartCoroutine(SpawnTriangle(basePosition, pattern));
                break;
            case FormationType.StaggeredLine:
                yield return StartCoroutine(SpawnStaggered(basePosition, pattern));
                break;
            case FormationType.V:
                yield return StartCoroutine(SpawnVFormation(basePosition, pattern));
                break;
            case FormationType.HorizontalLine:
                yield return StartCoroutine(SpawnHorizontalLine(basePosition, pattern));
                break;

            case FormationType.TrainStack:  // NOVO
                if (pattern.useTrainMovement)
                {
                    yield return StartCoroutine(SpawnTrainStack(basePosition, pattern));
                }
                break;
        }

        yield return new WaitForSeconds(pattern.postDelay);
    }


    private Vector2 GetBasePosition(SpawnPattern pattern)
    {
        // PRIORIDADE 1: Se o padrão tem spawn point específico
        if (pattern.useSpecificSpawnPoint && spawnPoints != null && spawnPoints.Length > 0)
        {
            if (pattern.specificSpawnPointIndex >= 0 && pattern.specificSpawnPointIndex < spawnPoints.Length)
            {
                Transform patternPoint = spawnPoints[pattern.specificSpawnPointIndex];
                Debug.Log($"Usando spawn point do padrão: {patternPoint.name}");
                return patternPoint.position;
            }
        }

        // PRIORIDADE 2: Se a onda tem um spawn point específico (fallback para compatibilidade)
        if (currentWaveSpawnPointIndex >= 0 && spawnPoints != null &&
            currentWaveSpawnPointIndex < spawnPoints.Length)
        {
            Transform wavePoint = spawnPoints[currentWaveSpawnPointIndex];
            Debug.Log($"Usando spawn point da onda: {wavePoint.name}");
            return wavePoint.position;
        }

        // PRIORIDADE 3: Aleatório dos spawn points
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Debug.Log($"Usando spawn point aleatório: {randomPoint.name}");
            return randomPoint.position;
        }

        // PRIORIDADE 4: Posição baseada na câmera
        return GetCameraBasedPosition(pattern);
    }

    private Vector2 GetCameraBasedPosition(SpawnPattern pattern)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        float spawnMargin = 2f;

        switch (pattern.baseSpawnPosition)
        {
            case SpawnPosition.Top:
                return new Vector2(0, cameraHeight + spawnMargin);
            case SpawnPosition.TopLeft:
                return new Vector2(-cameraWidth - spawnMargin, cameraHeight + spawnMargin);
            case SpawnPosition.TopRight:
                return new Vector2(cameraWidth + spawnMargin, cameraHeight + spawnMargin);
            case SpawnPosition.MiddleLeft:
                return new Vector2(-cameraWidth - spawnMargin, 0);
            case SpawnPosition.MiddleRight:
                return new Vector2(cameraWidth + spawnMargin, 0);
            case SpawnPosition.Bottom:
                return new Vector2(0, -cameraHeight - spawnMargin);
            case SpawnPosition.BottomLeft:
                return new Vector2(-cameraWidth - spawnMargin, -cameraHeight - spawnMargin);
            case SpawnPosition.BottomRight:
                return new Vector2(cameraWidth + spawnMargin, -cameraHeight - spawnMargin);
            default:
                return pattern.customPosition;
        }
    }

    // M�TODO NOVO: SpawnSingle (estava faltando)
    private IEnumerator SpawnSingle(Vector2 basePos, SpawnPattern pattern)
    {
        GameObject enemyPrefab = GetEnemyPrefab(pattern, 0);
        if (enemyPrefab != null)
        {
            GameObject enemy = SpawnEnemy(enemyPrefab, basePos);
            SetupEnemyMovement(enemy, pattern);
        }
        yield return new WaitForSeconds(pattern.entryDelay);
    }

    private IEnumerator SpawnLine(Vector2 basePos, SpawnPattern pattern)
    {
        for (int i = 0; i < pattern.numberOfEnemies; i++)
        {
            Vector2 offset = new Vector2(0, -i * pattern.spacing);
            Vector2 spawnPos = basePos + offset;

            GameObject enemyPrefab = GetEnemyPrefab(pattern, i);
            if (enemyPrefab != null)
            {
                GameObject enemy = SpawnEnemy(enemyPrefab, spawnPos);
                SetupEnemyMovement(enemy, pattern);
            }

            yield return new WaitForSeconds(pattern.entryDelay);
        }
    }

    private IEnumerator SpawnDoubleLine(Vector2 basePos, SpawnPattern pattern)
    {
        float halfSpacing = pattern.spacing / 2;

        for (int i = 0; i < pattern.numberOfEnemies; i += 2)
        {
            // Primeiro inimigo da dupla (esquerda)
            if (i < pattern.numberOfEnemies)
            {
                Vector2 pos1 = basePos + new Vector2(-halfSpacing, -i * pattern.spacing);
                GameObject enemyPrefab1 = GetEnemyPrefab(pattern, i);
                if (enemyPrefab1 != null)
                {
                    GameObject enemy1 = SpawnEnemy(enemyPrefab1, pos1);
                    SetupEnemyMovement(enemy1, pattern);
                }
            }

            // Segundo inimigo da dupla (direita)
            if (i + 1 < pattern.numberOfEnemies)
            {
                Vector2 pos2 = basePos + new Vector2(halfSpacing, -i * pattern.spacing);
                GameObject enemyPrefab2 = GetEnemyPrefab(pattern, i + 1);
                if (enemyPrefab2 != null)
                {
                    GameObject enemy2 = SpawnEnemy(enemyPrefab2, pos2);
                    SetupEnemyMovement(enemy2, pattern);
                }
            }

            yield return new WaitForSeconds(pattern.entryDelay);
        }
    }

    private IEnumerator SpawnTriangle(Vector2 basePos, SpawnPattern pattern)
    {
        int rows = Mathf.CeilToInt(Mathf.Sqrt(pattern.numberOfEnemies));
        int spawned = 0;

        for (int row = 0; row < rows && spawned < pattern.numberOfEnemies; row++)
        {
            int enemiesInRow = row + 1;
            float rowWidth = (enemiesInRow - 1) * pattern.spacing;
            float startX = -rowWidth / 2;

            for (int i = 0; i < enemiesInRow && spawned < pattern.numberOfEnemies; i++)
            {
                Vector2 offset = new Vector2(startX + i * pattern.spacing, -row * pattern.spacing);
                Vector2 spawnPos = basePos + offset;

                GameObject enemyPrefab = GetEnemyPrefab(pattern, spawned);
                if (enemyPrefab != null)
                {
                    GameObject enemy = SpawnEnemy(enemyPrefab, spawnPos);
                    SetupEnemyMovement(enemy, pattern);
                }

                spawned++;

                if (i < enemiesInRow - 1)
                {
                    yield return new WaitForSeconds(pattern.entryDelay / 2);
                }
            }

            yield return new WaitForSeconds(pattern.entryDelay);
        }
    }

    private IEnumerator SpawnStaggered(Vector2 basePos, SpawnPattern pattern)
    {
        for (int i = 0; i < pattern.numberOfEnemies; i++)
        {
            float offsetX = (i % 2 == 0) ? -pattern.spacing / 2 : pattern.spacing / 2;
            Vector2 offset = new Vector2(offsetX, -i * pattern.spacing);
            Vector2 spawnPos = basePos + offset;

            GameObject enemyPrefab = GetEnemyPrefab(pattern, i);
            if (enemyPrefab != null)
            {
                GameObject enemy = SpawnEnemy(enemyPrefab, spawnPos);
                SetupEnemyMovement(enemy, pattern);
            }

            yield return new WaitForSeconds(pattern.entryDelay);
        }
    }

    private IEnumerator SpawnVFormation(Vector2 basePos, SpawnPattern pattern)
    {
        int halfCount = pattern.numberOfEnemies / 2;

        // Lado esquerdo do V
        for (int i = 0; i < halfCount; i++)
        {
            float angle = 45f * (float)i / Mathf.Max(1, halfCount);
            Vector2 offset = new Vector2(-Mathf.Sin(angle * Mathf.Deg2Rad) * pattern.spacing * (i + 1),
                                        -Mathf.Cos(angle * Mathf.Deg2Rad) * pattern.spacing * (i + 1));
            Vector2 spawnPos = basePos + offset;

            GameObject enemyPrefab = GetEnemyPrefab(pattern, i);
            if (enemyPrefab != null)
            {
                GameObject enemy = SpawnEnemy(enemyPrefab, spawnPos);
                SetupEnemyMovement(enemy, pattern);
            }

            yield return new WaitForSeconds(pattern.entryDelay / 2);
        }

        // Lado direito do V
        for (int i = 0; i < halfCount; i++)
        {
            float angle = 45f * (float)i / Mathf.Max(1, halfCount);
            Vector2 offset = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad) * pattern.spacing * (i + 1),
                                        -Mathf.Cos(angle * Mathf.Deg2Rad) * pattern.spacing * (i + 1));
            Vector2 spawnPos = basePos + offset;

            GameObject enemyPrefab = GetEnemyPrefab(pattern, halfCount + i);
            if (enemyPrefab != null)
            {
                GameObject enemy = SpawnEnemy(enemyPrefab, spawnPos);
                SetupEnemyMovement(enemy, pattern);
            }

            yield return new WaitForSeconds(pattern.entryDelay / 2);
        }

        // Inimigo central se n�mero for �mpar
        if (pattern.numberOfEnemies % 2 != 0)
        {
            GameObject enemyPrefab = GetEnemyPrefab(pattern, pattern.numberOfEnemies - 1);
            if (enemyPrefab != null)
            {
                GameObject enemy = SpawnEnemy(enemyPrefab, basePos);
                SetupEnemyMovement(enemy, pattern);
            }
        }
    }

    private IEnumerator SpawnHorizontalLine(Vector2 basePos, SpawnPattern pattern)
    {
        float totalWidth = (pattern.numberOfEnemies - 1) * pattern.spacing;
        float startX = basePos.x - (totalWidth / 2);

        for (int i = 0; i < pattern.numberOfEnemies; i++)
        {
            float xPos = startX + (i * pattern.spacing);
            Vector2 spawnPos = new Vector2(xPos, basePos.y);

            GameObject enemyPrefab = GetEnemyPrefab(pattern, i);
            if (enemyPrefab != null)
            {
                GameObject enemy = SpawnEnemy(enemyPrefab, spawnPos);
                SetupEnemyMovement(enemy, pattern);
            }

            yield return new WaitForSeconds(pattern.entryDelay);
        }
    }

    private IEnumerator SpawnTrainLine(Vector2 basePos, SpawnPattern pattern)
    {
        List<GameObject> trainSegments = new List<GameObject>();

        // Spawna todos os inimigos da linha
        for (int i = 0; i < pattern.numberOfEnemies; i++)
        {
            Vector2 offset = new Vector2(0, -i * pattern.spacing);
            Vector2 spawnPos = basePos + offset;

            GameObject enemyPrefab = GetEnemyPrefab(pattern, i);
            if (enemyPrefab != null)
            {
                GameObject enemy = SpawnEnemy(enemyPrefab, spawnPos);
                trainSegments.Add(enemy);

                if (i == 0)
                {
                    // PRIMEIRO INIMIGO (L�DER)
                    // Configura movimento especial para o l�der
                    EnemyMover mover = enemy.GetComponent<EnemyMover>();
                    if (mover != null)
                    {
                        mover.SetTrainLeader(pattern.trainExitDirection, pattern.entrySpeed);
                    }

                    // Remove qualquer TrainSegment do l�der (ele n�o precisa)
                    TrainSegment leaderSegment = enemy.GetComponent<TrainSegment>();
                    if (leaderSegment != null)
                        Destroy(leaderSegment);
                }
                else
                {
                    // INIMIGOS SEGUIDORES
                    // Remove o EnemyMover dos seguidores (eles n�o se movem sozinhos)
                    EnemyMover mover = enemy.GetComponent<EnemyMover>();
                    if (mover != null)
                        Destroy(mover);

                    // Adiciona o componente TrainSegment para seguir o anterior
                    TrainSegment segment = enemy.AddComponent<TrainSegment>();

                    // Desativa qualquer outro movimento
                    Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        rb.linearVelocity = Vector2.zero;
                }
            }

            yield return new WaitForSeconds(pattern.entryDelay);
        }

        // Conecta os segmentos em cadeia (do �ltimo para o primeiro)
        for (int i = trainSegments.Count - 1; i > 0; i--)
        {
            TrainSegment segment = trainSegments[i].GetComponent<TrainSegment>();
            if (segment != null)
            {
                // Cada segmento segue o que est� � sua frente
                segment.SetTarget(trainSegments[i - 1].transform);
            }
        }

        // LOG para debug
        Debug.Log($"Trem criado com {trainSegments.Count} vag�es. L�der: {trainSegments[0].name}");
    }

    private IEnumerator SpawnTrainStack(Vector2 basePos, SpawnPattern pattern)
    {
        List<GameObject> trainSegments = new List<GameObject>();

        for (int i = 0; i < pattern.numberOfEnemies; i++)
        {
            Vector2 offset = new Vector2(0, -i * 0.3f);
            Vector2 spawnPos = basePos + offset;

            GameObject enemyPrefab = GetEnemyPrefab(pattern, i);
            if (enemyPrefab != null)
            {
                GameObject enemy = SpawnEnemy(enemyPrefab, spawnPos);
                enemy.transform.rotation = Quaternion.identity;

                Collider2D col = enemy.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;

                // TODOS os segmentos recebem TrainState (inclusive o líder)
                TrainState trainState = enemy.AddComponent<TrainState>();
                trainState.SaveConfig(pattern);

                trainSegments.Add(enemy);

                if (i == 0)
                {
                    // LÍDER - também precisa de TrainState para que os seguidores possam copiar
                    EnemyMover mover = enemy.GetComponent<EnemyMover>();
                    if (mover != null)
                    {
                        mover.SetRotationSettings(pattern.enableRotation, pattern.leaderRotationSpeed, pattern.spriteRotationOffset);

                        if (pattern.useWaypoints && pattern.waypoints != null && pattern.waypoints.Length > 0)
                        {
                            mover.SetTrainLeaderWithWaypoints(pattern.waypoints, pattern.loopWaypoints, pattern.waypointTolerance, pattern.entrySpeed);
                        }
                        else
                        {
                            mover.SetTrainLeader(pattern.trainExitDirection, pattern.entrySpeed);
                            mover.SetTrainTiming(pattern.trainEntryDuration, pattern.trainTurnDuration);
                        }
                    }

                    TrainSegment leaderSegment = enemy.GetComponent<TrainSegment>();
                    if (leaderSegment != null) Destroy(leaderSegment);
                }
                else
                {
                    // SEGUIDORES
                    EnemyMover mover = enemy.GetComponent<EnemyMover>();
                    if (mover != null) Destroy(mover);

                    TrainSegment segment = enemy.AddComponent<TrainSegment>();
                    segment.SetDesiredDistance(pattern.trainSpacing);
                    segment.SetRotationSettings(pattern.enableRotation, pattern.followerRotationSpeed, pattern.spriteRotationOffset);
                }

                yield return new WaitForSeconds(0.15f);
            }
        }

        yield return new WaitForSeconds(0.3f);

        for (int i = trainSegments.Count - 1; i > 0; i--)
        {
            TrainSegment segment = trainSegments[i].GetComponent<TrainSegment>();
            if (segment != null)
            {
                segment.SetTarget(trainSegments[i - 1].transform);
            }
        }
    }

    private float GetSpriteRotationOffset(GameObject obj)
    {
        SpriteRenderer sprite = obj.GetComponent<SpriteRenderer>();
        if (sprite == null) return 0f;

        // Tenta determinar a direção padrão do sprite
        // Se o sprite aponta para cima, offset = -90
        // Se aponta para direita, offset = 0
        // Se aponta para baixo, offset = 90
        // etc. Ajuste conforme seus sprites

        // Exemplo: se seu sprite aponta para cima:
        // return -90f;

        return 0f; // Padrão: sprite aponta para direita
    }

    private GameObject GetEnemyPrefab(SpawnPattern pattern, int index)
    {
        // CORRE��O: usa enemiesToSpawn em vez de customEnemies
        if (pattern.enemiesToSpawn != null && pattern.enemiesToSpawn.Length > 0)
        {
            return pattern.enemiesToSpawn[index % pattern.enemiesToSpawn.Length];
        }

        // Fallback para inimigos padr�o
        if (defaultEnemies != null && defaultEnemies.Length > 0)
        {
            return defaultEnemies[Random.Range(0, defaultEnemies.Length)];
        }

        Debug.LogWarning($"Nenhum inimigo encontrado para o �ndice {index}");
        return null;
    }

    private GameObject SpawnEnemy(GameObject prefab, Vector2 position)
    {
        if (prefab == null)
        {
            Debug.LogError("Tentativa de spawnar inimigo nulo!");
            return null;
        }

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity, enemyContainer);

        return enemy;
    }

    private void SetupEnemyMovement(GameObject enemy, SpawnPattern pattern)
    {
        if (enemy == null) return;

        // Se for movimento train, não configura movimento normal nos seguidores
        if (pattern.useTrainMovement)
        {
            // O líder será configurado no SpawnTrainLine
            return;
        }

        EnemyMover mover = enemy.GetComponent<EnemyMover>();
        if (mover != null)
        {
            // Configura o tipo de movimento baseado no padr�o
            mover.SetMovementType(pattern.movementType);

            switch (pattern.movementType)
            {
                case MovementType.Simple:
                case MovementType.Diagonal:
                    mover.SetSimpleMovement(pattern.entryDirection, pattern.entrySpeed);
                    break;

                case MovementType.CurveExit:
                    mover.SetCurveExit(pattern.curveExitY, pattern.curveExitDirection,
                                       pattern.curveDuration, pattern.curvePath);
                    break;

                case MovementType.Arc:
                    mover.SetArcMovement(pattern.arcHeight, pattern.arcWidth, pattern.arcDuration);
                    break;

                case MovementType.LShape:
                    mover.SetLShape(pattern.lShapeTurnPoint, pattern.lShapeExitDirection, pattern.turnOnX);
                    break;

                case MovementType.ZigZag:
                    mover.SetZigZag(pattern.zigZagAmplitude, pattern.zigZagFrequency, pattern.zigZagDuration);
                    break;

                case MovementType.Decelerate:
                    mover.SetDecelerate(pattern.decelerationRate, pattern.stopDuration,
                                       pattern.exitAfterStop, pattern.exitDirection, pattern.exitSpeed);
                    break;
            }

            // Configura velocidade base
            mover.SetEntrySpeed(pattern.entrySpeed);
        }
    }

    public Transform[] GetSpawnPoints()
    {
        return spawnPoints;
    }
}