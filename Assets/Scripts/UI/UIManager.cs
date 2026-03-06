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
