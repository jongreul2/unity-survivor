# 월드 스페이스 바 구현 계획

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 플레이어 머리 위에 XP 진행도 바, 적 머리 위에 HP 바를 월드 스페이스 Canvas로 표시한다.

**Architecture:** 재사용 가능한 `WorldSpaceBar` MonoBehaviour가 자식으로 World Space Canvas + Image를 생성하고, Billboard 처리로 항상 카메라를 향한다. 적 프리팹에 미리 부착하여 풀링과 호환되며, 플레이어에도 동일 컴포넌트를 사용한다.

**Tech Stack:** Unity 2022+, C#, World Space Canvas, UI Image

---

### Task 1: EnemyInstance에 maxHP 필드 추가

**Files:**
- Modify: `Assets/Scripts/Enemy/EnemyInstance.cs:6-7`
- Modify: `Assets/Tests/EditMode/EnemyManagerTest.cs`

**Step 1: 실패하는 테스트 작성**

`Assets/Tests/EditMode/EnemyManagerTest.cs` 끝에 추가:

```csharp
[Test]
public void EnemyInstance_HasMaxHP_Field()
{
    var instance = new EnemyInstance { currentHP = 50f, maxHP = 50f, isActive = true };
    Assert.AreEqual(50f, instance.maxHP);
}

[Test]
public void EnemyInstance_HPRatio_ReturnsCorrectValue()
{
    var instance = new EnemyInstance { currentHP = 30f, maxHP = 50f, isActive = true };
    Assert.AreEqual(0.6f, instance.HPRatio, 0.001f);
}
```

**Step 2: 테스트 실행하여 실패 확인**

Unity EditMode 테스트 실행. 예상: `maxHP` 및 `HPRatio` 미정의로 컴파일 에러.

**Step 3: 최소 구현**

`Assets/Scripts/Enemy/EnemyInstance.cs` 수정:

```csharp
[System.Serializable]
public class EnemyInstance
{
    public float currentHP;
    public float maxHP;
    public float moveSpeed;
    public Transform transform;
    public EnemyData data;
    public bool isActive;

    public bool IsDead => currentHP <= 0f;
    public float HPRatio => maxHP > 0f ? currentHP / maxHP : 0f;

    // ... 기존 메서드 유지
}
```

**Step 4: EnemyManager.SpawnEnemy에서 maxHP 설정**

`Assets/Scripts/Enemy/EnemyManager.cs:80-83` 수정:

```csharp
var instance = new EnemyInstance
{
    currentHP = data.maxHP * hpMultiplier,
    maxHP = data.maxHP * hpMultiplier,
    moveSpeed = data.moveSpeed,
    transform = obj.transform,
    data = data,
    isActive = true
};
```

**Step 5: 테스트 실행하여 통과 확인**

**Step 6: 커밋**

```bash
git add Assets/Scripts/Enemy/EnemyInstance.cs Assets/Scripts/Enemy/EnemyManager.cs Assets/Tests/EditMode/EnemyManagerTest.cs
git commit -m "feat: EnemyInstance에 maxHP 및 HPRatio 추가"
```

---

### Task 2: WorldSpaceBar 컴포넌트 생성

**Files:**
- Create: `Assets/Scripts/UI/WorldSpaceBar.cs`
- Create: `Assets/Tests/EditMode/WorldSpaceBarTest.cs`

**Step 1: 실패하는 테스트 작성**

`Assets/Tests/EditMode/WorldSpaceBarTest.cs` 생성:

```csharp
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class WorldSpaceBarTest
{
    [Test]
    public void SetValue_ClampsToZeroOne()
    {
        var obj = new GameObject("TestBar");
        var bar = obj.AddComponent<WorldSpaceBar>();
        bar.InitializeForTest();

        bar.SetValue(1.5f);
        Assert.AreEqual(1f, bar.CurrentValue, 0.001f);

        bar.SetValue(-0.5f);
        Assert.AreEqual(0f, bar.CurrentValue, 0.001f);

        Object.DestroyImmediate(obj);
    }

    [Test]
    public void SetValue_UpdatesCurrentValue()
    {
        var obj = new GameObject("TestBar");
        var bar = obj.AddComponent<WorldSpaceBar>();
        bar.InitializeForTest();

        bar.SetValue(0.6f);
        Assert.AreEqual(0.6f, bar.CurrentValue, 0.001f);

        Object.DestroyImmediate(obj);
    }

    [Test]
    public void Initialize_CreatesFillTransform()
    {
        var obj = new GameObject("TestBar");
        var bar = obj.AddComponent<WorldSpaceBar>();
        bar.InitializeForTest();

        Assert.IsTrue(bar.HasFill);

        Object.DestroyImmediate(obj);
    }
}
```

**Step 2: 테스트 실행하여 실패 확인**

예상: `WorldSpaceBar` 클래스 미정의로 컴파일 에러.

