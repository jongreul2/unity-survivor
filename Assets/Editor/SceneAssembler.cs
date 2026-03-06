using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SceneAssembler : EditorWindow
{
    [MenuItem("GeometrySurvivor/1. ScriptableObject 에셋 생성")]
    static void CreateScriptableObjects()
    {
        // --- EnemyData ---
        var chaserPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Chaser.prefab");
        var tankPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Tank.prefab");

        var chaserData = ScriptableObject.CreateInstance<EnemyData>();
        chaserData.enemyName = "추적자";
        chaserData.prefab = chaserPrefab;
        chaserData.maxHP = 10f;
        chaserData.moveSpeed = 5f;
        chaserData.damage = 10f;
        chaserData.xpValue = 5;
        chaserData.color = new Color(1f, 0.2f, 0.2f, 1f);
        AssetDatabase.CreateAsset(chaserData, "Assets/ScriptableObjects/Enemies/ChaserData.asset");

        var tankData = ScriptableObject.CreateInstance<EnemyData>();
        tankData.enemyName = "탱커";
        tankData.prefab = tankPrefab;
        tankData.maxHP = 50f;
        tankData.moveSpeed = 2f;
        tankData.damage = 20f;
        tankData.xpValue = 15;
        tankData.color = new Color(1f, 0.8f, 0f, 1f);
        AssetDatabase.CreateAsset(tankData, "Assets/ScriptableObjects/Enemies/TankData.asset");

        // --- WeaponData ---
        var projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/Projectile.prefab");

        var weaponData = ScriptableObject.CreateInstance<WeaponData>();
        weaponData.weaponName = "기본 무기";
        weaponData.baseDamage = 10f;
        weaponData.fireRate = 2f;
        weaponData.projectileSpeed = 15f;
        weaponData.projectilePrefab = projPrefab;
        weaponData.levelBonuses = new LevelBonus[]
        {
            new() { damageMultiplier = 1.0f, fireRateMultiplier = 1.0f, projectileCount = 1, particleScale = 1.0f },
            new() { damageMultiplier = 1.2f, fireRateMultiplier = 1.1f, projectileCount = 1, particleScale = 1.2f },
            new() { damageMultiplier = 1.5f, fireRateMultiplier = 1.2f, projectileCount = 2, particleScale = 1.5f },
            new() { damageMultiplier = 1.8f, fireRateMultiplier = 1.3f, projectileCount = 3, particleScale = 1.8f },
            new() { damageMultiplier = 2.2f, fireRateMultiplier = 1.5f, projectileCount = 4, particleScale = 2.0f },
            new() { damageMultiplier = 3.0f, fireRateMultiplier = 2.0f, projectileCount = 5, particleScale = 2.5f },
        };
        AssetDatabase.CreateAsset(weaponData, "Assets/ScriptableObjects/Weapons/BasicWeaponData.asset");

        // --- SpawnWaveData ---
        var waveEarly = ScriptableObject.CreateInstance<SpawnWaveData>();
        waveEarly.startTime = 0f;
        waveEarly.spawnInterval = 1.5f;
        waveEarly.enemyData = chaserData;
        waveEarly.hpMultiplier = 1.0f;
        waveEarly.maxConcurrentEnemies = 15;
        AssetDatabase.CreateAsset(waveEarly, "Assets/ScriptableObjects/Waves/Wave_Early.asset");

        var waveMid = ScriptableObject.CreateInstance<SpawnWaveData>();
        waveMid.startTime = 60f;
        waveMid.spawnInterval = 1.0f;
        waveMid.enemyData = tankData;
        waveMid.hpMultiplier = 1.0f;
        waveMid.maxConcurrentEnemies = 25;
        AssetDatabase.CreateAsset(waveMid, "Assets/ScriptableObjects/Waves/Wave_Mid.asset");

        var waveLate = ScriptableObject.CreateInstance<SpawnWaveData>();
        waveLate.startTime = 180f;
        waveLate.spawnInterval = 0.5f;
        waveLate.enemyData = chaserData;
        waveLate.hpMultiplier = 2.0f;
        waveLate.maxConcurrentEnemies = 40;
        AssetDatabase.CreateAsset(waveLate, "Assets/ScriptableObjects/Waves/Wave_Late.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SceneAssembler] ScriptableObject 에셋 생성 완료!");
    }

    [MenuItem("GeometrySurvivor/2. Physics Layer 설정")]
    static void SetupPhysicsLayers()
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");

        SetLayer(layers, 6, "Player");
        SetLayer(layers, 7, "Enemy");
        SetLayer(layers, 8, "Projectile");
        SetLayer(layers, 9, "XPGem");

        tagManager.ApplyModifiedProperties();

        // Collision Matrix 설정
        // 모든 커스텀 레이어 간 충돌 해제 후 필요한 것만 활성화
        int playerLayer = 6;
        int enemyLayer = 7;
        int projectileLayer = 8;
        int xpGemLayer = 9;

        // 커스텀 레이어 간 충돌 전부 해제
        Physics.IgnoreLayerCollision(playerLayer, projectileLayer, true);
        Physics.IgnoreLayerCollision(playerLayer, playerLayer, true);
        Physics.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
        Physics.IgnoreLayerCollision(enemyLayer, xpGemLayer, true);
        Physics.IgnoreLayerCollision(projectileLayer, projectileLayer, true);
        Physics.IgnoreLayerCollision(projectileLayer, xpGemLayer, true);
        Physics.IgnoreLayerCollision(xpGemLayer, xpGemLayer, true);

        // 필요한 충돌만 활성화
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        Physics.IgnoreLayerCollision(playerLayer, xpGemLayer, false);
        Physics.IgnoreLayerCollision(enemyLayer, projectileLayer, false);

        Debug.Log("[SceneAssembler] Physics Layer 설정 완료!");
    }

    static void SetLayer(SerializedProperty layers, int index, string name)
    {
        var layer = layers.GetArrayElementAtIndex(index);
        if (string.IsNullOrEmpty(layer.stringValue))
        {
            layer.stringValue = name;
        }
    }

    [MenuItem("GeometrySurvivor/3. 씬 조립 (전체 자동화)")]
    static void AssembleScene()
    {
        // SO 로드
        var chaserData = AssetDatabase.LoadAssetAtPath<SpawnWaveData>("Assets/ScriptableObjects/Waves/Wave_Early.asset");
        var waveMid = AssetDatabase.LoadAssetAtPath<SpawnWaveData>("Assets/ScriptableObjects/Waves/Wave_Mid.asset");
        var waveLate = AssetDatabase.LoadAssetAtPath<SpawnWaveData>("Assets/ScriptableObjects/Waves/Wave_Late.asset");
        var weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/ScriptableObjects/Weapons/BasicWeaponData.asset");
        var xpGemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/XP/XPGem.prefab");
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab");
        var matGround = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Mat_Ground.mat");

        // VFX 프리팹 로드
        var vfxHitSpark = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/VFX_HitSpark.prefab");
        var vfxEnemyDeath = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/VFX_EnemyDeath.prefab");
        var vfxMuzzleFlash = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/VFX_MuzzleFlash.prefab");
        var vfxLevelUp = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/VFX_LevelUp.prefab");

        if (chaserData == null || weaponData == null || xpGemPrefab == null || playerPrefab == null)
        {
            Debug.LogError("[SceneAssembler] 먼저 '1. ScriptableObject 에셋 생성'을 실행하세요!");
            return;
        }

        // ===== 기존 오브젝트 정리 =====
        // 이미 존재하는 Manager/Gameplay 오브젝트가 있으면 삭제
        DestroyIfExists("--- Managers ---");
        DestroyIfExists("--- Gameplay ---");
        DestroyIfExists("Canvas");
        DestroyIfExists("EventSystem");
        DestroyIfExists("Global Volume");

        // ===== Managers 그룹 =====
        var managersRoot = new GameObject("--- Managers ---");

        // GameManager
        var gmObj = new GameObject("GameManager");
        gmObj.transform.SetParent(managersRoot.transform);
        var gm = gmObj.AddComponent<GameManager>();
        gm.gameDuration = 300f;

        // EnemyManager
        var emObj = new GameObject("EnemyManager");
        emObj.transform.SetParent(managersRoot.transform);
        var em = emObj.AddComponent<EnemyManager>();
        em.spawnDistance = 15f;
        em.waves = new SpawnWaveData[] { chaserData, waveMid, waveLate };

        // WeaponManager
        var wmObj = new GameObject("WeaponManager");
        wmObj.transform.SetParent(managersRoot.transform);
        var wm = wmObj.AddComponent<WeaponManager>();
        wm.weaponData = weaponData;
        wm.maxProjectileRange = 20f;

        // XPManager
        var xmObj = new GameObject("XPManager");
        xmObj.transform.SetParent(managersRoot.transform);
        var xm = xmObj.AddComponent<XPManager>();
        xm.xpThresholds = new int[] { 10, 25, 50, 100, 200 };
        xm.xpGemPrefab = xpGemPrefab;
        xm.gemMagnetRange = 3f;
        xm.gemMoveSpeed = 10f;

        // ParticleManager
        var pmObj = new GameObject("ParticleManager");
        pmObj.transform.SetParent(managersRoot.transform);
        var pm = pmObj.AddComponent<ParticleManager>();
        pm.entries = new ParticleManager.ParticleEntry[]
        {
            new() { type = ParticleType.MuzzleFlash, prefab = vfxMuzzleFlash, poolSize = 10 },
            new() { type = ParticleType.HitSpark, prefab = vfxHitSpark, poolSize = 30 },
            new() { type = ParticleType.EnemyDeath, prefab = vfxEnemyDeath, poolSize = 20 },
            new() { type = ParticleType.LevelUp, prefab = vfxLevelUp, poolSize = 3 },
        };

        // ===== Gameplay 그룹 =====
        var gameplayRoot = new GameObject("--- Gameplay ---");

        // Player
        var playerObj = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        playerObj.name = "Player";
        playerObj.transform.SetParent(gameplayRoot.transform);
        playerObj.transform.position = new Vector3(0f, 0.5f, 0f);
        playerObj.layer = LayerMask.NameToLayer("Player");
        var player = playerObj.GetComponent<PlayerController>();
        if (player == null) player = playerObj.AddComponent<PlayerController>();
        player.maxHP = 100f;
        player.moveSpeed = 7f;
        player.mapBoundary = 25f;

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(gameplayRoot.transform);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(5f, 1f, 5f);
        if (matGround != null)
            ground.GetComponent<Renderer>().sharedMaterial = matGround;
        var groundCollider = ground.GetComponent<Collider>();
        if (groundCollider != null)
            groundCollider.isTrigger = false;

        // ===== Camera 설정 =====
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 15f, -8f);
            cam.transform.LookAt(Vector3.zero);

            if (cam.GetComponent<CameraFollow>() == null)
            {
                var cf = cam.gameObject.AddComponent<CameraFollow>();
                cf.target = playerObj.transform;
                cf.offset = new Vector3(0f, 15f, -8f);
                cf.smoothSpeed = 5f;
            }
            if (cam.GetComponent<CameraShake>() == null)
                cam.gameObject.AddComponent<CameraShake>();

            // Post Processing 활성화
            var camData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (camData != null)
                camData.renderPostProcessing = true;
        }

        // ===== UI =====
        var canvas = CreateCanvas();
        var uiManager = canvas.AddComponent<UIManager>();

        // HUD 그룹
        var hud = CreateUIObject("HUD", canvas.transform);
        SetFullStretch(hud.GetComponent<RectTransform>());

        // HP Bar
        var hpBar = CreateSlider("HPBar", hud.transform,
            new Vector2(160f, -40f), new Vector2(250f, 25f),
            TextAnchor.UpperLeft, new Color(1f, 0.2f, 0.2f));
        hpBar.value = 1f;

        // Timer
        var timerText = CreateTMPText("TimerText", hud.transform,
            "5:00", 36, TextAlignmentOptions.Center,
            new Vector2(0f, -40f), new Vector2(200f, 50f),
            TextAnchor.UpperCenter);

        // XP Bar
        var xpBar = CreateSlider("XPBar", hud.transform,
            new Vector2(0f, 30f), new Vector2(600f, 15f),
            TextAnchor.LowerCenter, new Color(0f, 0.8f, 1f));
        xpBar.value = 0f;

        // Level Text
        var levelText = CreateTMPText("LevelText", hud.transform,
            "Lv.0", 24, TextAlignmentOptions.Center,
            new Vector2(-340f, 30f), new Vector2(80f, 30f),
            TextAnchor.LowerCenter);
        levelText.color = Color.yellow;

        // GameOver Panel
        var gameOverPanel = CreatePanel("GameOverPanel", canvas.transform);
        var goTitle = CreateTMPText("Title", gameOverPanel.transform,
            "GAME OVER", 48, TextAlignmentOptions.Center,
            new Vector2(0f, 60f), new Vector2(400f, 80f), TextAnchor.MiddleCenter);
        goTitle.color = new Color(1f, 0.2f, 0.2f);
        goTitle.fontStyle = FontStyles.Bold;
        var survivalTimeText = CreateTMPText("SurvivalTime", gameOverPanel.transform,
            "생존 시간: 0:00", 24, TextAlignmentOptions.Center,
            new Vector2(0f, 0f), new Vector2(300f, 40f), TextAnchor.MiddleCenter);
        var restartBtnGO = CreateButton("RestartButton", gameOverPanel.transform,
            "다시 시작", new Vector2(0f, -60f), new Vector2(200f, 50f));
        gameOverPanel.SetActive(false);

        // Cleared Panel
        var clearedPanel = CreatePanel("ClearedPanel", canvas.transform);
        var clTitle = CreateTMPText("Title", clearedPanel.transform,
            "SURVIVED!", 48, TextAlignmentOptions.Center,
            new Vector2(0f, 30f), new Vector2(400f, 80f), TextAnchor.MiddleCenter);
        clTitle.color = Color.green;
        clTitle.fontStyle = FontStyles.Bold;
        var restartBtnCl = CreateButton("RestartButton", clearedPanel.transform,
            "다시 시작", new Vector2(0f, -40f), new Vector2(200f, 50f));
        clearedPanel.SetActive(false);

        // UIManager 참조 연결
        uiManager.hpBar = hpBar;
        uiManager.xpBar = xpBar;
        uiManager.timerText = timerText;
        uiManager.levelText = levelText;
        uiManager.gameOverPanel = gameOverPanel;
        uiManager.survivalTimeText = survivalTimeText;
        uiManager.restartButtonGameOver = restartBtnGO.GetComponent<Button>();
        uiManager.clearedPanel = clearedPanel;
        uiManager.restartButtonCleared = restartBtnCl.GetComponent<Button>();

        // ===== Global Volume =====
        var volumeObj = new GameObject("Global Volume");
        var volume = volumeObj.AddComponent<Volume>();
        volume.isGlobal = true;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        var bloom = profile.Add<Bloom>();
        bloom.threshold.overrideState = true;
        bloom.threshold.value = 0.8f;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = 1.5f;
        bloom.scatter.overrideState = true;
        bloom.scatter.value = 0.7f;

        var vignette = profile.Add<Vignette>();
        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0.3f;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = 0.5f;

        AssetDatabase.CreateAsset(profile, "Assets/Settings/GameVolumeProfile.asset");
        volume.profile = profile;

        // ===== GameManager 참조 연결 =====
        gm.player = player;
        gm.enemyManager = em;
        gm.weaponManager = wm;
        gm.xpManager = xm;
        gm.particleManager = pm;
        gm.uiManager = uiManager;

        // ===== 씬 저장 =====
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[SceneAssembler] 씬 조립 완료! Ctrl+S로 씬을 저장하세요.");
    }

    // ===== UI 헬퍼 메서드 =====

    static void DestroyIfExists(string name)
    {
        var obj = GameObject.Find(name);
        if (obj != null) DestroyImmediate(obj);
    }

    static GameObject CreateCanvas()
    {
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        return canvasObj;
    }

    static GameObject CreateUIObject(string name, Transform parent)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    static void SetFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void SetAnchor(RectTransform rt, TextAnchor anchor)
    {
        Vector2 a = anchor switch
        {
            TextAnchor.UpperLeft => new Vector2(0f, 1f),
            TextAnchor.UpperCenter => new Vector2(0.5f, 1f),
            TextAnchor.UpperRight => new Vector2(1f, 1f),
            TextAnchor.MiddleLeft => new Vector2(0f, 0.5f),
            TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
            TextAnchor.MiddleRight => new Vector2(1f, 0.5f),
            TextAnchor.LowerLeft => new Vector2(0f, 0f),
            TextAnchor.LowerCenter => new Vector2(0.5f, 0f),
            TextAnchor.LowerRight => new Vector2(1f, 0f),
            _ => new Vector2(0.5f, 0.5f),
        };
        rt.anchorMin = a;
        rt.anchorMax = a;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    static Slider CreateSlider(string name, Transform parent, Vector2 pos, Vector2 size, TextAnchor anchor, Color fillColor)
    {
        var sliderObj = new GameObject(name, typeof(RectTransform));
        sliderObj.transform.SetParent(parent, false);
        var rt = sliderObj.GetComponent<RectTransform>();
        SetAnchor(rt, anchor);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        // Background
        var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(sliderObj.transform, false);
        var bgRt = bg.GetComponent<RectTransform>();
        SetFullStretch(bgRt);
        bg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Fill Area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        var faRt = fillArea.GetComponent<RectTransform>();
        SetFullStretch(faRt);

        // Fill
        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fillRt = fill.GetComponent<RectTransform>();
        SetFullStretch(fillRt);
        fill.GetComponent<Image>().color = fillColor;

        var slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;

        return slider;
    }

    static TextMeshProUGUI CreateTMPText(string name, Transform parent, string text, float fontSize,
        TextAlignmentOptions alignment, Vector2 pos, Vector2 size, TextAnchor anchor)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var rt = obj.GetComponent<RectTransform>();
        SetAnchor(rt, anchor);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;

        return tmp;
    }

    static GameObject CreatePanel(string name, Transform parent)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rt = panel.GetComponent<RectTransform>();
        SetFullStretch(rt);
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
        return panel;
    }

    static GameObject CreateButton(string name, Transform parent, string label, Vector2 pos, Vector2 size)
    {
        var btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        var rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        btnObj.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1f);

        var textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        var textRt = textObj.GetComponent<RectTransform>();
        SetFullStretch(textRt);
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btnObj;
    }
}
