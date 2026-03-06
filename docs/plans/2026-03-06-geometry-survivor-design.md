# Geometry Survivor - 설계 문서

**작성일:** 2026-03-06
**상태:** 승인됨
**목적:** 프로토타입 (디자이너 없음, 핵심 재미 검증 목적)

---

## 1. 핵심 컨셉

- **장르:** 탄막 생존 액션 (뱀파이어 서바이버 스타일)
- **아트 스타일:** 미니멀리스트 3D 기하학 도형
- **매력 포인트:** 화면을 가득 채우는 화려한 파티클 효과와 기하학적 탄막 패턴
- **세션 길이:** 5분 (생존 시 클리어, 사망 시 게임오버)
- **기술 스택:** Unity 3D (URP), C#

---

## 2. 아키텍처 결정 사항

| 항목 | 결정 | 근거 |
|------|------|------|
| 엔티티 관리 | 하이브리드 (MonoBehaviour 껍데기 + 중앙 Manager 배열 순회) | 성능과 개발 편의성 균형 |
| 데이터 정의 | ScriptableObject | 에디터에서 즉시 밸런스 조정 가능 |
| 충돌 처리 | Unity Physics (Rigidbody + Collider) | 프로토타입 속도 우선. Primitive Collider로 충분 |
| 무기 시스템 | 단일 무기, 레벨업 강화 | 파티클 폭발의 시각적 쾌감 검증에 집중 |
| 성장 시스템 | 경험치 젬 드롭 → 자동 레벨업 | 핵심 피드백 루프 확보 |
| 종료 조건 | 5분 생존 클리어 / 사망 시 게임오버 | 명확한 목표 + 긴장감 |
| 카메라 | Tilted Top-Down (60~70도) | 3D 도형 입체감 + 탑다운 게임플레이 |

---

## 3. 전체 아키텍처

```
GameManager (게임 상태, 타이머, 게임오버/클리어 판정)
  ├── EnemyManager (적 배열, 일괄 이동, 풀링 관리)
  ├── WeaponManager (발사체 배열, 일괄 이동, 풀링 관리)
  ├── XPManager (젬 드롭, 수집, 레벨업)
  ├── ParticleManager (파티클 풀링)
  └── ObjectPoolManager (범용 오브젝트 풀)
```

**핵심 원칙:**
- 중앙 Manager가 엔티티 배열을 소유하고 Update()에서 일괄 순회
- 개별 적/발사체 GameObject에는 Update() 없음
- 적, 발사체, 경험치 젬, 파티클 모두 Object Pooling

---

## 4. 데이터 모델

### EnemyData (ScriptableObject)

| 필드 | 타입 | 설명 |
|------|------|------|
| enemyName | string | "추적자", "탱커" |
| prefab | GameObject | Sphere, Cylinder 프리팹 |
| maxHP | float | 추적자: 낮음, 탱커: 높음 |
| moveSpeed | float | 추적자: 빠름, 탱커: 느림 |
| damage | float | 플레이어 접촉 시 데미지 |
| xpValue | int | 처치 시 드롭하는 경험치량 |
| color | Color | 빨강, 노랑 |

### WeaponData (ScriptableObject)

| 필드 | 타입 | 설명 |
|------|------|------|
| weaponName | string | 무기 이름 |
| baseDamage | float | 기본 데미지 |
| fireRate | float | 초당 발사 횟수 |
| projectileSpeed | float | 발사체 속도 |
| projectilePrefab | GameObject | 발사체 프리팹 |
| levelBonuses | LevelBonus[] | 레벨별 강화 테이블 |

### LevelBonus (struct)

| 필드 | 타입 | 설명 |
|------|------|------|
| damageMultiplier | float | 데미지 배율 |
| fireRateMultiplier | float | 발사 속도 배율 |
| projectileCount | int | 동시 발사 수 |
| particleScale | float | 파티클 크기/양 배율 |

### 런타임 인스턴스 (struct)

**EnemyInstance:** currentHP, transform, data(SO 참조), isActive
**ProjectileInstance:** direction, transform, damage, isActive

### 레벨업 테이블

- xpThresholds: int[] (예: [10, 25, 50, 100, ...])
- 현재 경험치 >= 임계값이면 레벨업 → WeaponData.levelBonuses[level] 적용

---

## 5. 게임 루프 & 적 스포너

### 매 프레임 루프

