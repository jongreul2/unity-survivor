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
    public UIManager uiManager;

    public GameState State { get; private set; }
    public GameTimer Timer { get; private set; }

    public event Action OnGameOver;
    public event Action OnGameCleared;

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
