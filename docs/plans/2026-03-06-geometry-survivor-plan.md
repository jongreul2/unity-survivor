# Geometry Survivor 구현 계획서

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 5분 생존 목표의 미니멀리스트 3D 탄막 생존 게임 프로토타입 구현

**Architecture:** 하이브리드 패턴 — 중앙 Manager들이 엔티티 배열을 소유하고 일괄 순회하며, GameObject는 렌더링용으로만 사용. Object Pooling으로 생성/파괴 비용 제거. ScriptableObject로 게임 데이터 정의.

**Tech Stack:** Unity 3D (URP), C#, Unity Test Framework (NUnit)

**설계 문서:** `docs/plans/2026-03-06-geometry-survivor-design.md`

---

## 프로젝트 폴더 구조

```
Assets/
├── Scripts/
│   ├── Core/           (ObjectPool, GameManager)
│   ├── Data/           (ScriptableObject 클래스)
│   ├── Player/         (PlayerController)
│   ├── Enemy/          (EnemyManager, EnemyInstance)
│   ├── Weapon/         (WeaponManager, ProjectileInstance)
│   ├── XP/             (XPManager)
│   ├── UI/             (UIManager)
│   └── VFX/            (ParticleManager)
├── ScriptableObjects/  (SO 에셋 파일)
│   ├── Enemies/
│   ├── Weapons/
│   └── Waves/
├── Prefabs/
│   ├── Player/
│   ├── Enemies/
│   ├── Projectiles/
│   ├── XP/
│   └── VFX/
├── Materials/
├── Settings/           (URP 설정)
├── Scenes/
└── Tests/
    └── EditMode/
```

---

## Task 1: 프로젝트 초기 설정

**Files:**
- Create: `Assets/Scripts/Core/` (폴더)
- Create: `Assets/Scripts/Data/` (폴더)
- Create: `Assets/Scripts/Player/` (폴더)
- Create: `Assets/Scripts/Enemy/` (폴더)
- Create: `Assets/Scripts/Weapon/` (폴더)
- Create: `Assets/Scripts/XP/` (폴더)
- Create: `Assets/Scripts/UI/` (폴더)
- Create: `Assets/Scripts/VFX/` (폴더)
- Create: `Assets/Tests/EditMode/` (폴더)

**Step 1: Unity 프로젝트 생성**

Unity Hub에서 새 3D (URP) 프로젝트 생성. 프로젝트 이름: `GeometrySurvivor`

> 이 단계는 Unity Hub에서 수동으로 수행해야 합니다.

**Step 2: 폴더 구조 생성**

```bash
mkdir -p Assets/Scripts/{Core,Data,Player,Enemy,Weapon,XP,UI,VFX}
mkdir -p Assets/Tests/EditMode
mkdir -p Assets/ScriptableObjects/{Enemies,Weapons,Waves}
mkdir -p Assets/Prefabs/{Player,Enemies,Projectiles,XP,VFX}
mkdir -p Assets/Materials
mkdir -p Assets/Scenes
```

**Step 3: Edit Mode 테스트 Assembly Definition 생성**

Create: `Assets/Tests/EditMode/EditModeTests.asmdef`