**Step 3: 최소 구현**

`Assets/Scripts/UI/WorldSpaceBar.cs` 생성:

```csharp
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceBar : MonoBehaviour
{
    [Header("설정")]
    public Color barColor = Color.red;
    public float barWidth = 0.8f;
    public float barHeight = 0.08f;
    public float yOffset = 1.0f;

    public float CurrentValue { get; private set; }
    public bool HasFill => _fill != null;

    private RectTransform _fill;
    private Canvas _canvas;

    private void Awake()
    {
        CreateBar();
    }

    public void InitializeForTest()
    {
        if (_canvas == null) CreateBar();
    }

    private void CreateBar()
    {
        // Canvas
        var canvasObj = new GameObject("BarCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0f, yOffset, 0f);

        _canvas = canvasObj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;

        var rt = canvasObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(barWidth, barHeight);
        rt.localScale = Vector3.one;

        // Background
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        var bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        // Fill
        var fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObj.transform, false);
        _fill = fillObj.AddComponent<RectTransform>();
        _fill.anchorMin = Vector2.zero;
        _fill.anchorMax = Vector2.one;
        _fill.offsetMin = Vector2.zero;
        _fill.offsetMax = Vector2.zero;
        _fill.pivot = new Vector2(0f, 0.5f);
        _fill.anchorMax = new Vector2(1f, 1f);
        var fillImg = fillObj.AddComponent<Image>();
        fillImg.color = barColor;

        SetValue(1f);
    }

    public void SetValue(float normalized)
    {
        CurrentValue = Mathf.Clamp01(normalized);
        if (_fill != null)
        {
            _fill.anchorMax = new Vector2(CurrentValue, 1f);
        }
    }

    private void LateUpdate()
    {
        if (_canvas == null) return;
        var cam = Camera.main;
        if (cam != null)
        {
            _canvas.transform.rotation = cam.transform.rotation;
        }
    }

    private void OnEnable()
    {
        SetValue(1f);
    }
}
```

**Step 4: 테스트 실행하여 통과 확인**

**Step 5: 커밋**

```bash
git add Assets/Scripts/UI/WorldSpaceBar.cs Assets/Tests/EditMode/WorldSpaceBarTest.cs
git commit -m "feat: WorldSpaceBar 컴포넌트 생성"
```

---

### Task 3: 적 프리팹에 WorldSpaceBar 연결

**Files:**
- Modify: `Assets/Scripts/Enemy/EnemyManager.cs:77-96`
- Modify: `Assets/Scripts/Core/DamageHandler.cs`

**Step 1: DamageHandler에 WorldSpaceBar 참조 추가**

`Assets/Scripts/Core/DamageHandler.cs` 수정:

```csharp
[RequireComponent(typeof(Collider))]
public class DamageHandler : MonoBehaviour
{
    [HideInInspector] public EnemyInstance enemyInstance;
    [HideInInspector] public EnemyManager enemyManager;
    [HideInInspector] public ParticleManager particleManager;
    [HideInInspector] public XPManager xpManager;
    [HideInInspector] public WorldSpaceBar healthBar;

    private void OnTriggerEnter(Collider other)
    {
        if (enemyInstance == null || !enemyInstance.isActive) return;

        var projHandler = other.GetComponent<ProjectileHandler>();
        if (projHandler != null && projHandler.projectileInstance != null && projHandler.projectileInstance.isActive)
        {
            enemyInstance.TakeDamage(projHandler.projectileInstance.damage);

            if (healthBar != null)
                healthBar.SetValue(enemyInstance.HPRatio);

            particleManager?.Play(ParticleType.HitSpark, transform.position, enemyInstance.data.color);

            projHandler.weaponManager.ReturnProjectile(projHandler.projectileInstance);

            if (enemyInstance.IsDead)
            {
                particleManager?.Play(ParticleType.EnemyDeath, transform.position, enemyInstance.data.color, 1.5f);

                if (xpManager != null)
                {
                    xpManager.DropGem(transform.position, enemyInstance.data.xpValue);
                }

                enemyManager.RemoveEnemy(enemyInstance);
            }
        }
    }
}
```

**Step 2: EnemyManager.SpawnEnemy에서 바 초기화**

`Assets/Scripts/Enemy/EnemyManager.cs` SpawnEnemy 메서드 수정 — damageHandler 설정 부분 뒤에 추가:

```csharp
var damageHandler = obj.GetComponent<DamageHandler>();
if (damageHandler != null)
{
    damageHandler.enemyInstance = instance;
    damageHandler.enemyManager = this;
    damageHandler.particleManager = _particleManager;
    damageHandler.xpManager = _xpManager;

    var healthBar = obj.GetComponent<WorldSpaceBar>();
    if (healthBar == null)
        healthBar = obj.AddComponent<WorldSpaceBar>();
    healthBar.barColor = new Color(1f, 0.2f, 0.2f);
    healthBar.barWidth = 0.8f;
    healthBar.barHeight = 0.08f;
    healthBar.yOffset = 1.0f;
    healthBar.SetValue(1f);
    damageHandler.healthBar = healthBar;
}
```

