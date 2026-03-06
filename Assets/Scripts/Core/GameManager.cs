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

    private void Start()
    {
        State = GameState.Playing;
        Timer = new GameTimer(gameDuration);

        enemyManager.Initialize(player.transform, particleManager, xpManager);
        weaponManager.Initialize(player.transform, enemyManager);
        xpManager.Initialize(player.transform);

        if (uiManager != null)
        {
            uiManager.Initialize(this, player, xpManager.XPSystem);
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