```json
{
    "name": "EditModeTests",
    "rootNamespace": "",
    "references": [
        "GUID:<GameAssemblyGUID>"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

**Step 4: 커밋**

```bash
git add -A
git commit -m "chore: 프로젝트 초기 폴더 구조 설정"
```

---

## Task 2: ObjectPool 제네릭 시스템

**Files:**
- Create: `Assets/Scripts/Core/ObjectPool.cs`
- Test: `Assets/Tests/EditMode/ObjectPoolTest.cs`

**Step 1: 실패하는 테스트 작성**

```csharp
// Assets/Tests/EditMode/ObjectPoolTest.cs
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ObjectPoolTest
{
    private GameObject _prefab;
    private ObjectPool _pool;
    private Transform _parent;

    [SetUp]
    public void SetUp()
    {
        _prefab = new GameObject("TestPrefab");
        _parent = new GameObject("PoolParent").transform;
        _pool = new ObjectPool(_prefab, 5, _parent);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_parent.gameObject);
        Object.DestroyImmediate(_prefab);
    }

    [Test]
    public void Pool_InitializesWithCorrectCount()
    {
        Assert.AreEqual(5, _parent.childCount);
    }

    [Test]
    public void Get_ReturnsActiveObject()
    {
        GameObject obj = _pool.Get();
        Assert.IsNotNull(obj);
        Assert.IsTrue(obj.activeSelf);
    }

    [Test]
    public void Return_DeactivatesObject()
    {
        GameObject obj = _pool.Get();
        _pool.Return(obj);
        Assert.IsFalse(obj.activeSelf);
    }

    [Test]
    public void Get_WhenPoolExhausted_CreatesNewObject()
    {
        for (int i = 0; i < 6; i++)
            _pool.Get();
        Assert.AreEqual(6, _parent.childCount);
    }
}
```

**Step 2: 테스트 실행하여 실패 확인**

Unity Test Runner (Edit Mode) 실행.
Expected: FAIL — `ObjectPool` 클래스가 존재하지 않음.

**Step 3: 최소 구현 작성**

```csharp
// Assets/Scripts/Core/ObjectPool.cs
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly GameObject _prefab;
    private readonly Transform _parent;
    private readonly Queue<GameObject> _available = new();

    public ObjectPool(GameObject prefab, int initialSize, Transform parent)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(_prefab, _parent);
            obj.SetActive(false);
            _available.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        GameObject obj;
        if (_available.Count > 0)
        {
            obj = _available.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(_prefab, _parent);
        }
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        _available.Enqueue(obj);
    }
}
```

**Step 4: 테스트 실행하여 통과 확인**

Unity Test Runner (Edit Mode) 실행.
Expected: 4/4 PASS

**Step 5: 커밋**

```bash
git add Assets/Scripts/Core/ObjectPool.cs Assets/Tests/EditMode/ObjectPoolTest.cs
git commit -m "feat: ObjectPool 제네릭 풀링 시스템 구현"
```

---

## Task 3: ScriptableObject 데이터 클래스

**Files:**
- Create: `Assets/Scripts/Data/EnemyData.cs`
- Create: `Assets/Scripts/Data/WeaponData.cs`
- Create: `Assets/Scripts/Data/LevelBonus.cs`
- Create: `Assets/Scripts/Data/SpawnWaveData.cs`
- Test: `Assets/Tests/EditMode/DataModelTest.cs`

**Step 1: 실패하는 테스트 작성**

```csharp
// Assets/Tests/EditMode/DataModelTest.cs
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class DataModelTest
{
    [Test]
    public void EnemyData_CanBeCreated()
    {
        var data = ScriptableObject.CreateInstance<EnemyData>();
        data.enemyName = "추적자";
        data.maxHP = 10f;
        data.moveSpeed = 5f;
        data.damage = 1f;
        data.xpValue = 5;
        data.color = Color.red;

        Assert.AreEqual("추적자", data.enemyName);
        Assert.AreEqual(10f, data.maxHP);

        Object.DestroyImmediate(data);
    }

    [Test]
    public void WeaponData_LevelBonusApplied()
    {
        var data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseDamage = 10f;
        data.fireRate = 1f;
        data.levelBonuses = new LevelBonus[]
        {
            new() { damageMultiplier = 1.0f, fireRateMultiplier = 1.0f, projectileCount = 1, particleScale = 1.0f },
            new() { damageMultiplier = 1.5f, fireRateMultiplier = 1.2f, projectileCount = 2, particleScale = 1.5f },
        };

        float lv1Damage = data.baseDamage * data.levelBonuses[0].damageMultiplier;
        float lv2Damage = data.baseDamage * data.levelBonuses[1].damageMultiplier;

        Assert.AreEqual(10f, lv1Damage);
        Assert.AreEqual(15f, lv2Damage);

        Object.DestroyImmediate(data);
    }

    [Test]
    public void SpawnWaveData_HasCorrectFields()
    {
        var data = ScriptableObject.CreateInstance<SpawnWaveData>();
        data.startTime = 60f;
        data.spawnInterval = 1.0f;
        data.hpMultiplier = 1.5f;
        data.maxConcurrentEnemies = 30;

        Assert.AreEqual(60f, data.startTime);
        Assert.AreEqual(30, data.maxConcurrentEnemies);

        Object.DestroyImmediate(data);
    }
}
```

**Step 2: 테스트 실행하여 실패 확인**

Expected: FAIL — 데이터 클래스들이 존재하지 않음.

**Step 3: 데이터 클래스 구현**

```csharp
// Assets/Scripts/Data/LevelBonus.cs
using System;

