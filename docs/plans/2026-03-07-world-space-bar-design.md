# 월드 스페이스 바 설계

## 개요
플레이어 머리 위에 XP 진행도 바, 적 머리 위에 HP 바를 월드 스페이스 Canvas로 표시한다.

## 접근법
World Space Canvas + UI Image 기반. 각 캐릭터 오브젝트의 자식으로 부착하여 Billboard 처리.

## 새로 만들 파일
- `Assets/Scripts/UI/WorldSpaceBar.cs` — 재사용 가능한 월드 스페이스 바 컴포넌트

## 수정할 파일
- `EnemyInstance.cs` — `maxHP` 필드 추가
- `EnemyManager.cs` — 스폰 시 바 초기화, UpdateEnemies에서 바 값 갱신
- `DamageHandler.cs` — 피격 시 바 값 즉시 갱신
- `GameManager.cs` — 플레이어 XP 바 초기화 및 업데이트 연결
- `SceneAssembler.cs` — 프리팹에 WorldSpaceBar 컴포넌트 추가 메뉴

## WorldSpaceBar 컴포넌트
- Awake에서 World Space Canvas + Background Image + Fill Image 자식 생성
- LateUpdate에서 Camera.main을 향한 Billboard 처리
- `SetValue(float normalized)` — 0~1 비율로 Fill의 localScale.x 갱신
- 설정: barColor, barWidth(0.8), barHeight(0.08), yOffset(1.2)
- 풀링 호환: OnEnable 시 값 리셋

## 데이터 흐름

### 적 HP 바
```
EnemyManager.SpawnEnemy()
  -> EnemyInstance.maxHP = data.maxHP * hpMultiplier
  -> WorldSpaceBar.SetValue(1.0)
DamageHandler.OnTriggerEnter()
  -> enemyInstance.TakeDamage()
  -> WorldSpaceBar.SetValue(currentHP / maxHP)
```

### 플레이어 XP 바
```
GameManager.Start()
  -> Player의 WorldSpaceBar 초기화 (색상: 시안)
XPSystem.OnLevelUp / UIManager.Update()
  -> WorldSpaceBar.SetValue(currentXP / xpForNextLevel)
  -> 만렙: SetValue(1.0)
```

## 크기 및 위치
- 적 바: 너비 0.8, 높이 0.08, Y 오프셋 1.0
- 플레이어 XP 바: 너비 1.0, 높이 0.06, Y 오프셋 1.2
- 적 바 색상: 빨간색, 플레이어 XP 바 색상: 시안
