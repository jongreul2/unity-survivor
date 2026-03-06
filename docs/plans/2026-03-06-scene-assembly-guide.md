# Geometry Survivor 씬 조립 가이드

> Unity 에디터에서 순서대로 따라하는 상세 가이드입니다.

---

## 0. 사전 확인

Unity에서 프로젝트를 열고 Console 창에 컴파일 에러가 없는지 확인합니다.
- `Assets/Scripts/` 폴더의 모든 스크립트가 정상 컴파일되어야 합니다.
- 에러가 있다면 먼저 해결한 뒤 진행하세요.

---

## 1. Physics Layer 설정

**Edit > Project Settings > Tags and Layers**

| Layer 번호 | 이름 |
|-----------|------|
| 6 | Player |
| 7 | Enemy |
| 8 | Projectile |
| 9 | XPGem |

**Edit > Project Settings > Physics**

Layer Collision Matrix에서 **아래 조합만 체크** (나머지 해제):

| | Player | Enemy | Projectile | XPGem |
|--|--------|-------|------------|-------|
| **Player** | - | ✅ | - | ✅ |
| **Enemy** | ✅ | - | ✅ | - |
| **Projectile** | - | ✅ | - | - |
| **XPGem** | ✅ | - | - | - |

> Default 레이어 간 충돌은 유지해도 무방합니다.

---

## 2. Material 생성