[Serializable]
public struct LevelBonus
{
    public float damageMultiplier;
    public float fireRateMultiplier;
    public int projectileCount;
    public float particleScale;
}
```

```csharp
// Assets/Scripts/Data/EnemyData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "GeometrySurvivor/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public GameObject prefab;
    public float maxHP;
    public float moveSpeed;
    public float damage;
    public int xpValue;
    public Color color;
}
```

```csharp
// Assets/Scripts/Data/WeaponData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "GeometrySurvivor/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float baseDamage;
    public float fireRate;
    public float projectileSpeed;
    public GameObject projectilePrefab;
    public LevelBonus[] levelBonuses;
}
```

```csharp
// Assets/Scripts/Data/SpawnWaveData.cs
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
```

**Step 4: 테스트 실행하여 통과 확인**

Expected: 3/3 PASS

**Step 5: 커밋**

```bash
git add Assets/Scripts/Data/ Assets/Tests/EditMode/DataModelTest.cs
git commit -m "feat: ScriptableObject 데이터 모델 (EnemyData, WeaponData, SpawnWaveData) 구현"
```

---

## Task 4: PlayerController — 이동 & 체력

**Files:**
- Create: `Assets/Scripts/Player/PlayerController.cs`
- Test: `Assets/Tests/EditMode/PlayerControllerTest.cs`

**Step 1: 실패하는 테스트 작성**

```csharp
// Assets/Tests/EditMode/PlayerControllerTest.cs
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class PlayerControllerTest
{
    private GameObject _playerObj;
    private PlayerController _player;

    [SetUp]
    public void SetUp()
    {
        _playerObj = new GameObject("Player");
        _player = _playerObj.AddComponent<PlayerController>();
        _player.maxHP = 100f;
        _player.moveSpeed = 5f;
        _player.mapBoundary = 25f;
        _player.InitializeForTest();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_playerObj);
    }

    [Test]
    public void TakeDamage_ReducesHP()
    {
        _player.TakeDamage(30f);
        Assert.AreEqual(70f, _player.CurrentHP);
    }

    [Test]
    public void TakeDamage_BelowZero_ClampedToZero()
    {
        _player.TakeDamage(150f);
        Assert.AreEqual(0f, _player.CurrentHP);
    }

    [Test]
    public void IsDead_WhenHPZero_ReturnsTrue()
    {
        _player.TakeDamage(100f);
        Assert.IsTrue(_player.IsDead);
    }

    [Test]
    public void ClampPosition_RestrictsToMapBoundary()
    {
        _playerObj.transform.position = new Vector3(30f, 0f, 30f);
        _player.ClampPosition();
        Vector3 pos = _playerObj.transform.position;
        Assert.LessOrEqual(Mathf.Abs(pos.x), 25f);
        Assert.LessOrEqual(Mathf.Abs(pos.z), 25f);
    }
}
```

**Step 2: 테스트 실행하여 실패 확인**

Expected: FAIL — `PlayerController` 클래스가 존재하지 않음.

**Step 3: 최소 구현 작성**

```csharp
// Assets/Scripts/Player/PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("스탯")]
    public float maxHP = 100f;
    public float moveSpeed = 5f;
    public float mapBoundary = 25f;

    public float CurrentHP { get; private set; }
    public bool IsDead => CurrentHP <= 0f;

    private void Start()
    {
        CurrentHP = maxHP;
    }

    // 테스트용 초기화 (Start()는 Edit Mode에서 호출되지 않음)
    public void InitializeForTest()
    {
        CurrentHP = maxHP;
    }

    private void Update()
    {
        HandleMovement();
        ClampPosition();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0f, v).normalized;
        transform.position += dir * (moveSpeed * Time.deltaTime);
    }

    public void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -mapBoundary, mapBoundary);
        pos.z = Mathf.Clamp(pos.z, -mapBoundary, mapBoundary);
        transform.position = pos;
    }

    public void TakeDamage(float amount)
    {
        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
    }
}
```

**Step 4: 테스트 실행하여 통과 확인**

Expected: 4/4 PASS

**Step 5: 커밋**

```bash
git add Assets/Scripts/Player/PlayerController.cs Assets/Tests/EditMode/PlayerControllerTest.cs
git commit -m "feat: PlayerController 이동 및 체력 시스템 구현"
```

---

## Task 5: EnemyManager — 적 관리 & 스폰

**Files:**
- Create: `Assets/Scripts/Enemy/EnemyInstance.cs`
- Create: `Assets/Scripts/Enemy/EnemyManager.cs`
- Test: `Assets/Tests/EditMode/EnemyManagerTest.cs`

**Step 1: 실패하는 테스트 작성**

```csharp
// Assets/Tests/EditMode/EnemyManagerTest.cs
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class EnemyManagerTest
{
    [Test]
    public void EnemyInstance_TakeDamage_ReducesHP()
    {
        var instance = new EnemyInstance { currentHP = 10f, isActive = true };
        instance.TakeDamage(3f);
        Assert.AreEqual(7f, instance.currentHP);
    }

    [Test]
    public void EnemyInstance_TakeDamage_Dies_WhenHPZero()
    {
        var instance = new EnemyInstance { currentHP = 5f, isActive = true };
        instance.TakeDamage(5f);
        Assert.IsTrue(instance.IsDead);
    }

    [Test]
    public void EnemyInstance_MoveToward_MovesInCorrectDirection()
    {
        var obj = new GameObject("Enemy");
        obj.transform.position = new Vector3(10f, 0f, 0f);
        var instance = new EnemyInstance
        {
            currentHP = 10f,
            isActive = true,
            transform = obj.transform,
            moveSpeed = 5f
        };

        Vector3 target = Vector3.zero;
        instance.MoveToward(target, 1f);

        // x가 10보다 작아져야 함 (원점 방향으로 이동)
        Assert.Less(obj.transform.position.x, 10f);

        Object.DestroyImmediate(obj);
    }
}
```

**Step 2: 테스트 실행하여 실패 확인**

Expected: FAIL — `EnemyInstance` 클래스가 존재하지 않음.

**Step 3: EnemyInstance 구현**

```csharp
// Assets/Scripts/Enemy/EnemyInstance.cs
using UnityEngine;

[System.Serializable]
public class EnemyInstance
{
    public float currentHP;
    public float moveSpeed;
    public Transform transform;
    public EnemyData data;
    public bool isActive;

