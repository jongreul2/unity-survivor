using System;

public class GameTimer
{
    public float TotalTime { get; }
    public float RemainingTime { get; private set; }
    public float ElapsedTime => TotalTime - RemainingTime;
    public bool IsComplete => RemainingTime <= 0f;

    public string FormattedTime
    {
        get
        {
            int minutes = (int)(RemainingTime / 60f);
            int seconds = (int)(RemainingTime % 60f);
            return $"{minutes}:{seconds:D2}";
        }
    }

    public GameTimer(float totalTime)
    {
        TotalTime = totalTime;
        RemainingTime = totalTime;
    }

    public void Tick(float deltaTime)
    {
        RemainingTime = Math.Max(0f, RemainingTime - deltaTime);
    }
}
