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