    public bool IsDead => currentHP <= 0f;

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
    }

    public void MoveToward(Vector3 target, float deltaTime)
    {
        if (transform == null) return;
        Vector3 dir = (target - transform.position).normalized;
        transform.position += dir * (moveSpeed * deltaTime);
    }
}
```

**Step 4: 테스트 실행하여 통과 확인**

Expected: 3/3 PASS

**Step 5: EnemyManager 구현**

```csharp
// Assets/Scripts/Enemy/EnemyManager.cs
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

            // spawnInterval 체크는 간단한 타이머로 처리
            // (실제 구현 시 wave별 마지막 스폰 시간 추적 필요)
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
```

**Step 6: 커밋**

```bash
git add Assets/Scripts/Enemy/ Assets/Tests/EditMode/EnemyManagerTest.cs
git commit -m "feat: EnemyInstance 및 EnemyManager 적 관리/스폰 시스템 구현"
```

---

## Task 6: WeaponManager — 자동 발사 & 발사체

**Files:**
- Create: `Assets/Scripts/Weapon/ProjectileInstance.cs`
- Create: `Assets/Scripts/Weapon/WeaponManager.cs`
- Test: `Assets/Tests/EditMode/WeaponManagerTest.cs`

**Step 1: 실패하는 테스트 작성**

```csharp
// Assets/Tests/EditMode/WeaponManagerTest.cs
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class WeaponManagerTest
{
    [Test]
    public void ProjectileInstance_MoveForward_UpdatesPosition()
    {
        var obj = new GameObject("Projectile");
        obj.transform.position = Vector3.zero;
        var instance = new ProjectileInstance
        {
            transform = obj.transform,
            direction = Vector3.forward,
            speed = 10f,
            damage = 5f,
            isActive = true
        };

        instance.MoveForward(1f);

        Assert.AreEqual(10f, obj.transform.position.z, 0.01f);
        Object.DestroyImmediate(obj);
    }

    [Test]
    public void GetDamageAtLevel_AppliesLevelBonus()
    {
        var data = ScriptableObject.CreateInstance<WeaponData>();
        data.baseDamage = 10f;
        data.levelBonuses = new LevelBonus[]
        {
            new() { damageMultiplier = 1.0f, fireRateMultiplier = 1.0f, projectileCount = 1, particleScale = 1f },
            new() { damageMultiplier = 2.0f, fireRateMultiplier = 1.5f, projectileCount = 3, particleScale = 2f },
        };

        float lv0 = WeaponManager.GetDamageAtLevel(data, 0);
        float lv1 = WeaponManager.GetDamageAtLevel(data, 1);

        Assert.AreEqual(10f, lv0);
        Assert.AreEqual(20f, lv1);

        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetProjectileCountAtLevel_ReturnsCorrectCount()
    {
        var data = ScriptableObject.CreateInstance<WeaponData>();
        data.levelBonuses = new LevelBonus[]
        {
            new() { damageMultiplier = 1f, fireRateMultiplier = 1f, projectileCount = 1, particleScale = 1f },
            new() { damageMultiplier = 1f, fireRateMultiplier = 1f, projectileCount = 3, particleScale = 1f },
            new() { damageMultiplier = 1f, fireRateMultiplier = 1f, projectileCount = 5, particleScale = 1f },
        };

        Assert.AreEqual(1, WeaponManager.GetProjectileCountAtLevel(data, 0));
        Assert.AreEqual(5, WeaponManager.GetProjectileCountAtLevel(data, 2));

        Object.DestroyImmediate(data);
    }
}
```

**Step 2: 테스트 실행하여 실패 확인**

Expected: FAIL — `ProjectileInstance`, `WeaponManager` 클래스가 존재하지 않음.

**Step 3: ProjectileInstance 구현**

```csharp
// Assets/Scripts/Weapon/ProjectileInstance.cs
using UnityEngine;

[System.Serializable]
public class ProjectileInstance
{
    public Transform transform;
    public Vector3 direction;
    public float speed;
    public float damage;
    public bool isActive;

    public void MoveForward(float deltaTime)
    {
        if (transform == null) return;
        transform.position += direction * (speed * deltaTime);
    }
}
```

**Step 4: WeaponManager 구현**

```csharp
// Assets/Scripts/Weapon/WeaponManager.cs
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public WeaponData weaponData;
    public float maxProjectileRange = 20f;

    private ObjectPool _projectilePool;
    private readonly List<ProjectileInstance> _projectiles = new();
    private Transform _playerTransform;
    private int _currentLevel;
    private float _fireTimer;

    public List<ProjectileInstance> Projectiles => _projectiles;

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
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

        // 가장 가까운 적 방향 또는 기본 전방
        Vector3 baseDir = Vector3.forward;

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
            _projectiles.Add(instance);
        }
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
```

**Step 5: 테스트 실행하여 통과 확인**

Expected: 3/3 PASS

**Step 6: 커밋**

```bash
git add Assets/Scripts/Weapon/ Assets/Tests/EditMode/WeaponManagerTest.cs
git commit -m "feat: WeaponManager 자동 발사 및 발사체 관리 시스템 구현"
```

---

## Task 7: XPManager — 경험치 젬 & 레벨업

**Files:**
- Create: `Assets/Scripts/XP/XPManager.cs`
- Test: `Assets/Tests/EditMode/XPManagerTest.cs`

**Step 1: 실패하는 테스트 작성**

```csharp
// Assets/Tests/EditMode/XPManagerTest.cs
using NUnit.Framework;

[TestFixture]
public class XPManagerTest
{
    [Test]
    public void AddXP_IncreasesCurrentXP()
    {
        var xpSystem = new XPSystem(new int[] { 10, 25, 50 });
        xpSystem.AddXP(5);
        Assert.AreEqual(5, xpSystem.CurrentXP);
    }

    [Test]
    public void AddXP_TriggersLevelUp_WhenThresholdReached()
    {
        var xpSystem = new XPSystem(new int[] { 10, 25, 50 });
        bool leveledUp = false;
        xpSystem.OnLevelUp += (level) => leveledUp = true;

        xpSystem.AddXP(10);
        Assert.IsTrue(leveledUp);
        Assert.AreEqual(1, xpSystem.CurrentLevel);
    }

    [Test]
    public void AddXP_MultipleLevelUps()
    {
        var xpSystem = new XPSystem(new int[] { 10, 25, 50 });
        xpSystem.AddXP(10); // Lv 1
        xpSystem.AddXP(15); // Lv 2
        Assert.AreEqual(2, xpSystem.CurrentLevel);
    }

    [Test]
    public void AddXP_MaxLevel_DoesNotExceed()
    {
        var xpSystem = new XPSystem(new int[] { 10 });
        xpSystem.AddXP(10); // Lv 1 (max)
        xpSystem.AddXP(100);
        Assert.AreEqual(1, xpSystem.CurrentLevel);
    }

