using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("스폰 설정")]
    public SpawnWaveData[] waves;
    public float spawnDistance = 15f;

    private readonly List<EnemyInstance> _enemies = new();
    private readonly Dictionary<EnemyData, ObjectPool> _pools = new();
    private Transform _playerTransform;
    private float _gameTime;
    private readonly Dictionary<SpawnWaveData, float> _lastSpawnTimes = new();

    public int ActiveCount => _enemies.FindAll(e => e.isActive).Count;
    public List<EnemyInstance> Enemies => _enemies;

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
        _gameTime = 0f;

        foreach (var wave in waves)
        {
            if (!_pools.ContainsKey(wave.enemyData))
            {
                var poolParent = new GameObject($"Pool_{wave.enemyData.enemyName}").transform;
                poolParent.SetParent(transform);
                _pools[wave.enemyData] = new ObjectPool(wave.enemyData.prefab, 20, poolParent);
            }
            _lastSpawnTimes[wave] = 0f;
        }
    }

    private void Update()
    {
        _gameTime += Time.deltaTime;
        TrySpawn();
        UpdateEnemies();
    }

    private void TrySpawn()
    {
        foreach (var wave in waves)
        {
            if (_gameTime < wave.startTime) continue;
            if (ActiveCount >= wave.maxConcurrentEnemies) continue;

            if (_gameTime - _lastSpawnTimes[wave] >= wave.spawnInterval)
            {
                SpawnEnemy(wave.enemyData, wave.hpMultiplier);
                _lastSpawnTimes[wave] = _gameTime;
            }
        }
    }

    public void SpawnEnemy(EnemyData data, float hpMultiplier)
    {
        Vector2 circle = Random.insideUnitCircle.normalized * spawnDistance;
        Vector3 spawnPos = _playerTransform.position + new Vector3(circle.x, 0f, circle.y);

        GameObject obj = _pools[data].Get();
        obj.transform.position = spawnPos;

        var instance = new EnemyInstance
        {
            currentHP = data.maxHP * hpMultiplier,
            moveSpeed = data.moveSpeed,
            transform = obj.transform,
            data = data,
            isActive = true
        };

        var damageHandler = obj.GetComponent<DamageHandler>();
        if (damageHandler != null)
        {
            damageHandler.enemyInstance = instance;
            damageHandler.enemyManager = this;
        }

        _enemies.Add(instance);
    }

    private void UpdateEnemies()
    {
        if (_playerTransform == null) return;

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            if (!enemy.isActive) continue;

            if (enemy.IsDead)
            {
                enemy.isActive = false;
                _pools[enemy.data].Return(enemy.transform.gameObject);
                continue;
            }

            enemy.MoveToward(_playerTransform.position, Time.deltaTime);
        }
    }

    public void RemoveEnemy(EnemyInstance enemy)
    {
        enemy.isActive = false;
        _pools[enemy.data].Return(enemy.transform.gameObject);
    }
}