**Step 3: 테스트 실행하여 기존 테스트 통과 확인**

**Step 4: 커밋**

```bash
git add Assets/Scripts/Core/DamageHandler.cs Assets/Scripts/Enemy/EnemyManager.cs
git commit -m "feat: 적 체력 바 연결 — DamageHandler에서 피격 시 갱신"
```

---

### Task 4: 플레이어 XP 바 연결

**Files:**
- Modify: `Assets/Scripts/Core/GameManager.cs:25-39`
- Modify: `Assets/Scripts/UI/UIManager.cs:42-58`

**Step 1: GameManager.Start에서 플레이어에 WorldSpaceBar 추가**

`Assets/Scripts/Core/GameManager.cs` Start 메서드 수정:

```csharp
private WorldSpaceBar _playerXPBar;

private void Start()
{
    State = GameState.Playing;
    Timer = new GameTimer(gameDuration);

    enemyManager.Initialize(player.transform, particleManager, xpManager);
    weaponManager.Initialize(player.transform, enemyManager);
    xpManager.Initialize(player.transform);

    // 플레이어 XP 월드 스페이스 바
    _playerXPBar = player.GetComponent<WorldSpaceBar>();
    if (_playerXPBar == null)
        _playerXPBar = player.gameObject.AddComponent<WorldSpaceBar>();
    _playerXPBar.barColor = new Color(0f, 0.8f, 1f);
    _playerXPBar.barWidth = 1.0f;
    _playerXPBar.barHeight = 0.06f;
    _playerXPBar.yOffset = 1.2f;
    _playerXPBar.SetValue(0f);

    if (uiManager != null)
    {
        uiManager.Initialize(this, player, xpManager.XPSystem, _playerXPBar);
    }

    xpManager.XPSystem.OnLevelUp += OnPlayerLevelUp;
}
```

**Step 2: UIManager.Initialize 시그니처 수정 및 XP 바 업데이트**

`Assets/Scripts/UI/UIManager.cs` 수정:

```csharp
private WorldSpaceBar _playerXPBar;

public void Initialize(GameManager gameManager, PlayerController player, XPSystem xpSystem, WorldSpaceBar playerXPBar = null)
{
    _gameManager = gameManager;
    _player = player;
    _xpSystem = xpSystem;
    _playerXPBar = playerXPBar;

    gameOverPanel.SetActive(false);
    clearedPanel.SetActive(false);

    restartButtonGameOver.onClick.AddListener(_gameManager.RestartGame);
    restartButtonCleared.onClick.AddListener(_gameManager.RestartGame);

    _gameManager.OnGameOver += ShowGameOver;
    _gameManager.OnGameCleared += ShowCleared;
}

private void Update()
{
    if (_gameManager == null || _gameManager.State != GameManager.GameState.Playing) return;

    hpBar.value = _player.CurrentHP / _player.maxHP;
    timerText.text = _gameManager.Timer.FormattedTime;

    if (!_xpSystem.IsMaxLevel)
    {
        float xpRatio = (float)_xpSystem.CurrentXP / _xpSystem.XPForNextLevel;
        xpBar.value = xpRatio;
        if (_playerXPBar != null) _playerXPBar.SetValue(xpRatio);
    }
    else
    {
        xpBar.value = 1f;
        if (_playerXPBar != null) _playerXPBar.SetValue(1f);
    }

    levelText.text = $"Lv.{_xpSystem.CurrentLevel}";
}
```

**Step 3: 기존 테스트 통과 확인**

**Step 4: 커밋**

```bash
git add Assets/Scripts/Core/GameManager.cs Assets/Scripts/UI/UIManager.cs
git commit -m "feat: 플레이어 머리 위 XP 월드 스페이스 바 연결"
```

---

### Task 5: SceneAssembler 업데이트 (선택)

**Files:**
- Modify: `Assets/Editor/SceneAssembler.cs`

SceneAssembler에서 프리팹 수정 메뉴를 추가하여, 에디터에서 프리팹에 WorldSpaceBar를 미리 부착할 수 있도록 한다. 이 태스크는 런타임 AddComponent 방식이 이미 동작하므로 선택 사항이다.

---

## 실행 순서 요약

| Task | 내용 | 의존성 |
|------|------|--------|
| 1 | EnemyInstance maxHP/HPRatio | 없음 |
| 2 | WorldSpaceBar 컴포넌트 | 없음 |
| 3 | 적 HP 바 연결 | Task 1, 2 |
| 4 | 플레이어 XP 바 연결 | Task 2 |
| 5 | SceneAssembler 업데이트 | Task 2 (선택) |