    [Test]
    public void XPForNextLevel_ReturnsCorrectThreshold()
    {
        var xpSystem = new XPSystem(new int[] { 10, 25, 50 });
        Assert.AreEqual(10, xpSystem.XPForNextLevel);
        xpSystem.AddXP(10);
        Assert.AreEqual(25, xpSystem.XPForNextLevel);
    }
}
```

**Step 2: 테스트 실행하여 실패 확인**

Expected: FAIL — `XPSystem` 클래스가 존재하지 않음.

**Step 3: XPSystem (순수 로직) + XPManager (MonoBehaviour) 구현**

```csharp
// Assets/Scripts/XP/XPSystem.cs
using System;

public class XPSystem
{
    private readonly int[] _thresholds;

    public int CurrentXP { get; private set; }
    public int CurrentLevel { get; private set; }
    public int MaxLevel => _thresholds.Length;
    public bool IsMaxLevel => CurrentLevel >= MaxLevel;

    public int XPForNextLevel => IsMaxLevel ? 0 : _thresholds[CurrentLevel];

    public event Action<int> OnLevelUp;

    public XPSystem(int[] thresholds)
    {
        _thresholds = thresholds;
        CurrentXP = 0;
        CurrentLevel = 0;
    }

    public void AddXP(int amount)
    {
        if (IsMaxLevel) return;

        CurrentXP += amount;

        while (!IsMaxLevel && CurrentXP >= _thresholds[CurrentLevel])
        {
            CurrentXP -= _thresholds[CurrentLevel];
            CurrentLevel++;
            OnLevelUp?.Invoke(CurrentLevel);
        }
    }
}
```

```csharp
// Assets/Scripts/XP/XPManager.cs
using UnityEngine;

public class XPManager : MonoBehaviour
{
    public int[] xpThresholds = { 10, 25, 50, 100, 200 };
    public GameObject xpGemPrefab;
    public float gemMagnetRange = 2f;
    public float gemMoveSpeed = 10f;

    private ObjectPool _gemPool;
    private XPSystem _xpSystem;
    private Transform _playerTransform;

    public XPSystem XPSystem => _xpSystem;

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
        _xpSystem = new XPSystem(xpThresholds);

