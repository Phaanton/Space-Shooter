using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnIntervalTimer = 0f;
    [SerializeField] private float spawnPositionXRange = 8f;
    [SerializeField] private float[] spawnPositionY;
    [SerializeField] private int enemiesCreated = 0;

    [SerializeField] private int score = 0;
    [SerializeField] private int level = 1;
    [SerializeField] private int scoreToLevelUp = 100;

    [SerializeField] private GameObject bossIntro;
    [SerializeField] private bool bossSpawned = false;
    [SerializeField] private float bossSpawnDelay = 3f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (level < 5)
        {
            if (spawnIntervalTimer <= 0)
            {
                SpawnEnemy();
                spawnIntervalTimer = spawnInterval;
            }
            else
            {
                spawnIntervalTimer -= Time.deltaTime;
            }
        }
        else
        {
            SpawnBossIntro();
        }
    }

    private void SpawnBossIntro()
    {
        if (!bossSpawned && enemiesCreated <= 0)
        {
            if (bossSpawnDelay > 0)
            {
                bossSpawnDelay -= Time.deltaTime;
                return;
            }
            Instantiate(bossIntro, new Vector3(0, 0, 0), Quaternion.identity);
            bossSpawned = true;
        }
    }

    private void SpawnEnemy()
    {
        if (enemiesCreated <= 0)
        {
            int qntyEnemies = level * 4;

            for (int i = 0; i <= qntyEnemies; i++)
            {

                GameObject enemyToSpawn;

                float chance = Random.Range(0f, level);

                if (chance > 2f)
                {
                    enemyToSpawn = enemies[1];
                }
                else
                {
                    enemyToSpawn = enemies[0];
                }

                float randomX = Random.Range(-spawnPositionXRange, spawnPositionXRange);
                float randomY = Random.Range(spawnPositionY[0], spawnPositionY[1]);

                Vector3 spawnPosition = new Vector3(randomX, randomY, 0);
                bool canSpawn = CheckPosition(spawnPosition, enemyToSpawn.transform.localScale);

                if (canSpawn)
                {
                    Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);
                    enemiesCreated++;
                }
            }
        }
    }

    public void Scoring(int score)
    {
        this.score += score;

        if (this.score >= scoreToLevelUp * level)
        {
            level++;
        }
    }

    public void DecreaseEnemies()
    {
        enemiesCreated--;
        if (enemiesCreated < 0)
        {
            enemiesCreated = 0;
        }
    }

    private bool CheckPosition(Vector3 position, Vector3 size)
    {
        Collider2D occupiedSpace = Physics2D.OverlapBox(position, size, 0f);

        if (occupiedSpace == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}