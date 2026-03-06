using NUnit.Framework;

[TestFixture]
public class GameManagerTest
{
    [Test]
    public void GameTimer_TickDown_ReducesTime()
    {
        var timer = new GameTimer(300f);
        timer.Tick(1f);
        Assert.AreEqual(299f, timer.RemainingTime, 0.01f);
    }

    [Test]
    public void GameTimer_ReachesZero_IsComplete()
    {
        var timer = new GameTimer(1f);
        timer.Tick(1f);
        Assert.IsTrue(timer.IsComplete);
    }

    [Test]
    public void GameTimer_FormattedTime_CorrectFormat()
    {
        var timer = new GameTimer(125f);
        Assert.AreEqual("2:05", timer.FormattedTime);
    }

    [Test]
    public void GameTimer_DoesNotGoBelowZero()
    {
        var timer = new GameTimer(1f);
        timer.Tick(5f);
        Assert.AreEqual(0f, timer.RemainingTime);
    }
}