        var poolParent = new GameObject("XPGemPool").transform;
        poolParent.SetParent(transform);
        _gemPool = new ObjectPool(xpGemPrefab, 30, poolParent);
    }

    public void DropGem(Vector3 position, int xpValue)
    {
        GameObject gem = _gemPool.Get();
        gem.transform.position = position;
        var gemComponent = gem.GetComponent<XPGem>();
        if (gemComponent != null)
        {
            gemComponent.xpValue = xpValue;
        }
    }

    private void Update()
    {
        // 젬 자석 효과 및 수집은 XPGem 컴포넌트에서 처리하거나
        // 여기서 Physics.OverlapSphere로 일괄 처리 가능
    }

    public void CollectGem(GameObject gem, int xpValue)
    {
        _xpSystem.AddXP(xpValue);
        _gemPool.Return(gem);
    }
}
```

**Step 4: 테스트 실행하여 통과 확인**

Expected: 5/5 PASS

**Step 5: 커밋**

```bash
git add Assets/Scripts/XP/ Assets/Tests/EditMode/XPManagerTest.cs
git commit -m "feat: XPSystem 경험치 및 레벨업 로직 구현"
```

---

## Task 8: 충돌 처리 — DamageHandler

**Files:**
- Create: `Assets/Scripts/Core/DamageHandler.cs`
- Create: `Assets/Scripts/XP/XPGem.cs`

**Step 1: 충돌 레이어 설계**

| Layer | 충돌 대상 |
|-------|----------|
| Player | Enemy, XPGem |
| Enemy | Projectile |
| Projectile | Enemy |
| XPGem | Player |

**Step 2: DamageHandler 구현 (발사체 → 적)**

```csharp
// Assets/Scripts/Core/DamageHandler.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageHandler : MonoBehaviour
{
    [HideInInspector] public EnemyInstance enemyInstance;
    [HideInInspector] public EnemyManager enemyManager;

    private void OnTriggerEnter(Collider other)
    {
        if (enemyInstance == null || !enemyInstance.isActive) return;

        // 발사체 충돌 처리
        var projHandler = other.GetComponent<ProjectileHandler>();
        if (projHandler != null && projHandler.projectileInstance.isActive)
        {
            enemyInstance.TakeDamage(projHandler.projectileInstance.damage);

            // 파티클 이벤트는 여기서 발행 (Task 10에서 연결)

            projHandler.weaponManager.ReturnProjectile(projHandler.projectileInstance);

            if (enemyInstance.IsDead)
            {
                // XP 드롭 이벤트 발행 (Task 9에서 연결)
                enemyManager.RemoveEnemy(enemyInstance);
            }
        }
    }
}
```

**Step 3: ProjectileHandler 구현**

```csharp
// Assets/Scripts/Weapon/ProjectileHandler.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileHandler : MonoBehaviour
{
    [HideInInspector] public ProjectileInstance projectileInstance;
    [HideInInspector] public WeaponManager weaponManager;
}
```

**Step 4: XPGem 구현 (플레이어 → 젬 수집)**

```csharp
// Assets/Scripts/XP/XPGem.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class XPGem : MonoBehaviour
{
    public int xpValue;
    public float magnetRange = 3f;
    public float moveSpeed = 10f;

    private Transform _playerTransform;
    private XPManager _xpManager;

    public void Initialize(Transform playerTransform, XPManager xpManager)
    {
        _playerTransform = playerTransform;
        _xpManager = xpManager;
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        if (dist < magnetRange)
        {
            // 플레이어 쪽으로 빨려감
            transform.position = Vector3.MoveTowards(
                transform.position, _playerTransform.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            _xpManager.CollectGem(gameObject, xpValue);
        }
    }
}
```

**Step 5: 적 → 플레이어 접촉 데미지 (PlayerController에 추가)**

`Assets/Scripts/Player/PlayerController.cs`에 OnTriggerEnter 추가:

```csharp
// PlayerController.cs에 추가
private float _damageCooldown = 0.5f;
private float _lastDamageTime;

private void OnTriggerEnter(Collider other)
{
    if (Time.time - _lastDamageTime < _damageCooldown) return;

    var damageHandler = other.GetComponent<DamageHandler>();
    if (damageHandler != null && damageHandler.enemyInstance.isActive)
    {
        TakeDamage(damageHandler.enemyInstance.data.damage);
        _lastDamageTime = Time.time;
    }
}
```

**Step 6: 커밋**

```bash
git add Assets/Scripts/Core/DamageHandler.cs Assets/Scripts/Weapon/ProjectileHandler.cs Assets/Scripts/XP/XPGem.cs Assets/Scripts/Player/PlayerController.cs
git commit -m "feat: 충돌 처리 시스템 (발사체→적, 적→플레이어, 젬 수집) 구현"
```

---

## Task 9: GameManager — 타이머 & 게임 상태

**Files:**
- Create: `Assets/Scripts/Core/GameManager.cs`
- Test: `Assets/Tests/EditMode/GameManagerTest.cs`

**Step 1: 실패하는 테스트 작성**

```csharp
// Assets/Tests/EditMode/GameManagerTest.cs
using NUnit.Framework;

[TestFixture]
public class GameManagerTest
{
    [Test]
    public void GameTimer_TickDown_ReducesTime()
    {
        var timer = new GameTimer(300f);
        timer.Tick(1f);
        Assert.AreEqual(299f, timer.RemainingTime, 0.01f);
    }

    [Test]
    public void GameTimer_ReachesZero_IsComplete()
    {
        var timer = new GameTimer(1f);
        timer.Tick(1f);
        Assert.IsTrue(timer.IsComplete);
    }

    [Test]
    public void GameTimer_FormattedTime_CorrectFormat()
    {
        var timer = new GameTimer(125f);
        Assert.AreEqual("2:05", timer.FormattedTime);
    }

    [Test]
    public void GameTimer_DoesNotGoBelowZero()
    {
        var timer = new GameTimer(1f);
        timer.Tick(5f);
        Assert.AreEqual(0f, timer.RemainingTime);
    }
}
```

**Step 2: 테스트 실행하여 실패 확인**

Expected: FAIL — `GameTimer` 클래스가 존재하지 않음.

**Step 3: GameTimer (순수 로직) + GameManager 구현**

```csharp
// Assets/Scripts/Core/GameTimer.cs
using System;

public class GameTimer
{
    public float TotalTime { get; }
    public float RemainingTime { get; private set; }
    public float ElapsedTime => TotalTime - RemainingTime;
    public bool IsComplete => RemainingTime <= 0f;

    public string FormattedTime
    {
        get
        {
            int minutes = (int)(RemainingTime / 60f);
            int seconds = (int)(RemainingTime % 60f);
            return $"{minutes}:{seconds:D2}";
        }
    }

    public GameTimer(float totalTime)
    {
        TotalTime = totalTime;
        RemainingTime = totalTime;
    }

    public void Tick(float deltaTime)
    {
        RemainingTime = Math.Max(0f, RemainingTime - deltaTime);
    }
}
```

```csharp
// Assets/Scripts/Core/GameManager.cs
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, GameOver, Cleared }

    [Header("설정")]
    public float gameDuration = 300f;

    [Header("참조")]
    public PlayerController player;
    public EnemyManager enemyManager;
    public WeaponManager weaponManager;
    public XPManager xpManager;
    public ParticleManager particleManager;

    public GameState State { get; private set; }
    public GameTimer Timer { get; private set; }

    public event Action OnGameOver;
    public event Action OnGameCleared;

    private void Start()
    {
        State = GameState.Playing;
        Timer = new GameTimer(gameDuration);

        enemyManager.Initialize(player.transform);
        weaponManager.Initialize(player.transform);
        xpManager.Initialize(player.transform);

        xpManager.XPSystem.OnLevelUp += OnPlayerLevelUp;
    }

    private void Update()
    {
        if (State != GameState.Playing) return;

        Timer.Tick(Time.deltaTime);

        if (player.IsDead)
        {
            State = GameState.GameOver;
            OnGameOver?.Invoke();
            Time.timeScale = 0f;
            return;
        }

        if (Timer.IsComplete)
        {
            State = GameState.Cleared;
            OnGameCleared?.Invoke();
            Time.timeScale = 0f;
        }
    }

    private void OnPlayerLevelUp(int newLevel)
    {
        weaponManager.SetLevel(newLevel);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
```

**Step 4: 테스트 실행하여 통과 확인**

Expected: 4/4 PASS

**Step 5: 커밋**

```bash
git add Assets/Scripts/Core/GameTimer.cs Assets/Scripts/Core/GameManager.cs Assets/Tests/EditMode/GameManagerTest.cs
git commit -m "feat: GameTimer 및 GameManager 게임 상태/타이머 관리 구현"
```

---

## Task 10: ParticleManager — 파티클 풀링 & 연출

**Files:**
- Create: `Assets/Scripts/VFX/ParticleManager.cs`
- Create: `Assets/Scripts/VFX/ParticleType.cs`

**Step 1: ParticleType enum 작성**

```csharp
// Assets/Scripts/VFX/ParticleType.cs
public enum ParticleType
{
    MuzzleFlash,
    HitSpark,
    EnemyDeath,
    XPCollect,
    LevelUp
}
```

**Step 2: ParticleManager 구현**

```csharp
// Assets/Scripts/VFX/ParticleManager.cs
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [System.Serializable]
    public class ParticleEntry
    {
        public ParticleType type;
        public GameObject prefab;
        public int poolSize;
    }

    public ParticleEntry[] entries;

    private readonly Dictionary<ParticleType, ObjectPool> _pools = new();

    private void Awake()
    {
        foreach (var entry in entries)
        {
            var parent = new GameObject($"ParticlePool_{entry.type}").transform;
            parent.SetParent(transform);
            _pools[entry.type] = new ObjectPool(entry.prefab, entry.poolSize, parent);
        }
    }

    public void Play(ParticleType type, Vector3 position, Color color, float scale = 1f)
    {
        if (!_pools.ContainsKey(type)) return;

        GameObject obj = _pools[type].Get();
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * scale;

        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = color;
            ps.Play();

            // 재생 완료 후 풀 반환
            var returnToPool = obj.GetComponent<ParticleReturnToPool>();
            if (returnToPool == null)
                returnToPool = obj.AddComponent<ParticleReturnToPool>();
            returnToPool.Setup(_pools[type], obj);
        }
    }
}
```

**Step 3: ParticleReturnToPool 헬퍼 구현**

```csharp
// Assets/Scripts/VFX/ParticleReturnToPool.cs
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleReturnToPool : MonoBehaviour
{
    private ObjectPool _pool;
    private GameObject _target;

    public void Setup(ObjectPool pool, GameObject target)
    {
        _pool = pool;
        _target = target;
    }

    private void OnParticleSystemStopped()
    {
        _pool?.Return(_target);
    }
}
```

**Step 4: DamageHandler에 파티클 연결 (수정)**

`Assets/Scripts/Core/DamageHandler.cs`의 OnTriggerEnter에 ParticleManager 호출 추가:

```csharp
// DamageHandler.cs 수정 — 필드 추가
[HideInInspector] public ParticleManager particleManager;

