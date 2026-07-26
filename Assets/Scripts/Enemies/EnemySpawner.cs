using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<Transform> spawnPositions;
    [SerializeField] private List<GameObject> enemies;
    [SerializeField] private Transform centralReference;
    [SerializeField] private Transform enemySpawnParent;

    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnInterval = 2f;
    [SerializeField] private float maxSpawnInterval = 5f;
    [SerializeField] private bool autoStartSpawning = true;

    [Header("Spawner Limits (Optional)")]
    [SerializeField] private int maxEnemiesToSpawn = 0; // Set > 0 to limit total spawns

    private float spawnTimer;
    private int currentSpawnCount = 0;
    private bool isSpawning = false;

    private void Start()
    {
        if (autoStartSpawning)
        {
            StartSpawning();
        }
    }

    private void Update()
    {
        if (!isSpawning || GameManager.cannotAct) return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnEnemy();
            ResetTimer();
        }
    }

    public void StartSpawning()
    {
        isSpawning = true;
        ResetTimer();
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    private void ResetTimer()
    {
        spawnTimer = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void SpawnEnemy()
    {
        // Safety checks to prevent errors if references aren't assigned
        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] No enemies assigned to spawn list!");
            return;
        }

        if (spawnPositions == null || spawnPositions.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] No spawn positions assigned!");
            return;
        }

        // Check if max spawn limit is reached
        if (maxEnemiesToSpawn > 0 && currentSpawnCount >= maxEnemiesToSpawn)
        {
            //StopSpawning();
            return;
        }

        // Pick random enemy prefab and random spawn location
        GameObject randomEnemyPrefab = enemies[Random.Range(0, enemies.Count)];
        Transform randomSpawnPos = spawnPositions[Random.Range(0, spawnPositions.Count)];

        float checkRadius = 0.5f;
        LayerMask blockingLayers = LayerMask.GetMask("Enemy");

        Collider2D hit = Physics2D.OverlapCircle(randomSpawnPos.position, checkRadius, blockingLayers);

        if (hit != null)
        {
            // Spot blocked
            return;
        }

        // Spawn the enemy
        GameObject enemy = Instantiate(randomEnemyPrefab, randomSpawnPos.position - new Vector3(0, 0.5f, 0), Quaternion.identity, enemySpawnParent);
        BaseEnemy enemyScript = enemy.GetComponent<BaseEnemy>();
        enemyScript.Death += UpdateSpawnCount;
        if(enemyScript.GetType() == typeof(ShootingEnemy))
        {
            //set direction
            float directionToCentralReference = centralReference.position.x > randomSpawnPos.position.x ? 1 : -1;
            ((ShootingEnemy)enemyScript).SetDirectionToFace(new Vector2(directionToCentralReference, 0));
        }
        currentSpawnCount++;
    }

    void UpdateSpawnCount()
    {
        currentSpawnCount--;
    }

    public void ResetSelf()
    {
        currentSpawnCount = 0;
        foreach (BaseEnemy enemy in enemySpawnParent.GetComponentsInChildren<BaseEnemy>())
        {
            Destroy(enemy.gameObject);
        }
        gameObject.SetActive(false);
    }
}