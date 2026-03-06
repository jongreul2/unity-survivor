using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public WeaponData weaponData;
    public float maxProjectileRange = 20f;

    private ObjectPool _projectilePool;
    private readonly List<ProjectileInstance> _projectiles = new();
    private Transform _playerTransform;
    private EnemyManager _enemyManager;
    private int _currentLevel;
    private float _fireTimer;

    public List<ProjectileInstance> Projectiles => _projectiles;

    public void Initialize(Transform playerTransform, EnemyManager enemyManager = null)
    {
        _playerTransform = playerTransform;
        _enemyManager = enemyManager;
        _currentLevel = 0;
        _fireTimer = 0f;

        var poolParent = new GameObject("ProjectilePool").transform;
        poolParent.SetParent(transform);
        _projectilePool = new ObjectPool(weaponData.projectilePrefab, 30, poolParent);
    }

    public void SetLevel(int level)
    {
        _currentLevel = Mathf.Clamp(level, 0, weaponData.levelBonuses.Length - 1);
    }

    private void Update()
    {
        HandleFiring();
        UpdateProjectiles();
    }

    private void HandleFiring()
    {
        if (_playerTransform == null) return;

        LevelBonus bonus = weaponData.levelBonuses[_currentLevel];
        float interval = 1f / (weaponData.fireRate * bonus.fireRateMultiplier);

        _fireTimer += Time.deltaTime;
        if (_fireTimer < interval) return;
        _fireTimer = 0f;

        Fire(bonus);
    }

    private void Fire(LevelBonus bonus)
    {
        int count = bonus.projectileCount;
        float spreadAngle = count > 1 ? 30f : 0f;
        float startAngle = -spreadAngle / 2f;
        float step = count > 1 ? spreadAngle / (count - 1) : 0f;

        Vector3 baseDir = FindNearestEnemyDirection();

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + step * i;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDir;

            GameObject obj = _projectilePool.Get();
            obj.transform.position = _playerTransform.position;

            var instance = new ProjectileInstance
            {
                transform = obj.transform,
                direction = dir.normalized,
                speed = weaponData.projectileSpeed,
                damage = weaponData.baseDamage * bonus.damageMultiplier,
                isActive = true
            };

            var handler = obj.GetComponent<ProjectileHandler>();
            if (handler != null)
            {
                handler.projectileInstance = instance;
                handler.weaponManager = this;
            }

            _projectiles.Add(instance);
        }
    }

    private Vector3 FindNearestEnemyDirection()
    {
        if (_enemyManager == null || _playerTransform == null)
            return Vector3.forward;

        float minDist = float.MaxValue;
        Vector3 nearestDir = Vector3.forward;

        foreach (var enemy in _enemyManager.Enemies)
        {
            if (!enemy.isActive || enemy.transform == null) continue;
            float dist = Vector3.Distance(_playerTransform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestDir = (enemy.transform.position - _playerTransform.position).normalized;
            }
        }

        return nearestDir;
    }

    private void UpdateProjectiles()
    {
        if (_playerTransform == null) return;

        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var proj = _projectiles[i];
            if (!proj.isActive) continue;

            proj.MoveForward(Time.deltaTime);

            float dist = Vector3.Distance(proj.transform.position, _playerTransform.position);
            if (dist > maxProjectileRange)
            {
                proj.isActive = false;
                _projectilePool.Return(proj.transform.gameObject);
            }
        }
    }

    public void ReturnProjectile(ProjectileInstance proj)
    {
        proj.isActive = false;
        _projectilePool.Return(proj.transform.gameObject);
    }

    public static float GetDamageAtLevel(WeaponData data, int level)
    {
        level = Mathf.Clamp(level, 0, data.levelBonuses.Length - 1);
        return data.baseDamage * data.levelBonuses[level].damageMultiplier;
    }

    public static int GetProjectileCountAtLevel(WeaponData data, int level)
    {
        level = Mathf.Clamp(level, 0, data.levelBonuses.Length - 1);
        return data.levelBonuses[level].projectileCount;
    }
}