// OnTriggerEnter 내부 — 히트 시
particleManager?.Play(ParticleType.HitSpark, transform.position, enemyInstance.data.color);

// 처치 시
if (enemyInstance.IsDead)
{
    particleManager?.Play(ParticleType.EnemyDeath, transform.position, enemyInstance.data.color, 1.5f);
    // XP 드롭
    enemyManager.RemoveEnemy(enemyInstance);
}
```

**Step 5: 커밋**

```bash
git add Assets/Scripts/VFX/ Assets/Scripts/Core/DamageHandler.cs
git commit -m "feat: ParticleManager 파티클 풀링 및 연출 시스템 구현"
```

---

## Task 11: UIManager — HUD & 게임오버/클리어

**Files:**
- Create: `Assets/Scripts/UI/UIManager.cs`

**Step 1: UIManager 구현**

```csharp
// Assets/Scripts/UI/UIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public Slider hpBar;
    public Slider xpBar;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI levelText;

    [Header("게임오버")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI survivalTimeText;
    public Button restartButtonGameOver;

    [Header("클리어")]
    public GameObject clearedPanel;
    public Button restartButtonCleared;

    private GameManager _gameManager;
    private PlayerController _player;
    private XPSystem _xpSystem;

    public void Initialize(GameManager gameManager, PlayerController player, XPSystem xpSystem)
    {
        _gameManager = gameManager;
        _player = player;
        _xpSystem = xpSystem;

        gameOverPanel.SetActive(false);
        clearedPanel.SetActive(false);

        restartButtonGameOver.onClick.AddListener(_gameManager.RestartGame);
        restartButtonCleared.onClick.AddListener(_gameManager.RestartGame);

        _gameManager.OnGameOver += ShowGameOver;
        _gameManager.OnGameCleared += ShowCleared;
    }

    private void Update()
    {
        if (_gameManager.State != GameManager.GameState.Playing) return;

        // HP 바
        hpBar.value = _player.CurrentHP / _player.maxHP;

        // 타이머
        timerText.text = _gameManager.Timer.FormattedTime;

        // XP 바
        if (!_xpSystem.IsMaxLevel)
        {
            xpBar.value = (float)_xpSystem.CurrentXP / _xpSystem.XPForNextLevel;
        }
        else
        {
            xpBar.value = 1f;
        }

        // 레벨
        levelText.text = $"Lv.{_xpSystem.CurrentLevel}";
    }

    private void ShowGameOver()
    {
        float elapsed = _gameManager.Timer.ElapsedTime;
        int min = (int)(elapsed / 60f);
        int sec = (int)(elapsed % 60f);
        survivalTimeText.text = $"생존 시간: {min}:{sec:D2}";
        gameOverPanel.SetActive(true);
    }

    private void ShowCleared()
    {
        clearedPanel.SetActive(true);
    }
}
```

**Step 2: 커밋**

```bash
git add Assets/Scripts/UI/UIManager.cs
git commit -m "feat: UIManager HUD 및 게임오버/클리어 UI 구현"
```

---

## Task 12: 카메라 & Post Processing

**Files:**
- Create: `Assets/Scripts/Core/CameraFollow.cs`

**Step 1: CameraFollow 구현**

```csharp
// Assets/Scripts/Core/CameraFollow.cs
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new(0f, 15f, -8f);  // Tilted top-down (~65도)
    public float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position);
    }
}
```

**Step 2: CameraShake 유틸리티 구현**

```csharp
// Assets/Scripts/Core/CameraShake.cs
using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private void Awake() => Instance = this;

    public void Shake(float duration = 0.1f, float magnitude = 0.15f)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