**Assets/Materials/** 폴더에서 우클릭 > Create > Material

| Material 이름 | Shader | Base Color (RGBA Hex) | 용도 |
|---------------|--------|----------------------|------|
| Mat_Player | Universal Render Pipeline/Lit | `#FFFFFF` (흰색) | 플레이어 |
| Mat_Chaser | Universal Render Pipeline/Lit | `#FF3333` (빨간색) | 추적자 적 |
| Mat_Tank | Universal Render Pipeline/Lit | `#FFCC00` (노란색) | 탱커 적 |
| Mat_Projectile | Universal Render Pipeline/Lit | `#AAEEFF` (밝은 하늘색) | 발사체 |
| Mat_XPGem | Universal Render Pipeline/Lit | `#00FFCC` (청록색) | 경험치 젬 |
| Mat_Ground | Universal Render Pipeline/Lit | `#1A1A2E` (진한 남색) | 바닥 |

### Emission 설정 (발사체/젬에 빛 번짐 효과)

`Mat_Projectile`과 `Mat_XPGem`에 추가 설정:
1. Inspector에서 **Emission** 체크
2. Emission Color를 Base Color와 동일하게 설정
3. Intensity를 **2** 로 설정 (Bloom과 연동되어 빛 번짐 발생)

---

## 3. 프리팹 생성

각 프리팹을 Hierarchy에서 먼저 만들고, `Assets/Prefabs/` 하위 폴더로 드래그하여 프리팹화합니다.

### 3-1. Player 프리팹

1. Hierarchy > 우클릭 > **3D Object > Cube**
2. 이름: `Player`
3. Transform: Position `(0, 0.5, 0)`, Scale `(1, 1, 1)`
4. **Material:** `Mat_Player` 드래그 앤 드롭
5. **컴포넌트 추가:**
   - `PlayerController` (Add Component)
     - maxHP: `100`
     - moveSpeed: `7`
     - mapBoundary: `25`
6. **Rigidbody 설정:**
   - Add Component > Rigidbody
   - ✅ Is Kinematic
   - Use Gravity: 해제
7. **BoxCollider 설정:**
   - 기존 BoxCollider 선택
   - ✅ Is Trigger
8. **Layer:** `Player` (6번)
9. Hierarchy에서 `Assets/Prefabs/Player/` 폴더로 드래그 → 프리팹 생성
10. **Hierarchy에서 삭제하지 않음** (씬에 하나 남겨둠)

### 3-2. Enemy_Chaser 프리팹

1. Hierarchy > 우클릭 > **3D Object > Sphere**
2. 이름: `Enemy_Chaser`
3. Transform: Scale `(1, 1, 1)`
4. **Material:** `Mat_Chaser`
5. **컴포넌트 추가:**
   - `DamageHandler` (Add Component)
6. **Rigidbody:**
   - ✅ Is Kinematic
   - Use Gravity: 해제
7. **SphereCollider:**
   - ✅ Is Trigger
8. **Layer:** `Enemy` (7번)
9. `Assets/Prefabs/Enemies/` 폴더로 드래그 → 프리팹 생성
10. **Hierarchy에서 삭제**

### 3-3. Enemy_Tank 프리팹

1. Hierarchy > 우클릭 > **3D Object > Cylinder**
2. 이름: `Enemy_Tank`
3. Transform: Scale `(1, 1, 1)`
4. **Material:** `Mat_Tank`
5. **컴포넌트 추가:**
   - `DamageHandler` (Add Component)
6. **Rigidbody:**
   - ✅ Is Kinematic
   - Use Gravity: 해제
7. **CapsuleCollider:**
   - ✅ Is Trigger
8. **Layer:** `Enemy` (7번)
9. `Assets/Prefabs/Enemies/` 폴더로 드래그 → 프리팹 생성
10. **Hierarchy에서 삭제**

### 3-4. Projectile 프리팹

1. Hierarchy > 우클릭 > **3D Object > Sphere**
2. 이름: `Projectile`
3. Transform: Scale `(0.2, 0.2, 0.2)`
4. **Material:** `Mat_Projectile`
5. **컴포넌트 추가:**
   - `ProjectileHandler` (Add Component)
6. **Rigidbody:**
   - ✅ Is Kinematic
   - Use Gravity: 해제
7. **SphereCollider:**
   - ✅ Is Trigger
8. **Layer:** `Projectile` (8번)
9. `Assets/Prefabs/Projectiles/` 폴더로 드래그 → 프리팹 생성
10. **Hierarchy에서 삭제**

### 3-5. XPGem 프리팹

1. Hierarchy > 우클릭 > **3D Object > Cube**
2. 이름: `XPGem`
3. Transform: Scale `(0.3, 0.3, 0.3)`, Rotation `(45, 45, 0)` ← 다이아몬드 느낌
4. **Material:** `Mat_XPGem`
5. **컴포넌트 추가:**
   - `XPGem` (Add Component)
     - magnetRange: `3`
     - moveSpeed: `10`
6. **Rigidbody:**
   - ✅ Is Kinematic
   - Use Gravity: 해제
7. **BoxCollider:**
   - ✅ Is Trigger
   - Size: `(1, 1, 1)` (기본값 유지)
8. **Layer:** `XPGem` (9번)
9. `Assets/Prefabs/XP/` 폴더로 드래그 → 프리팹 생성
10. **Hierarchy에서 삭제**

---

## 4. 파티클 프리팹 생성

### 4-1. VFX_HitSpark

1. Hierarchy > 우클릭 > **Effects > Particle System**
2. 이름: `VFX_HitSpark`
3. **Particle System 설정:**
   - Duration: `0.3`
   - Looping: **해제**
   - Start Lifetime: `0.2`
   - Start Speed: `8`
   - Start Size: `0.15`
   - Max Particles: `20`
   - Simulation Space: `World`
   - **Stop Action:** `Callback` ← 중요! (풀 반환에 필요)
4. **Emission:**
   - Rate over Time: `0`
   - Bursts: Count `10`, Time `0`
5. **Shape:**
   - Shape: `Sphere`
   - Radius: `0.1`
6. **Color over Lifetime:**
   - 흰색 → 투명 (Alpha 1→0)
7. **Size over Lifetime:**
   - 커브: 1 → 0 (점점 작아짐)
8. **Renderer:**
   - Material: Default-Particle (또는 기본 URP 파티클 Material)
9. `Assets/Prefabs/VFX/` 폴더로 드래그 → 프리팹 생성
10. Hierarchy에서 삭제

### 4-2. VFX_EnemyDeath

1. Hierarchy > 우클릭 > **Effects > Particle System**
2. 이름: `VFX_EnemyDeath`
3. **Particle System 설정:**
   - Duration: `0.5`
   - Looping: **해제**
   - Start Lifetime: `0.4`
   - Start Speed: `12`
   - Start Size: `0.3`
   - Max Particles: `30`
   - Simulation Space: `World`
   - **Stop Action:** `Callback`
4. **Emission:**
   - Rate over Time: `0`
   - Bursts: Count `20`, Time `0`
5. **Shape:**
   - Shape: `Sphere`
   - Radius: `0.3`
6. **Color over Lifetime:**
   - 원래 색상 → 투명
7. **Size over Lifetime:**
   - 커브: 1 → 0
8. `Assets/Prefabs/VFX/` 폴더로 드래그 → 프리팹 생성
9. Hierarchy에서 삭제

### 4-3. VFX_MuzzleFlash

1. Hierarchy > 우클릭 > **Effects > Particle System**
2. 이름: `VFX_MuzzleFlash`
3. **Particle System 설정:**
   - Duration: `0.1`
   - Looping: **해제**
   - Start Lifetime: `0.1`
   - Start Speed: `0`
   - Start Size: `0.5`
   - Start Color: `#FFFFFF`
   - Max Particles: `5`
   - **Stop Action:** `Callback`
4. **Emission:**
   - Bursts: Count `3`, Time `0`
5. **Size over Lifetime:**
   - 커브: 1 → 0 (빠르게 사라짐)
6. `Assets/Prefabs/VFX/` 폴더로 드래그 → 프리팹 생성
7. Hierarchy에서 삭제

### 4-4. VFX_LevelUp

1. Hierarchy > 우클릭 > **Effects > Particle System**
2. 이름: `VFX_LevelUp`
3. **Particle System 설정:**
   - Duration: `0.8`
   - Looping: **해제**
   - Start Lifetime: `0.6`
   - Start Speed: `5`
   - Start Size: `0.2`
   - Start Color: `#FFFF00` (노란색)
   - Max Particles: `50`
   - **Stop Action:** `Callback`
4. **Emission:**
   - Bursts: Count `40`, Time `0`
5. **Shape:**
   - Shape: `Circle`
   - Radius: `1`
   - Emit from Edge: **체크** ← 원형 충격파 효과
6. `Assets/Prefabs/VFX/` 폴더로 드래그 → 프리팹 생성
7. Hierarchy에서 삭제

---

## 5. ScriptableObject 에셋 생성

**Assets/ScriptableObjects/** 폴더에서 우클릭 > Create > GeometrySurvivor

### 5-1. EnemyData 에셋

**Assets/ScriptableObjects/Enemies/ChaserData.asset**

| 필드 | 값 |
|------|---|
| enemyName | 추적자 |
| prefab | `Prefabs/Enemies/Enemy_Chaser` |
| maxHP | `10` |
| moveSpeed | `5` |
| damage | `10` |
| xpValue | `5` |
| color | `#FF3333` (빨간색) |

**Assets/ScriptableObjects/Enemies/TankData.asset**

| 필드 | 값 |
|------|---|
| enemyName | 탱커 |
| prefab | `Prefabs/Enemies/Enemy_Tank` |
| maxHP | `50` |
| moveSpeed | `2` |
| damage | `20` |
| xpValue | `15` |
| color | `#FFCC00` (노란색) |

### 5-2. WeaponData 에셋

**Assets/ScriptableObjects/Weapons/BasicWeaponData.asset**

| 필드 | 값 |
|------|---|
| weaponName | 기본 무기 |
| baseDamage | `10` |
| fireRate | `2` |
| projectileSpeed | `15` |
| projectilePrefab | `Prefabs/Projectiles/Projectile` |

**levelBonuses 배열 (6개):**

| Index | damageMultiplier | fireRateMultiplier | projectileCount | particleScale |
|-------|------------------|--------------------|-----------------|---------------|
| 0 (Lv0) | 1.0 | 1.0 | 1 | 1.0 |
| 1 (Lv1) | 1.2 | 1.1 | 1 | 1.2 |
| 2 (Lv2) | 1.5 | 1.2 | 2 | 1.5 |
| 3 (Lv3) | 1.8 | 1.3 | 3 | 1.8 |
| 4 (Lv4) | 2.2 | 1.5 | 4 | 2.0 |
| 5 (Lv5) | 3.0 | 2.0 | 5 | 2.5 |

### 5-3. SpawnWaveData 에셋

**Assets/ScriptableObjects/Waves/Wave_Early.asset** (초반)

| 필드 | 값 |
|------|---|
| startTime | `0` |
| spawnInterval | `1.5` |
| enemyData | `ChaserData` |
| hpMultiplier | `1.0` |
| maxConcurrentEnemies | `15` |

**Assets/ScriptableObjects/Waves/Wave_Mid.asset** (중반)

| 필드 | 값 |
|------|---|
| startTime | `60` |
| spawnInterval | `1.0` |
| enemyData | `TankData` |
| hpMultiplier | `1.0` |
| maxConcurrentEnemies | `25` |

**Assets/ScriptableObjects/Waves/Wave_Late.asset** (후반)

| 필드 | 값 |
|------|---|
| startTime | `180` |
| spawnInterval | `0.5` |
| enemyData | `ChaserData` |
| hpMultiplier | `2.0` |
| maxConcurrentEnemies | `40` |

---

## 6. 씬 계층 구성

Hierarchy를 아래 구조로 구성합니다. 빈 GameObject를 만들어 그룹으로 사용합니다.

```
SampleScene
├── --- Managers ---              (빈 GameObject, 정리용)
│   ├── GameManager               ← GameManager 컴포넌트
│   ├── EnemyManager              ← EnemyManager 컴포넌트
│   ├── WeaponManager             ← WeaponManager 컴포넌트
│   ├── XPManager                 ← XPManager 컴포넌트
│   └── ParticleManager           ← ParticleManager 컴포넌트
│
├── --- Gameplay ---              (빈 GameObject, 정리용)
│   ├── Player                    ← 이미 씬에 있음 (Step 3-1)
│   └── Ground                    ← Plane
│
├── Main Camera                   ← CameraFollow + CameraShake 컴포넌트
│
├── Canvas                        ← UI 요소 (Step 7)
│   ├── HUD
│   │   ├── HPBar
│   │   ├── Timer
│   │   ├── XPBar
│   │   └── LevelText
│   ├── GameOverPanel
│   └── ClearedPanel
│
├── EventSystem                   ← Canvas와 함께 자동 생성
│
└── Global Volume                 ← Post Processing (Step 8)
```

### 6-1. Managers 구성

1. **GameManager 오브젝트:**
   - 빈 GameObject 생성, 이름: `GameManager`
   - Add Component: `GameManager`
   - gameDuration: `300`
   - 참조 필드들은 아래에서 모두 연결한 후 마지막에 드래그

2. **EnemyManager 오브젝트:**
   - 빈 GameObject 생성, 이름: `EnemyManager`
   - Add Component: `EnemyManager`
   - spawnDistance: `15`
   - waves 배열 (Size: 3):
     - [0]: `Wave_Early`
     - [1]: `Wave_Mid`
     - [2]: `Wave_Late`

3. **WeaponManager 오브젝트:**
   - 빈 GameObject 생성, 이름: `WeaponManager`
   - Add Component: `WeaponManager`
   - weaponData: `BasicWeaponData`
   - maxProjectileRange: `20`

4. **XPManager 오브젝트:**
   - 빈 GameObject 생성, 이름: `XPManager`
   - Add Component: `XPManager`
   - xpThresholds: `10, 25, 50, 100, 200`
   - xpGemPrefab: `Prefabs/XP/XPGem`
   - gemMagnetRange: `3`
   - gemMoveSpeed: `10`

5. **ParticleManager 오브젝트:**
   - 빈 GameObject 생성, 이름: `ParticleManager`
   - Add Component: `ParticleManager`
   - entries 배열 (Size: 4):

| Index | type | prefab | poolSize |
|-------|------|--------|----------|
| 0 | MuzzleFlash | `VFX_MuzzleFlash` | 10 |
| 1 | HitSpark | `VFX_HitSpark` | 30 |
| 2 | EnemyDeath | `VFX_EnemyDeath` | 20 |
| 3 | LevelUp | `VFX_LevelUp` | 3 |

### 6-2. Ground 생성

1. Hierarchy > 우클릭 > **3D Object > Plane**
2. 이름: `Ground`
3. Transform: Position `(0, 0, 0)`, Scale `(5, 1, 5)` → 실제 크기 50x50 유닛
4. Material: `Mat_Ground`
5. Collider의 Is Trigger: **해제** (바닥은 트리거 아님)

### 6-3. Camera 설정

1. `Main Camera` 선택
2. **Add Component:** `CameraFollow`
   - target: `Player` (씬의 Player 드래그)
   - offset: `(0, 15, -8)`
   - smoothSpeed: `5`
3. **Add Component:** `CameraShake`
4. Transform 초기 위치: `(0, 15, -8)`
5. Rotation: 카메라가 Player를 바라보도록 자동 설정됨 (CameraFollow.LookAt)

---

## 7. UI 구성

### 7-1. Canvas 생성

1. Hierarchy > 우클릭 > **UI > Canvas**
2. Canvas 설정:
   - Render Mode: `Screen Space - Overlay`
   - Canvas Scaler:
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 x 1080`
     - Match: `0.5`

### 7-2. HUD 그룹

빈 GameObject `HUD`를 Canvas 하위에 생성.

#### HP 바 (좌측 상단)

1. HUD 하위에 우클릭 > **UI > Slider**
2. 이름: `HPBar`
3. RectTransform:
   - Anchor: Top-Left
   - Pos: `(160, -40)`
   - Size: `(250, 25)`
4. Slider 설정:
   - Min: `0`, Max: `1`, Value: `1`
   - Interactable: **해제**
5. 자식 오브젝트 색상:
   - Background: `#333333`
   - Fill: `#FF3333` (빨간색)
6. Handle 오브젝트: **삭제** (표시만 하는 바)

#### 타이머 (상단 중앙)

1. HUD 하위에 우클릭 > **UI > Text - TextMeshPro**
2. 이름: `TimerText`
3. RectTransform:
   - Anchor: Top-Center
   - Pos: `(0, -40)`
   - Size: `(200, 50)`
4. TextMeshPro 설정:
   - Text: `5:00`
   - Font Size: `36`
   - Alignment: Center
   - Color: `#FFFFFF`

#### XP 바 (하단)

1. HUD 하위에 우클릭 > **UI > Slider**
2. 이름: `XPBar`
3. RectTransform:
   - Anchor: Bottom-Center
   - Pos: `(0, 30)`
   - Size: `(600, 15)`
4. Slider 설정: Min `0`, Max `1`, Value `0`, Interactable: 해제
5. Fill 색상: `#00CCFF` (밝은 파란색)
6. Handle: 삭제

#### 레벨 텍스트 (XP 바 옆)

1. HUD 하위에 우클릭 > **UI > Text - TextMeshPro**
2. 이름: `LevelText`
3. RectTransform:
   - Anchor: Bottom-Center
   - Pos: `(-340, 30)`
   - Size: `(80, 30)`
4. TextMeshPro 설정:
   - Text: `Lv.0`
   - Font Size: `24`
   - Alignment: Center
   - Color: `#FFFF00`

### 7-3. 게임오버 패널

1. Canvas 하위에 우클릭 > **UI > Panel**
2. 이름: `GameOverPanel`
3. Image 색상: `(0, 0, 0, 0.7)` (반투명 검정)
4. **기본 비활성:** Inspector에서 체크박스 해제
5. 하위 요소:

| 요소 | 타입 | 텍스트 | 위치 | 크기 |
|------|------|--------|------|------|
| Title | Text-TMP | `GAME OVER` | `(0, 60)` | `(400, 80)` |
| SurvivalTime | Text-TMP | `생존 시간: 0:00` | `(0, 0)` | `(300, 40)` |
| RestartButton | Button-TMP | `다시 시작` | `(0, -60)` | `(200, 50)` |

- Title: Font Size `48`, Color `#FF3333`, Bold, Center
- SurvivalTime: Font Size `24`, Color `#FFFFFF`, Center
- RestartButton: 기본 스타일

### 7-4. 클리어 패널

1. Canvas 하위에 우클릭 > **UI > Panel**
2. 이름: `ClearedPanel`
3. Image 색상: `(0, 0, 0, 0.7)`
4. **기본 비활성**
5. 하위 요소:

| 요소 | 타입 | 텍스트 | 위치 | 크기 |
|------|------|--------|------|------|
| Title | Text-TMP | `SURVIVED!` | `(0, 30)` | `(400, 80)` |
| RestartButton | Button-TMP | `다시 시작` | `(0, -40)` | `(200, 50)` |

- Title: Font Size `48`, Color `#00FF00`, Bold, Center

### 7-5. UIManager 컴포넌트

1. Canvas (또는 별도 빈 오브젝트)에 `UIManager` 컴포넌트 추가
2. 필드 연결:

| 필드 | 드래그 대상 |
|------|------------|
| hpBar | `HPBar` (Slider) |
| xpBar | `XPBar` (Slider) |
| timerText | `TimerText` (TextMeshProUGUI) |
| levelText | `LevelText` (TextMeshProUGUI) |
| gameOverPanel | `GameOverPanel` (GameObject) |
| survivalTimeText | `SurvivalTime` (TextMeshProUGUI) |
| restartButtonGameOver | `GameOverPanel/RestartButton` (Button) |
| clearedPanel | `ClearedPanel` (GameObject) |
| restartButtonCleared | `ClearedPanel/RestartButton` (Button) |

---

## 8. Post Processing 설정

### 8-1. Global Volume

1. Hierarchy > 우클릭 > **Volume > Global Volume**
2. 이름: `Global Volume`
3. Volume 컴포넌트에서 **New** 클릭 → 새 Profile 생성

### 8-2. Bloom 추가

1. Add Override > **Post-processing > Bloom**
2. 설정:
   - Threshold: `0.8` ✅
   - Intensity: `1.5` ✅
   - Scatter: `0.7` ✅

### 8-3. Vignette 추가

1. Add Override > **Post-processing > Vignette**
2. 설정:
   - Intensity: `0.3` ✅
   - Smoothness: `0.5` ✅

### 8-4. URP Renderer에서 Post Processing 활성화

1. `Assets/Settings/PC_Renderer.asset` 선택
2. Inspector에서 **Post Processing** 체크 확인
3. Main Camera에서 **Rendering > Post Processing** 체크 확인

---

## 9. GameManager 참조 연결 (최종)

씬의 `GameManager` 오브젝트를 선택하고 Inspector에서 모든 참조를 드래그:

| 필드 | 드래그 대상 |
|------|------------|
| player | 씬의 `Player` |
| enemyManager | 씬의 `EnemyManager` |
| weaponManager | 씬의 `WeaponManager` |
| xpManager | 씬의 `XPManager` |
| particleManager | 씬의 `ParticleManager` |
| uiManager | `UIManager` 컴포넌트가 있는 오브젝트 |

---

## 10. 최종 체크리스트

플레이 전 확인 사항:

- [ ] Console에 컴파일 에러 없음
- [ ] 모든 프리팹의 Collider에 `Is Trigger` 체크됨 (Ground 제외)
- [ ] 모든 프리팹의 Rigidbody에 `Is Kinematic` 체크됨
- [ ] 각 오브젝트의 Layer가 올바르게 설정됨
- [ ] Physics Collision Matrix가 위 표와 일치
- [ ] 모든 ScriptableObject 에셋의 prefab 필드에 프리팹이 연결됨
- [ ] GameManager의 모든 참조 필드가 연결됨
- [ ] EnemyManager의 waves 배열에 3개 웨이브 에셋이 할당됨
- [ ] WeaponManager의 weaponData에 BasicWeaponData 에셋이 할당됨
- [ ] XPManager의 xpGemPrefab에 XPGem 프리팹이 할당됨
- [ ] ParticleManager의 entries에 4개 파티클 프리팹이 할당됨
- [ ] UIManager의 모든 UI 참조가 연결됨
- [ ] 파티클 프리팹의 Stop Action이 모두 `Callback`으로 설정됨
- [ ] Camera에 CameraFollow의 target이 Player로 설정됨
- [ ] Post Processing이 카메라와 렌더러에서 활성화됨
- [ ] GameOverPanel과 ClearedPanel이 비활성 상태

---

## 11. 플레이 테스트

**▶ Play** 버튼을 누르고 확인:

1. **이동:** WASD / 방향키로 플레이어가 이동하는지
2. **경계:** 맵 가장자리(25 유닛)에서 플레이어가 멈추는지
3. **자동 발사:** 발사체가 자동으로 나가는지
4. **적 스폰:** 카메라 바깥에서 적이 나타나 플레이어를 추적하는지
5. **충돌:** 발사체가 적을 맞추면 적이 데미지를 받고 죽는지
6. **젬 수집:** 적 처치 시 젬이 드롭되고 가까이 가면 빨려오는지
7. **레벨업:** 젬을 모으면 레벨업하고 발사체 수가 증가하는지
8. **타이머:** 상단 타이머가 5:00에서 카운트다운하는지
9. **게임오버:** 적에게 맞아 HP가 0이 되면 게임오버 패널이 뜨는지
10. **파티클:** 히트/처치 시 파티클이 재생되는지

문제가 있으면 Console 로그를 확인하세요.
