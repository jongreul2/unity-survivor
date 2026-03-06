# Geometry Survivor

**5분 생존 미니멀리스트 3D 탄막 액션 게임**

화면을 가득 채우는 기하학적 도형의 적들을 물리치고 5분간 생존하세요!

---

## 게임 소개

Geometry Survivor는 뱀파이어 서바이버 스타일의 탄막 생존 액션 게임입니다. 미니멀리스트 3D 기하학 도형으로 구성된 세계에서 자동 발사되는 무기와 레벨업 시스템을 활용하여 밀려오는 적들로부터 5분간 살아남는 것이 목표입니다.

### 핵심 특징

- 자동 발사 무기 + WASD 이동의 간단한 조작
- 경험치 젬 수집을 통한 레벨업 성장 시스템
- 시간이 지날수록 강해지는 적 웨이브
- 화려한 파티클 이펙트와 Bloom 후처리 효과

---

## 조작법

| 키 | 동작 |
|----|------|
| `W` / `UpArrow` | 위로 이동 |
| `S` / `DownArrow` | 아래로 이동 |
| `A` / `LeftArrow` | 왼쪽으로 이동 |
| `D` / `RightArrow` | 오른쪽으로 이동 |

> 공격은 **자동 발사**됩니다. 가장 가까운 적을 향해 발사체가 날아갑니다.

---

## 게임 규칙

### 승리 조건
- **5분(300초)** 동안 생존하면 클리어

### 패배 조건
- HP가 0이 되면 게임오버

### 경험치 & 레벨업
- 적을 처치하면 **경험치 젬**(청록색 다이아몬드)이 드롭됩니다
- 젬에 가까이 가면 자동으로 빨려들어 수집됩니다
- 경험치가 일정량 모이면 레벨업하며, 무기가 강화됩니다

### 레벨업 효과

| 레벨 | 데미지 배율 | 발사 속도 | 동시 발사 수 |
|------|-----------|----------|------------|
| 0 | x1.0 | x1.0 | 1발 |
| 1 | x1.2 | x1.1 | 1발 |
| 2 | x1.5 | x1.2 | 2발 |
| 3 | x1.8 | x1.3 | 3발 |
| 4 | x2.2 | x1.5 | 4발 |
| 5 | x3.0 | x2.0 | 5발 |

---

## 적 종류

### 추적자 (Sphere, 빨간색)
- 빠르지만 체력이 낮은 적
- HP: 10 / 이동속도: 5 / 접촉 데미지: 10
- 경험치: 5

### 탱커 (Cylinder, 노란색)
- 느리지만 체력이 높은 적
- HP: 50 / 이동속도: 2 / 접촉 데미지: 20
- 경험치: 15

---

## 웨이브 구성

| 구간 | 시간 | 스폰 간격 | 적 종류 | 최대 동시 수 |
|------|------|----------|--------|------------|
| 초반 | 0~60초 | 1.5초 | 추적자 | 15마리 |
| 중반 | 60초~ | 1.0초 | 탱커 추가 | 25마리 |
| 후반 | 180초~ | 0.5초 | 추적자(HP x2) | 40마리 |

---

## HUD 안내

```
[HP Bar]                    [4:32]
                            타이머 (카운트다운)

                  플레이어
                    [ ]
            적 적       적
              적   적


[Lv.3] [========----] XP Bar
```

- **좌측 상단**: HP 바 (빨간색)
- **상단 중앙**: 남은 시간
- **하단**: 레벨 + 경험치 바

---

## 기술 스택

- **엔진**: Unity 6000.x (URP)
- **언어**: C#
- **Input**: Unity Input System 패키지
- **UI**: TextMeshPro
- **후처리**: URP Volume (Bloom, Vignette)

## 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── Core/       GameManager, GameTimer, ObjectPool, CameraFollow, CameraShake, DamageHandler
│   ├── Data/       EnemyData, WeaponData, LevelBonus, SpawnWaveData (ScriptableObject)
│   ├── Player/     PlayerController
│   ├── Enemy/      EnemyManager, EnemyInstance
│   ├── Weapon/     WeaponManager, ProjectileInstance, ProjectileHandler
│   ├── XP/         XPSystem, XPManager, XPGem
│   ├── UI/         UIManager
│   └── VFX/        ParticleManager, ParticleType, ParticleReturnToPool
├── Prefabs/        Player, Enemies, Projectiles, XP, VFX
├── ScriptableObjects/  Enemies, Weapons, Waves (밸런스 데이터)
├── Materials/      Mat_Player, Mat_Chaser, Mat_Tank, Mat_Projectile, Mat_XPGem, Mat_Ground
├── Tests/EditMode/ ObjectPoolTest, DataModelTest, PlayerControllerTest 등
└── Scenes/         SampleScene (메인 씬)
```

---

## 빌드 & 실행

1. **Unity Hub**에서 Unity 6000.x 이상 버전으로 프로젝트를 엽니다
2. `Assets/Scenes/SampleScene`을 열고 **Play** 버튼을 누릅니다
3. 빌드: **File > Build Settings > Build** (Windows Standalone)

## 밸런스 조정

`Assets/ScriptableObjects/` 폴더의 에셋을 Unity Inspector에서 수정하면 코드 변경 없이 즉시 밸런스를 조정할 수 있습니다.

- `Enemies/ChaserData`, `TankData` — 적 스탯
- `Weapons/BasicWeaponData` — 무기 스탯 및 레벨별 보너스
- `Waves/Wave_Early`, `Wave_Mid`, `Wave_Late` — 스폰 타이밍 및 난이도