```

**Step 3: URP Post Processing 설정 (Unity 에디터에서 수행)**

- Global Volume 생성 → Bloom (Intensity: 1.5, Threshold: 0.8) 추가
- Vignette (Intensity: 0.3) 추가 — 피격 시 스크립트로 강도 조절

**Step 4: 커밋**

```bash
git add Assets/Scripts/Core/CameraFollow.cs Assets/Scripts/Core/CameraShake.cs
git commit -m "feat: CameraFollow (Tilted Top-Down) 및 CameraShake 구현"
```

---

## Task 13: 씬 조립 & 통합

**Step 1: 프리팹 생성 (Unity 에디터)**

| 프리팹 | 형태 | 설정 |
|--------|------|------|
| Player | Cube (1,1,1) 흰색 Material | Rigidbody(Kinematic), BoxCollider(IsTrigger) |
| Enemy_Chaser | Sphere 빨간색 Material | Rigidbody(Kinematic), SphereCollider(IsTrigger), DamageHandler |
| Enemy_Tank | Cylinder 노란색 Material | Rigidbody(Kinematic), CapsuleCollider(IsTrigger), DamageHandler |
| Projectile | Sphere (0.2,0.2,0.2) 밝은색 Material | Rigidbody(Kinematic), SphereCollider(IsTrigger), ProjectileHandler |
| XPGem | Cube (0.3,0.3,0.3) 45도 회전, 청록색 Material | Rigidbody(Kinematic), BoxCollider(IsTrigger), XPGem |

**Step 2: ScriptableObject 에셋 생성 (Unity 에디터)**

Assets > Create > GeometrySurvivor 메뉴에서:

- `Assets/ScriptableObjects/Enemies/ChaserData.asset` — maxHP:10, moveSpeed:5, damage:10, xpValue:5, color:Red
- `Assets/ScriptableObjects/Enemies/TankData.asset` — maxHP:50, moveSpeed:2, damage:20, xpValue:15, color:Yellow
- `Assets/ScriptableObjects/Weapons/BasicWeaponData.asset` — baseDamage:10, fireRate:2, projectileSpeed:15
- `Assets/ScriptableObjects/Waves/` — 초반/중반/후반 3개 웨이브 에셋

**Step 3: 씬 계층 구성 (Unity 에디터)**

설계 문서의 씬 구조에 따라 빈 GameObject들을 배치하고 Manager 컴포넌트 연결:

1. GameManager 오브젝트 → GameManager 컴포넌트 → 참조 연결
2. EnemyManager 오브젝트 → waves 배열에 SO 에셋 할당
3. WeaponManager 오브젝트 → weaponData에 SO 에셋 할당
4. XPManager 오브젝트 → xpGemPrefab 할당
5. ParticleManager 오브젝트 → entries 배열 설정
6. Camera에 CameraFollow + CameraShake 부착
7. Ground: Plane (50,1,50) 진한 남색 Material
8. Canvas + UI 요소 배치 → UIManager 연결
9. Global Volume → Bloom + Vignette

**Step 4: Physics Layer 설정**

Edit > Project Settings > Tags and Layers:
- Layer 6: Player
- Layer 7: Enemy
- Layer 8: Projectile
- Layer 9: XPGem

Physics Collision Matrix에서 필요한 조합만 활성화.

**Step 5: 플레이 테스트 및 밸런스 조정**

**Step 6: 커밋**

```bash
git add -A
git commit -m "feat: 씬 조립 및 전체 시스템 통합 완료"
```

---

## 태스크 요약

| Task | 내용 | 의존성 |
|------|------|--------|
| 1 | 프로젝트 초기 설정 | 없음 |
| 2 | ObjectPool 시스템 | 없음 |
| 3 | ScriptableObject 데이터 클래스 | 없음 |
| 4 | PlayerController (이동 & 체력) | 없음 |
| 5 | EnemyManager (적 관리 & 스폰) | Task 2, 3 |
| 6 | WeaponManager (자동 발사 & 발사체) | Task 2, 3 |
| 7 | XPManager (경험치 & 레벨업) | Task 2 |
| 8 | 충돌 처리 DamageHandler | Task 4, 5, 6, 7 |
| 9 | GameManager (타이머 & 상태) | Task 4, 5, 6, 7 |
| 10 | ParticleManager (파티클 연출) | Task 2, 8 |
| 11 | UIManager (HUD & 패널) | Task 9 |
| 12 | 카메라 & Post Processing | Task 4 |
| 13 | 씬 조립 & 통합 | 전체 |

### 병렬 실행 가능 그룹

- **Group A (독립):** Task 1, 2, 3, 4 → 동시 실행 가능
- **Group B:** Task 5, 6, 7 → Group A 완료 후 동시 실행 가능
- **Group C:** Task 8, 9, 10, 11, 12 → Group B 완료 후 (8→10은 순차, 나머지 병렬 가능)
- **Group D:** Task 13 → 전체 완료 후
