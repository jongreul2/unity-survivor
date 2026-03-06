using UnityEngine;

[CreateAssetMenu(fileName = "NewSpawnWaveData", menuName = "GeometrySurvivor/SpawnWaveData")]
public class SpawnWaveData : ScriptableObject
{
    public float startTime;
    public float spawnInterval;
    public EnemyData enemyData;
    public float hpMultiplier;
    public int maxConcurrentEnemies;
}