1. PlayerController: 입력 → 이동
2. WeaponManager: 자동 발사 판정
3. EnemyManager: 전체 적 이동 (플레이어 방향 추적)
4. Unity Physics: 충돌 콜백 처리
5. XPManager: 젬 수집 판정
6. GameManager: 타이머/사망 체크

### 적 스폰 규칙

- 카메라 바깥 일정 거리에서 랜덤 위치에 스폰

| 구간 | 시간 | 스폰 간격 | 적 구성 |
|------|------|----------|--------|
| 초반 | 0~60초 | 1.5초 | 추적자(Sphere)만 |
| 중반 | 60~180초 | 1.0초 | 추적자 + 탱커(Cylinder) 혼합 |
| 후반 | 180~300초 | 0.5초 | 대량 혼합 + 체력 배율 증가 |

### SpawnWaveData (ScriptableObject)

| 필드 | 타입 | 설명 |
|------|------|------|
| startTime | float | 웨이브 시작 시각 |
| spawnInterval | float | 스폰 간격 |
| enemyData | EnemyData | 적 종류 |
| hpMultiplier | float | 체력 배율 |
| maxConcurrentEnemies | int | 동시 최대 수 |

---

## 6. 파티클 & 시각 연출

### 파티클 이벤트

| 이벤트 | 연출 | 비고 |
|--------|------|------|
| 발사체 발사 | 작은 머즐 플래시 (하얀 빛) | 레벨업 시 크기 증가 |
| 발사체 적중 | 적 색상 기반 히트 스파크 | 빨강/노랑 파편 |
| 적 처치 | 폭발 파티클 + 도형 파편화 | 핵심 쾌감 요소 |
| 경험치 젬 수집 | 플레이어로 빨려드는 빛 트레일 | 자석 효과 |
| 레벨업 | 원형 충격파 | 1회성 |

### 레벨 연동 파티클 스케일링

- Lv1: 기본 파티클 (파편 5개)
- Lv3: 파편 15개 + 크기 1.5배
- Lv5: 파편 30개 + 크기 2배 + Camera Shake

### URP Post Processing

| 효과 | 용도 |
|------|------|
| Bloom | 발사체와 폭발에 HDR 색상 → 빛 번짐 |
| Vignette | 피격 시 화면 가장자리 붉게 |
| Camera Shake | 처치 시 미세 흔들림 |

### 파티클 풀링

- 히트 스파크 30개, 처치 폭발 20개, 머즐 플래시 10개
- ParticleSystem.Stop() + OnParticleSystemStopped 콜백으로 풀 반환
- 동시 재생 한도 초과 시 가장 오래된 것 강제 반환

---

## 7. 씬 구성 & UI

### 씬 계층

```
Main Scene
├── Managers (GameManager, EnemyManager, WeaponManager, XPManager, ParticleManager, ObjectPoolManager)
├── Gameplay (Player, Ground, Camera)
├── Object Pools (EnemyPool, ProjectilePool, XPGemPool, ParticlePool)
├── UI Canvas (Screen Space)
└── Post Processing (Global Volume)
```

### UI 구성

| UI 요소 | 위치 | 설명 |
|---------|------|------|
| HP 바 | 좌측 상단 | 슬라이더, 피격 시 빨간 깜빡임 |
| 타이머 | 상단 중앙 | "4:32" 카운트다운 |
| 레벨 & XP 바 | 하단 | 경험치 진행도 + 레벨 숫자 |
| 게임오버 패널 | 중앙 (비활성) | "GAME OVER" + 생존 시간 + 재시작 |
| 클리어 패널 | 중앙 (비활성) | "SURVIVED!" + 재시작 |

### 바닥

- 고정 크기 50x50 유닛
- 진한 회색/남색 단색 Plane
- 경계 이동 제한 (클램핑)

---

## 8. 엔티티 목록

| 엔티티 | 형태 | 색상 | 역할 |
|--------|------|------|------|
| 플레이어 | Cube | 하얀색 | 이동, 자동 공격 |
| 추적자 | Sphere | 빨간색 | 빠른 추적 |
| 탱커 | Cylinder | 노란색 | 높은 체력 |
| 발사체 | 작은 Sphere | 하얀/밝은색 | 직선 이동, 적 타격 |
| 경험치 젬 | 작은 다이아몬드(회전 Cube) | 초록/청록색 | 수집 시 경험치 |
